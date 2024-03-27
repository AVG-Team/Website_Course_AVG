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
        MyDataDataContext _data = new MyDataDataContext();

        public ActionResult Index(int courseId, int lessonId = 1)
        {
            var lessonsCourse = from ff in _data.lessons.Where(x => x.course_id == courseId) select ff;

            if (lessonsCourse.Count() < 1)
            {
                Helpers.addCookie("Error", "The course is not found, please try again later. If it still doesn't work, please don't leave and report to admin so we can handle it, thank you.");
                return RedirectToAction("Index", "Home");
            }

            lesson lesson = lessonsCourse.Where(x => x.id == lessonId).FirstOrDefault();

            if (lesson == null)
            {
                Helpers.addCookie("Error", "The lesson is not found, please try again later. If it still doesn't work, please don't leave and report to admin so we can handle it, thank you.");
                return RedirectToAction("Index", "Home");
            }

            List<lesson> lessons = lessonsCourse.ToList();

            string courseTitle = lesson.course.title;
            ViewBag.CourseTitle = courseTitle;

            int userId = Helpers.GetUserFromToken().id;
            detail_course detailCourse = _data.detail_courses.Where(x => x.user_id == userId && x.course_id == courseId).FirstOrDefault();
            if(detailCourse == null)
            {
                Helpers.addCookie("Error", "It seems you have not registered for the course, please try again. If this is an error, please report to the admin before leaving the site, thank you.");
                return RedirectToAction("Index", "Home");
            }

            int lessonLearnedId = detailCourse.lesson_learned_id ?? 1;
            ViewBag.LessonLearnedId = lessonLearnedId;
            ViewBag.Lessons = lessons;

            Identity identity = Helpers.GetIdentity(lesson, lessons);
            ViewBag.Identity = identity;


            string fileJson = Server.MapPath("~/ltweb-avg-b91359369629.json");
            string url = Helpers.GetVideoLessonUrl(lesson.video, fileJson);
            ViewBag.Url = url;

            return View(lesson);
        }
    }
}