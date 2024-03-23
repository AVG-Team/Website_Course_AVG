using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Website_Course_AVG.Models;
using Website_Course_AVG.Managers;

namespace Website_Course_AVG.Controllers
{
    public class NoteController : Controller
    {
        MyDataDataContext _data = new MyDataDataContext();
        // GET: Note
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Create(int lessonId, string content, string time)
        {
            try
            {
                note note = new note();
                user user = Helpers.GetUserFromToken();
                note.lesson_id = lessonId;
                note.content = content;
                note.user_id = user.id;

                _data.notes.InsertOnSubmit(note);
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