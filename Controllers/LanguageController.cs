using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Website_Course_AVG.Managers;

namespace Website_Course_AVG.Controllers
{
    public class LanguageController : Controller
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
            Helpers.AddCookie("Notify", ResourceHelper.GetResource("Change Language Successful!"));
            return Redirect(Request.UrlReferrer?.ToString());
        }
    }
}