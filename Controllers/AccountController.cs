using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json.Linq;
using MimeKit;
using Octokit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Razor.Tokenizer.Symbols;
using System.Web.Services.Description;
using Website_Course_AVG.Managers;
using Website_Course_AVG.Models;
using System.Runtime.Caching;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using Website_Course_AVG.Managers;

namespace Website_Course_AVG.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private readonly MyDataDataContext _context = new MyDataDataContext();

        public AccountController()
        {

        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [Website_Course_AVG.Attributes.AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Website_Course_AVG.Attributes.AllowAnonymous]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            var userManager = new Website_Course_AVG.Managers.UserManager();
            if (userManager.IsAuthenticated())
            {
                Helpers.AddCookie("Error", ResourceHelper.GetResource("You are logging in !!!"));
                return View(model);
            }
            if (ModelState.IsValid)
            {
                account account = _context.accounts.Where(x => x.username == model.userName).FirstOrDefault();

                if (account == null)
                {
                    Helpers.AddCookie("Error", "username and password are wrong !!!");
                    return View(model);
                }


                bool isVerify = await userManager.ValidatePasswordAsync(account, model.Password);
                if (isVerify)
                {
                    user user = account.users.FirstOrDefault();
                    if (user == null)
                    {
                        Helpers.AddCookie("Error", "username and password are wrong !!!");
                        return View(model);
                    }

                    userManager.login(account.username);
                    Helpers.AddCookie("Notify", ResourceHelper.GetResource("Login Successful!"));
                    return RedirectToAction("Index", "Home");
                }

                Helpers.AddCookie("Error", ResourceHelper.GetResource("Username and password are wrong !!!"));
                return View(model);
            }

            Helpers.AddCookie("Error", ResourceHelper.GetResource("Error Unknown, Please Try Again!"));
            return View(model);
        }

        //
        // GET: /Account/VerifyCode
        [Website_Course_AVG.Attributes.AllowAnonymous]
        public ActionResult VerifyCode()
        {
            //// Require that the user has already logged in via username/password or external login
            //if (!await SignInManager.HasBeenVerifiedAsync())
            //{
            //    return View("Error");
            //}
            //return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
            return View();
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [Website_Course_AVG.Attributes.AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            //if (!ModelState.IsValid)
            //{
            //    return View(model);
            //}

            //// The following code protects for brute force attacks against the two factor codes. 
            //// If a user enters incorrect codes for a specified amount of time then the user account 
            //// will be locked out for a specified amount of time. 
            //// You can configure the account lockout settings in IdentityConfig
            //var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            //switch (result)
            //{
            //    case SignInStatus.Success:
            //        return RedirectToLocal(model.ReturnUrl);
            //    case SignInStatus.LockedOut:
            //        return View("Lockout");
            //    case SignInStatus.Failure:
            //    default:
            //        ModelState.AddModelError("", "Invalid code.");
            //        return View(model);
            //}
            return View(model);
        }

        //
        // GET: /Account/Register
        [Website_Course_AVG.Attributes.AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [Website_Course_AVG.Attributes.AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            var UserManager = new Website_Course_AVG.Managers.UserManager();
            if (ModelState.IsValid)
            {

                //if user have not ever register before
                account account = new account();
                account.username = model.userName;
                account.password = model.Password;
                var result = await UserManager.CreateAccountUserAsync(model.userName, account, model.Email);
                if (result.Succeeded)
                {
                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");					
                    if (UserManager.SendEmailAsync(model.Email, "Verify Email", "Your code to verify email is: ", Helpers.GenerateRandomString(10), "Register", "verifyEmail").Result)
                    {
                        Helpers.AddCookie("Notify", "Access email to verify");
                        
                    }
                    //UserManager.login(model.userName);
                    return View(model);
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [Website_Course_AVG.Attributes.AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [Website_Course_AVG.Attributes.AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword

        [HttpPost]
        [Website_Course_AVG.Attributes.AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel forgotPassword, String returnUrl)
        {
            user user = _context.users.Where(x => x.email == forgotPassword.Email).FirstOrDefault();

            if (user == null)
            {
                Helpers.AddCookie("Error", ResourceHelper.GetResource("Error Unknown"));
                return RedirectToAction("Index", "Home");
            }
            int countForgotPassword = user.forgot_passwords.Where(x => x.created_at >= DateTime.Now.AddMinutes(-30)).Count();

            if (countForgotPassword > 3)
            {
                Helpers.AddCookie("Error", ResourceHelper.GetResource("We noticed that you pressed forgot password too many times in one day, please try again after 30 minutes, thank you"));
                return RedirectToAction("Index", "Home");
            }

            var userManager = new UserManager();
            var subject = ResourceHelper.GetResource("AVG Courses - Reset Password");

            String messageHead = ResourceHelper.GetResource("Your password recovery code is");
            String messageLast = Helpers.GenerateRandomString(10);
            if (!userManager.IsAuthenticated())
            {
                if (await userManager.SendEmailAsync(forgotPassword.Email, subject, messageHead + messageLast, messageLast, "ForgotPassword", "ResetPassword") == false)
                {
                    return View();
                }
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }
            Helpers.AddCookie("Error", ResourceHelper.GetResource("Has Error"));
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [Website_Course_AVG.Attributes.AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //Reset Password
        [Website_Course_AVG.Attributes.AllowAnonymous]
        public ActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        [Website_Course_AVG.Attributes.AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            UserManager userManager = new UserManager();
            if (ModelState.IsValid)
            {
                Helpers.AddCookie("Error", ResourceHelper.GetResource("You enter error Code or Re-password"));
                return View();
            }
            if (!userManager.ResetPassword(model.Password, model.Email, model.Code)) return View(model);
            Helpers.AddCookie("Notify", ResourceHelper.GetResource("Reset Password Successful!"));
            return RedirectToAction("ResetPasswordConfirmation", "Account");
        }


        [Website_Course_AVG.Attributes.AllowAnonymous]
        public ActionResult EmailConfirmation()
        {
            return View();
        }

        [Website_Course_AVG.Attributes.AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [Website_Course_AVG.Attributes.AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback
        [Website_Course_AVG.Attributes.AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                Helpers.AddCookie("Error", ResourceHelper.GetResource("Error Unknown, Please Try Again!"));
                return RedirectToAction("Login");
            }

            var UserManager = new Website_Course_AVG.Managers.UserManager();

            if (UserManager.CheckUsername(loginInfo.DefaultUserName))
            {
                Helpers.AddCookie("Notify", ResourceHelper.GetResource("Login Successful!"));
                UserManager.login(loginInfo.DefaultUserName);
                return RedirectToAction("Index", "Home");
            }
            else if (UserManager.CheckUsername(loginInfo.Email))
            {
                user user = _context.users.Where(x => x.email == loginInfo.Email).FirstOrDefault();

                Helpers.AddCookie("Notify", ResourceHelper.GetResource("Login with Gmail Successful!"));
                UserManager.login(user.account.username);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ReturnUrl = returnUrl;
            ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
            ViewBag.Email = loginInfo.Email;
            return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [Website_Course_AVG.Attributes.AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model,
            string returnUrl, string Email, string loginProvider = "", string username = "", string provideKey = "")
        {
            var UserManager = new Website_Course_AVG.Managers.UserManager();

            if (UserManager.IsAuthenticated())
            {
                Helpers.AddCookie("Error", ResourceHelper.GetResource("You are logging in !!!"));
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();

                if (loginProvider == "Github")
                {
                    info = new ExternalLoginInfo();
                    info.Email = username;
                    info.DefaultUserName = username;
                    info.Login = new UserLoginInfo(loginProvider, provideKey);
                }

                if (info == null && loginProvider != "Github")
                {
                    Helpers.AddCookie("Error", ResourceHelper.GetResource("Error Unknow, Please Try Again!"));
                    return View("ExternalLoginFailure");
                }

                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };

                if (!UserManager.CheckUsername(info.Email))
                {
                    var userAccount = new account();
                    if (info.Email == null && (info.Login.LoginProvider == "Twitter" || info.Login.LoginProvider == "Github"))
                    {
                        info.Email = Email;
                        userAccount.username = info.DefaultUserName;
                    }
                    else
                        userAccount.username = info.Email;
                    userAccount.provide = info.Login.LoginProvider;
                    userAccount.provide_id = info.Login.ProviderKey;

                    var result = await UserManager.CreateAccountUserAsync(info.DefaultUserName, userAccount, info.Email);

                    if (result.Succeeded)
                    {
                        UserManager.login(info.Email);
                        Helpers.AddCookie("Notify", ResourceHelper.GetResource("Login Successful!"));
                        return RedirectToLocal(returnUrl);
                    }
                    AddErrors(result);
                    Helpers.AddCookie("Error", ResourceHelper.GetResource("Error Unknow, Please Try Again!"), 30);
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        [Website_Course_AVG.Attributes.Authorize]
        public ActionResult LogOff()
        {
            var UserManager = new Website_Course_AVG.Managers.UserManager();
            UserManager.logout();

            Helpers.AddCookie("Notify", ResourceHelper.GetResource("Logout Successful!"), 5);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [Website_Course_AVG.Attributes.AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion

        #region Github


        public async Task<ActionResult> GithubLogin(string code)
        {
            var client = new HttpClient();
            var parameters = new Dictionary<string, string>
            {
                { "client_id", Helpers.GetValueFromAppSetting("ClientIdGH") },
                { "client_secret", Helpers.GetValueFromAppSetting("ClientSecretGH") },
                { "code", code },
                //{ "redirect_uri", Helpers.GetRedirectUrlGH() }
            };
            var content = new FormUrlEncodedContent(parameters);
            var response = await client.PostAsync("https://github.com/login/oauth/access_token", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            var values = HttpUtility.ParseQueryString(responseContent);
            var accessToken = values["access_token"];
            string idGithub = Helpers.GetValueFromAppSetting("IdGH");
            var client1 = new GitHubClient(new Octokit.ProductHeaderValue(idGithub));
            var tokenAuth = new Credentials(accessToken);
            client1.Credentials = tokenAuth;
            var user = await client1.User.Current();
            var login = user.Login;
            var provideKey = user.Id.ToString();
            if (login == null) return RedirectToAction("Login");

            var UserManager = new Website_Course_AVG.Managers.UserManager();

            if (UserManager.CheckUsername(login))
            {
                Helpers.AddCookie("Notify", ResourceHelper.GetResource("Login Successful!"));
                UserManager.login(login);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.LoginProvider = "Github";
            ViewBag.Email = user.Email;
            ViewBag.login = login;
            ViewBag.provideKey = provideKey;
            return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = user.Email });
        }

        #endregion

        public ActionResult SendCode(string returnurl, bool rememberme)
        {
            throw new System.NotImplementedException();
        }

        public ActionResult verifyEmail()
        {
            return View();
        }

        [HttpPost]
        public ActionResult verifyEmail(verifyEmail model)
        {
            UserManager userManager = new UserManager();
            user user = _context.users.Where(x => x.email == model.Email).First();
            //_context.DeferredLoadingEnabled = false;
            int countForgotPassword = user.forgot_passwords.Where(x => x.created_at >= DateTime.Now.AddMinutes(-30)).Count();

            if (countForgotPassword > 3)
            {
                Helpers.AddCookie("Error", "We noticed that you received code too many times in one day, please try again after 30 minutes, thank you");
                return View(model);
            }

            forgot_password _forgot = _context.forgot_passwords.Where(x => x.code == model.code).FirstOrDefault();
            if (_forgot == null)
            {
                Helpers.AddCookie("Error", "You entered wrong code!!!");
                return View(model);
            }
            if (!userManager.IsAuthenticated())
            {
                Helpers.AddCookie("Notify", "Check Email successfull");
                account account = _context.accounts.Where(x => x.id == user.account_id).FirstOrDefault();
                userManager.login(account.username);
                Helpers.AddCookie("Notify", "Login successfull");
                return RedirectToAction("Index", "Home");
            }
            Helpers.AddCookie("Error", "Has Error");
            return View(model);
        }

        private string RemoveChar(string text)
        {
            string word = text.Normalize(NormalizationForm.FormD);
            StringBuilder ketQua = new StringBuilder();

            foreach (char letter in word)
            {
                UnicodeCategory theLoai = CharUnicodeInfo.GetUnicodeCategory(letter);
                if (theLoai != UnicodeCategory.NonSpacingMark)
                {
                    ketQua.Append(letter);
                }
            }

            return ketQua.ToString().Normalize(NormalizationForm.FormC);
        }

        public ActionResult Autocomplete(string keyword)
        {
            string keywordWithoutDiacritics = RemoveChar(keyword).ToLower();

            Dictionary<string, string> wordDictionary = new Dictionary<string, string>();

            // Thử lấy dữ liệu từ cache trước
            var cachedData = MemoryCache.Default.Get("cachedData_" + keywordWithoutDiacritics);
            if (cachedData != null)
            {
                return Json((List<string>)cachedData, JsonRequestBehavior.AllowGet);
            }

            foreach (var sach in _context.courses)
            {
                string tensachWithoutDiacritics = RemoveChar(sach.title).ToLower();
                if (!wordDictionary.ContainsKey(tensachWithoutDiacritics))
                {
                    wordDictionary.Add(tensachWithoutDiacritics, sach.title);
                }
            }

            var suggestions = wordDictionary
                                .Where(entry => entry.Key.Contains(keywordWithoutDiacritics) || entry.Value.Contains(keyword))
                                .Select(entry => entry.Value)
                                .Take(5)
                                .ToList();

            // Lưu dữ liệu vào cache
            MemoryCache.Default.Add("cachedData_" + keywordWithoutDiacritics, suggestions, DateTimeOffset.Now.AddMinutes(10));

            return Json(suggestions, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ProfieUser()
        {
            user user = Helpers.GetUserFromToken();
            ProfieUser model = new ProfieUser()
            {
                gender = user.gender == true ? "female" : " male",
                birthday = user.birthday,
                fullName = user.fullname,
                Password = "",
                ConfirmPassword = "",
                email = user.email
            };
            return View(model);
        }

        [HttpPost]
        [Website_Course_AVG.Attributes.User]
        public ActionResult ProfieUser(ProfieUser model)
        {
            UserManager userManager = new UserManager();
            user userFromToken = Helpers.GetUserFromToken();
            user user = _context.users.Where(x => x.email == userFromToken.email).First();


            if (model.ConfirmPassword != model.Password)
            {
                Helpers.AddCookie("invalid", "Confirm Password is not equal Password");
                return View(model);
            }

            if (model.birthday >= DateTime.Now)
            {
                Helpers.AddCookie("Error", "Your birthday must smaller nowaday!!");
                return View(model);
            }

            user.fullname = model.fullName;
            if (!Regex.IsMatch(model.fullName, @"^.{6,30}$"))
            {
                Helpers.AddCookie("Error", "Your email is not valid!!");
                return View(model);
            }
            user.account.username = model.fullName;
            user.birthday = model.birthday;
            user.gender = model.gender == "Male" ? false : true;
            bool isChange = false;

            if (model.Password.Length > 0)
            {
                //VALIDATE
                if (Regex.IsMatch(model.Password.ToString(), @"^.{6,30}$"))
                {
                    user.account.password = BCrypt.Net.BCrypt.HashPassword(model.Password);
                    isChange = true;
                }
            }

            _context.SubmitChanges();
            if (isChange)
            {
                userManager.logout();
                Helpers.AddCookie("Notify", "Logout Successful", 5);
                return RedirectToAction("Index", "Home");
            }

            Helpers.AddCookie("Error", "Has Error", 5);
            return View(model);
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}