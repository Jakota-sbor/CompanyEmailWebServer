using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CompanyWebServer.Models;
using System.ComponentModel;
using System.Data.Entity.Core.Objects;
using System.Data.Linq;
using System.Threading;
using System.Data.SqlClient;

namespace CompanyWebServer.Controllers
{
    public class EmailsController : Controller
    {
        private LinqDBDataContext db = new LinqDBDataContext();

        public async Task<ActionResult> Index([DefaultValue("")]string SearchAddressFrom, [DefaultValue("")]string SearchAddressTo, [DefaultValue("")]string SearchDate)
        {
            ViewBag.CurrentDate = DateTime.Now;
            IQueryable<Emails> query= from email in db.Emails select email;
            if (!string.IsNullOrEmpty(SearchAddressFrom) && !string.IsNullOrEmpty(SearchAddressTo) && !string.IsNullOrEmpty(SearchDate))
                query = from email in db.Emails where email.EmailFrom.Contains(SearchAddressFrom) && email.EmailTo.Contains(SearchAddressTo) && email.EmailTime.Date == Convert.ToDateTime(SearchDate).Date select email;
            else if (!string.IsNullOrEmpty(SearchAddressFrom))
                query = from email in db.Emails where email.EmailFrom.Contains(SearchAddressFrom) select email;
            else if (!string.IsNullOrEmpty(SearchAddressTo))
                query = from email in db.Emails where email.EmailTo.Contains(SearchAddressTo) select email;
            else if (!string.IsNullOrEmpty(SearchDate))
                query = from email in db.Emails where email.EmailTime.Date == Convert.ToDateTime(SearchDate).Date select email;

            if (query != null)
                return View(await ExecuteAsync(query, db));
            else
                return View();
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Emails emails = db.Emails.First(e=>e.Id==id);
            if (emails == null)
            {
                return HttpNotFound();
            }
            return View(emails);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,EmailTime,EmailTo,EmailFrom,EmailTopic,EmailText")] Emails emails)
        {
            if (ModelState.IsValid)
            {
                db.InsertEmail(emails.EmailTo, emails.EmailFrom, emails.EmailTopic, emails.EmailText);
                db.SubmitChanges();
                return RedirectToAction("Index");
            }

            return View(emails);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Emails emails = db.Emails.First(e => e.Id == id);
            if (emails == null)
                return HttpNotFound();
            return View(emails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,EmailTime,EmailTo,EmailFrom,EmailTopic,EmailText")] Emails emails)
        {
            if (ModelState.IsValid)
            {
                db.EditEmail(emails.Id, emails.EmailTime, emails.EmailTo, emails.EmailFrom, emails.EmailTopic, emails.EmailText);
                db.SubmitChanges();
                return RedirectToAction("Index");
            }
            return View(emails);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Emails emails = db.Emails.First(e=>e.Id ==id);
            if (emails == null)
                return HttpNotFound();
            return View(emails);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            db.DeleteEmail(id);
            db.SubmitChanges();
            return RedirectToAction("Index");
        }

        [HttpPut]
        public HttpStatusCodeResult Add(Emails emails)
        {
            if (ModelState.IsValid)
            {
                db.InsertEmail(emails.EmailTo, emails.EmailFrom, emails.EmailTopic, emails.EmailText);
                db.SubmitChanges();
                return new HttpStatusCodeResult(200);
            }
            return new HttpStatusCodeResult(400);
        }

        [HttpGet]
        public JsonResult GetEmails(string emailAddress)
        {
            if (!string.IsNullOrEmpty(emailAddress))
                return Json(db.Emails.Where(e => e.EmailTo == emailAddress).OrderBy(e => e.EmailTime).ToList(), JsonRequestBehavior.AllowGet);
            else
                return null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }

        protected static async Task<IEnumerable<T>> ExecuteAsync<T>(IQueryable<T> query,
    DataContext ctx,
    CancellationToken token = default(CancellationToken))
        {
            var cmd = (SqlCommand)ctx.GetCommand(query);

            if (cmd.Connection.State == ConnectionState.Closed)
                await cmd.Connection.OpenAsync(token);
            var reader = await cmd.ExecuteReaderAsync(token);

            return ctx.Translate<T>(reader);
        }
    }
}
