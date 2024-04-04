using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using Website_Course_AVG.Models;
using System.IO;
using System.Text.RegularExpressions;
using Website_Course_AVG.Models;
using System.Security.Principal;

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
            return global::System.Configuration.ConfigurationManager.AppSettings[key];
        }

        public static string GetRedirectUrlGH()
        {
            HttpContext currentContext = HttpContext.Current;
            string currentUrl = currentContext.Request.Url.GetLeftPart(UriPartial.Authority);

            return currentUrl + "/Account/GithubLogin";
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

        public static List<string> ReadJsonFromFile(string filePath)
        {
            List<string> sensitiveWords = new List<string>();

            try
            {
                string jsonText = File.ReadAllText(filePath);

                sensitiveWords = JsonSerializer.Deserialize<List<string>>(jsonText);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading json file: " + ex.Message);
            }

            return sensitiveWords;
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

            // Tạo dấu vân tay bằng cách kết hợp các thuộc tính
            string deviceFingerprint = $"{userAgent}_{ipAddress}_{screenWidth}_{screenHeight}_{timeZone}";

            // Lưu hoặc xử lý dấu vân tay ở đây (ví dụ: lưu vào CSDL, so sánh với dấu vân tay đã lưu, ...)

            return deviceFingerprint;
        }
    }
}