using System.Collections.Generic;

namespace Website_Course_AVG.Models
{
    public class AdminViewModels
    {
        public List<course> Courses { get; set; }
        public List<category> Categories { get; set; }
        public List<user> Users { get; set; }
        public List<lesson> Lessons { get; set; }
        public List<comment> Comments { get; set; }
        public List<contact> Contacts { get; set; }
        public List<exercise> Exercises { get; set; }
        public List<option> Options { get; set; }
        public List<order> Orders { get; set; }
        public List<promotion> Promotions { get; set; }
    }
}