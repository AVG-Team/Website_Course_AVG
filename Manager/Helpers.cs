using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using System;
using System.Net.Http;
using System.Security.AccessControl;
using System.Web;
using Website_Course_AVG.Models;

namespace Website_Course_AVG.Managers
{
    public partial class Helpers
    {
        public static void addCookie(string key, string value, int second = 10)
        {
            HttpCookie cookie = new HttpCookie(key, value);
            cookie.Expires = DateTime.Now.AddSeconds(second);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public static bool IsAuthenticated()
        {
            UserManager userManager = new UserManager();
            return userManager.IsAuthenticated();
        }

        public static user GetUserFromToken()
        {
            UserManager userManager = new UserManager();
            return userManager.GetUserFromToken();
        }

        public static string GetValueFromAppSetting(string key)
        {
            return global::System.Configuration.ConfigurationManager.AppSettings[key];
        }

        public static string UrlGithubLogin()
        {
            string clientIdGh = GetValueFromAppSetting("ClientIdGH");
            string redirectUrl = GetValueFromAppSetting("RedirectUrl");
            return
                "https://github.com//login/oauth/authorize?client_id=" + clientIdGh + "&redirect_uri=" + redirectUrl + "&scope=user:email";
        }

        public static string GetVideoLessonUrl(string fileName, string fileJson, int seconds = 300)
        {
            GoogleCredential google = GoogleCredential.FromFile(fileJson);

            var bucketName = "video-lesson";

            UrlSigner urlSigner = UrlSigner.FromCredential(google);
            string url = urlSigner.Sign(
                bucketName,
                fileName,
                TimeSpan.FromSeconds(seconds),
                HttpMethod.Get);

            return url;
        }

    }
}