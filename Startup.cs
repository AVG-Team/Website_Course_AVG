using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Website_Course_AVG.Startup))]
namespace Website_Course_AVG
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {

            ConfigureAuth(app);
        }
    }
}
