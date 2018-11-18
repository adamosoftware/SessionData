using SessionData.SqlServer;
using System;
using System.Configuration;

namespace ConsoleTest
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

			var session = new AppSession(connectionString);
			//session["Greeting"] = "hello";
			//Console.WriteLine(session["Greeting"] as string);
			//Console.ReadLine();

			var userSession = new UserSession(connectionString, "adamo");
			userSession["Greeting"] = "this be my hello to you";

		}
	}
}