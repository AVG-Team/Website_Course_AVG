using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Website_Course_AVG.Managers;
using Website_Course_AVG.Models;

namespace Website_Course_AVG.Controllers
{
    public class CourseController : Controller
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

            var imageCodes = courses.Select(c => c.image_code).Distinct().ToList();

            var images = _data.images.Where(i => imageCodes.Contains(i.code) && i.category == false).ToList();

            foreach (var course in courses)
            {
                var image = images.FirstOrDefault(i => i.code == course.image_code);
                if (image != null)
                {
                    course.image_code = image.image1;
                }
            }
            var list = courses.ToPagedList(pageNumber, 12);
            return View(list);
        }

        // GET: Courses
        public JsonResult GetCourse(int? page, int? categoryId, int? index)
        {
            try
            {
                PopulateDropList();
                if (page == null)
                {
                    page = 1;
                }

                /*var courses = from c in _data.courses
                        .Include(m => m.category)
                        .Where(m => m.deleted_at == null)
                              select c;*/
                /*if (categoryId != null)
                {
                    courses = courses.Where(m => m.category_id == categoryId);
                }

                if (index == 1)
                {
                    courses = courses.Where(m => m.price != 0);
                }
                else
                {
                    courses = courses.Where(m => m.price == 0);
                }*/
                var courses = ClassificationCourse(categoryId, index);
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

        public ActionResult Details()
        {
            var courses = _data.courses.Where(c => c.deleted_at == null).ToList();
            var lessons = _data.lessons.Where(l => l.deleted_at == null).ToList();
            var detailCourses = _data.detail_courses.Where(d => d.deleted_at == null).ToList();
            var viewmodel = new CoursesViewModel()
            {
                Courses = courses,
                Lessons = lessons,
                DetailCourses = detailCourses
            };
            return View(viewmodel);
        }

        [HttpGet]
        public ActionResult Details(int? id)
        {
            var course = _data.courses.FirstOrDefault(c => c.id == id);
            if(course != null)
            {
                var courses = _data.courses.Where(c => c.deleted_at == null).ToList();
                var images = _data.images.Where(i => i.code.Equals(course.image_code) && i.category == true).ToList();
                var lessons = _data.lessons.Where(l => l.course_id == id).ToList();
                var detailCourses = _data.detail_courses.Where(d => d.course_id == id).ToList();
                var viewModel = new CoursesViewModel()
                {
                    Courses = courses,
                    Images = images,
                    Lessons = lessons,
                    Course = course,
                    DetailCourses = detailCourses
                };
                return View(viewModel);
            }

            return RedirectToAction("Index");
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

        private IQueryable<course> ClassificationCourse(int? id, int? idCost)
        {
            var courses = from c in _data.courses
                    .Include(m => m.category)
                    .Where(m => m.deleted_at == null)
                          select c;
            if (id != null)
            {
                courses = courses.Where(m => m.category_id == id && m.price != 0);
            }

            if (idCost == 1)
            {
                courses = courses.Where(m => m.price != 0);
            }
            else if (idCost == 0)
            {
                courses = courses.Where(m => m.price == 0);
            }
            return courses;
        }

        [Authorize]
        [HttpGet]
        public ActionResult GetCourseUser()
        {
            try
            {
                user currentUser = Helpers.GetUserFromToken();
                if (currentUser != null)
                {
                    var userCourses = _data.detail_courses.Where(dc => dc.user_id == currentUser.id).Select(dc => new CourseUserViewModel()
                    {
                        course = dc.course,
                        user = currentUser
                    }).ToList();
                    return View(userCourses);
                }
                return View();
            }
            catch(Exception ex)
            {
                Helpers.AddCookie("Error", "Error Unkown");
                return RedirectToAction("Index", "Home");
            }
        }
    } 
}