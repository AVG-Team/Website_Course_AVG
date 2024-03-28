using PagedList;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Website_Course_AVG.Models;

namespace Website_Course_AVG.Controllers
{
    public class CoursesController : Controller
    {
        private readonly MyDataDataContext _data = new MyDataDataContext();

        public ActionResult Index(int? page)
        {
            ViewBag.page = page;
            PopulateDropList();
            if (page == null)
            {
                page = 1;
            }

            var courses = from c in _data.courses
                    .Include(m => m.category)
                    .Where(m => m.deleted_at == null)
                          select c;
            var category = _data.categories.ToList();
            ViewBag.Categories = category;
            int pageNumber = (page ?? 1);

            var list = courses.ToPagedList(pageNumber, 12);
            return View(list);
        }
        // GET: Courses
        public JsonResult GetCourse(int? page, int? CategoryID)
        {
            try
            {
                PopulateDropList();
                if (page == null)
                {
                    page = 1;
                }
                var courses = from c in _data.courses
                        .Include(m => m.category)
                        .Where(m => m.deleted_at == null)
                              select c;
                if (CategoryID != null)
                {
                    courses = courses.Where(m => m.category_id == CategoryID);
                }
                int pageNumber = (page ?? 1);

                var list = courses.ToPagedList(pageNumber, 12);
                var view = RenderViewToString("Courses", "ListCourses", list);
                return ResponseHelper.SuccessResponse("", view);
            }
            catch (Exception ex)
            {
                return ResponseHelper.ErrorResponse("");
            }

        }

        private SelectList GetCategories(int? selectId)
        {
            return new SelectList(_data.categories
                .OrderBy(c => c.name)
                .ThenBy(c => c.id), "id", "name", selectId);

        }

        private void PopulateDropList(course course = null)
        {
            ViewData["CategoryID"] = GetCategories(course?.category_id);
        }

        public ActionResult Details(int CourseId = 8)
        {
            if (CourseId == null)
                return HttpNotFound();
            else
            {
                var imageCode = "";
                var course = _data.courses.Where(c => c.id == CourseId).ToList();
                foreach (var item in course)
                {
                    imageCode = item.image_code;
                }

                var images = _data.images.Where(i => i.code.Equals(imageCode)).ToList();
                var lessons = _data.lessons.Where(l => l.course_id == CourseId).ToList();
                var viewModel = new CoursesViewModel()
                {
                    Courses = course,
                    Images = images,
                    Lessons = lessons
                };

                return View(viewModel);
            }
        }
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