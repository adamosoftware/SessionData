using System;
using System.Configuration;
using System.Data.SqlClient;
using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SessionData.SqlServer.SqlServer;
using System.Linq;
using System.Collections.Generic;

namespace Tests
{
	[TestClass]
	public class SqlServerDictionaryTests
	{
		private static SqlConnection GetConnection(string name)
		{
			string connectionString = GetConnectionString(name);
			return new SqlConnection(connectionString);
		}

		private static string GetConnectionString(string name)
		{
			return ConfigurationManager.ConnectionStrings[name].ConnectionString;
		}
		
		[ClassInitialize]
		public static void InitDb(TestContext testContext)
		{
			try
			{
				using (var cn = GetConnection("master"))
				{
					cn.Execute("CREATE DATABASE [DbDictionaryTest]");
				}
			}
			catch
			{
				// do nothing
			}
		}

		[ClassCleanup]
		public static void Teardown()
		{
			// not sure this actually works, but it doesn't hurt anything
			try
			{
				using (var cn = GetConnection("master"))
				{
					cn.Execute("DROP DATABASE [DbDictionaryTest]");
				}
			}
			catch 
			{
				// do nothing
			}
		}

		[TestMethod]
		public void BuiltInDeserializers()
		{
			var store = GetDictionary();
			store["Greeting"] = "hello";
			store["DateValue"] = DateTime.Today;
			store["BoolValue"] = true;
			store["IntValue"] = 329;
			store["DoubleValue"] = 45D;
			store["DecimalValue"] = 398m;

			Assert.IsTrue(store["Greeting"].Equals("hello"));
			Assert.IsTrue(store["DateValue"].Equals(DateTime.Today));
			Assert.IsTrue(store["BoolValue"].Equals(true));
			Assert.IsTrue(store["IntValue"].Equals(329));
			Assert.IsTrue(store["DoubleValue"].Equals(45D));
			Assert.IsTrue(store["DecimalValue"].Equals(398m));
		}

		[TestMethod]
		public void MultipleEntries()
		{
			var store = GetDictionary();
			store.SaveEntries(new Dictionary<string, object>()
			{
				{ "Greeting", "hello" },
				{ "DateValue", DateTime.Today },
				{ "BoolValue", true },
				{ "IntValue", 329 },
				{ "DoubleValue", 45D },
				{ "DecimalValue", 398m }
			});

			Assert.IsTrue(store["Greeting"].Equals("hello"));
			Assert.IsTrue(store["DateValue"].Equals(DateTime.Today));
			Assert.IsTrue(store["BoolValue"].Equals(true));
			Assert.IsTrue(store["IntValue"].Equals(329));
			Assert.IsTrue(store["DoubleValue"].Equals(45D));
			Assert.IsTrue(store["DecimalValue"].Equals(398m));
		}

		private static SqlServerDictionary GetDictionary()
		{			
			return new SqlServerDictionary(GetConnectionString("DefaultConnection"), "Test"); 
		}

		[TestMethod]
		public void CustomDeserializer()
		{
			var store = GetDictionary();
			store.Deserializers.Add(typeof(MyCustomType), (json) => JsonConvert.DeserializeObject<MyCustomType>(json));

			var mycustom = new MyCustomType()
			{
				FirstName = "Bongo",
				LastName = "Juniper",
				WhateverDate = DateTime.Today.Add(new TimeSpan(27, 2, 0, 0)),
				YouKnowIt = true,
				Items = new MyNestedType[]
				{
					new MyNestedType() { OrderId = 232, Description = "This is great", Quantity = 2.4m, Price = 110m },
					new MyNestedType() { OrderId = 118, Description = "This be the second item", Quantity = 45.2m, Price = 448m }
				}
			};

			store["Custom"] = mycustom;

			Assert.IsTrue(store["Custom"].Equals(mycustom));
		}
	}

	internal class MyCustomType
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public DateTime WhateverDate { get; set; }
		public bool YouKnowIt { get; set; }
		public MyNestedType[] Items { get; set; }

		public override bool Equals(object obj)
		{
			MyCustomType test = obj as MyCustomType;
			if (test != null)
			{
				if (!FirstName.Equals(test.FirstName)) return false;
				if (!LastName.Equals(test.LastName)) return false;
				if (!WhateverDate.Equals(test.WhateverDate)) return false;
				if (!YouKnowIt.Equals(test.YouKnowIt)) return false;
				if (!Items.SequenceEqual(test.Items)) return false;

				return true;
			}

			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}

	internal class MyNestedType
	{
		public int OrderId { get; set; }
		public string Description { get; set; }
		public decimal Quantity { get; set; }
		public decimal Price { get; set; }

		public override bool Equals(object obj)
		{
			MyNestedType test = obj as MyNestedType;
			if (test != null)
			{
				if (OrderId != test.OrderId) return false;
				if (!Description.Equals(test.Description)) return false;
				if (Quantity != Quantity) return false;
				if (Price != Price) return false;

				return true;
			}

			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
