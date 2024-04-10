using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Website_Course_AVG.Models
{
    public class ReportViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
        public string Fullname { get; set; }

        /*[Display(Name = "Phone")]
        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
        [RegularExpression(@"^\s*-?[0-9]{1,10}\s*$", ErrorMessage = "Số điện thoại phải chứa 10 chữ số.")]*/
        public string Phone { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public contact contact { get; set; }
    }
}