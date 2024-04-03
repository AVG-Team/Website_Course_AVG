using System.Collections.Generic;
using PagedList;

namespace Website_Course_AVG.Models
{
    public class AdminViewModels
    {
        #region model
        public course Course { get; set; }
        public category Category { get; set; }
        public user User { get; set; }
        public lesson Lesson { get; set; }
        public comment Comment { get; set; }
        public contact Contact { get; set; }
        public exercise Exercise { get; set; }
        public option Option { get; set; }
        public order Order { get; set; }
        public promotion Promotion { get; set; }
        public image Image { get; set; }

        #endregion


        #region List
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
        public List<image> Images { get; set; }
        public IPagedList<course> CoursesPagedList { get; set; }
        #endregion

    }

}