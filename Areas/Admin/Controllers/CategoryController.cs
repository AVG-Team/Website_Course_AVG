using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using PagedList;
using Website_Course_AVG.Areas.Admin.Data.ViewModels;
using Website_Course_AVG.Attributes;
using Website_Course_AVG.Models;

namespace Website_Course_AVG.Areas.Admin.Controllers
{
    public class CategoryController : Controller
    {
        private readonly MyDataDataContext _data = new MyDataDataContext();

        public CategoryController()
        {
            ViewBag.controller = "Category";
        }

        [Admin]
        public ActionResult Index(int? page)
        {
            var categories = _data.categories.ToList();
            var courses = _data.courses.ToList();
            var pageNumber = page ?? 1;
            var pageSize = 10;
            var categoriesListPage = categories.ToPagedList(pageNumber, pageSize);
            var viewModel = new AdminViewModels()
            {
                Categories = categories,
                Courses = courses,
                CategoriesPagedList = categoriesListPage
            };
            return View(viewModel);
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

        [Admin]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Insert(AdminViewModels model)
        {
            if (ModelState.IsValid)
            {
                var category = new category()
                {
                    name = model.Category.name,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                };
                _data.categories.InsertOnSubmit(category);
                _data.SubmitChanges();
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        [Admin]
        public ActionResult Update(int? id)
        {
            var category = _data.categories.FirstOrDefault(c => c.id == id);
            var courses = _data.courses.Where(c => c.deleted_at == null).ToList();
            var adminView = new AdminViewModels()
            {
                Category = category,
                Courses = courses
            };
            return View(adminView);
        }

        [Admin]
        [HttpPost]
        public ActionResult Update(FormCollection form, category model)
        {
            var category = _data.categories.FirstOrDefault(c => c.id == model.id);
            if (category != null)
            {
                category.name = form["Category.name"];
                category.updated_at = DateTime.Now;
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
        public ActionResult Delete(category model)
        {
            var category = _data.categories.FirstOrDefault(c => c.id == model.id);
            if (category != null)
            {
                category.deleted_at = DateTime.Now;
                _data.SubmitChanges();
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
        [HttpGet]
        public JsonResult GetCategoryById(int? id)
        {
            try
            {
                var category = _data.categories.Where(c => c.id == id).ToList();
                var model = new AdminViewModels()
                {
                    Categories = category
                };
                var view = RenderViewToString("Admin", "GetCategoryById", model);
                return ResponseHelper.SuccessResponse("", view);
            }
            catch (Exception ex)
            {
                return ResponseHelper.ErrorResponse("Don't get category by id !");
            }
        }
        protected string RenderViewToString(string controllerName, string viewName, object viewData)
        {
            using (var writer = new StringWriter())
            {
                var routeData = new RouteData();
                routeData.Values.Add("controller", controllerName);
                var fakeControllerContext = new ControllerContext(new HttpContextWrapper(new HttpContext(new HttpRequest(null, "http://google.com", null), new HttpResponse(null))), routeData, new CategoryController());
                var razorViewEngine = new RazorViewEngine();
                var razorViewResult = razorViewEngine.FindView(fakeControllerContext, viewName, "", false);

                var viewContext = new ViewContext(fakeControllerContext, razorViewResult.View, new ViewDataDictionary(viewData), new TempDataDictionary(), writer);
                razorViewResult.View.Render(viewContext, writer);
                return writer.ToString();

            }
        }
    }
}