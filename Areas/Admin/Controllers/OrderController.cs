using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using Website_Course_AVG.Areas.Admin.Data.ViewModels;
using Website_Course_AVG.Attributes;
using Website_Course_AVG.Models;

namespace Website_Course_AVG.Areas.Admin.Controllers
{
    public class OrderController : Controller
    {
        private readonly MyDataDataContext data = new MyDataDataContext();

        public OrderController()
        {
            ViewBag.controller = "Order";
        }
        // GET: Admin/Order

        [Admin]
        public ActionResult Index(int? page)
        {
            var order = data.orders.ToList();
            var user = data.users.Where(m => m.deleted_at == null).ToList();
            var pageNumber = page?? 1;
            var pageSize = 10;
            var ordersPagedList = order.ToPagedList(pageNumber, pageSize);
            var viewModel = new AdminViewModels()
            {
                Orders = order,
                Users = user,
                OrdersPagedList = ordersPagedList
            };

            return View(viewModel);
        }

        [Admin]
        public ActionResult Update(int? id)
        {
            var order = data.orders.FirstOrDefault(o => o.id == id);
            var viewModel = new AdminViewModels()
            {
                Order = order
            };
            return View(viewModel);
        }

        [HttpPost]
        [Admin]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection form,int? id)
        {
            var order = data.orders.FirstOrDefault(o => o.id == id);
            if (order != null)
            {
                if (ModelState.IsValid)
                {
                    order.status = int.Parse(form["Order.status"]);
                    order.updated_at = DateTime.Now;
                    data.SubmitChanges();
                    return RedirectToAction("Index");
                }
            }

            return RedirectToAction("Index");
        }

        [Admin]
        public ActionResult Approve()
        {
            return View("Index");
        }
        [HttpPost]
        [Admin]
        public ActionResult Approve(int id)
        {
            var order = data.orders.FirstOrDefault(o => o.id == id);
            if (order != null)
            {
                order.status = 1;
                order.updated_at = DateTime.Now;
                data.SubmitChanges();
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        public ActionResult Delete()
        {
            return View("Index");
        }

        [HttpPost]
        [Admin]
        public ActionResult Delete(int id)
        {
            var order = data.orders.FirstOrDefault(o => o.id == id && o.status == 0);
            if (order != null)
            {
                order.deleted_at = DateTime.Now;
                data.SubmitChanges();
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
    }
}