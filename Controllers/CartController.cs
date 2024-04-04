using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
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
                var selectedCourseIds = GetItem(itemCookie.Value);
                var coursesInCart = db.courses.Where(c => selectedCourseIds.Contains(c.id)).ToList();
                ViewBag.TotalAmount = coursesInCart.Sum(c => c.price);
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
                var selectedCourseIds = GetItem(itemCookie.Value);
                var coursesInCart = db.courses.Where(c => selectedCourseIds.Contains(c.id)).ToList();
                ViewBag.SubTotal = coursesInCart.Sum(c => c.price);
                ViewBag.Discount = 0;
                ViewBag.Total = ViewBag.SubTotal;
                return View(new CartViewModels
                {
                    Courses = coursesInCart,
                    CourseCount = coursesInCart.Count
                });
            }
            else
            {
                Helpers.AddCookie("Error", "No course in your cart");
                return RedirectToAction("Index", "Cart");
            }
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
                    return ResponseHelper.ErrorResponse("Promotion code does not exist.");
                }
                if (promo.out_of_date < DateTime.Now)
                {
                    return ResponseHelper.ErrorResponse("Promotion code has expired.");
                }
                if (countPromo >= promo.max_time)
                {
                    return ResponseHelper.ErrorResponse("Promotion code has been used up.");
                }
                if (percent > 100)
                {
                    percent = 100;
                }
                var itemCookie = Request.Cookies["Item"];
                if (itemCookie != null)
                {
                    var selectedCourseIds = GetItem(itemCookie.Value);
                    var coursesInCart = db.courses.Where(c => selectedCourseIds.Contains(c.id)).ToList();
                    var totalAmount = coursesInCart.Sum(c => c.price);
                    var discountAmount = (totalAmount * percent) / 100;
                    var newTotal = totalAmount - discountAmount;

                    TempData["promotionId"] = promo.id;

                    string message = $"You get {percent}% off from {promo.name}.";
                    return ResponseHelper.SuccessResponse(message, new { newTotalAmount = newTotal, discount = percent, promotionId = promo.id });
                }
            }
            return ResponseHelper.ErrorResponse("Promotion code does not exist.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(string paymentMethod)
        {
            var itemCookie = Request.Cookies["Item"];

            user user = Helpers.GetUserFromToken();

            if (itemCookie != null)
            {
                var selectedCourseIds = GetItem(itemCookie.Value);
                var coursesInCart = db.courses.Where(c => selectedCourseIds.Contains(c.id)).ToList();
                var total = coursesInCart.Sum(c => c.price);

                int? promotionId = null;
                if (TempData["promotionId"] != null)
                {
                    promotionId = (int)TempData["promotionId"];
                }

                var newOrder = new order
                {
                    status = 1,
                    total = total,
                    type_payment = paymentMethod,
                    code_order = GenerateOrderCode(),
                    user_id = user.id,
                    promotion_id = promotionId,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now,
                };

                db.orders.InsertOnSubmit(newOrder);
                db.SubmitChanges();

                foreach (var course in coursesInCart)
                {
                    var detailOrder = new detail_order
                    {
                        order_id = newOrder.id,
                        course_id = course.id,
                        price = course.price,
                        created_at = DateTime.Now,
                        updated_at = DateTime.Now,
                    };

                    var detailCourse = new detail_course
                    {
                        type = 0,
                        course_id = course.id,
                        user_id = user.id,
                        created_at = DateTime.Now,
                        updated_at = DateTime.Now,
                    };
                    db.detail_orders.InsertOnSubmit(detailOrder);
                    db.detail_courses.InsertOnSubmit(detailCourse);
                }

                db.SubmitChanges();
                Response.Cookies["Item"].Expires = DateTime.Now.AddDays(-1);
                TempData.Remove("promotionId");
                Helpers.AddCookie("Notify", "Payment success!");
                return RedirectToAction("Index", "Home");
            }
            else
            {
                Helpers.AddCookie("Error", "No course in your cart");
                return RedirectToAction("Index", "Cart");
            }
        }
        private string GenerateOrderCode()
        {
            string randomString = Guid.NewGuid().ToString().Substring(0, 8);
            string orderCode = "AVG_" + randomString;

            while (OrderCodeExists(orderCode) == false)
            {
                randomString = Guid.NewGuid().ToString().Substring(0, 8);
                orderCode = "AVG_" + randomString;
            }
            return orderCode;
        }

        private bool OrderCodeExists(string orderCode)
        {
            var order = db.orders.Where(c => c.code_order == orderCode).FirstOrDefault();
            if (order != null)
            {
                return false;
            }
            return true;
        }

        private List<int> GetItem(string base64String)
        {
            byte[] data = Convert.FromBase64String(base64String);
            string decodedString = Encoding.UTF8.GetString(data);
            return decodedString.Split(';').Select(int.Parse).ToList();
        }
    }
}