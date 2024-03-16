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
using Website_Course_AVG.Manager;

namespace Website_Course_AVG.Attributes
{
    public class Authorize : ActionFilterAttribute
    {
        UserManager UserManager = new UserManager();
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!UserManager.IsAuthenticated())
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "Home", action = "Index" }));
                return;
            }

            base.OnActionExecuting(filterContext);
        }

        public void DistinguishUser(user user)
        {
			MyDataDataContext _data1 = new MyDataDataContext();
			string token = HttpContext.Current.Request.Cookies["AuthToken"]?.Value;
			string username = TokenHelper.GetUsernameFromToken(token);
			user user = _data1.users.Where(x => x.email == username).FirstOrDefault();
			
		}
    }
}