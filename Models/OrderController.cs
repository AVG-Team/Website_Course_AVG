using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Website_Course_AVG.Managers;

namespace Website_Course_AVG.Models
{
    public class OrderController : Controller
    {
        MyDataDataContext _data = new MyDataDataContext();

        // type payment = 0 momo
        // type payment = 1 vnpay
        // type payment = 2 ngan hang
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(int paymentMethod, string discountCode = null)
        {
            var itemCookie = Request.Cookies["Item"];

            user user = Helpers.GetUserFromToken();

            if (itemCookie != null)
            {
                var selectedCourseIds = Helpers.GetItem(itemCookie.Value);
                var coursesInCart = _data.courses.Where(c => selectedCourseIds.Contains(c.id)).ToList();
                var total = coursesInCart.Sum(c => c.price);

                int? promotionId = null;

                if (discountCode == null)
                {
                    promotion promotion = _data.promotions.Where(x => x.code_promotion == discountCode).FirstOrDefault();
                    if (promotion != null)
                    {
                        promotionId = promotion.id;
                    }
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

                _data.orders.InsertOnSubmit(newOrder);
                _data.SubmitChanges();

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
                    _data.detail_orders.InsertOnSubmit(detailOrder);
                    _data.detail_courses.InsertOnSubmit(detailCourse);
                }

                _data.SubmitChanges();
                Response.Cookies["Item"].Expires = DateTime.Now.AddDays(-1);
                TempData.Remove("promotionId");
                Helpers.AddCookie("Notify", "Payment success!");
                return RedirectToAction("Index", "Home");
            }
            Helpers.AddCookie("Error", "No course in your cart");
            return RedirectToAction("Index", "Cart");
        }

        private string GenerateOrderCode()
        {
            string randomString = Helpers.GenerateRandomString(8);
            string orderCode = "AVG_" + randomString;

            while (OrderCodeExists(orderCode) == false)
            {
                randomString = Helpers.GenerateRandomString(8);
                orderCode = "AVG_" + randomString;
            }
            return orderCode;
        }

        private bool OrderCodeExists(string orderCode)
        {
            var order = _data.orders.Where(c => c.code_order == orderCode).FirstOrDefault();
            if (order != null)
            {
                return false;
            }
            return true;
        }
    }
}