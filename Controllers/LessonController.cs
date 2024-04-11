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
    [Website_Course_AVG.Attributes.Authorize]
    public class LessonController : Controller
    {
        MyDataDataContext _data = new MyDataDataContext();

        //Todo : Set lại redirect to action về course detail
        public ActionResult Index(int courseId, int? lessonId)
        {
            var lessonsCourse = from ff in _data.lessons.Where(x => x.course_id == courseId) select ff;

            if (lessonsCourse.Count() < 1)
            {
                Helpers.AddCookie("Error", ResourceHelper.GetResource("The course is not found, please try again later. If it still doesn't work, please don't leave and report to admin so we can handle it, thank you."));
                return RedirectToAction("Index", "Home");
            }

            user user = Helpers.GetUserFromToken();
            int userId = user.id;
            detail_course detailCourse = _data.detail_courses.Where(x => x.user_id == userId && x.course_id == courseId).FirstOrDefault();
            if (detailCourse == null)
            {
                Helpers.AddCookie("Error", ResourceHelper.GetResource("It seems you have not registered for the course, please try again. If this is an error, please report to the admin before leaving the site, thank you."));
                return RedirectToAction("Index", "Home");
            }

            if (lessonId == null)
            {
                int lessonLearnedIdTmp = detailCourse.lesson_learned_id ?? 0;
                if (lessonLearnedIdTmp == 0)
                {
                    lessonId = lessonsCourse.OrderBy(x => x.index).First().id;
                }
                else
                {
                    lessonId = lessonLearnedIdTmp;
                }
            }

            lesson lesson = lessonsCourse.Where(x => x.id == lessonId).FirstOrDefault();

            if (lesson == null)
            {
                Helpers.AddCookie("Error", ResourceHelper.GetResource("The lesson is not found, please try again later. If it still doesn't work, please don't leave and report to admin so we can handle it, thank you."));
                return RedirectToAction("Index", "Home");
            }

            List<lesson> lessons = lessonsCourse.OrderBy(x => x.index).ToList();
            int idLessonFirstOfCourse = lessons.First().id;

            string courseTitle = lesson.course.title;
            ViewBag.CourseTitle = courseTitle;

            int lessonLearnedId = detailCourse.lesson_learned_id ?? idLessonFirstOfCourse - 1;
            ViewBag.LessonLearnedId = lessonLearnedId;
            ViewBag.Lessons = lessons;

            Identity identity = Helpers.GetIdentity(lesson, lessons);
            ViewBag.Identity = identity;

            if (lessonLearnedId + 1 < lesson.id && user.role != 2)
            {
                Helpers.AddCookie("Error", ResourceHelper.GetResource("You have not finished studying the previous lesson, please return to the previous lesson"));
                return RedirectToAction("Detail", "Course");
            }

            string fileJson = Server.MapPath("~/ltweb-avg-b91359369629.json");
            string url = Helpers.GetVideoLessonUrl(lesson.video, fileJson);
            ViewBag.Url = url;

            // add view
            lesson lessonTmp = _data.lessons.Where(x => x.id == lesson.id).First();
            int view = lesson.views ?? 0;
            lessonTmp.views = view + 1;
            _data.SubmitChanges();

            return View(lesson);
        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        [Website_Course_AVG.Attributes.Authorize]
        public JsonResult SetLessonLearnedId(int courseId, int lessonId)
        {
            try
            {
                user user = Helpers.GetUserFromToken();
                int a = lessonId;
                detail_course detail_course = _data.detail_courses.Where(x => x.course_id == courseId && x.user_id == user.id).FirstOrDefault();
                if (detail_course == null)
                {
                    string errorMessage = ResourceHelper.GetResource("No courses found, please try again!");
                    return ResponseHelper.ErrorResponse(errorMessage);
                }

                if (lessonId > detail_course.lesson_learned_id)
                {
                    detail_course.lesson_learned_id = lessonId;
                    _data.SubmitChanges();
                    return ResponseHelper.SuccessResponse(ResourceHelper.GetResource("Success!"));
                }

                return ResponseHelper.SuccessResponse("");
            }
            catch (Exception ex)
            {
                return ResponseHelper.ErrorResponse(ResourceHelper.GetResource("Error Unknown"));
            }
        }
    }
}