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

			var store = new AppDictionary(connectionString);
			store["Greeting"] = "hello";
			store["DateValue"] = DateTime.Today;
			store["BoolValue"] = true;
			store["IntValue"] = 329;

			Console.WriteLine(store["Greeting"] as string);
			Console.WriteLine((DateTime)store["DateValue"]);
			Console.WriteLine((bool)store["BoolValue"]);
			Console.WriteLine((int)store["IntValue"]);
			Console.ReadLine();

			//var userSession = new SessionDictionary(connectionString, "adamo");
			//userSession["Greeting"] = "this be my hello to you";

		}
	}
}