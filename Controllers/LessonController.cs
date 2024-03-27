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

        //Todo : Set lại redirect to action về course detail
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

            List<lesson> lessons = lessonsCourse.OrderBy(x => x.index).ToList();

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

            // 14 qua , 15 qua
            if(lessonLearnedId + 1 < lesson.id)
            {
                Helpers.addCookie("Error", "You have not finished studying the previous lesson, please return to the previous lesson");
                return RedirectToAction("Detail", "Course");
            }


            string fileJson = Server.MapPath("~/ltweb-avg-b91359369629.json");
            string url = Helpers.GetVideoLessonUrl(lesson.video, fileJson);
            ViewBag.Url = url;

            return View(lesson);
        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult SetLessonLearnedId(int courseId, int lessonId)
        {
            try
            {
                user user = Helpers.GetUserFromToken();
                int a = lessonId;
                detail_course detail_course = _data.detail_courses.Where(x => x.course_id == courseId && x.user_id == user.id).FirstOrDefault();
                if (detail_course == null)
                {
                    string errorMessage = "No courses found, please try again";
                    return ResponseHelper.ErrorResponse(errorMessage);
                }

                if (lessonId > detail_course.lesson_learned_id)
                {
                    detail_course.lesson_learned_id = lessonId;
                    _data.SubmitChanges();
                    return ResponseHelper.SuccessResponse("Success");
                } 

                return ResponseHelper.SuccessResponse("");
            } catch (Exception ex)
            {
                return ResponseHelper.ErrorResponse("Error Unknown");
            }
        }
    }
}