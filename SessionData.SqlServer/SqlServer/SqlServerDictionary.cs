using Dapper;
using System;
using System.Data;
using System.Data.SqlClient;

namespace SessionData.SqlServer.SqlServer
{
	public class SqlServerDictionary : DbDictionary
	{
		private readonly string _tableName;

		public SqlServerDictionary(string connectionString, string tableName) : base(connectionString)
		{
			_tableName = tableName;
		}

		protected override void Initialize(IDbConnection connection)
		{
			if (!SchemaExists(connection, "session"))
			{
				connection.Execute("CREATE SCHEMA [session]");
			}

			if (!TableExists(connection, "session", _tableName))
			{
				connection.Execute(
					$@"CREATE TABLE [session].[{_tableName}] (
						[Key] nvarchar(255) NOT NULL,
						[Data] nvarchar(max) NOT NULL,
						CONSTRAINT [PK_session_{_tableName}] PRIMARY KEY ([Key])
					)");
			}
		}

		protected override string QueryCommand => $"SELECT [Data] FROM [session].[{_tableName}] WHERE [Key]=@key";

		protected override string InsertCommand => $"INSERT INTO [session].[{_tableName}] ([Key], [Data]) VALUES (@key, @data)";

		protected override string UpdateCommand => $"UPDATE [session].[{_tableName}] SET [Data]=@data WHERE [Key]=@key";

		protected override IDbConnection GetConnection()
		{
			return new SqlConnection(ConnectionString);
		}

		protected override bool KeyExists(IDbConnection connection, string key)
		{
			return Exists(connection, $"SELECT 1 FROM [session].[{_tableName}] WHERE [Key]=@key", new { key = FormatKey(key) });
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