using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Website_Course_AVG.Models;
using System.Web.Mvc;
using Website_Course_AVG.Managers;
using System.Web.Routing;

namespace Website_Course_AVG.Attributes
{
    public class AuthorizeAttribute : ActionFilterAttribute
    {
        UserManager UserManager = new UserManager();
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!UserManager.IsAuthenticated())
            {
                string currentArea = (string)filterContext.RouteData.DataTokens["area"];

                if (currentArea.Equals("Admin"))
                {
                    Helpers.AddCookie("Error", "You don't have permission to access the admin page");
                }
                else
                {
                    Helpers.AddCookie("Error", "You aren't login!!!");
                }
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary {
                        { "area", "" }, 
                        { "controller", "Home" }, 
                        { "action", "Index" } 
                    }); ;
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}