using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Web;
using System.Web.UI.WebControls;
using Website_Course_AVG.Models;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Threading;
using System.Web.Mvc;
using Microsoft.IdentityModel.Logging;

namespace Website_Course_AVG.Managers
{
    public partial class Helpers
    {
        public static void AddCookie(string key, string value, int second = 3)
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

        public static bool IsUser()
        {
            UserManager userManager = new UserManager();
            return userManager.IsUser();
        }

        public static bool IsAdmin()
        {
            UserManager userManager = new UserManager();
            return userManager.IsAdmin();
        }

        public static user GetUserFromToken()
        {
            UserManager userManager = new UserManager();
            return userManager.GetUserFromToken();
        }

        public static string GetValueFromAppSetting(string key)
        {
            return MvcApplication.Configuration[key];
        }

        public static string GetRedirectUrlGH()
        {
            HttpContext currentContext = HttpContext.Current;
            string currentUrl = currentContext.Request.Url.GetLeftPart(UriPartial.Authority);

            return currentUrl + "/Account/GithubLogin";
        }

        public static string GetRedirectUrlMoMo()
        {
            HttpContext currentContext = HttpContext.Current;
            string currentUrl = currentContext.Request.Url.GetLeftPart(UriPartial.Authority);

            return currentUrl + "/Order/ConfirmMoMoPaymentClient";
        }

        public static string GetRedirectUrlVNPay()
        {
            HttpContext currentContext = HttpContext.Current;
            string currentUrl = currentContext.Request.Url.GetLeftPart(UriPartial.Authority);

            return currentUrl + "/Order/ConfirmVNPayPaymentClient";
        }


        public static string UrlGithubLogin()
        {
            string clientIdGh = GetValueFromAppSetting("ClientIdGH");
            string redirectUrl = GetRedirectUrlGH();
            return
                "https://github.com//login/oauth/authorize?client_id=" + clientIdGh + "&redirect_uri=" + redirectUrl + "&scope=user:email";
        }

        public static string GenerateRandomString(int length = 10)
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

        // 30 * 24 * 60 *60 = 30 ngày
        public static string GetVideoLessonUrl(video video, string fileJson, int seconds = 7 * 24 * 60 * 60)
        {
            if (video == null)
            {
                throw new ArgumentNullException(nameof(video));
            }

            string url = GetSignedUrl(video, fileJson, seconds);

            return url;
        }

        //30 * 24 * 60 *60 = 30 ngày
        public static string GetExerciseUrl(exercise exercise, string fileJson, int seconds = 7 * 24 * 60 * 60)
        {
            if (exercise == null)
            {
                throw new ArgumentNullException(nameof(exercise));
            }

            string url = GetSignedUrl(exercise, fileJson, seconds);

            return url;
        }

