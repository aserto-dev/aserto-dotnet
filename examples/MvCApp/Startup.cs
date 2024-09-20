using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MvCApp.Startup))]
namespace MvCApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
