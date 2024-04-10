using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using PagedList;
using Website_Course_AVG.Areas.Admin.Data.ViewModels;
using Website_Course_AVG.Managers;
using Website_Course_AVG.Models;

namespace Website_Course_AVG.Areas.Admin.Controllers
{
    public class LessonController : Controller
    {
        public readonly MyDataDataContext _data = new MyDataDataContext();
        // GET: Admin/Lesson
        public ActionResult Index(int? page)
        {
            var lessons = _data.lessons.ToList();
            var courses= _data.courses.Where(c => c.deleted_at == null).ToList();
            var images = _data.images.ToList();
            var exercises = _data.exercises.Where(e => e.deleted_at == null).ToList();
            var pageNumber = page ?? 1;
            var pageSize = 10;
            var lessonsPageList = lessons.ToPagedList(pageNumber, pageSize);
            ViewBag.controller = "Lesson";
            var adminView = new AdminViewModels()
            {
                Lessons = lessons,
                Courses = courses,
                Images = images,
                Exercises = exercises,
                LessonsPagedList = lessonsPageList

            };
            return View(adminView);
        }

        public ActionResult Insert(course course)
        {
            var videos = _data.videos.ToList();
            var images = _data.images.ToList();
            var lessons = _data.lessons.ToList();
            var courses = _data.courses.Where(c => c.deleted_at == null).ToList();
            var exercises = _data.exercises.Where(e => e.deleted_at == null).ToList();
            /*
            if (course == null)
            {
                ViewBag.index = null;
            }
            else
            {
                var courseId = 
                ViewBag.index = GetIndexCurrent(lessons, course.id) + 1;
            }
            */
           
            var adminView = new AdminViewModels()
            {
                Lessons = lessons,
                Courses = courses,
                Images = images,
                Exercises = exercises,
                Videos = videos
            };
            return View(adminView);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Insert(AdminViewModels models)
        {
            if (ModelState.IsValid)
            {
                var lesson = new lesson()
                {
                    title = models.Lesson.title,
                    description = models.Lesson.description,
                    course_id = models.Lesson.course_id,
                    /*image_code = models.Lesson.image_code,
                    time = models.Lesson.time,*/
                    video_id = models.Lesson.video_id,
                    index = models.Lesson.index,
                    /*views = models.Lesson.views,*/
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                };
                _data.lessons.InsertOnSubmit(lesson);
                _data.SubmitChanges();
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        public ActionResult Update(int? id)
        {
            var lesson = _data.lessons.FirstOrDefault(l => l.id == id);
            var courses = _data.courses.Where(c => c.deleted_at == null).ToList();
            var images = _data.images.ToList();
            var exercises = _data.exercises.Where(e => e.deleted_at == null).ToList();
            var categories = _data.categories.Where(c => c.deleted_at == null).ToList();
            var adminView = new AdminViewModels()
            {
                Lesson = lesson,
                Courses = courses,
                Categories = categories,
                Images = images,
                Exercises = exercises
            };
            return View(adminView);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection form, int? id)
        {
            var lesson = _data.lessons.FirstOrDefault(l => l.id == id);
            if (lesson != null)
            {
                lesson.title = form["Lesson.title"];
                lesson.description = form["Lesson.description"];
                lesson.course_id = Convert.ToInt32(form["Lesson.course_id"]);
                /*lesson.image_code = form["Lesson.image_code"];
                lesson.time = Convert.ToInt32(form["Lesson.time"]);*/
                lesson.video_id = int.Parse(form["Lesson.video_id"]);
                lesson.index = Convert.ToInt32(form["Lesson.index"]);
                /*lesson.views = Convert.ToInt32(form["views"]);*/
                lesson.updated_at = DateTime.Now;
                _data.SubmitChanges();
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult Delete(int? id)
        {
            var lesson = _data.lessons.FirstOrDefault(l => l.id == id);
            if (lesson != null)
            {
                lesson.deleted_at = DateTime.Now;
                _data.SubmitChanges();
            }
            return RedirectToAction("Index");
        }   

        private int GetIndexCurrent(List<lesson> lessons , int? idCourse)
        {
            int index = 0;
            foreach (var lesson in lessons)
            {
                if (lesson.index > index)
                {
                    index = lesson.index;
                }
            }
            return index;
        }

        private void UploadGG(string fileName)
        {
            string fileJson = Server.MapPath("~/ltweb-avg-b91359369629.json");
            GoogleCredential google = GoogleCredential.FromFile(fileJson);

            // Tạo client Google Cloud Storage
            var storageClient = StorageClient.Create(google);

            // Tạo bucket
            string projectId = Helpers.GetValueFromAppSetting("ProjectIdGG");

            var bucketName = "video-lesson";

            try
            {
                var bucket = storageClient.GetBucket(bucketName);
            }
            catch (Exception ex)
            {
                var bucket = storageClient.CreateBucket(projectId, bucketName);
            }

            // Tải file video lên bucket
            var filePath = Server.MapPath("~/Content/Upload/" + fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                storageClient.UploadObject(bucketName, fileName, null, fileStream);
            }

        }
    }
}