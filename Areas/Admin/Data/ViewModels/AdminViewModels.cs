using System.Collections.Generic;
using System.Web;
using Octokit;
using PagedList;
using Website_Course_AVG.Models;

namespace Website_Course_AVG.Areas.Admin.Data.ViewModels
{
    public class AdminViewModels
    {
        #region model
        public account Account { get; set; }
        public course Course { get; set; }
        public category Category { get; set; }
        public user User { get; set; }
        public lesson Lesson { get; set; }
        public comment Comment { get; set; }
        public contact Contact { get; set; }
        public exercise Exercise { get; set; }
        public option Option { get; set; }
        public order Order { get; set; }
        public user AUser { get; set; }
        public promotion Promotion { get; set; }
        public image Image { get; set; }
        public video Video { get; set; }
        public report Report { get; set; }

        #endregion


        #region List
        public List<account> Accounts { get; set; }
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
        public List<video> Videos { get; set; }
        public IPagedList<account> AccountsPagedList { get; set; }
        public List<report> Reports { get; set; }
        public IPagedList<course> CoursesPagedList { get; set; }
        public IPagedList<category> CategoriesPagedList { get; set; }
        public IPagedList<user> UsersPagedList { get; set; }
        public IPagedList<lesson> LessonsPagedList { get; set; }
        public IPagedList<comment> CommentsPagedList { get; set; }
        public IPagedList<contact> ContactsPagedList { get; set; }
        public IPagedList<exercise> ExercisesPagedList { get; set; }
        public IPagedList<option> OptionsPagedList { get; set; }
        public IPagedList<order> OrdersPagedList { get; set; }
        public IPagedList<promotion> PromotionsPagedList { get; set; }
        public IPagedList<image> ImagesPagedList { get; set; }
        public IPagedList<report> ReportsPagedList { get;set; }

        #endregion
    }

}