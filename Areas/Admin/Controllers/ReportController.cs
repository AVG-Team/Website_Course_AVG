using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Website_Course_AVG.Areas.Admin.Data.ViewModels;
using Website_Course_AVG.Models;

namespace Website_Course_AVG.Areas.Admin.Controllers
{
    public class ReportController : Controller
    {
        // GET: Admin/Report
        private readonly MyDataDataContext _data = new MyDataDataContext();
        public ActionResult Index(int? page)
        {
            var reports = _data.reports.ToList();
            var pageNumber = page ?? 1;
            var pageSize = 5;
            var adminView = new AdminViewModels()
            {
                Reports = reports,
                ReportsPagedList = reports.ToPagedList(pageNumber, pageSize)
            };

            return View(adminView);
        }
        public ActionResult Update(int? id)
        {
            var report = _data.reports.FirstOrDefault(c => c.id == id);
            var reports = _data.reports.Where(c => c.deleted_at == null).ToList();
            var adminView = new AdminViewModels()
            {
                Report = report,
                Reports = reports
            };
            return View(adminView);
        }

        [HttpPost]
        public ActionResult Update(FormCollection form, category model)
        {
            var report = _data.reports.FirstOrDefault(c => c.id == model.id);
            if (report != null)
            {
                report.fullname = form["Report.fullname"];
                report.email = form["Report.email"];
                report.phone = form["Report.phone"];
                report.subject = form["Report.subject"];
                report.message = form["Report.message"];
                report.status = Convert.ToBoolean(form["Report.status"]);
                report.updated_at = DateTime.Now;
                _data.SubmitChanges();
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        public ActionResult Delete()
        {
            return View("Index");
        }
        [HttpPost]
        public ActionResult Delete(report model)
        {
            var report = _data.reports.FirstOrDefault(c => c.id == model.id);
            if (report != null)
            {
                report.deleted_at = DateTime.Now;
                _data.SubmitChanges();
                return RedirectToAction("Index");
            }
            return RedirectToAction("Contact","Index");
        }
    }
}