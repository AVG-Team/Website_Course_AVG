using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using Website_Course_AVG.Models;


namespace Website_Course_AVG.Controllers
{
    public class AdminController : Controller
    {
        private readonly MyDataDataContext _data = new MyDataDataContext();
        
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        } 

        // GET : Course 
        public ActionResult Course(int? page)
        {
            var courses = _data.courses.ToList();
            var pageNumber = page ?? 1;
            var pageSize = 10;
            AdminViewModels adminView = new AdminViewModels()
            {
                Courses = courses,
                CoursesPagedList = courses.ToPagedList(pageNumber, pageSize)
            };

            return View(adminView);
        }
    }
}