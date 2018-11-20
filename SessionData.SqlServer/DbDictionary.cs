using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;

namespace SessionData
{
	public abstract partial class DbDictionary : Dictionary<string, object>
	{
		public DbDictionary(string connectionString)
		{			
			ConnectionString = connectionString;
			Serializers = new Dictionary<Type, Func<object, string>>();
			Deserializers = new Dictionary<Type, Func<string, object>>()
			{
				{ typeof(string), (json) => JsonConvert.DeserializeObject(json) },
				{ typeof(DateTime), (json) => JsonConvert.DeserializeObject<DateTime>(json) },
				{ typeof(bool), (json) => JsonConvert.DeserializeObject<bool>(json) },
				{ typeof(int), (json) => JsonConvert.DeserializeObject<int>(json) },
				{ typeof(double), (json) => JsonConvert.DeserializeObject<double>(json) },
				{ typeof(decimal), (json) => JsonConvert.DeserializeObject<decimal>(json) }
			};
		}
		
		protected abstract IDbConnection GetConnection();		
		protected abstract void Initialize(IDbConnection connection);
		protected abstract string QueryCommand { get; }
		protected abstract string InsertCommand { get; }
		protected abstract string UpdateCommand { get; }
		protected abstract string DeleteCommand { get; }

		protected string ConnectionString { get; }
		protected abstract bool KeyExists(IDbConnection connection, string key);
		
		/// <summary>
		/// Override this (setting to true) to prevent the Initialize method from executing, which can be a performance benefit.
		/// Leave this false initially to ensure that the backing table is created. But after the table is created,
		/// it's no longer necessary to check for table existence with every dictionary access.
		/// </summary>
		protected virtual bool IsInitialized { get { return false; } }

		/// <summary>
		/// If you need to customize the serialization behavior of a specific type, add a handler here
		/// </summary>
		public Dictionary<Type, Func<object, string>> Serializers { get; private set; }

		/// <summary>
		/// If you need to handle deserialization of a certain type in a specific way, add a handler here
		/// </summary>
		public Dictionary<Type, Func<string, object>> Deserializers { get; private set; }

		/// <summary>
		/// Override this if you need to customize the base serialization behavior
		/// </summary>
		protected virtual string Serialize(object value)
		{
			return JsonConvert.SerializeObject(value);
		}

		/// <summary>
		/// Override this to modify keys, for example to prefix with a user name or session Id
		/// </summary>
		protected virtual string FormatKey(string key)
		{
			return key;
		}

		public new object this[string key]
		{
			get { return GetEntry(key); }
			set { SaveEntry(key, value); }
		}

		/// <summary>
		/// Saves multiple entries at once (more efficient than setting them individually through the default indexer access
		/// </summary>		
		public void SaveEntries(Dictionary<string, object> entries)
		{
			using (var cn = GetConnection())
			{
				InitializeInner(cn);
				foreach (var entry in entries)
				{
					string type = entry.Value.GetType().Name;
					string json = SerializeInner(entry.Value);
					SaveEntryInner(cn, entry.Key, type, json);
				}
			}
		}

		public new bool ContainsKey(string key)
		{
			using (var cn = GetConnection())
			{
				InitializeInner(cn);
				return KeyExists(cn, FormatKey(key));
			}
		}

		public new void Add(string key, object value)
		{
			string json = SerializeInner(value);

			using (var cn = GetConnection())
			{
				InitializeInner(cn);
				cn.Execute(InsertCommand, new { key = FormatKey(key), data = json, type = value.GetType().Name });
			}
		}

		public new void Remove(string key)
		{
			using (var cn = GetConnection())
			{
				InitializeInner(cn);
				cn.Execute(DeleteCommand, new { key = FormatKey(key) });
			}
		}

		private void SaveEntry(string key, object value)
		{			
			string type = value.GetType().Name;

			string json = SerializeInner(value);
			
			using (var cn = GetConnection())
			{
				InitializeInner(cn);
				SaveEntryInner(cn, key, type, json);
			}
		}

		private void SaveEntryInner(IDbConnection cn, string key, string type, string json)
		{
			if (KeyExists(cn, key))
			{
				cn.Execute(UpdateCommand, new { key = FormatKey(key), data = json, type });
			}
			else
			{
				cn.Execute(InsertCommand, new { key = FormatKey(key), data = json, type });
			}
		}

		private object GetEntry(string key)
		{
			using (var cn = GetConnection())
			{
				InitializeInner(cn);

				var data = cn.QuerySingle<KeyValue>(QueryCommand, new { key = FormatKey(key) });				

				foreach (var handler in Deserializers)
				{
					if (data.Type.Equals(handler.Key.Name)) return handler.Value.Invoke(data.Data);
				}

				return JsonConvert.DeserializeObject(data.Data);
			}
		}

		private string SerializeInner(object value)
		{
			Type t = value.GetType();
			if (Serializers.ContainsKey(t)) return Serializers[t].Invoke(value);
			return Serialize(value);
		}

		private void InitializeInner(IDbConnection connection)
		{
			if (IsInitialized) return;
			Initialize(connection);
		}
	}

	internal class KeyValue
	{
		public string Type { get; set; }
		public string Data { get; set; }
	}
}