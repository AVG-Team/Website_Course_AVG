using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Website_Course_AVG.Models;
using MailKit.Net.Smtp;
using MimeKit;
using System.Web.Mvc;
using System.IO;
using System.Web.Routing;
using Website_Course_AVG.Controllers;
using System.Configuration;
using Octokit;
using System.Web.Helpers;

namespace Website_Course_AVG.Managers
{
    public class UserManager
    {
        private readonly MyDataDataContext _data = new MyDataDataContext();

        public UserManager()
        {
        }

        public async Task<IdentityResult> CreateAccountUserAsync(string fullname, account account, string email)
        {
            if (account.username == null)
                return IdentityResult.Failed();

            try
            {
                if (account.password == null)
                    account.password = Helpers.GenerateRandomString();

                if (!_data.users.Where(x => x.email == email).Any())
                {
                    string password = BCrypt.Net.BCrypt.HashPassword(account.password);
                    account.password = password;
                    account.created_at = DateTime.Now;
                    account.updated_at = DateTime.Now;
                    _data.accounts.InsertOnSubmit(account);
                    _data.SubmitChanges();

                    account accountTmp = _data.accounts.Where(x => x.username == account.username).First();

                    user user = new user();
                    user.fullname = fullname;
                    user.email = email;
                    user.account_id = accountTmp.id;
                    user.created_at = DateTime.Now;
                    user.updated_at = DateTime.Now;
                    _data.users.InsertOnSubmit(user);
                    _data.SubmitChanges();

                    return IdentityResult.Success;
                }


                Helpers.AddCookie("Error", "This email or username already belong to one account");
                return IdentityResult.Failed();

            }
            catch (Exception ex)
            {
                return IdentityResult.Failed();
            }
        }

        //true : exist ; false : no exist
        public bool CheckUsername(string username)
        {
            bool flag = false;
            account account = _data.accounts.Where(x => x.username == username).FirstOrDefault();
            if (account != null)
                flag = true;
            user user = _data.users.Where(x => x.email == username).FirstOrDefault();
            if (user != null)
                flag = true;

            return flag;
        }

