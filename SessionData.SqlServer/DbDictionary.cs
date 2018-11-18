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
				{ typeof(double), (json) => JsonConvert.DeserializeObject<double>(json) }				
			};
		}

		protected abstract IDbConnection GetConnection();		
		protected abstract void Initialize(IDbConnection connection);
		protected abstract string QueryCommand { get; }
		protected abstract string InsertCommand { get; }
		protected abstract string UpdateCommand { get; }

		protected string ConnectionString { get; }
		protected abstract bool KeyExists(IDbConnection connection, string key);

		public Dictionary<Type, Func<object, string>> Serializers { get; private set; }
		public Dictionary<Type, Func<string, object>> Deserializers { get; private set; }

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

		public new bool ContainsKey(string key)
		{
			using (var cn = GetConnection())
			{
				Initialize(cn);
				return KeyExists(cn, FormatKey(key));
			}
		}

		public new void Add(string key, object value)
		{
			string json = SerializeInner(value);

			using (var cn = GetConnection())
			{
				cn.Execute(InsertCommand, new { key = FormatKey(key), data = json, type = value.GetType().Name });
			}
		}
		private void SaveEntry(string key, object value)
		{			
			string type = value.GetType().Name;

			string json = SerializeInner(value);
			
			using (var cn = GetConnection())
			{
				Initialize(cn);
				if (KeyExists(cn, key))
				{					
					cn.Execute(UpdateCommand, new { key = FormatKey(key), data = json, type });
				}
				else
				{
					cn.Execute(InsertCommand, new { key = FormatKey(key), data = json, type });
				}
			}
		}

		private object GetEntry(string key)
		{
			using (var cn = GetConnection())
			{
				Initialize(cn);
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
			foreach (var handler in Serializers)
			{
				if (handler.Key.Equals(value.GetType()))
				{
					return handler.Value.Invoke(value);					
				}
			}

			return Serialize(value);
		}
	}

	internal class KeyValue
	{
		public string Type { get; set; }
		public string Data { get; set; }
	}
}