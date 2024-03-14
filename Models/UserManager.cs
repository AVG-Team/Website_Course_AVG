using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Website_Course_AVG.Models
{
    public class UserManager
    {
        private readonly MyDataDataContext _data;

        public UserManager()
        {
            _data = new MyDataDataContext();
        }

        public async Task<IdentityResult> CreateUserAsync(string fullname, account account)
        {
            if (account.username == null)
                return IdentityResult.Failed();

            int id = 0;

            try
            {
                var tmp = _data.accounts.OrderByDescending(p => p.id).FirstOrDefault();
                if (tmp == null)
                    id = 1;
                else
                    id = tmp.id + 1;
                if (account.password == null)
                    account.password = "12345678";
                string password = BCrypt.Net.BCrypt.HashPassword(account.password);
                account accountTmp = new account();
                accountTmp = account;
                accountTmp.password = password;
                accountTmp.id = id;
                _data.accounts.InsertOnSubmit(accountTmp);

                user user = new user();
                user.fullname = fullname;
                user.email = account.username;
                user.account_id = id;
                _data.users.InsertOnSubmit(user);

                _data.SubmitChanges();
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed();
            }
        }


        public async Task<user> FindByNameAsync(string username)
        {
            // Thêm code để tìm user theo username từ database (sử dụng _connection để truy vấn dữ liệu)

            return null; // Trả về null nếu không tìm thấy user
        }

        public async Task<bool> ValidatePasswordAsync(user user, string password)
        {
            // Thêm code để kiểm tra mật khẩu khớp với mật khẩu được lưu trữ hay không

            return false;
        }
    }
}
