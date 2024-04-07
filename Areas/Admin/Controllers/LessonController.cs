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

        public ActionResult Insert()
        {
            var lessons = _data.lessons.ToList();
            var courses = _data.courses.Where(c => c.deleted_at == null).ToList();
            var images = _data.images.ToList();
            var exercises = _data.exercises.Where(e => e.deleted_at == null).ToList();
            ViewBag.index = GetIndexCurrent(lessons) + 1;
            var adminView = new AdminViewModels()
            {
                Lessons = lessons,
                Courses = courses,
                Images = images,
                Exercises = exercises
            };
            return View(adminView);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Insert(lesson model)
        {
            if (ModelState.IsValid)
            {
                var lesson = new lesson()
                {
                    title = model.title,
                    description = model.description,
                    course_id = model.course_id,
                    image_code = model.image_code,
                    time = model.time,
                    video_id = model.video_id,
                    index = model.index,
                    views = model.views,
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
            var adminView = new AdminViewModels()
            {
                Lesson = lesson,
                Courses = courses,
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
            if (ModelState.IsValid)
            {/*
                lesson.title = form["title"];
                lesson.description = form["description"];
                lesson.course_id = Convert.ToInt32(form["course_id"]);
                lesson.image_code = form["image_code"];
                lesson.time = Convert.ToInt32(form["time"]);
                lesson.video_id = form["video"];
                lesson.index = Convert.ToInt32(form["index"]);
                lesson.views = Convert.ToInt32(form["views"]);
                lesson.updated_at = DateTime.Now;
                _data.SubmitChanges();*/
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }

        private int GetIndexCurrent(List<lesson> lessons)
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