using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using Website_Course_AVG.Models;
using Website_Course_AVG.Managers;
using System.IO;
using System.Web.Routing;


namespace Website_Course_AVG.Controllers
{
    public class AdminController : Controller
    {
        private readonly MyDataDataContext _data = new MyDataDataContext();
        
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }
        
        // GET : Course 
        #region Course

        public ActionResult Course(int? page)
        {
            var courses = _data.courses.ToList();
            var categories = _data.categories.Where(cate => cate.deleted_at == null).ToList();
            var images = _data.images.ToList();
            var pageNumber = page ?? 1;
            var pageSize = 10;
            AdminViewModels adminView = new AdminViewModels()
            {
                Courses = courses,
                Categories = categories,
                Images = images,
                CoursesPagedList = courses.ToPagedList(pageNumber, pageSize)
            };

            return View(adminView);
        }

        public ActionResult InsertCourse()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InsertCourse(AdminViewModels models)
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
                    image_code = models.Course.image_code
                };
                _data.courses.InsertOnSubmit(course);
                _data.SubmitChanges();
                Helpers.AddCookie("success", "Insert course successfully");
                return RedirectToAction("Course");
            }

            Helpers.AddCookie("error", "Insert course failed");
            return RedirectToAction("Course");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateCourse(FormCollection collection,AdminViewModels models)
        {
            if (ModelState.IsValid)
            {
                var course = _data.courses.FirstOrDefault(c => c.id == models.Course.id);
                if (course != null)
                {
                    course.title = models.Course.title;
                    course.description = models.Course.description;
                    course.price = models.Course.price;
                    course.author = models.Course.author;
                    course.category_id = models.Course.category_id;
                    course.image_code = models.Course.image_code;
                    _data.SubmitChanges();
                    Helpers.AddCookie("success", "Update course successfully");
                    return RedirectToAction("Course");
                }
            }
            Helpers.AddCookie("error", "Update course failed");
            return RedirectToAction("Course");
        }
        public JsonResult ReloadTableCourse(int? page)
        {
            try
            {
                var courses = _data.courses.ToList();
                var categories = _data.categories.Where(cate => cate.deleted_at == null).ToList();
                var images = _data.images.ToList();
                var pageNumber = page ?? 1;
                var pageSize = 10;
                AdminViewModels adminView = new AdminViewModels()
                {
                    Courses = courses,
                    Categories = categories,
                    Images = images,
                    CoursesPagedList = courses.ToPagedList(pageNumber, pageSize)
                };
                var view = RenderViewToString("Admin", "Course", adminView);
                return ResponseHelper.SuccessResponse("", view);
            }
            catch (Exception ex)
            {
                return ResponseHelper.ErrorResponse("");
            }
        }

        #endregion
        

        protected string RenderViewToString(string controllerName, string viewName, object viewData)
        {
            using (var writer = new StringWriter())
            {
                var routeData = new RouteData();
                routeData.Values.Add("controller", controllerName);
                var fakeControllerContext = new ControllerContext(new HttpContextWrapper(new HttpContext(new HttpRequest(null, "http://google.com", null), new HttpResponse(null))), routeData, new CoursesController());
                var razorViewEngine = new RazorViewEngine();
                var razorViewResult = razorViewEngine.FindView(fakeControllerContext, viewName, "", false);

                var viewContext = new ViewContext(fakeControllerContext, razorViewResult.View, new ViewDataDictionary(viewData), new TempDataDictionary(), writer);
                razorViewResult.View.Render(viewContext, writer);
                return writer.ToString();

            }
        }
    }
}