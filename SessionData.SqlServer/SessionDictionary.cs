using SessionData.SqlServer.SqlServer;

namespace SessionData.SqlServer
{
	public class SessionDictionary : SqlServerDictionary
	{
		private readonly string _sessionId;

		public SessionDictionary(string connectionString, string sessionId) : base(connectionString, "User")
		{
			_sessionId = sessionId;
		}

		protected override string FormatKey(string key)
		{
			return $"{_sessionId}.{key}";
		}
	}
}