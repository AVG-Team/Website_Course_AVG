using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using Website_Course_AVG.Models;
using System.IO;
using System.Web.Routing;
using Website_Course_AVG.Areas.Admin.Data.ViewModels;


namespace Website_Course_AVG.Areas.Admin.Controllers
{
    public class AdminController : Controller
    {
        private readonly MyDataDataContext _data = new MyDataDataContext();

        public AdminController()
        {
            ViewBag.controller = "Admin";
        }
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

    }
}