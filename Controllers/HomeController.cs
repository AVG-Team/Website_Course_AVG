using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Website_Course_AVG.Manager;
using Website_Course_AVG.Managers;
using Website_Course_AVG.Models;

namespace Website_Course_AVG.Controllers
{
    public class HomeController : Controller
    {
        private readonly MyDataDataContext data = new MyDataDataContext();
        public ActionResult Index()
        {
            var categories = data.categories.ToList();

            List<CategoryCourseViewModels> categoryViewModels = new List<CategoryCourseViewModels>();

            foreach (var category in categories)
            {
                CategoryCourseViewModels categoryViewModel = new CategoryCourseViewModels
                {
                    Category = category,
                    Courses = data.courses.Where(c => c.category_id == category.id).ToList()
                };

                categoryViewModels.Add(categoryViewModel);
            }

            return View(categoryViewModels);
        }

        public ActionResult About()
        {
            ViewBag.Message = ResourceHelper.GetResource("Your application description page.");

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}