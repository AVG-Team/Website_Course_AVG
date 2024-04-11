using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using Website_Course_AVG.Areas.Admin.Data.ViewModels;
using Website_Course_AVG.Managers;
using Website_Course_AVG.Models;
using System.Net;
using Website_Course_AVG.Attributes;

namespace Website_Course_AVG.Areas.Admin.Controllers
{
    public class ExerciseController : Controller
    {
        private readonly MyDataDataContext _data = new MyDataDataContext();

        public ExerciseController()
        {
            ViewBag.controller = "Exercise";
        }

        // GET: Admin/Exercise

        [Admin]
        public ActionResult Index(int? page)
        {
            var exercises = _data.exercises.ToList();
            var lessons = _data.lessons.Where(l => l.deleted_at == null).ToList();
            var pageNumber = page ?? 1;
            var pageSize = 10;
            var exercisePageList = exercises.ToPagedList(pageNumber, pageSize);
            var adminView = new AdminViewModels()
            {
                Exercises = exercises,
                Lessons = lessons,
                ExercisesPagedList = exercisePageList
            };
            return View(adminView);
        }

        [Admin]
        public ActionResult Insert()
        {
            var lesson = _data.lessons.Where(l => l.deleted_at == null).ToList();
            var adminView = new AdminViewModels()
            {
                Lessons = lesson
            };
            return View(adminView);
        }

        [Admin]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Insert(HttpPostedFileBase file,AdminViewModels model)
        {
            if (ModelState.IsValid)
            {
                if (file != null && file.ContentLength > 0)
                {
                    try
                    {
                        string fileJson = Server.MapPath("~/ltweb-avg-b91359369629.json");
                        GoogleCredential google = GoogleCredential.FromFile(fileJson);

                        // Tạo client Google Cloud Storage
                        var storageClient = StorageClient.Create(google);

                        // Tạo bucket
                        string projectId = Helpers.GetValueFromAppSetting("ProjectIdGG");

                        var uploadFolderPath = Server.MapPath("~/App_Data/UploadedFiles");
                        if (!Directory.Exists(uploadFolderPath))
                        {
                            Directory.CreateDirectory(uploadFolderPath);
                        }
                        var fileName = Path.GetFileName(file.FileName);
                        var path = Path.Combine(uploadFolderPath, fileName);
                        file.SaveAs(path);

                        var bucketName = "exercise-lesson";

                        try
                        {
                            var bucket = storageClient.GetBucket(bucketName);
                        }
                        catch (Exception ex)
                        {
                            var bucket = storageClient.CreateBucket(projectId, bucketName);
                        }


                        using (var fileStream = new FileStream(path, FileMode.Open))
                        {
                            storageClient.UploadObject(bucketName, fileName, null, fileStream);
                        }

                        var exercise = new exercise()
                        {
                            title = model.Exercise.title,
                            content = model.Exercise.content,
                            name= fileName,
                            time = DateTime.Now.AddMonths(1),
                            lesson_id = model.Exercise.lesson_id,
                            created_at = DateTime.Now,
                            updated_at = DateTime.Now
                        };
                       
                        exercise.link = Helpers.GetExerciseUrl(exercise, fileJson);
                        
                        _data.exercises.InsertOnSubmit(exercise);
                        _data.SubmitChanges();

                        DeleteFile(fileName);
                        
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                    }
                }
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View(model);
        }

        [Admin]
        public ActionResult Update(int? id)
        {
            var exercise = _data.exercises.FirstOrDefault(e => e.id == id);
            var lesson = _data.lessons.Where(l => l.deleted_at == null).ToList();
            var adminView = new AdminViewModels()
            {
                Exercise = exercise,
                Lessons = lesson
            };
            return View(adminView);
        }

        [HttpPost]
        [Admin]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection form,HttpPostedFileBase file)
        {
            var idCurrent = int.Parse(form["Exercise.id"]);
            var exercise = _data.exercises.FirstOrDefault(e => e.id == idCurrent);
            if (exercise != null)
            {
                if (ModelState.IsValid)
                {
                    string fileJson = Server.MapPath("~/ltweb-avg-b91359369629.json");
                    GoogleCredential google = GoogleCredential.FromFile(fileJson);

                    // Tạo client Google Cloud Storage
                    var storageClient = StorageClient.Create(google);

                    // Tạo bucket
                    string projectId = Helpers.GetValueFromAppSetting("ProjectIdGG");

                    var uploadFolderPath = Server.MapPath("~/Content/UploadedFiles");
                    if (!Directory.Exists(uploadFolderPath))
                    {
                        Directory.CreateDirectory(uploadFolderPath);
                    }
                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(uploadFolderPath, fileName);
                    file.SaveAs(path);

                    var bucketName = "exercise-lesson";

                    try
                    {
                        var bucket = storageClient.GetBucket(bucketName);
                    }
                    catch (Exception ex)
                    {
                        var bucket = storageClient.CreateBucket(projectId, bucketName);
                    }


                    using (var fileStream = new FileStream(path, FileMode.Open))
                    {
                        storageClient.UploadObject(bucketName, fileName, null, fileStream);
                    }

                    exercise.title = form["Exercise.title"];
                    exercise.content = form["Exercise.content"];
                    exercise.name = fileName;
                    exercise.link = Helpers.GetExerciseUrl(exercise, fileJson);
                    exercise.time = DateTime.Now.AddMonths(1);
                    exercise.lesson_id = int.Parse(form["Exercise.lesson_id"]);
                    exercise.updated_at = DateTime.Now;



                    _data.SubmitChanges();
                    return RedirectToAction("Index");
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult Delete()
        {
            return View("Index");
        }

        [HttpPost]
        [Admin]
        public ActionResult Delete(int? id)
        {
            var exercise = _data.exercises.FirstOrDefault(e => e.id == id);
            if (exercise != null)
            {
                exercise.deleted_at = DateTime.Now;
                _data.SubmitChanges();
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        public void DeleteFile(string fileName)
        {
            try
            {
                string filePath = Server.MapPath("~/App_Data/UploadedFiles/" + fileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}