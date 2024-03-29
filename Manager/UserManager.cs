using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Website_Course_AVG.Managers;
using Website_Course_AVG.Models;
using Octokit;
using System.Runtime.ConstrainedExecution;
using MailKit.Net.Smtp;
using MimeKit;
using Org.BouncyCastle.Math.Field;
using System.Web.Mvc;
using System.Security.Policy;
using System.IO;
using System.Web.Routing;
using Website_Course_AVG.Controllers;
using Microsoft.Ajax.Utilities;
using System.Configuration;
using System.Web.UI;

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
                    account.password = "12345678";

                if (!_data.users.Where(x=> x.email ==email).Any() || !_data.users.Where(x => x.fullname == account.username).Any())
                {
					string password = BCrypt.Net.BCrypt.HashPassword(account.password);
					account.password = password;
					_data.accounts.InsertOnSubmit(account);
					_data.SubmitChanges();

					account accountTmp = _data.accounts.Where(x => x.username == account.username).First();

					user user = new user();
					user.fullname = fullname;
					user.email = email;
					user.account_id = accountTmp.id;
					_data.users.InsertOnSubmit(user);
					_data.SubmitChanges();
			
					return IdentityResult.Success;
				}


                Helpers.addCookie("Error", "This email or username already belong to one account");
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
            MyDataDataContext _data1 = new MyDataDataContext();
            string token = HttpContext.Current.Request.Cookies["AuthToken"]?.Value;
            string username = TokenHelper.GetUsernameFromToken(token);
            user user = _data1.users.Where(x => x.email == username).FirstOrDefault();
            return user;
        }

        public bool IsAuthenticated()
        {
            string authToken = HttpContext.Current.Request.Cookies["AuthToken"]?.Value;
            return !string.IsNullOrEmpty(authToken);
        }

        //role = 1 : user
        public bool IsUser()
        {
            if (!IsAuthenticated())
                return false;
            user user = GetUserFromToken();
            return user != null;
        }

        // role = 2 : admin
        public bool IsAdmin()
        {
            if (!IsUser())
                return false;
            user user = GetUserFromToken();
            return user.role > 1;
        }

        public void login(string email)
        {
            var token = TokenHelper.GenerateToken(email);
            HttpCookie cookie = new HttpCookie("AuthToken", token);
            cookie.Expires = DateTime.Now.AddDays(30);
            HttpContext.Current.Response.Cookies.Add(cookie);
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

        public async Task<bool> ValidatePasswordAsync(user user, string password)
        {
            // Thêm code để kiểm tra mật khẩu khớp với mật khẩu được lưu trữ hay không

            return false;
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
		public async Task<bool> SendEmailAsync(string toEmail, string subject, string message, string messageLast)
		{
			string ourMail = ConfigurationManager.AppSettings["OurMail"];
			string password = ConfigurationManager.AppSettings["Password"];

			user user = _data.users.Where(x => x.email == toEmail).FirstOrDefault();
            if (user == null)
            {
				Helpers.addCookie("Error", "Have not have email yet");
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
			emailConfirmation.RedirectURL = currentUrl.Replace("ForgotPassword", "ResetPassword");

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

        public bool resetPassword(String newPassword, String toEmail, String code)
        {
			user user = _data.users.FirstOrDefault(x => x.email == toEmail);

			if (user == null)
			{
				Helpers.addCookie("Error", "You have not already sign up");
                return false;
			}

			forgot_password forgot_Password = _data.forgot_passwords.FirstOrDefault(x => x.user_id == user.id);
			if (forgot_Password == null)
			{
				Helpers.addCookie("Error", "You have not already sign up");
                return false;
			}
            if(forgot_Password.expired_date <= DateTime.Now)
            {
				Helpers.addCookie("Error", "This code is expired");
				return false;
			}
			try
            {
				if (code == forgot_Password.code)
				{
					account account = _data.accounts.Where(x => x.username == user.email).FirstOrDefault();
					if (account != null)
					{

						account.password = newPassword;
						forgot_Password.type = true;
						_data.SubmitChanges();
					}

				}

                return true;
            }
            catch (Exception ex)
            {
				Helpers.addCookie("Error", ex.Message);
                return false; 
			}

        }
    }
}
