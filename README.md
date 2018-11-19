# SessionData

Nuget package: **DbDictionary.SqlServer**

This is intended as a replacement for ASP.NET Session backed by SQL Server, although it's not a drop-in replacement. At heart, it's a  Dictionary type that uses SQL Server for storage and Newtonsoft Json.NET for serialization.

The core implementation is an abstract class [DbDictionary](https://github.com/adamosoftware/SessionData/blob/master/SessionData.SqlServer/DbDictionary.cs). The [SQL Server implementation](https://github.com/adamosoftware/SessionData/blob/master/SessionData.SqlServer/SqlServer/SqlServerDictionary.cs) builds on this. You can customize the database table name, but the `session` schema use cannot be changed.

There are two public types [AppDictionary](https://github.com/adamosoftware/SessionData/blob/master/SessionData.SqlServer/AppDictionary.cs) and [SessionDictionary](https://github.com/adamosoftware/SessionData/blob/master/SessionData.SqlServer/SessionDictionary.cs). `AppDictionary` writes to a table called `session.App`, and is scoped to your application. `SessionDictionary` writes to a table called `session.User` and is intended for user session-level data.

Reliable, straightforward serialization is a key ingredient of persistent Session management, so DbDictionary uses Json.NET for serialization. There are several [built-in types handled](https://github.com/adamosoftware/SessionData/blob/master/SessionData.SqlServer/DbDictionary.cs#L15), but you'll need to add handlers for your own types most likely. This is usually as simple as adding an entry to the [Deserializers](https://github.com/adamosoftware/SessionData/blob/master/SessionData.SqlServer/DbDictionary.cs#L51) property. See an example from the integration test project [here](https://github.com/adamosoftware/SessionData/blob/master/Tests/SqlServerDictionaryTests.cs#L110) to see how a custom type is handled.
