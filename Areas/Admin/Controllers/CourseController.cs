using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using PagedList;
using Website_Course_AVG.Areas.Admin.Data.ViewModels;
using Website_Course_AVG.Models;

namespace Website_Course_AVG.Areas.Admin.Controllers
{
    public class CourseController : Controller
    {
        private readonly MyDataDataContext _data = new MyDataDataContext();

        public ActionResult Index(int? page)
        {
            var courses = _data.courses.ToList();
            var categories = _data.categories.Where(cate => cate.deleted_at == null).ToList();
            var images = _data.images.ToList();
            var pageNumber = page ?? 1;
            var pageSize = 10;
            ViewBag.controller = "Course";
            var adminView = new AdminViewModels()
            {
                Courses = courses,
                Categories = categories,
                Images = images,
                CoursesPagedList = courses.ToPagedList(pageNumber, pageSize)
            };

            return View(adminView);
        }
        

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
        }

        public ActionResult Update(int? id)
        {
            var course = _data.courses.FirstOrDefault(c => c.id == id);
            var categories = _data.categories.Where( c => c.deleted_at == null ).ToList();
            var adminView= new AdminViewModels()
            {
                Course = course,
                Categories = categories
            };
            return View(adminView);
        }
        
        [HttpPost]
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
        public ActionResult Delete(course model)
        {
            var course = _data.courses.FirstOrDefault(c => c.id == model.id);
            if (course != null)
            {
                course.deleted_at = DateTime.Now;
                _data.SubmitChanges();
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }


        [HttpGet]
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
            catch (Exception ex)
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