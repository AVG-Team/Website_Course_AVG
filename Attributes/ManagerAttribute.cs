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
    public class ManagerAttribute : ActionFilterAttribute
    {
        UserManager UserManager = new UserManager();
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!UserManager.IsManager())
            {
                Helpers.AddCookie("Error", "You do not have permission to enter here!!!");
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "Home", action = "Index" }));
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}