using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Website_Course_AVG.Models;
using Website_Course_AVG.Managers;

namespace Website_Course_AVG.Controllers
{
    [Website_Course_AVG.Attributes.User]
    public class NoteController : Controller
    {
        MyDataDataContext _data = new MyDataDataContext();
        // GET: Note
        public ActionResult Index()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
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
                } else
                {
                    note.lesson_id = lessonId;
                    note.user_id = user.id;
                    note.content = content;
                    note.time = time;

                    _data.notes.InsertOnSubmit(note);
                }

                _data.SubmitChanges();

                return ResponseHelper.SuccessResponse("Add Note Successful");
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                return ResponseHelper.ErrorResponse(errorMessage);
            }
        }
    }
}