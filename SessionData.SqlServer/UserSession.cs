using SessionData.SqlServer.SqlServer;

namespace SessionData.SqlServer
{
	public class UserSession : SqlServerDictionary
	{
		private readonly string _userName;

		public UserSession(string connectionString, string userName) : base(connectionString, "User")
		{
			_userName = userName;
		}

		protected override string FormatKey(string key)
		{
			return $"{_userName}.{key}";
		}
	}
}