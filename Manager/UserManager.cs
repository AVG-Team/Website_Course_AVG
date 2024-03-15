using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Website_Course_AVG.Manager;
using Website_Course_AVG.Models;

namespace Website_Course_AVG.Managers
{
    public class UserManager
    {
        private readonly MyDataDataContext _data = new MyDataDataContext();

        public UserManager()
        {
        }

        public async Task<IdentityResult> CreateAccountUserAsync(string fullname, account account)
        {
            if (account.username == null)
                return IdentityResult.Failed();

            try
            {
                if (account.password == null)
                    account.password = "12345678";
                string password = BCrypt.Net.BCrypt.HashPassword(account.password);
                account.password = password;
                _data.accounts.InsertOnSubmit(account);
                _data.SubmitChanges();

                account accountTmp = _data.accounts.Where(x => x.username == account.username).First();

                user user = new user();
                user.fullname = fullname;
                user.email = account.username;
                user.account_id = accountTmp.id;
                _data.users.InsertOnSubmit(user);
                _data.SubmitChanges();
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed();
            }
        }

        //true : exist ; false : no exist
        public bool CheckUsername(string username)
        {
            user user = _data.users.Where(x => x.email == username).FirstOrDefault();
            if (user == null)
                return false;
            account account = _data.accounts.Where(x => x.username == username).FirstOrDefault();
            if (account == null)
                return false;

            return true;
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
    }
}
