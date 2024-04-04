using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website_Course_AVG.Models
{
    public class CartViewModels
    {
        public List<course> Courses { get; set; }
        public int CourseCount { get; set; }
    }
}