using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Website_Course_AVG.Models;

namespace Website_Course_AVG.Controllers
{
    public class CartController : Controller
    {
        public ActionResult Index()
        {
            List<Course> courses = GetCartItems();
            return View(courses);
        }

        [HttpPost]
        public ActionResult AddToCart(int courseId)
        {
            List<Course> courses = GetCartItems();
            courses.Add(new Course { id = courseId, title = "Course " + courseId });
            UpdateCartItems(courses);
            return RedirectToAction("Index");
        }

        /*public List<Course> GetCourses()
        {
            List<Course> courses = new List<Course>();
            using (var context = new MyDataDataContext())
            {
                courses = context.courses.ToList();
            }
            return courses;
        }*/

        private List<Course> GetCartItems()
        {
            List<Course> courses = new List<Course>();
            if (Request.Cookies["Cart"] != null)
            {
                HttpCookie cookie = Request.Cookies["Cart"];
                string json = HttpUtility.UrlDecode(cookie.Value);
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                courses = serializer.Deserialize<List<Course>>(json);
            }
            return courses;
        }

        private void UpdateCartItems(List<Course> courses)
        {
            HttpCookie cookie = new HttpCookie("Cart");
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string json = serializer.Serialize(courses);
            cookie.Value = HttpUtility.UrlEncode(json);
            cookie.Expires = DateTime.Now.AddDays(1);
            Response.Cookies.Add(cookie);
        }

        public class Course
        {
            public int id { get; set; }
            public string title { get; set; }
        }
    }
}