using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Website_Course_AVG.Managers;
using Website_Course_AVG.Models;

namespace Website_Course_AVG.Controllers
{
    public class LessonController : Controller
    {
        public LessonController() { }
        public ActionResult Index(int courseId)
        {
            return View();
        }
    }
}