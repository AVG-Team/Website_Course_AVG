using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using Website_Course_AVG.Models;
using System.IO;
using System.Web.Routing;
using Website_Course_AVG.Areas.Admin.Data.ViewModels;


namespace Website_Course_AVG.Areas.Admin.Controllers
{
    public class AdminController : Controller
    {
        private readonly MyDataDataContext _data = new MyDataDataContext();

        public AdminController()
        {
            ViewBag.controller = "Admin";
        }
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        #region Exercise

        public ActionResult Exercise(int? page)
        {
            var exercises = _data.exercises.ToList();
            var pageNumber = page ?? 1;
            var pageSize = 10;
            var exercisesListPage = exercises.ToPagedList(pageNumber, pageSize);
            var viewModel = new AdminViewModels()
            {
                Exercises = exercises,
                ExercisesPagedList = exercisesListPage
            };
            return View(viewModel);
        }

        public ActionResult InsertExercise()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InsertExercise(AdminViewModels model)
        {
            if (ModelState.IsValid)
            {
                var exercise = new exercise()
                {
                    title = model.Exercise.title,
                    content = model.Exercise.content,
                    name = model.Exercise.name,
                    link = model.Exercise.link,
                    time = model.Exercise.time,
                    lesson_id = model.Exercise.lesson_id
                };
                _data.exercises.InsertOnSubmit(exercise);
                _data.SubmitChanges();
                return RedirectToAction("Exercise");
            }
            return RedirectToAction("Exercise");
        }

        public ActionResult UpdateExercise()
        {
            return View("Exercise");
        }

        [HttpPost]
        public ActionResult UpdateExercise(FormCollection form, exercise model)
        {
            var exercise = _data.exercises.FirstOrDefault(c => c.id == model.id);
            if (exercise != null)
            {
                exercise.title = form["Exercise.title"];
                exercise.content = form["Exercise.content"];
                exercise.name = form["Exercise.name"];
                exercise.link = form["Exercise.link"];
                exercise.time = DateTime.Parse(form["Exercise.time"]);
                exercise.lesson_id = int.Parse(form["Exercise.lesson_id"]);
                exercise.updated_at = DateTime.Now;
                _data.SubmitChanges();
                return RedirectToAction("Exercise");
            }
            return RedirectToAction("Exercise");
        }

        public ActionResult DeleteExercise()
        {
            return View("Exercise");
        }
        [HttpPost]
        public ActionResult DeleteExercise(exercise model)
        {
            var exercise = _data.exercises.FirstOrDefault(c => c.id == model.id);
            if (exercise != null)
            {
                exercise.deleted_at = DateTime.Now;
                _data.SubmitChanges();
                return RedirectToAction("Exercise");
            }
            return RedirectToAction("Exercise");
        }

        public JsonResult GetExerciseById(int? id)
        {
            try
            {
                var exercise = _data.exercises.Where(c => c.id == id).ToList();
                var model = new AdminViewModels()
                {
                    Exercises = exercise
                };
                var view = RenderViewToString("Admin", "GetExerciseById", model);
                return ResponseHelper.SuccessResponse("", view);
            }
            catch (Exception ex)
            {
                return ResponseHelper.ErrorResponse("Don't get exercise by id !");
            }
        }
        #endregion

        #region Lesson

        public ActionResult Lesson(int? page)
        {
            var lessons = _data.lessons.ToList();
            var pageNumber = page ?? 1;
            var pageSize = 10;
            var lessonsListPage = lessons.ToPagedList(pageNumber, pageSize);
            var viewModel = new AdminViewModels()
            {
                Lessons = lessons,
                LessonsPagedList = lessonsListPage
            };
            return View(viewModel);
        }

        public ActionResult InsertLesson()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InsertLesson(AdminViewModels model)
        {
            if (ModelState.IsValid)
            {
                var lesson = new lesson()
                {
                    title = model.Lesson.title,
                    description = model.Lesson.description,
                    image_code = model.Lesson.image_code,
                    time = model.Lesson.time,
                    video_id = model.Lesson.video_id,
                    views = model.Lesson.views,
                    index = model.Lesson.index,
                    course_id = model.Lesson.course_id
                };
                _data.lessons.InsertOnSubmit(lesson);
                _data.SubmitChanges();
                return RedirectToAction("Lesson");
            }
            return RedirectToAction("Lesson");
        }


        public ActionResult UpdateLesson()
        {
            return View("Lesson");
        }

        [HttpPost]
        public ActionResult UpdateLesson(FormCollection form, lesson model)
        {
            var lesson = _data.lessons.FirstOrDefault(c => c.id == model.id);
            if (lesson != null)
            {
                lesson.title = form["Lesson.title"];
                lesson.description = form["Lesson.description"];
                lesson.image_code = form["Lesson.image_code"];
                lesson.time = int.Parse(form["Lesson.time"]);
                lesson.video_id = int.Parse(form["Lesson.video_id"]);
                lesson.views = int.Parse(form["Lesson.views"]);
                lesson.index = int.Parse(form["Lesson.index"]);
                lesson.course_id = int.Parse(form["Lesson.course_id"]);
                lesson.updated_at = DateTime.Now;
                _data.SubmitChanges();
                return RedirectToAction("Lesson");
            }
            return RedirectToAction("Lesson");
        }

        public ActionResult DeleteLesson()
        {
            return View("Lesson");
        }

        [HttpPost]
        public ActionResult DeleteLesson(lesson model)
        {
            var lesson = _data.lessons.FirstOrDefault(c => c.id == model.id);
            if (lesson != null)
            {
                lesson.deleted_at = DateTime.Now;
                _data.SubmitChanges();
                return RedirectToAction("Lesson");
            }
            return RedirectToAction("Lesson");
        }
        #endregion

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