using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Website_Course_AVG.Managers;
using Website_Course_AVG.Models;

namespace Website_Course_AVG.Controllers
{
    public class ExerciseController : Controller
    {
        // GET: Exercise
        [Website_Course_AVG.Attributes.Authorize]
        public JsonResult GetExercise(int lessonId)
        {
            try
            {
                MyDataDataContext _data = new MyDataDataContext();
                string fileJson = Server.MapPath("~/ltweb-avg-b91359369629.json");
                exercise exercise = _data.exercises.Where(x => x.lesson_id == lessonId).FirstOrDefault();

                if (exercise == null)
                {
                    return ResponseHelper.ErrorResponse(ResourceHelper.GetResource("Exercise is null"));
                }

                string url = Helpers.GetExerciseUrl(exercise, fileJson);

                return ResponseHelper.SuccessResponse(ResourceHelper.GetResource("Get Exercise Successful!"), url);
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                return ResponseHelper.ErrorResponse(errorMessage);
            }
        }
    }
}