using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PagedList;
using Website_Course_AVG.Areas.Admin.Data.ViewModels;
using Website_Course_AVG.Managers;
using Website_Course_AVG.Models;
using System.IO;
using System.Web.Routing;
using Website_Course_AVG.Attributes;

namespace Website_Course_AVG.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {
        private readonly MyDataDataContext _data = new MyDataDataContext();
        private ApplicationUserManager _userManager;
        private ApplicationSignInManager _signInManager;

        public AccountController()
        {
            ViewBag.Title = "Account";
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
        [Attributes.Authorize]
        [Admin]
        public ActionResult Index(int? page)
        {
            var account = _data.accounts.ToList();
            var user = _data.users.ToList();
            var pageNumber = page ?? 1;
            var pageSize = 5;
            var acountPageList = account.ToPagedList(pageNumber, pageSize);
            var usePageList = user.ToPagedList(pageNumber, pageSize);

            var viewModel = new AdminViewModels()
            {
                Accounts = account,
                Users = user,
                AccountsPagedList = acountPageList,
                UsersPagedList = usePageList
            };

            return View(viewModel);
        }
        [Admin]
        public ActionResult Update(int? id)
        {
            var user = _data.users.FirstOrDefault(c => c.id == id);
            var viewModel = new AdminViewModels()
            {
                User = user
            };
            return View(viewModel);

        }
        [Admin]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection form, int? id)
        {
            var user = _data.users.FirstOrDefault(c => c.id == id);
            if (user != null)
            {
                user.fullname = form["User.fullname"];
                user.birthday = DateTime.Parse(form["User.birthday"]);
                user.gender = bool.Parse(form["User.gender"]);
                user.role = int.Parse(form["User.role"]);

                user.updated_at = DateTime.Now;

                _data.SubmitChanges();
            }

            return RedirectToAction("Index");
        }

        

        public JsonResult UserParticial(int? page)
        {
            try
            {
                var account = _data.accounts.ToList();
                var user = _data.users.ToList();
                var pageNumber = page ?? 1;
                var pageSize = 5;
                var acountPageList = account.ToPagedList(pageNumber, pageSize);
                var usePageList = user.ToPagedList(pageNumber, pageSize);

                var viewModel = new AdminViewModels()
                {
                    Accounts = account,
                    Users = user,
                    AccountsPagedList = acountPageList,
                    UsersPagedList = usePageList
                };
                var view = RenderViewToString("Account", "UserParticial", viewModel);
                return ResponseHelper.SuccessResponse("", view);
            }
            catch (Exception ex)
            {
                return ResponseHelper.ErrorResponse("");
            }
            
        }

        [System.Web.Mvc.AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [System.Web.Mvc.AllowAnonymous]
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

        [System.Web.Mvc.AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [System.Web.Mvc.AllowAnonymous]
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

        protected string RenderViewToString(string controllerName, string viewName, object viewData)
        {
            using (var writer = new StringWriter())
            {
                var routeData = new RouteData();
                routeData.Values.Add("controller", controllerName);
                var fakeControllerContext = new ControllerContext(new HttpContextWrapper(new HttpContext(new HttpRequest(null, "http://google.com", null), new HttpResponse(null))), routeData, new CourseController());
                var razorViewEngine = new RazorViewEngine();
                var razorViewResult = razorViewEngine.FindView(fakeControllerContext, viewName, "", false);

                var viewContext = new ViewContext(fakeControllerContext, razorViewResult.View, new ViewDataDictionary(viewData), new TempDataDictionary(), writer);
                razorViewResult.View.Render(viewContext, writer);
                return writer.ToString();

            }
        }
    }
}