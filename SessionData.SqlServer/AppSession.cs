using SessionData.SqlServer.SqlServer;

namespace SessionData.SqlServer
{
	public class AppSession : SqlServerDictionary
	{
		public AppSession(string connectionString) : base(connectionString, "App")
		{
		}
	}
}