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
        /*public ActionResult PageNotFound()
        {
            Response.StatusCode = 404;
            ViewBag.ErrorCode = "404";
            return View();
        }
        public ActionResult BadRequest()
        {
            Response.StatusCode = 400;
            ViewBag.ErrorCode = "400";
            return View();
        }*/
        public ActionResult NotFound()
        {
            ActionResult result;

            object model = Request.Url.PathAndQuery;

            if (!Request.IsAjaxRequest())
                result = View(model);
            else
                result = PartialView("_NotFound", model);

            return result;
        }
    }
}