        public user GetUserFromToken()
        {
            using (MyDataDataContext _data1 = new MyDataDataContext())
            {
                if (HttpContext.Current == null || HttpContext.Current.Request.Cookies["AuthToken"] == null)
                {
                    return null;
                }

                string token = HttpContext.Current.Request.Cookies["AuthToken"].Value;
                string username = TokenHelper.GetUsernameFromToken(token);

                try
                {
                    account account = _data1.accounts.Where(x => x.username == username).FirstOrDefault();
                    return account?.users.FirstOrDefault();
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public bool IsAuthenticated()
        {
            string authToken = HttpContext.Current.Request.Cookies["AuthToken"]?.Value;
            return !string.IsNullOrEmpty(authToken) && GetUserFromToken() != null;
        }

        //role = 1 : user
        public bool IsUser()
        {
            if (!IsAuthenticated())
                return false;
            user user = GetUserFromToken();
            return user != null && (user.role == null || user.role <= 1);
        }

        // role = 2 : admin
        public bool IsManager()
        {
            if (!IsAuthenticated())
                return false;
            user user = GetUserFromToken();
            return user != null && user.role > 1;
        }

        // role = 3 : admin
        public bool IsAdmin()
        {
            if (!IsAuthenticated())
                return false;
            user user = GetUserFromToken();
            return user != null && user.role == 3;
        }

        public void login(string username)
        {
            using (MyDataDataContext _data = new MyDataDataContext())
            {
                try
                {
                    var token = TokenHelper.GenerateToken(username);

                    account account = _data.accounts.Where(x => x.username == username).FirstOrDefault();
                    if (account != null)
                    {
                        account.info = Helpers.GetDeviceFingerprint();
                        account.token = token;
                        account.updated_at = DateTime.Now;
                        _data.SubmitChanges();

                        HttpCookie cookie = new HttpCookie("AuthToken", token);
                        cookie.Expires = DateTime.Now.AddDays(30);
                        HttpContext.Current.Response.Cookies.Add(cookie);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public void logout()
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies["AuthToken"];
            if (cookie != null)
            {
                cookie.Expires = DateTime.Now.AddDays(-1);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }

        public async Task<bool> ValidatePasswordAsync(account account, string password)
        {
            if (account == null || string.IsNullOrEmpty(password))
            {
                return false;
            }

            return BCrypt.Net.BCrypt.Verify(password, account.password);
        }


        protected string RenderViewToString(String controllerName, string viewName, object viewData)
        {
            using (var writer = new StringWriter())
            {
                var routeData = new RouteData();
                routeData.Values.Add("controller", controllerName);
                var fakeControllerContext = new ControllerContext(new HttpContextWrapper(new HttpContext(new HttpRequest(null, "http://google.com", null), new HttpResponse(null))), routeData, new AccountController());
                var razorViewEngine = new RazorViewEngine();
                var razorViewResult = razorViewEngine.FindView(fakeControllerContext, viewName, "", false);

                var viewContext = new ViewContext(fakeControllerContext, razorViewResult.View, new ViewDataDictionary(viewData), new TempDataDictionary(), writer);
                razorViewResult.View.Render(viewContext, writer);
                return writer.ToString();

            }
        }
        public async Task<bool> SendEmailAsync(string toEmail, string subject, string message, string messageLast, string fromModel, string toModel)
        {
            string ourMail = Helpers.GetValueFromAppSetting("OurMail");
            string password = Helpers.GetValueFromAppSetting("Password");

            if (ourMail == null || password == null)
                return false;

            user user = _data.users.Where(x => x.email == toEmail).FirstOrDefault();
            if (user == null)
            {
                Helpers.AddCookie("Error", "Have not have email yet");
                return false;
            }
            forgot_password forgot_Password = new forgot_password();
            forgot_Password.user_id = user.id;
            forgot_Password.code = messageLast;
            forgot_Password.type = false;
            forgot_Password.created_at = DateTime.Now;
            forgot_Password.expired_date = DateTime.Now.AddMinutes(30);
            _data.forgot_passwords.InsertOnSubmit(forgot_Password);
            _data.SubmitChanges();


            var messageBody = new MimeMessage();
            messageBody.From.Add(new MailboxAddress("AVG_Team", ourMail));
            messageBody.To.Add(new MailboxAddress("", toEmail));
            messageBody.Subject = subject;
            var bodyBuilder = new BodyBuilder();

            EmailConfirmation emailConfirmation = new EmailConfirmation();
            emailConfirmation.Code = messageLast;
            string currentUrl = HttpContext.Current.Request.Url.AbsoluteUri;
            string urlWebsite = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
            emailConfirmation.RedirectURL = currentUrl.Replace(fromModel, toModel);
            emailConfirmation.UrlWebsite = urlWebsite;
            emailConfirmation.Name = user.fullname;


            bodyBuilder.HtmlBody = RenderViewToString("Account", "EmailConfirmation", emailConfirmation);

            messageBody.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, false);
                client.Authenticate(ourMail, password);
                client.Send(messageBody);
                client.Disconnect(true);
            }
            return true;
        }

        public bool ResetPassword(String newPassword, String toEmail, String code)
        {
            forgot_password forgot_Password = _data.forgot_passwords.FirstOrDefault(x => x.code == code);

            if (forgot_Password == null || forgot_Password.type == true || forgot_Password.expired_date < DateTime.Now)
            {
                Helpers.AddCookie("Error", "The code is incorrect or has been used or has expired, please try again");
                return false;
            }

            user user = _data.users.FirstOrDefault(x => x.id == forgot_Password.user_id);
            if (user == null)
            {
                Helpers.AddCookie("Error", "You have not already sign up");
                return false;
            }

            forgot_password forgot_PasswordCheck = user.forgot_passwords.OrderByDescending(x => x.created_at).First();
            if (forgot_PasswordCheck.code != code)
            {
                Helpers.AddCookie("Error", "You are using old code, please check the latest email, thank you");
                return false;
            }

            account account = user.account;
            try
            {
                if (account != null)
                {
                    account.password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                    forgot_Password.type = true;
                    _data.SubmitChanges();
                    return true;
                }
                Helpers.AddCookie("Error", "Error Unknow, Please try again");
                return false;
            }
            catch (Exception ex)
            {
                Helpers.AddCookie("Error", ex.Message);
                return false;
            }

        }

    }
}