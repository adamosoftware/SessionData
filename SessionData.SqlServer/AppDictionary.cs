using SessionData.SqlServer.SqlServer;

namespace SessionData.SqlServer
{
	public class AppDictionary : SqlServerDictionary
	{
		public AppDictionary(string connectionString) : base(connectionString, "App")
		{			
		}
	}
}