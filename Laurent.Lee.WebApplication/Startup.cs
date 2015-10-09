using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Laurent.Lee.WebApplication.Startup))]
namespace Laurent.Lee.WebApplication
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
