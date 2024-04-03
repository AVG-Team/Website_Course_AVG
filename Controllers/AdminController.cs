using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Website_Course_AVG.Models;

namespace Website_Course_AVG.Controllers
{
    public class AdminController : Controller
    {
        private MyDataDataContext _data;
        public AdminController(MyDataDataContext data)
        {
            _data = data;
        }
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        // GET : Course 
        public ActionResult Course()
        {
            var courses = _data.courses.ToList();

            AdminViewModels adminView = new AdminViewModels()
            {
                Courses = courses
            };

            return View(adminView);
        }
    }
}