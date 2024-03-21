using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Octokit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using Website_Course_AVG.Managers;
using Website_Course_AVG.Models;

namespace Website_Course_AVG.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
		private readonly MyDataDataContext _context;

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
        [Website_Course_AVG.Attributes.AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
			var userManager = new Website_Course_AVG.Managers.UserManager();
			if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    user user = userManager.GetUserFromToken();
                    userManager.login(user.email);
                    Helpers.addCookie("Notify", "Login Successful");
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [Website_Course_AVG.Attributes.AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [Website_Course_AVG.Attributes.AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
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

				//if user is already log in



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
					Helpers.addCookie("Notify", "Register Successful");
					UserManager.login(model.Email);
                    return RedirectToAction("Index", "Home");
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
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [Website_Course_AVG.Attributes.AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
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
                Helpers.addCookie("Error", "Error Unknow, Please Try Again");
                return RedirectToAction("Login");
            }

            var UserManager = new Website_Course_AVG.Managers.UserManager();

            if (UserManager.CheckUsername(loginInfo.DefaultUserName))
            {
                Helpers.addCookie("Notify", "Login Successful");
                UserManager.login(loginInfo.DefaultUserName);
                return RedirectToAction("Index", "Home");
            }
            else if (UserManager.CheckUsername(loginInfo.Email))
            {
                Helpers.addCookie("Notify", "Login Successful");
                UserManager.login(loginInfo.Email);
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
                Helpers.addCookie("Error", "You are logging in.");
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
                    Helpers.addCookie("Error", "Error Unknow, Please Try Again");
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
                        Helpers.addCookie("Notify", "Login Successful");
                        return RedirectToLocal(returnUrl);
                    }
                    AddErrors(result);
                    Helpers.addCookie("Error", "Error Unknow, Please Try Again", 30);
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

            Helpers.addCookie("Notify", "Logout Successful");
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [Website_Course_AVG.Attributes.AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
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
                { "client_id", ConfigurationManager.AppSettings["ClientIdGH"].ToString() },
                { "client_secret", ConfigurationManager.AppSettings["ClientSecretGH"].ToString() },
                { "code", code },
                { "redirect_uri", ConfigurationManager.AppSettings["RedirectUrl"].ToString() }
            };
            var content = new FormUrlEncodedContent(parameters);
            var response = await client.PostAsync("https://github.com/login/oauth/access_token", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            var values = HttpUtility.ParseQueryString(responseContent);
            var accessToken = values["access_token"];
            var client1 = new GitHubClient(new Octokit.ProductHeaderValue("Huynguyenjv"));
            var tokenAuth = new Credentials(accessToken);
            client1.Credentials = tokenAuth;
            var user = await client1.User.Current();
            var login = user.Login;
            var provideKey = user.Id.ToString();
            if (login == null) return RedirectToAction("Login");

            var UserManager = new Website_Course_AVG.Managers.UserManager();

            if (UserManager.CheckUsername(login))
            {
                Helpers.addCookie("Notify", "Login Successful");
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
    }
}