        public static string GetSignedUrl(object item, string fileJson, int seconds = 7 * 24 * 60 * 60)
        {
            string bucketName;
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            GoogleCredential google = GoogleCredential.FromFile(fileJson);

            if (item is video)
            {
                var video = item as video;
                if (!string.IsNullOrEmpty(video.link) && video.time < DateTime.Now)
                {
                    return video.link;
                }

                bucketName = "video-lesson";
            }
            else if (item is exercise)
            {
                var exercise = item as exercise;
                if (!string.IsNullOrEmpty(exercise.link) && exercise.time < DateTime.Now)
                {
                    return exercise.link;
                }

                bucketName = "exercise-lesson";
            }
            else
            {
                throw new ArgumentException("Unsupported item type");
            }

            UrlSigner urlSigner = UrlSigner.FromCredential(google);
            string url = urlSigner.Sign(
                bucketName,
                (item is video) ? (item as video).name : (item as exercise).name,
                TimeSpan.FromSeconds(seconds),
                HttpMethod.Get);

            using (MyDataDataContext dataContext = new MyDataDataContext())
            {
                if (item is video)
                {
                    var videoTmp = dataContext.videos.FirstOrDefault(x => x.name == (item as video).name);

                    if (videoTmp != null)
                    {
                        videoTmp.link = url;
                        videoTmp.updated_at = DateTime.Now;
                        videoTmp.time = DateTime.Now.AddSeconds(seconds);

                        dataContext.SubmitChanges();
                    }
                }
                else if (item is exercise)
                {
                    var exerciseTmp = dataContext.exercises.FirstOrDefault(x => x.name == (item as exercise).name);

                    if (exerciseTmp != null)
                    {
                        exerciseTmp.link = url;
                        exerciseTmp.updated_at = DateTime.Now;
                        exerciseTmp.time = DateTime.Now.AddSeconds(seconds);

                        dataContext.SubmitChanges();
                    }
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
                lesson previous = lessons.Where(x => x.index == (indexCurrentLesson - 1)).FirstOrDefault();
                if (previous != null)
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

        public static List<string> ReadJsonFromFile(string filePath)
        {
            List<string> listString = new List<string>();

            try
            {
                string jsonText = File.ReadAllText(filePath);

                listString = JsonSerializer.Deserialize<List<string>>(jsonText);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading json file: " + ex.Message);
            }

            return listString;
        }

        //public static string SanitizeInput(string input)
        //{
        //    List<string> sensitiveWords = ReadJsonFromFile("~/sensitive_words.json");

        //    int sensitiveWordCount = 0;
        //    foreach (string word in sensitiveWords)
        //    {
        //        if (Regex.IsMatch(input, @"\b" + word + @"\b", RegexOptions.IgnoreCase))
        //        {
        //            sensitiveWordCount++;
        //        }
        //    }

        //    if (sensitiveWordCount > 3)
        //    {
        //        return "Error: Input contains sensitive words.";
        //    }

        //    foreach (string word in sensitiveWords)
        //    {
        //        input = Regex.Replace(input, @"\b" + word + @"\b", "*");
        //    }

        //    return input;
        //}

        public static bool isBadWord(string input, string json)
        {
            List<string> sensitiveWords = ReadJsonFromFile(json);

            int sensitiveWordCount = 0;
            foreach (string word in sensitiveWords)
            {
                if (Regex.IsMatch(input, @"\b" + word + @"\b", RegexOptions.IgnoreCase))
                {
                    sensitiveWordCount++;
                }
            }

            if (sensitiveWordCount >= 1)
            {
                return true;
            }

            return false;
        }

        public static string GetDeviceFingerprint()
        {
            HttpContext context = HttpContext.Current;
            string userAgent = context.Request.UserAgent;
            string ipAddress = context.Request.UserHostAddress;
            string screenWidth = context.Request.Browser.ScreenPixelsWidth.ToString();
            string screenHeight = context.Request.Browser.ScreenPixelsHeight.ToString();
            string timeZone = TimeZoneInfo.Local.DisplayName;
            string deviceFingerprint = $"{userAgent}_{ipAddress}_{screenWidth}_{screenHeight}_{timeZone}";

            return deviceFingerprint;
        }

        // get item in cart
        public static List<int> GetItem(string base64String)
        {
            byte[] data = Convert.FromBase64String(base64String);
            string decodedString = Encoding.UTF8.GetString(data);
            return decodedString.Split(';').Select(int.Parse).ToList();
        }



        public static string SlugToString(string slug)
        {
            if (string.IsNullOrEmpty(slug))
                return "";
            string result = slug.Replace("-", " ");
            result = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(result);
            return result.ToLower().Trim();
        }

        public static string StringToSlug(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            string slug = input.ToLower().Trim();
            slug = Regex.Replace(slug, @"[^a-z0-9\s-+#]", "");
            slug = Regex.Replace(slug, @"\s+", "-");
            slug = Regex.Replace(slug, @"-+", "-");
            return slug;
        }


        private static HashSet<SelectListItem> GetLanguageSelectListItem()
        {
            return (new HashSet<SelectListItem>
            {
                new SelectListItem { Text = "English", Value = "EN"},
                new SelectListItem { Text = "Vietnamese", Value = "VI"}
            });
        }

        public static SelectList GetLanguageDictionaryElement()
        {
            int previouslySelectedLanguageIndex = 1;
            HttpCookie languageCookie = HttpContext.Current.Request.Cookies["Language"];

            HashSet<SelectListItem> list = GetLanguageSelectListItem();

            if (languageCookie?.Value != null)
            {
                foreach (var item in list.Where(item => item.Text == languageCookie.Value))
                {
                    previouslySelectedLanguageIndex = Int32.Parse(item.Value);
                    break;
                }
            }

            return new SelectList(list, "Value", "Text", previouslySelectedLanguageIndex);
        }
    }
}