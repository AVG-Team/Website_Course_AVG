﻿using System.Collections.Generic;

namespace Website_Course_AVG.Models
{
    public class CoursesViewModel
    {
        public List<course> Courses { get; set; }
        public List<image> Images { get; set; }
        public List<lesson> Lessons { get; set; }
    }
}