using Octokit;
using System;
using System.Linq;
using System.Text;
using System.Web;
using Website_Course_AVG.Models;

namespace Website_Course_AVG.Managers
{
    public partial class Helpers
    {
        public static void AddCookie(string key, string value, int second = 10)
        {
            HttpCookie cookie = new HttpCookie(key, value);
            cookie.Expires = DateTime.Now.AddSeconds(second);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public static bool IsAuthenticated()
        {
            UserManager userManager = new UserManager();
            return userManager.IsUser();
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

        public static string GenerateString(int length = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder sb = new StringBuilder(length);
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                sb.Append(chars[random.Next(chars.Length)]);
            }

            return sb.ToString();
        }

        public static user GetUserFromToken()
        {
            UserManager userManager = new UserManager();
            return userManager.GetUserFromToken();
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