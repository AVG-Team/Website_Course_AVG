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
using Microsoft.IdentityModel.Tokens;
using System.Security.Policy;
using System.Net;
using VNPay;
using Website_Course_AVG.Attributes;

namespace Website_Course_AVG.Controllers
{
    [Website_Course_AVG.Attributes.Authorize]
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


                if (paymentMethod == 1)
                {
                    return RedirectToAction("Momo", new MoMoRequest
                    {
                        OrderInfo = "Order Courses",
                        Amount = Amount,
                        OrderCode = orderCode,
                        ExtraData = user.email + discountCode
                    });
                }
                else
                {
                    return RedirectToAction("VNPay", new VNPayRequest
                    {
                        OrderInfo = "Order Courses",
                        Amount = Amount,
                        OrderCode = orderCode,
                        ExtraData = user.email + discountCode,
                        CreatedAt = newOrder.created_at ?? DateTime.Now,
                        locale = "vn",
                    });
                }

            }
            Helpers.AddCookie("Error", ResourceHelper.GetResource("No course in your cart!"));
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

        public ActionResult ConfirmMoMoPaymentClient(MomoResult result)
        {
            string rMessage = result.message;
            string rOrderId = result.orderId;
            string rErrorCode = result.errorCode;
            if (rErrorCode != "0")
            {
                Helpers.AddCookie("Error", ResourceHelper.GetResource("Momo payment failed"));
                return RedirectToAction("Index", "Cart");
            }

            var itemCookie = Request.Cookies["Item"];
            if (itemCookie == null)
            {
                Helpers.AddCookie("Error", ResourceHelper.GetResource("Unknown error, please contact admin to fix the error"));
                return RedirectToAction("Index", "Cart");
            }

            string orderCode = result.orderId;
            order order = _data.orders.Where(x => x.code_order == orderCode).FirstOrDefault();
            if (order == null)
            {
                Helpers.AddCookie("Error", ResourceHelper.GetResource("Unknown error, please contact admin to fix the error"));
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
            Helpers.AddCookie("Notify", ResourceHelper.GetResource("Payment success!"));
            return RedirectToAction("Index", "Home");
        }

        public ActionResult VNPay(VNPayRequest vnpayRequest)
        {
            //Get Config Info
            string vnp_Returnurl = Helpers.GetRedirectUrlVNPay();
            string vnp_Url = Helpers.GetValueFromAppSetting("VNP_URL");
            string vnp_TmnCode = Helpers.GetValueFromAppSetting("VNP_TMPCODE"); //Ma website
            string vnp_HashSecret = Helpers.GetValueFromAppSetting("VNP_HASHSECRET"); //Chuoi bi mat
            if (string.IsNullOrEmpty(vnp_TmnCode) || string.IsNullOrEmpty(vnp_HashSecret))
            {
                Helpers.AddCookie("Error", ResourceHelper.GetResource("Error Unknown, Please Try Again!"));
                return RedirectToAction("Index", "Home");
            }
            VNPayLibrary vnpay = new VNPayLibrary();

            vnpay.AddRequestData("vnp_Version", VNPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", (vnpayRequest.Amount * 100).ToString());

            //if (cboBankCode.SelectedItem != null && !string.IsNullOrEmpty(cboBankCode.SelectedItem.Value))
            //{
            //    vnpay.AddRequestData("vnp_BankCode", cboBankCode.SelectedItem.Value);
            //}
            vnpay.AddRequestData("vnp_CreateDate", vnpayRequest.CreatedAt.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", "127.0.0.1");
            if (!string.IsNullOrEmpty(vnpayRequest.locale))
            {
                vnpay.AddRequestData("vnp_Locale", vnpayRequest.locale);
            }
            else
            {
                vnpay.AddRequestData("vnp_Locale", "vn");
            }
            vnpay.AddRequestData("vnp_OrderInfo", vnpayRequest.OrderInfo);
            vnpay.AddRequestData("vnp_OrderType", "topup");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", vnpayRequest.OrderCode);
            vnpay.AddRequestData("vnp_ExpireDate", DateTime.Now.AddMinutes(60).ToString("yyyyMMddHHmmss"));
            //Billing
            user user = Helpers.GetUserFromToken();
            string email = user.account.username;
            if (user.email != null)
            {
                email = user.email.Trim();
            }

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);

            return Redirect(paymentUrl);
        }

        public ActionResult ConfirmVNPayPaymentClient(object sender, EventArgs e)
        {
            if (Request.QueryString.Count > 0)
            {
                var itemCookie = Request.Cookies["Item"];
                if (itemCookie == null)
                {
                    Helpers.AddCookie("Error", ResourceHelper.GetResource("Unknown error, please contact admin to fix the error"));
                    return RedirectToAction("Index", "Cart");
                }

                string vnp_HashSecret = Helpers.GetValueFromAppSetting("VNP_HASHSECRET");
                var vnpayData = Request.QueryString;
                VNPayLibrary vnpay = new VNPayLibrary();

                foreach (string s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s, vnpayData[s]);
                    }
                }
                //vnp_TxnRef: Ma don hang merchant gui VNPAY tai command=pay    
                //vnp_TransactionNo: Ma GD tai he thong VNPAY
                //vnp_ResponseCode:Response code from VNPAY: 00: Thanh cong, Khac 00: Xem tai lieu
                //vnp_SecureHash: HmacSHA512 cua du lieu tra ve

                string orderId = vnpay.GetResponseData("vnp_TxnRef");
                long vnpayTranId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                String vnp_SecureHash = Request.QueryString["vnp_SecureHash"];
                String TerminalID = Request.QueryString["vnp_TmnCode"];
                long vnp_Amount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100;
                String bankCode = Request.QueryString["vnp_BankCode"];

                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                    {
                        order order = _data.orders.Where(x => x.code_order == orderId).FirstOrDefault();
                        if (order == null)
                        {
                            Helpers.AddCookie("Error", ResourceHelper.GetResource("Unknown error, please contact admin to fix the error"));
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
                    else
                    {

                        Helpers.AddCookie("Error", ResourceHelper.GetResource("Unknown error, please contact admin to fix the error , Code Error : ") + vnp_ResponseCode);
                        return RedirectToAction("Index", "Cart");
                    }
                }
            }
            Helpers.AddCookie("Error", ResourceHelper.GetResource("Unknown error, please contact admin to fix the error"));
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