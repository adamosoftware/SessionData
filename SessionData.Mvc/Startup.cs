using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SessionData.Mvc.Startup))]
namespace SessionData.Mvc
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
