using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Website_Course_AVG.Managers;
using Website_Course_AVG.Models;

namespace Website_Course_AVG.Controllers
{
    public class HomeController : Controller
    {
        private readonly MyDataDataContext _data = new MyDataDataContext();
        public ActionResult Index()
        {
            var categories = _data.categories.ToList();

            List<CategoryCourseViewModels> categoryViewModels = new List<CategoryCourseViewModels>();

            foreach (var category in categories)
            {
                CategoryCourseViewModels categoryViewModel = new CategoryCourseViewModels
                {
                    Category = category,
                    Courses = _data.courses.Where(c => c.category_id == category.id).ToList()
                };
                var imageCodes = categoryViewModel.Courses.Select(c => c.image_code).Distinct().ToList();

                var images = _data.images.Where(i => imageCodes.Contains(i.code) && i.category == false).ToList();

                foreach (var course in categoryViewModel.Courses)
                {
                    var image = images.FirstOrDefault(i => i.code == course.image_code);
                    if (image != null)
                    {
                        course.image_code = image.image1;
                    }
                }

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
            var contact = _data.contacts.FirstOrDefault();
            if (contact != null)
            {
                ReportViewModel reportView = new ReportViewModel()
                {
                    contact = contact
                };
                return View(reportView);
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Website_Course_AVG.Attributes.AllowAnonymous]
        public ActionResult Report(ReportViewModel model)
        {
            if (model.Phone.Length > 10)
            {
                Helpers.AddCookie("Error", "Phone Input Error, Length <= 10");
                return RedirectToAction("Contact");
            }
            try
            {
                using (MyDataDataContext _data = new MyDataDataContext())
                {
                    DateTime today = DateTime.Today;

                    // Đếm số lần email đã gửi trong ngày từ địa chỉ email đã cho
                    int count = _data.reports
                        .Count(r => r.email == model.Email &&
                                    r.created_at >= today &&
                                    r.created_at < today.AddDays(1));

                    if (count >= 5)
                    {
                        Helpers.AddCookie("Error", "It seems like spam email, please try again after 1 day.");
                        return RedirectToAction("Index", "Home");
                    }
                    report report = new report()
                    {
                        fullname = model.Fullname,
                        email = model.Email,
                        phone = model.Phone,
                        subject = model.Subject,
                        message = model.Message,
                        status = false,
                        created_at = DateTime.Now,
                        updated_at = DateTime.Now
                    };
                    _data.reports.InsertOnSubmit(report);
                    _data.SubmitChanges();
                    Helpers.AddCookie("Notify", "Thank you, Report has been sent");
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception)
            {
                Helpers.AddCookie("Error", "Error Unknown, Please Try Again!!!");
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [Website_Course_AVG.Attributes.AllowAnonymous]
        public ActionResult SearchCourses(FormCollection form)
        {
            var query = form["query"];
            var course = _data.courses.FirstOrDefault(x => x.title == query);
            if (course != null)
            {
                return RedirectToAction("Details", "Course", new { id = course.id });
            }
            return RedirectToAction("Index", "Home");
        }

    }
}