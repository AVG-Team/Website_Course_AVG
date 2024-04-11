using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml.Linq;
using Website_Course_AVG.Managers;
using Website_Course_AVG.Models;

namespace Website_Course_AVG.Controllers
{
    public class CartController : Controller
    {
        private MyDataDataContext db = new MyDataDataContext();

        public ActionResult Index()
        {
            var itemCookie = Request.Cookies["Item"];
            if (itemCookie != null && !string.IsNullOrEmpty(itemCookie.Value))
            {
                var selectedCourseIds = Helpers.GetItem(itemCookie.Value);
                var coursesInCart = db.courses.Where(c => selectedCourseIds.Contains(c.id)).ToList();
                ViewBag.TotalAmount = coursesInCart.Sum(c => c.price);
                var imageCodes = coursesInCart.Select(c => c.image_code).Distinct().ToList();

                var images = db.images.Where(i => imageCodes.Contains(i.code) && i.category == false).ToList();

                foreach (var course in coursesInCart)
                {
                    var image = images.FirstOrDefault(i => i.code == course.image_code);
                    if (image != null)
                    {
                        course.image_code = image.image1;
                    }
                }
                return View(new CartViewModels
                {
                    Courses = coursesInCart,
                    CourseCount = coursesInCart.Count
                });
            }
            return View(new CartViewModels { Courses = new List<course>(), CourseCount = 0 });
        }

        [Website_Course_AVG.Attributes.User]
        public ActionResult Payment()
        {
            var itemCookie = Request.Cookies["Item"];
            if (itemCookie != null)
            {
                var selectedCourseIds = Helpers.GetItem(itemCookie.Value);
                var coursesInCart = db.courses.Where(c => selectedCourseIds.Contains(c.id)).ToList();
                var coursesTmp = new List<course>(coursesInCart);
                List<int> ids = new List<int>();
                bool isRemove = false;
                var imageCodes = coursesInCart.Select(c => c.image_code).Distinct().ToList();

                var images = db.images.Where(i => imageCodes.Contains(i.code) && i.category == false).ToList();

                foreach (var course in coursesInCart)
                {
                    var image = images.FirstOrDefault(i => i.code == course.image_code);
                    if (image != null)
                    {
                        course.image_code = image.image1;
                    }
                }

                foreach (var item in coursesInCart)
                {
                    int idUser = Helpers.GetUserFromToken().id;
                    detail_course detail_Course = item.detail_courses.Where(x => x.user_id == idUser).FirstOrDefault();
                    if (detail_Course != null)
                    {
                        isRemove = true;
                        coursesTmp.Remove(item);
                        if (coursesTmp.Count() == 0)
                        {
                            Helpers.AddCookie("Item", "", -30 * 24 * 60 * 60);
                            Helpers.AddCookie("Error", ResourceHelper.GetResource("No course in your cart!"));
                            return RedirectToAction("Index", "Cart");
                        }
                    }
                    else
                    {
                        ids.Add(item.id);
                    }
                }

                if (isRemove)
                {
                    string result = string.Join(";", ids);
                    byte[] bytes = Encoding.UTF8.GetBytes(result);
                    string base64String = Convert.ToBase64String(bytes);
                    Helpers.AddCookie("Error", ResourceHelper.GetResource("The course has been purchased"));
                    Helpers.AddCookie("Item", base64String, 30 * 24 * 60 * 60);
                }
                return View(coursesTmp);
            }
            Helpers.AddCookie("Error", ResourceHelper.GetResource("No course in your cart!"));
            return RedirectToAction("Index", "Course");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Website_Course_AVG.Attributes.User]
        public ActionResult CheckDiscountCode(string discountCode)
        {
            var promo = db.promotions.FirstOrDefault(c => c.code_promotion.Equals(discountCode));
            if (promo != null)
            {
                var countPromo = db.orders.Where(d => d.promotion_id == promo.id).Count();
                var percent = promo.percent;
                if (promo.active == false)
                {
                    string message = ResourceHelper.GetResource("Promotion code does not exist.");
                    return ResponseHelper.ErrorResponse(message);
                }
                if (promo.out_of_date < DateTime.Now)
                {
                    string message = ResourceHelper.GetResource("Promotion code has expired.");
                    return ResponseHelper.ErrorResponse(message);
                }
                if (countPromo >= promo.max_time)
                {
                    string message = ResourceHelper.GetResource("Promotion code has been used up.");
                    return ResponseHelper.ErrorResponse(message);
                }
                if (percent > 100)
                {
                    percent = 100;
                }
                var itemCookie = Request.Cookies["Item"];
                if (itemCookie != null)
                {
                    var selectedCourseIds = Helpers.GetItem(itemCookie.Value);
                    var coursesInCart = db.courses.Where(c => selectedCourseIds.Contains(c.id)).ToList();
                    var totalAmount = coursesInCart.Sum(c => c.price);
                    var discountAmount = (totalAmount * percent) / 100;
                    var newTotal = totalAmount - discountAmount;

                    TempData["promotionId"] = promo.id;

                    string message = $"You get {percent}% off from {promo.name}.";
                    return ResponseHelper.SuccessResponse(message, new { newTotalAmount = newTotal, discount = percent, promotionId = promo.id });
                }
            }

            return ResponseHelper.ErrorResponse(ResourceHelper.GetResource("Promotion code does not exist."));
        }
    }
}