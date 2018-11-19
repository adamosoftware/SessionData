Nuget package: **DbDictionary.SqlServer**

![img](https://ci.appveyor.com/api/projects/status/89mobj56wvosogvm?svg=true)

This is intended as a replacement for ASP.NET Session backed by SQL Server, although it's not a drop-in replacement. At heart, it's a  Dictionary type that uses SQL Server for storage and Newtonsoft Json.NET for serialization.

The core implementation is an abstract class [DbDictionary](https://github.com/adamosoftware/SessionData/blob/master/SessionData.SqlServer/DbDictionary.cs). The [SQL Server implementation](https://github.com/adamosoftware/SessionData/blob/master/SessionData.SqlServer/SqlServer/SqlServerDictionary.cs) builds on this. You can customize the database table name, but the `session` schema use cannot be changed.

There are two public types [AppDictionary](https://github.com/adamosoftware/SessionData/blob/master/SessionData.SqlServer/AppDictionary.cs) and [SessionDictionary](https://github.com/adamosoftware/SessionData/blob/master/SessionData.SqlServer/SessionDictionary.cs). `AppDictionary` writes to a table called `session.App`, and is scoped to your application. `SessionDictionary` writes to a table called `session.User` and is intended for user session-level data.

Reliable, straightforward serialization is a key ingredient of persistent Session management, so DbDictionary uses Json.NET for serialization. There are several [built-in types handled](https://github.com/adamosoftware/SessionData/blob/master/SessionData.SqlServer/DbDictionary.cs#L15), but you'll need to add handlers for your own types most likely. This is usually as simple as adding an entry to the [Deserializers](https://github.com/adamosoftware/SessionData/blob/master/SessionData.SqlServer/DbDictionary.cs#L51) property. See an example from the integration test project [here](https://github.com/adamosoftware/SessionData/blob/master/Tests/SqlServerDictionaryTests.cs#L110) to see how a custom type is handled.

## Using SessionDictionary in an MVC app

See the [HomeController](https://github.com/adamosoftware/SessionData/blob/master/SessionData.Mvc/Controllers/HomeController.cs) in the the sample MVC app for an example of SessionDictionary in use. Here are the key points:

- Declare an [instance](https://github.com/adamosoftware/SessionData/blob/master/SessionData.Mvc/Controllers/HomeController.cs#L13) of `SessionDictionary` in the controller.

- Override the controller's [Initialize method](https://github.com/adamosoftware/SessionData/blob/master/SessionData.Mvc/Controllers/HomeController.cs#L15) to new up the SessionDictionary instance, passing your app's connection string and the current session Id. In practice, you'd likely have a base controller class that's used throughout your application, since you probably wouldn't want to make this change to every controller individually. (In this case, you'd change the access from `private` to `protected`.) Note how in this situation custom serialization logic is added for `SelectList`. This is because this type doesn't serialize as you'd expect by default. The special SelectList serialization logic is done [here](https://github.com/adamosoftware/SessionData/blob/master/SessionData.Mvc/Classes/SelectListSerializer.cs).

- Use the `SessionDictionary` object like any other dictionary. Notice how a SelectList is created an assigned to the dictionary [here](https://github.com/adamosoftware/SessionData/blob/master/SessionData.Mvc/Controllers/HomeController.cs#L34). Then, it's rendered from another action [here](https://github.com/adamosoftware/SessionData/blob/master/SessionData.Mvc/Controllers/HomeController.cs#L43). This shows that this SelectList can be shared among different actions.
