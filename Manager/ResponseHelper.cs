using System.Net;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace Website_Course_AVG
{
    public static class ResponseHelper
    {
        public static JsonResult SuccessResponse(string message = "", object data = null)
        {
            var responseObject = new
            {
                success = true,
                data = data,
                message = message
            };

            return new JsonResult
            {
                Data = responseObject,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public static JsonResult ErrorResponse(string message = "", int status = 400)
        {
            var responseObject = new
            {
                success = false,
                data = new object(),
                message = message
            };

            HttpContext.Current.Response.StatusCode = status;
            return new JsonResult
            {
                Data = responseObject,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }
}
