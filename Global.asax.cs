
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Website_Course_AVG.Managers;
using Website_Course_AVG.Models;
using Microsoft.Extensions.Configuration;

namespace Website_Course_AVG
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static IConfiguration Configuration { get; private set; }

        protected void Application_Start()
        {
            var builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (Helpers.IsAuthenticated())
            {
                user user = Helpers.GetUserFromToken();
                string deviceFinger = Helpers.GetDeviceFingerprint();

                if (user.account.info != deviceFinger)
                {
                    Helpers.AddCookie("Error", "Looks like you're logged in somewhere else !!!");

                    UserManager userManager = new UserManager();
                    userManager.logout();

                    UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                    string homeUrl = urlHelper.Action("Login", "Account");

                    HttpContext.Current.Response.Redirect(homeUrl);
                }
            }
        }
    }
}
