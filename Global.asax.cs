using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Website_Course_AVG.Managers;
using Website_Course_AVG.Models;

namespace Website_Course_AVG
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
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
