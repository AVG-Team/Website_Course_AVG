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

		public async Task<bool> SendEmailAsync(string toEmail, string subject, string message, string messageLast)
		{
            var ourMail = "khai.nguyenanh03@gmail.com";
            var password = "oadp iffv mefs kbag";

			string resetPasswordLink = "https://localhost:44331/Account/ResetPassword"; // Đường dẫn tới trang 

			string body = $"<a href=\"{resetPasswordLink}\" style=\"color: blue; text-decoration: underline;\">Click here to reset your password.</a> ";

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

			bodyBuilder.HtmlBody = @"
                                    <html>
                                    <body>
                                        <div class=""width:100%; height: 100%"">
                                            <table width=""95%"" border=""0"" align=""center"" cellpadding=""0"" cellspacing=""0""
                                                style=""max-width:650px;background:#fff; border-radius:3px; text-align:left; padding-top: 50px;"">
                                                <tr>
                                                    <td style=""text-align:center; "">
                                                        <a href=""https://rakeshmandal.com"" title=""logo"" target=""_blank"">
                                                            <img style=""width: 60px; border-radius: 50%; display: block; border: 3px black;"" src=""Website_Course_AVG/Content/img/logo/avg_team.png"" title=""logo"" alt=""logo"">
                                                        </a>
                                                        <span
                                                            style=""display:inline-block; vertical-align:middle;margin-top:26px; margin-bottom:16px; border-bottom:1px solid #cecece; width:90%;""></span>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style=""padding:0 35px; justify-content:left;"">
                                                        <p style=""font-weight:bold; margin-bottom:10;font-family:'Rubik',sans-serif; "">xin chào bug,
                                                            <br>
                                                            <br>
                                                            Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu ở trang web của chúng tôi.
                                                            <br>
                                                            Nhập mã đặt lại mật khẩu sau đây:
                                                        </p>
                                                        <div style=""background-color: #e7f3ff; border: 2px solid #c4ddfb; border-radius: 5px; padding: 20px; width: 100px; margin-bottom: 26px;"">
                                                            <p style=""color:black; font-size: 16px; line-height: 1.5; margin: 0;"">" + messageLast + @"</p>
                                                        </div>
                                                        <a href=""" + resetPasswordLink + @"""
                                                        style=""width:400px; text-align:center;background:#1878f3;text-decoration:none !important; font-weight:500; margin-top:35px; color:#fff; font-size:14px;padding:10px 24px;display:inline-block;border-radius:10px;"">Nhấn vào đây để đổi mật khẩu</a>
                                                        <span
                                                            style=""display:inline-block; vertical-align:middle;margin-top:26px; margin-bottom:26px; border-bottom:1px solid #cecece; width:100%;""></span>
                                                    </td>
                                                </tr>
                                            </table>       
                                        </div>
                                    </body>
                                    </html>";



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
