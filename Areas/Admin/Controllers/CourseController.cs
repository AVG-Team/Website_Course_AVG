using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Imgur.API.Authentication;
using Imgur.API.Endpoints;
using Imgur.API.Models;
using PagedList;
using Website_Course_AVG.Areas.Admin.Data.ViewModels;
using Website_Course_AVG.Attributes;
using Website_Course_AVG.Managers;
using Website_Course_AVG.Models;

namespace Website_Course_AVG.Areas.Admin.Controllers
{
    public class CourseController : Controller
    {
        private readonly MyDataDataContext _data = new MyDataDataContext();

        public CourseController()
        {
            ViewBag.controller = "Course";
        }

        [Admin]
        public ActionResult Index(int? page)
        {
            var courses = _data.courses.ToList();
            var categories = _data.categories.Where(cate => cate.deleted_at == null).ToList();
            var images = _data.images.ToList();
            var pageNumber = page ?? 1;
            var pageSize = 10;

            var adminView = new AdminViewModels()
            {
                Courses = courses,
                Categories = categories,
                Images = images,
                CoursesPagedList = courses.ToPagedList(pageNumber, pageSize)
            };

            return View(adminView);
        }

        [Admin]
        public ActionResult Insert()
        {
            var course = _data.courses.ToList();
            var categories = _data.categories.Where(cate => cate.deleted_at == null).ToList();
            var adminView = new AdminViewModels()
            {
                Courses = course,
                Categories = categories
            };
            return View(adminView);
        }

        [HttpPost]
        [Admin]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Insert(IEnumerable<HttpPostedFileBase> files, HttpPostedFileBase file, AdminViewModels models)
        {
            try
            {
                var apiClient = new ApiClient("6efaec52e38d148", "5de13ec766f236d3d39808cb21fec395962922cf");
                var httpClient = new HttpClient();

                var oAuth2Endpoint = new OAuth2Endpoint(apiClient, httpClient);
                var authUrl = oAuth2Endpoint.GetAuthorizationUrl();

                var token = new OAuth2Token
                {
                    AccessToken = "4e5b5d07a81334ea5c5459dc5c1ef63458c296eb",
                    RefreshToken = "6e653d2f3b7c99cb1c135f690c5b297b8a1555b1",
                    AccountId = 180393165,
                    AccountUsername = "lvxadoniss1",
                    ExpiresIn = 315360000,
                    TokenType = "bearer"
                };

                apiClient.SetOAuth2Token(token);
                var imageEndpoint = new ImageEndpoint(apiClient, httpClient);
                var code = "AVG_IMG_" + Helpers.GenerateRandomString(8);
                string bannerImageUrl = null;

                if (file != null && file.ContentLength > 0)
                {
                    var bannerStream = file.InputStream;
                    var bannerUpload = await imageEndpoint.UploadImageAsync(bannerStream);
                    bannerImageUrl = bannerUpload.Link;

                    var newBanner = new image
                    {
                        image1 = bannerImageUrl,
                        type = true,
                        category = false,
                        code = code,
                    };
                    _data.images.InsertOnSubmit(newBanner);
                    _data.SubmitChanges();
                }

                foreach (var f in files)
                {
                    if (f != null && f.ContentLength > 0)
                    {
                        var fileStream = f.InputStream;
                        var imageUpload = await imageEndpoint.UploadImageAsync(fileStream);
                        var imageUrl = imageUpload.Link;

                        var newImage = new image
                        {
                            image1 = imageUrl,
                            type = true,
                            category = true,
                            code = code,
                        };
                        _data.images.InsertOnSubmit(newImage);
                    }
                }

                var course = new course()
                {
                    title = models.Course.title,
                    description = models.Course.description,
                    price = models.Course.price,
                    author = models.Course.author,
                    category_id = models.Course.category_id,
                    image_code = code,
                    created_at = DateTime.Now,
                };
                _data.courses.InsertOnSubmit(course);

                // Lưu thay đổi vào cơ sở dữ liệu
                _data.SubmitChanges();


                ViewBag.Message = "Images uploaded successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, "Error");
                var categories = _data.categories.Where(cate => cate.deleted_at == null).ToList();
                var adminView = new AdminViewModels()
                {
                    Categories = categories
                };
                return View(adminView);
            }
        }

        /*[HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Insert(AdminViewModels models)
        {
            if (ModelState.IsValid)
            {
                var course = new course()
                {
                    title = models.Course.title,
                    description = models.Course.description,
                    price = models.Course.price,
                    author = models.Course.author,
                    category_id = models.Course.category_id,
                    image_code = models.Course.image_code,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                };
                _data.courses.InsertOnSubmit(course);
                _data.SubmitChanges();
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }*/

        [Admin]
        public ActionResult Update(int? id)
        {
            var course = _data.courses.FirstOrDefault(c => c.id == id);
            var categories = _data.categories.Where(c => c.deleted_at == null).ToList();
            var adminView = new AdminViewModels()
            {
                Course = course,
                Categories = categories
            };
            return View(adminView);
        }

        [HttpPost]
        [Admin]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection form, int? id)
        {

            var course = _data.courses.FirstOrDefault(c => c.id == id);
            if (course != null)
            {
                course.title = form["Course.title"];
                course.description = form["Course.description"];
                course.price = long.Parse(form["Course.Price"]);
                course.author = form["Course.author"];
                course.category_id = int.Parse(form.Get("Course.category_id"));
                /*course.image_code = form.Get("Course.image_code");*/
                course.updated_at = DateTime.Now;
                _data.SubmitChanges();
                return RedirectToAction("Index");
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
            var course = _data.courses.FirstOrDefault(c => c.id == id);
            if (course != null)
            {
                course.deleted_at = DateTime.Now;
                _data.SubmitChanges();
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }


        [HttpGet]
        [Admin]
        public JsonResult GetCourseById(int? id)
        {
            try
            {
                var course = _data.courses.Where(c => c.id == id).ToList();
                var categories = _data.categories.ToList();
                var model = new AdminViewModels()
                {
                    Courses = course,
                    Categories = categories
                };
                var view = RenderViewToString("Admin/Course", "GetCourseById", model);
                return ResponseHelper.SuccessResponse("", view);
            }
            catch (Exception)
            {
                return ResponseHelper.ErrorResponse("");
            }
        }

        protected string RenderViewToString(string controllerName, string viewName, object viewData)
        {
            using (var writer = new StringWriter())
            {
                var routeData = new RouteData();
                routeData.Values.Add("controller", controllerName);
                var fakeControllerContext = new ControllerContext(new HttpContextWrapper(new HttpContext(new HttpRequest(null, "http://google.com", null), new HttpResponse(null))), routeData, new CourseController());
                var razorViewEngine = new RazorViewEngine();
                var razorViewResult = razorViewEngine.FindView(fakeControllerContext, viewName, "", false);

                var viewContext = new ViewContext(fakeControllerContext, razorViewResult.View, new ViewDataDictionary(viewData), new TempDataDictionary(), writer);
                razorViewResult.View.Render(viewContext, writer);
                return writer.ToString();

            }
        }
    }
}