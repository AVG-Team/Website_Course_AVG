using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Website_Course_AVG.Managers;
using Website_Course_AVG.Models;

namespace Website_Course_AVG.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {
        private readonly MyDataDataContext _data = new MyDataDataContext();
        private ApplicationUserManager _userManager;
        private ApplicationSignInManager _signInManager;

        public AccountController()
        {
            
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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


        // GET: Admin/Account
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel viewModel)
        {
            var userManager = new UserManager();
            if (userManager.IsAuthenticated())
            {
                Helpers.AddCookie("Error","You are logging!!");
                return View(viewModel);
            }

            if (ModelState.IsValid)
            {
                account account = _data.accounts.FirstOrDefault(a => a.username == viewModel.userName);

                if (account == null)
                {
                    Helpers.AddCookie("Error", "Username and password are wrong !!!");
                    return View(viewModel);
                }

                var user = account.users.FirstOrDefault();
                if (user == null)
                {
                    Helpers.AddCookie("Error", "Username and password are wrong !!!");
                    return View(viewModel);
                }
                if (user.role > 1)
                {
                    userManager.login(account.username);
                    Helpers.AddCookie("Notify", "Login Successfull");
                    return RedirectToAction("Index", "Admin");
                }
                Helpers.AddCookie("Error", "You are not admin");
                return View(viewModel);


            }

            Helpers.AddCookie("Error", "Error Unknow, Please Try Again");
            return View(viewModel);
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult>  Register(RegisterViewModel viewModel)
        {
            UserManager userManager = new UserManager();
            if (ModelState.IsValid)
            {
                var account = new account
                {
                    username = viewModel.userName,
                    password = viewModel.Password,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                };
                var result = await userManager.CreateAccountUserAsync(viewModel.userName, account, viewModel.Email);
                if (result.Succeeded)
                {
                    Helpers.AddCookie("Notify", "Register Successfull");
                    userManager.login(viewModel.userName);
                    return RedirectToAction("Login", "Account");
                }
                AddErrors(result);
            }
            Helpers.AddCookie("Error", "Error Unknow, Please Try Again");
            return View(viewModel);
        }


        public ActionResult Logout()
        {
            var UserManager = new UserManager();
            UserManager.logout();

            Helpers.AddCookie("Notify", "Logout Successful", 5);
            return RedirectToAction("Index", "Admin");
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
    }
}