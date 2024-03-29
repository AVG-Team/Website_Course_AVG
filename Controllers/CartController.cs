using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Numerics;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml.Schema;
using Website_Course_AVG.Models;

namespace Website_Course_AVG.Controllers
{
    [Website_Course_AVG.Attributes.User]
    public class CartController : Controller
    {
        private MyDataDataContext db = new MyDataDataContext();
        public ActionResult Index()
        {
            var selectedCourseIdsCookie = Request.Cookies["selected_course_ids"];
            if (selectedCourseIdsCookie != null)
            {
                var selectedCourseIds = selectedCourseIdsCookie.Value.Split(',').Select(int.Parse).ToList();
                int count = selectedCourseIds.Count();
                ViewBag.count = count;

                var coursesInCart = db.courses.Where(c => selectedCourseIds.Contains(c.id)).ToList();
                long? totalAmount = 0;
                foreach (var course in coursesInCart)
                {
                    totalAmount += course.price;
                }
                ViewBag.TotalAmount = totalAmount;
                return View(coursesInCart);
            }
            return View(new List<course>());
        }
        public ActionResult Payment()
        {
            var selectedCourseIdsCookie = Request.Cookies["selected_course_ids"];
            if (selectedCourseIdsCookie != null)
            {
                var selectedCourseIds = selectedCourseIdsCookie.Value.Split(',').Select(int.Parse).ToList();
                int count = selectedCourseIds.Count();
                ViewBag.count = count;

                var coursesInCart = db.courses.Where(c => selectedCourseIds.Contains(c.id)).ToList();
                long? totalAmount = 0;
                foreach (var course in coursesInCart)
                {
                    totalAmount += course.price;
                }
                ViewBag.TotalAmount = totalAmount;
                return View(coursesInCart);
            }
            return View(new List<course>());
        }

        public ActionResult CheckDiscountCode(string discountCode)
        {
            var promo = db.promotions.Where(c => c.code_promotion.Equals(discountCode)).First();
            if (promo != null)
                if (promo.active == false)
                {
                    return Json(new { success = false, message = "Promotion code has expired." });
                }
                else
                {
                    return Json(new { success = true, message = "You get " +  promo.percent + "% off from " + promo.name + "."});
                }
            else
            {
                return Json(new { success = false, message = "Promotion code does not exist." });
            }
        }
    }
}