using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Website_Course_AVG.Controllers
{
    public class Language : Controller
    {
        [HttpGet]
        public ActionResult Change(string languageAbbreviation)
        {
            if (languageAbbreviation != null)
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(languageAbbreviation);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(languageAbbreviation);
            }

            HttpCookie cookie = new HttpCookie("Language")
            {
                Value = languageAbbreviation
            };

            Response.Cookies.Add(cookie);

            return Redirect(Request.UrlReferrer?.ToString());
        }
    }
}