using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Website_Course_AVG.Models;

namespace MoMo
{
    public class MomoResult
    {
        public string partnerCode { get; set; }
        public string accessKey { get; set; }
        public string requestId { get; set; }
        public string amount { get; set; }
        public string orderId { get; set; }
        public string orderInfo { get; set; }
        public string orderType { get; set; }
        public string transId { get; set; }
        public string message { get; set; }
        public string localMessage { get; set; }
        public string responseTime { get; set; }
        public string errorCode { get; set; }
        public string payType { get; set; }
        public string extraData { get; set; }
        public string signature { get; set; }
    }
}