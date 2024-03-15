using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Website_Course_AVG.Models;

namespace Website_Course_AVG.Middleware
{
    public class AuthMiddleware : OwinMiddleware
    {
        public AuthMiddleware(OwinMiddleware next) : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            context.Set("IsLoggedIn", "21324234");
            await Next.Invoke(context);
        }

        private bool IsLoggedIn(IOwinContext context)
        {
            var user = context.Get<user>();
            if (user == null)
            {
                return false;
            }

            return true;
        }
    }
}