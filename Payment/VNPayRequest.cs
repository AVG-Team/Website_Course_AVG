using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Website_Course_AVG.Models;

namespace VNPay
{
    public class VNPayRequest
    {
        public string OrderInfo { get; set; }
        public long Amount { get; set; }
        public string OrderCode { get; set; }
        public string ExtraData { get; set; }
        public DateTime CreatedAt { get; set; }
        public string locale { get; set; }
    }
}