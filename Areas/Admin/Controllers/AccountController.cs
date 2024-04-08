using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Website_Course_AVG.Managers;
using Website_Course_AVG.Models;

namespace Website_Course_AVG.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {
        private readonly MyDataDataContext _data = new MyDataDataContext();
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
                    Helpers.AddCookie("Error", "Username is not exist!!");
                    return View(viewModel);
                }
                
            }
            return View();
        }

        public ActionResult Register()
        {
            return View();
        }
    }
}