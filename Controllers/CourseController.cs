using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Website_Course_AVG.Models;

namespace Website_Course_AVG.Controllers
{
    public class CourseController : Controller
    {
        // GET: Course
        MyDataDataContext data = new MyDataDataContext();
        public ActionResult Index()
        {
            var item = from s in data.courses select s;
            return View(item);
        }
    }
}