using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website_Course_AVG.Models
{
    public class CategoryCourseViewModels
    {
        public category Category { get; set; }
        public List<course> Courses { get; set; }
    }
}