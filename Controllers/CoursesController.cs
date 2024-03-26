using PagedList;
using System.Linq;
using System.Web.Mvc;
using Website_Course_AVG.Models;

namespace Website_Course_AVG.Controllers
{
    public class CoursesController : Controller
    {
        private readonly MyDataDataContext _data = new MyDataDataContext();
        // GET: Courses
        public ActionResult Index(int? page)
        {
            if (page == null)
            {
                page = 1;
            }
            var courses = _data.courses.Where(m => m.deleted_at == null).ToList();

            int pageNumber = (page ?? 1);

            return View(courses.ToPagedList(pageNumber, 12));
        }

        public ActionResult Details()
        {
            return View();
        }
    }
}