using Dapper;
using System;
using System.Data;
using System.Data.SqlClient;

namespace SessionData.SqlServer.SqlServer
{
	public class SqlServerDictionary : DbDictionary
	{
		private readonly string _tableName;
		private readonly string _schema;

		public SqlServerDictionary(string connectionString, string tableName, string schema = "session") : base(connectionString)
		{
			_tableName = tableName;
			_schema = schema;
		}

		protected override bool IsInitialized => false;

		protected override void Initialize(IDbConnection connection)
		{
			if (!SchemaExists(connection, _schema))
			{
				connection.Execute($"CREATE SCHEMA [{_schema}]");
			}

			if (!TableExists(connection, _schema, _tableName))
			{
				connection.Execute(
					$@"CREATE TABLE [{_schema}].[{_tableName}] (
						[Key] nvarchar(255) NOT NULL,
						[Type] nvarchar(255) NOT NULL,
						[Data] nvarchar(max) NOT NULL,
						CONSTRAINT [PK_{_schema}_{_tableName}] PRIMARY KEY ([Key])
					)");
			}
		}

		protected override string QueryCommand => $"SELECT [Type], [Data] FROM [{_schema}].[{_tableName}] WHERE [Key]=@key";

		protected override string InsertCommand => $"INSERT INTO [{_schema}].[{_tableName}] ([Key], [Type], [Data]) VALUES (@key, @type, @data)";

		protected override string UpdateCommand => $"UPDATE [{_schema}].[{_tableName}] SET [Type]=@type, [Data]=@data WHERE [Key]=@key";

		protected override string DeleteCommand => $"DELETE [{_schema}].[{_tableName}] WHERE [Key]=@key";

		protected override IDbConnection GetConnection()
		{
			return new SqlConnection(ConnectionString);
		}

		protected override bool KeyExists(IDbConnection connection, string key)
		{
			return Exists(connection, $"SELECT 1 FROM [{_schema}].[{_tableName}] WHERE [Key]=@key", new { key = FormatKey(key) });
		}

		private static bool Exists(IDbConnection connection, string query, object parameters)
		{
			return ((connection.QueryFirstOrDefault<int?>(query, parameters) ?? 0) == 1);
		}

		private bool SchemaExists(IDbConnection connection, string name)
		{
			return Exists(connection, "SELECT 1 FROM [sys].[schemas] WHERE [name]=@name", new { name });
		}

		private static bool TableExists(IDbConnection connection, string schema, string tableName)
		{
			return Exists(connection, "SELECT 1 FROM [sys].[tables] WHERE SCHEMA_NAME([schema_id])=@schema AND [name]=@tableName", new { schema, tableName });
		}
	}
}