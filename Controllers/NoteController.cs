using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Website_Course_AVG.Models;
using Website_Course_AVG.Managers;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.Routing;

namespace Website_Course_AVG.Controllers
{
    [Website_Course_AVG.Attributes.User]
    public class NoteController : Controller
    {
        MyDataDataContext _data = new MyDataDataContext();
        // GET: Note
        [Website_Course_AVG.Attributes.Authorize]
        public JsonResult Index(int lessonId)
        {
            try
            {
                user user = Helpers.GetUserFromToken();
                List<note> notes = _data.notes.Where(x => x.lesson_id == lessonId && x.user_id == user.id).OrderBy(x => x.time).ToList();

                string view = RenderViewToString("Note", "notes", notes);
                return ResponseHelper.SuccessResponse("", view);
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                return ResponseHelper.ErrorResponse(errorMessage);
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        [Website_Course_AVG.Attributes.Authorize]
        public JsonResult Create(int lessonId, string content, int time)
        {
            try
            {
                user user = Helpers.GetUserFromToken();
                note note = new note();
                note noteTmp = _data.notes.Where(x => x.lesson_id == lessonId && x.user_id == user.id && x.time == time).FirstOrDefault();
                if (noteTmp != null)
                {
                    note = noteTmp;
                    note.content = content;
                    note.time = time;
                    note.updated_at = DateTime.Now;
                }
                else
                {
                    note.lesson_id = lessonId;
                    note.user_id = user.id;
                    note.content = content;
                    note.time = time;
                    note.created_at = DateTime.Now;

                    _data.notes.InsertOnSubmit(note);
                }

                _data.SubmitChanges();

                return ResponseHelper.SuccessResponse(ResourceHelper.GetResource("Add Note Successful"));
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                return ResponseHelper.ErrorResponse(errorMessage);
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        [Website_Course_AVG.Attributes.Authorize]
        public JsonResult Edit(int id, string content)
        {
            try
            {
                user user = Helpers.GetUserFromToken();
                note noteTmp = _data.notes.Where(x => x.id == id && x.user_id == user.id).FirstOrDefault();
                if (noteTmp != null)
                {
                    noteTmp.content = content;
                    noteTmp.updated_at = DateTime.Now;
                }
                else
                {
                    return ResponseHelper.ErrorResponse(ResourceHelper.GetResource("Not find note"));
                }

                _data.SubmitChanges();

                return ResponseHelper.SuccessResponse("Edit Note Successful");
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                return ResponseHelper.ErrorResponse(errorMessage);
            }
        }

        [HttpDelete]
        [Website_Course_AVG.Attributes.Authorize]
        public JsonResult Delete(int id)
        {
            using (MyDataDataContext _data = new MyDataDataContext())
            {
                user user = Helpers.GetUserFromToken();
                note note = _data.notes.Where(x => x.id == id && x.user_id == user.id).FirstOrDefault();
                if (note != null)
                {
                    _data.notes.DeleteOnSubmit(note);
                    _data.SubmitChanges();

                    return ResponseHelper.SuccessResponse(ResourceHelper.GetResource("Delete successful"));
                }

                return ResponseHelper.ErrorResponse(ResourceHelper.GetResource("Error Unknown, Please Try Again!"));
            }
        }

        //Helper
        protected string RenderViewToString(string controllerName, string viewName, object viewData)
        {
            using (var writer = new StringWriter())
            {
                var routeData = new RouteData();
                routeData.Values.Add("controller", controllerName);
                var fakeControllerContext = new ControllerContext(new HttpContextWrapper(new HttpContext(new HttpRequest(null, "http://google.com", null), new HttpResponse(null))), routeData, new NoteController());
                var razorViewEngine = new RazorViewEngine();
                var razorViewResult = razorViewEngine.FindView(fakeControllerContext, viewName, "", false);

                var viewContext = new ViewContext(fakeControllerContext, razorViewResult.View, new ViewDataDictionary(viewData), new TempDataDictionary(), writer);
                razorViewResult.View.Render(viewContext, writer);
                return writer.ToString();

            }
        }
    }
}