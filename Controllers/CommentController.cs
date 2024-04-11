using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Website_Course_AVG.Attributes;
using Website_Course_AVG.Managers;
using Website_Course_AVG.Models;

namespace Website_Course_AVG.Controllers
{
    public class CommentController : Controller
    {

        MyDataDataContext _data = new MyDataDataContext();

        // GET: Comment
        [Website_Course_AVG.Attributes.Authorize]
        public ActionResult Index(int lessonId)
        {
            try
            {
                user user = Helpers.GetUserFromToken();
                List<comment> comments = _data.comments.Where(x => x.lesson_id == lessonId && x.type == 1).OrderBy(x => x.created_at).ToList();

                string view = RenderViewToString("Comment", "comments", comments);
                return ResponseHelper.SuccessResponse(ResourceHelper.GetResource("Get Comment Success"), view);
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
        public JsonResult Create(int lessonId, string content)
        {
            string fileJson = Server.MapPath("~/bad_words.json");

            if (Helpers.isBadWord(content, fileJson))
            {
                return ResponseHelper.ErrorResponse(ResourceHelper.GetResource("You are using too many words that violate community rules"));
            }

            try
            {
                user user = Helpers.GetUserFromToken();
                comment comment = new comment();
                comment commentTmp = _data.comments.Where(x => x.lesson_id == lessonId && x.user_id == user.id)
                    .OrderByDescending(x => x.created_at).FirstOrDefault();
                if (commentTmp != null && user.role != 2)
                {
                    DateTime created_at = commentTmp.created_at ?? DateTime.Now;
                    if (DateTime.Now < created_at.AddMinutes(30))
                    {
                        return ResponseHelper.ErrorResponse(ResourceHelper.GetResource("After 30 minutes you will be able to comment again"));
                    }
                }
                comment.content = content;
                comment.lesson_id = lessonId;
                comment.user_id = user.id;
                comment.type = 0;
                comment.created_at = DateTime.Now;
                comment.updated_at = DateTime.Now;
                if (user.role == 2)
                {
                    comment.type = 1;
                }

                _data.comments.InsertOnSubmit(comment);

                _data.SubmitChanges();


                return ResponseHelper.SuccessResponse(ResourceHelper.GetResource("Add Comment Successful!"), new
                {
                    id = comment.id,
                    content = comment.content,
                    name = comment.user.fullname,
                    role = comment.user.role,
                    type = comment.type,
                    ajax_delete = Url.Action("Delete", "Comment")
                });
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
                comment comment = _data.comments.Where(x => x.id == id && x.user_id == user.id).FirstOrDefault();
                if (comment != null)
                {
                    _data.comments.DeleteOnSubmit(comment);
                    _data.SubmitChanges();

                    return ResponseHelper.SuccessResponse(ResourceHelper.GetResource("Delete successful!"));
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