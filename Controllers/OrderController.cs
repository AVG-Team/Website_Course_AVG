using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Website_Course_AVG.Managers;
using MoMo;
using Org.BouncyCastle.Asn1.X9;
using Website_Course_AVG.Models;

namespace Website_Course_AVG.Controllers
{
    public class OrderController : Controller
    {
        MyDataDataContext _data = new MyDataDataContext();

        // type payment = 1 momo
        // type payment = 2 vnpay
        // type payment = 0 ngan hang
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(int paymentMethod, string discountCode = null)
        {
            var itemCookie = Request.Cookies["Item"];

            user user = Helpers.GetUserFromToken();

            if (itemCookie != null)
            {
                if (paymentMethod == 1)
                {
                    var selectedCourseIds = Helpers.GetItem(itemCookie.Value);
                    var coursesInCart = _data.courses.Where(c => selectedCourseIds.Contains(c.id)).ToList();
                    long total = coursesInCart.Sum(c => c.price) ?? 0;

                    long Amount = total;

                    int? promotionId = null;

                    if (discountCode != null)
                    {
                        promotion promotion = _data.promotions.Where(x => x.code_promotion == discountCode).FirstOrDefault();
                        if (promotion != null)
                        {
                            promotionId = promotion.id;
                            Amount = total - (total * (promotion.percent ?? 0) / 100);
                        }

                        discountCode = ";" + discountCode;
                    }

                    string orderCode = GenerateOrderCode();

                    var newOrder = new order
                    {
                        status = 0,
                        total = total,
                        type_payment = paymentMethod,
                        code_order = orderCode,
                        user_id = user.id,
                        promotion_id = promotionId,
                        created_at = DateTime.Now,
                        updated_at = DateTime.Now,
                    };

                    _data.orders.InsertOnSubmit(newOrder);
                    _data.SubmitChanges();

                    return RedirectToAction("Momo", new MoMoRequest
                    {
                        OrderInfo = "Order Courses",
                        Amount = Amount,
                        OrderCode = orderCode,
                        ExtraData = user.email + discountCode
                    });
                }

            }
            Helpers.AddCookie("Error", "No course in your cart");
            return RedirectToAction("Index", "Cart");
        }

        public ActionResult MoMo(MoMoRequest momoRequest)
        {
            //request params need to request to MoMo system
            string endpoint = Helpers.GetValueFromAppSetting("EndpointMomo");
            string partnerCode = Helpers.GetValueFromAppSetting("PartnerCodeMomo");
            string accessKey = Helpers.GetValueFromAppSetting("AccessKeyMomo");
            string serectkey = Helpers.GetValueFromAppSetting("SerectkeyMomo");
            string orderInfo = momoRequest.OrderInfo;
            string returnUrl = Helpers.GetRedirectUrlMoMo();
            string notifyurl = "https://ntd-dev.tech/SavePayment";

            string amount = momoRequest.Amount.ToString();
            string orderid = momoRequest.OrderCode;
            string requestId = momoRequest.OrderCode;
            string extraData = momoRequest.ExtraData;

            //Before sign HMAC SHA256 signature
            string rawHash = "partnerCode=" +
                partnerCode + "&accessKey=" +
                accessKey + "&requestId=" +
                requestId + "&amount=" +
                amount + "&orderId=" +
                orderid + "&orderInfo=" +
                orderInfo + "&returnUrl=" +
                returnUrl + "&notifyUrl=" +
                notifyurl + "&extraData=" +
                extraData;

            MoMoSecurity crypto = new MoMoSecurity();
            //sign signature SHA256
            string signature = crypto.signSHA256(rawHash, serectkey);

            //build body json request
            JObject message = new JObject
            {
                { "partnerCode", partnerCode },
                { "accessKey", accessKey },
                { "requestId", requestId },
                { "amount", amount },
                { "orderId", orderid },
                { "orderInfo", orderInfo },
                { "returnUrl", returnUrl },
                { "notifyUrl", notifyurl },
                { "extraData", extraData },
                { "requestType", "captureMoMoWallet" },
                { "signature", signature }

            };

            string responseFromMomo = PaymentRequest.sendPaymentRequest(endpoint, message.ToString());

            JObject jmessage = JObject.Parse(responseFromMomo);

            return Redirect(jmessage.GetValue("payUrl").ToString());
        }

        public ActionResult ConfirmPaymentClient(MomoResult result)
        {
            string rMessage = result.message;
            string rOrderId = result.orderId;
            string rErrorCode = result.errorCode;
            if (rErrorCode != "0")
            {
                Helpers.AddCookie("Error", "Momo payment failed");
                return RedirectToAction("Index", "Cart");
            }

            var itemCookie = Request.Cookies["Item"];
            if (itemCookie == null)
            {
                Helpers.AddCookie("Error", "Unknown error, please contact admin to fix the error");
                return RedirectToAction("Index", "Cart");
            }

            string orderCode = result.orderId;
            order order = _data.orders.Where(x => x.code_order == orderCode).FirstOrDefault();
            if (order == null)
            {
                Helpers.AddCookie("Error", "Unknown error, please contact admin to fix the error");
                return RedirectToAction("Index", "Cart");
            }

            var selectedCourseIds = Helpers.GetItem(itemCookie.Value);
            var coursesInCart = _data.courses.Where(c => selectedCourseIds.Contains(c.id)).ToList();

            user user = Helpers.GetUserFromToken();

            foreach (var course in coursesInCart)
            {
                var detailOrder = new detail_order
                {
                    order_id = order.id,
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

            order.status = 1;

            _data.SubmitChanges();

            Response.Cookies["Item"].Expires = DateTime.Now.AddDays(-1);
            TempData.Remove("promotionId");
            Helpers.AddCookie("Notify", "Payment success!");
            return RedirectToAction("Index", "Home");
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