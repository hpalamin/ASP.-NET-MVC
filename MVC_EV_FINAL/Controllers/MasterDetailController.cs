using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVC_EV_FINAL.ViewModels;
using MVC_EV_FINAL.Models;
using Newtonsoft.Json;
using System.Data.Entity;

namespace MVC_EV_FINAL.Controllers
{
    public class MasterDetailController : Controller
    {
        BookingDbContext db = new BookingDbContext();
        // GET: MasterDetail
        public ActionResult Index()
        {
            var clients = db.Clients.Include(c => c.bookingEntries.Select(b => b.Spot)).ToList();
            return View(clients);
        }
        //Return new spot from partial view
        public ActionResult AddNewSpot(int? id)
        {
            ViewBag.spots = new SelectList(db.Spots.ToList(), "SpotId", "SpotName", (id != null) ? id.ToString() : "");
            return PartialView("_addNewSpot");
        }

        [HttpPost]
        public JsonResult Create(string postedClient, string ClientId, HttpPostedFileBase file, int[] spot)
        {
            //add new client and bookingEntry
            if (ClientId == "")
            {
                Client client = JsonConvert.DeserializeObject<Client>(postedClient);
                string filePath;

                if (file != null)
                {
                    filePath = Path.Combine("/Images/", Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));
                    file.SaveAs(Server.MapPath(filePath));
                    client.Picture = filePath;
                }
                if (file == null)
                {
                    client.Picture = "";
                }

                foreach (var item in spot)
                {
                    BookingEntry bookingEntry = new BookingEntry()
                    {
                        ClientId = client.ClientId,
                        Client = client,
                        SpotId = item
                    };
                    db.BookingEntries.Add(bookingEntry);
                }
                db.SaveChanges();
            }

            //update existing entry
            if (ClientId != "")
            {
                string filePath;

                Client client = JsonConvert.DeserializeObject<Client>(postedClient);
                client.ClientId = int.Parse(ClientId);

                if (file != null)
                {
                    filePath = Path.Combine("/Images/", Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));
                    file.SaveAs(Server.MapPath(filePath));
                    client.Picture = filePath;
                }
                if (file == null)
                {
                    client.Picture = "";
                }

                var existsSpotEntry = db.BookingEntries.Where(x => x.ClientId == client.ClientId).ToList();

                foreach (var bookingEntry in existsSpotEntry)
                {
                    db.BookingEntries.Remove(bookingEntry);
                }

                foreach (var item in spot)
                {
                    BookingEntry bookingEntry = new BookingEntry()
                    {
                        ClientId = client.ClientId,
                        SpotId = item
                    };
                    db.BookingEntries.Add(bookingEntry);
                }

                db.Entry(client).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult Delete(int id)
        {
            var client = db.Clients.Find(id);
            var existsSpotEntry = db.BookingEntries.Where(x => x.ClientId == id).ToList();

            foreach (var bookingEntry in existsSpotEntry)
            {
                db.BookingEntries.Remove(bookingEntry);
            }
            db.Entry(client).State = EntityState.Deleted;
            db.SaveChanges();
            return Json("", JsonRequestBehavior.AllowGet);
        }

    }
}
