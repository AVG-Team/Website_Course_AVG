using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Website_Course_AVG.Models;

namespace MoMo
{
    public class MoMoRequest
    {
        public string OrderInfo { get; set; }
        public long Amount { get; set; }
        public string OrderCode { get; set; }
        public string ExtraData { get; set; }
    }
}