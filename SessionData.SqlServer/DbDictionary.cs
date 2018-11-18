using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;

namespace SessionData
{
	public abstract class DbDictionary : Dictionary<string, object>
	{
		public DbDictionary(string connectionString)
		{
			ConnectionString = connectionString;
			Serializers = new Dictionary<Type, Func<object, string>>();
			Deserializers = new Dictionary<Type, Func<string, object>>();
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
		/// Override this to modify keys, for example to prefix with a user name
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
			string json = JsonConvert.SerializeObject(value);

			using (var cn = GetConnection())
			{
				cn.Execute(InsertCommand, new { key = FormatKey(key), data = json });
			}
		}

		private void SaveEntry(string key, object value)
		{
			string json = Serialize(value);

			foreach (var handler in Serializers)
			{
				if (handler.Key.Equals(value.GetType()))
				{
					json = handler.Value.Invoke(value);
					break;
				}
			}

			using (var cn = GetConnection())
			{
				Initialize(cn);
				if (KeyExists(cn, key))
				{					
					cn.Execute(UpdateCommand, new { key = FormatKey(key), data = json });
				}
				else
				{
					cn.Execute(InsertCommand, new { key = FormatKey(key), data = json });
				}
			}
		}

		private object GetEntry(string key)
		{
			using (var cn = GetConnection())
			{
				Initialize(cn);
				string json = cn.QuerySingle<string>(QueryCommand, new { key = FormatKey(key) });
				return JsonConvert.DeserializeObject(json);
			}
		}
	}
}