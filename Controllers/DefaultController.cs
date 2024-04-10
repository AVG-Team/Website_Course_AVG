using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Website_Course_AVG.Managers;

namespace Website_Course_AVG.Controllers
{
    public class DefaultController : Controller
    {
        // GET: Default
        public ActionResult Index()
        {

            string fileJson = Server.MapPath("~/ltweb-avg-b91359369629.json");
            GoogleCredential google = GoogleCredential.FromFile(fileJson);

            // Tạo client Google Cloud Storage
            var storageClient = StorageClient.Create(google);

            // Tạo bucket
            string projectId = Helpers.GetValueFromAppSetting("ProjectIdGG");

            var bucketName = "exercise-lesson";

            try
            {
                var bucket = storageClient.GetBucket(bucketName);
            }
            catch (Exception ex)
            {
                var bucket = storageClient.CreateBucket(projectId, bucketName);
            }

            // Tải file video lên bucket
            var fileName = "AVG COURSES.docx";
            var filePath = Server.MapPath("~/Content/" + fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                storageClient.UploadObject(bucketName, fileName, null, fileStream);
            }

            return View();
        }

        //public ActionResult Success()
        //{
        //    string fileJson = Server.MapPath("~/ltweb-avg-b91359369629.json");
        //    string url = Helpers.GetVideoLessonUrl("test.mp4", fileJson);
        //    ViewBag.Url = url;
        //    return View("Index");
        //}
    }
}