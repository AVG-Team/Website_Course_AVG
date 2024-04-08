using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Website_Course_AVG.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult PageNotFound()
        {
            Response.StatusCode = 404;
            ViewBag.ErrorCode = "404";
            return View();
        }
    }
}