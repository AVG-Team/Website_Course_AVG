using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.AccessControl;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
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

        public static string GenerateRandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                int index = random.Next(chars.Length);
                builder.Append(chars[index]);
            }
            return builder.ToString();
        }

        public static string GetVideoLessonUrl(video video, string fileJson, int seconds = 300)
        {
            if (video == null)
            {
                throw new ArgumentNullException(nameof(video));
            }

            GoogleCredential google = GoogleCredential.FromFile(fileJson);

            var bucketName = "video-lesson";

            if(!string.IsNullOrEmpty(video.link) && video.time < DateTime.Now)
            {
                return video.link;
            }

            UrlSigner urlSigner = UrlSigner.FromCredential(google);
            string url = urlSigner.Sign(
                bucketName,
                video.name,
                TimeSpan.FromSeconds(seconds),
                HttpMethod.Get);


            using (MyDataDataContext dataContext = new MyDataDataContext())
            {
                video videoTmp = dataContext.videos.FirstOrDefault(x => x.name == video.name);

                if (videoTmp != null)
                {
                    videoTmp.link = url;
                    videoTmp.updated_at = DateTime.Now;
                    videoTmp.time = DateTime.Now.AddMonths(1);

                    dataContext.SubmitChanges();
                }
            }

            return url;
        }

        public static string ConvertTime(int time)
        {
            int hours = (int)(time / 3600);
            double remainingSeconds = time % 3600;
            int minutes = (int)(remainingSeconds / 60);
            int seconds = (int)(remainingSeconds % 60);

            string result =
                (hours < 10 ? "0" + hours : hours.ToString()) +
                ":" +
                (minutes < 10 ? "0" + minutes : minutes.ToString());

            if (seconds < 10)
            {
                result += ":0" + seconds;
            }
            else
            {
                result += ":" + seconds;
            }

            return result;
        }

        public static Identity GetIdentity(lesson lesson, List<lesson> lessons)
        {
            int indexCurrentLesson = lesson.index;

            int identityPrevious = 0;

            if (indexCurrentLesson >= 1)
            {
                lesson previous = lessons.Where(x => x.index == ( indexCurrentLesson - 1) ).FirstOrDefault();
                if(previous != null)
                {
                    identityPrevious = previous.id;
                }
            }
            int identityNext = 999999;

            lesson next = lessons.Where(x => x.index == (indexCurrentLesson + 1)).FirstOrDefault();
            if (next != null)
            {
                identityNext = next.id;
            }

            Identity index = new Identity
            {
                IdCurrent = lesson.id,
                IdPrevious = identityPrevious,
                IdNext = identityNext,
            };

            return index;
        }
    }
}