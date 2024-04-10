using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Website_Course_AVG.Areas.Admin.Data.ViewModels;
using Website_Course_AVG.Models;

namespace Website_Course_AVG.Areas.Admin.Controllers
{
    public class ContactController : Controller
    {
        // GET: Admin/Contact
        private readonly MyDataDataContext _data = new MyDataDataContext();
        public ActionResult Index()
        {
            contact contact = _data.contacts.FirstOrDefault();
            if(contact == null)
            {
                contact = new contact() {
                    name = "",
                    email = "",
                    address = "",
                    phone = "",
                    link_facebook = "",
                };
            }
            contact = new contact()
            {
                name = contact.name,
                email = contact.email,
                address = contact.address,
                phone = contact.phone,
                link_facebook = contact.link_facebook,
            };
            return View(contact);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Update(contact contact)
        {
            contact contact1 = _data.contacts.FirstOrDefault();
            if (contact1 == null)
            {
                _data.contacts.InsertOnSubmit(contact);
            } else
            {
                contact1.name = contact.name;
                contact1.phone = contact.phone;
                contact1.email = contact.email;
                contact1.address = contact.address;
                contact1.link_facebook = contact.link_facebook;
            }
            _data.SubmitChanges();
            return RedirectToAction("Index");
        }
    }
}