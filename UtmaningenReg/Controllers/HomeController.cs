using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using log4net;
using UtmaningenReg.Helpers;
using UtmaningenReg.Models;

namespace UtmaningenReg.Controllers
{
    public class HomeController : RegBaseController
    {
        private readonly ILog log;

        public HomeController()
        {   
            log = LogManager.GetLogger(GetType());
        }

        public ViewResult Index()
        {
            var dict = db.Banor.ToDictionary(bana => bana.Namn, bana => db.Registreringar.Include("Klasser").Include("Kanoter").Include("Invoice").Include("Deltagare").Where(reg => reg.Bana == bana.ID).ToList());

            var viewData = new RegIndexContainer { Banor = new Dictionary<string, BanaData>() };

            foreach (var keyValuePair in dict)
            {
                keyValuePair.Value.OrderBy(reg => reg.Startnummer);
                viewData.Banor.Add(keyValuePair.Key, new BanaData { Registreringar = keyValuePair.Value });
            }

            return View(viewData);
        }

        //
        // GET: /Reg/

        public ViewResult Startlista()
        {
            var dict = db.Banor.ToDictionary(bana => bana, bana => db.Registreringar.Include("Klasser").Include("Kanoter").Where(reg => reg.Bana == bana.ID).ToList());

            foreach (var registreringar in dict.Values)
            {
                registreringar.OrderBy(reg => reg.Startnummer);
            }

            return View(dict);
        }

        //
        // GET: /Reg/Create
        public ActionResult Create()
        {
            var start = db.StartOchSlut.Where(startSlut => startSlut.Namn == "Start").FirstOrDefault();
            if (start != null && DateTime.Now < start.Datum && !Request.IsAuthenticated)
            {
                return View("RegNotOpen", start.Datum);
            }

            var slut = db.StartOchSlut.Where(startSlut => startSlut.Namn == "Slut").FirstOrDefault();
            if (slut != null && DateTime.Now >= slut.Datum && !Request.IsAuthenticated)
            {
                return View("RegClosed");
            }

            FillViewData();

            if (Session["reg"] != null)
            {
                return View(Session["reg"]);
            }
#if DEBUG
            var registrering = new Registreringar
                               {
                                   Lagnamn = "Röda laget",
                                   Adress = "Kätterud 313, 65591, Karlstad",
                                   Bana = 1,
                                   Klass = 1,
                                   Epost = "martin@epost.com",
                                   Kanot = 1,
                                   Klubb = "KMS",
                                   Deltagare = new TrackableCollection<Deltagare> {
                                       new Deltagare {Förnamn = "Martin", Efternamn = "Andersson"},
                                       new Deltagare {Förnamn = "Karin", Efternamn = "Larsson"}
                                   },
                                   Telefon = "1234234234",
                                   Forseningsavgift = Avgift.Forseningsavgift(db)
                               };
            return View(registrering);
#endif
            
            return View(new Registreringar {Deltagare = new TrackableCollection<Deltagare> {new Deltagare(), new Deltagare()}, Forseningsavgift = Avgift.Forseningsavgift(db)});
        } 

        //
        // POST: /Reg/Create
        [HttpPost]
        [PreventSpam]
        public ActionResult Create(Registreringar registrering, string sendReg, string Bana, string rabattkod)
        {
            log.Debug("New registration. Lagnamn: " + registrering.Lagnamn);

            try
            {
                // Changed bana
                //if (!string.IsNullOrEmpty(Bana) && string.IsNullOrEmpty(sendReg))
                //{
                //    TrimDeltagare(registrering);
                //    FillViewData();
                //    Session["reg"] = registrering;
                //    return RedirectToAction("Create");
                //}

                registrering.Registreringstid = DateTime.Now;
                registrering.Forseningsavgift = Avgift.Forseningsavgift(db);

                TryValidateModel(registrering);

                if (ModelState.IsValid)
                {
                    if (!string.IsNullOrEmpty(rabattkod))
                    {
                        var discount = db.Rabatter.FirstOrDefault(rabatt => rabatt.Kod.ToLower() == rabattkod.ToLower());
                        if (discount != null)
                        {
                            registrering.Rabatt = discount.Summa;
                            registrering.Kommentar += "Rabattkod använd: " + discount.Kod;
                        }
                        else
                        {
                            TempData["ValidationError"] = "Fel rabattkod angavs.";
                            return RedirectToAction("Create");
                        }
                    }

                    FillRegistrering(registrering);
                    Session["reg"] = registrering;

                    return RedirectToAction("AcceptReg");
                }

                FillViewData();

                return View(registrering);
            }
            catch (Exception exception)
            {
                return ShowError("Error in POST create.", exception);
            }
        }

        //
        // GET: /AcceptReg/
        public ActionResult AcceptReg()
        {
            var reg = (Registreringar)Session["reg"];
            log.Debug("AcceptReg. Lagnamn: " + reg.Lagnamn);
            FillViewData();
            return View(reg);
        }

        [HttpPost]
        public ActionResult AcceptReg(Registreringar registrering, string button)
        {
            try
            {
                if (!string.IsNullOrEmpty(button) && button == "Ändra")
                {
                    return RedirectToAction("Create");
                }

                if (!string.IsNullOrEmpty(button) && button == "Faktura *")
                {
                    return RedirectToAction("Invoice", "Checkout");
                }

                if (!string.IsNullOrEmpty(button) && (button == "Betala" || button == "Klar"))
                {
                    log.Debug("AcceptReg Betala. Lagnamn: " + registrering.Lagnamn);

                    if (Request.IsAuthenticated)
                    {
                        SaveNewRegistration();
                        return RedirectToAction("Index");
                    }

                    var reg = (Registreringar)Session["reg"];

                    // Free start
                    if (Avgift.Kalk(reg) == 0)
                    {
                        reg.HarBetalt = true;
                        SaveNewRegistration();
                        return RedirectToAction("Redirect");
                    }

                    var checkout = new CheckoutModel
                    {
                        Registrering = reg,
                        SenderFirstName = reg.Deltagare[0].Förnamn,
                        SenderLastName = reg.Deltagare[0].Efternamn,
                        SenderEmail = reg.Epost
                    };

                    Session["checkout"] = checkout;

                    return RedirectToAction("Checkout", "Checkout");
                }

                return View(registrering);
            }
            catch (Exception exception)
            {
                return ShowError("Error in POST AcceptReg.", exception);
            }
        }

        //
        // GET: /RegAdmin/Details/5
        [Authorize]
        public ViewResult Details(int id)
        {
            Registreringar registreringar =
                db.Registreringar.Include("Klasser").Include("Kanoter").Include("Banor").Include("Invoice").Include("Deltagare").Single(r => r.ID == id);
            ViewData["Forseningsavgift"] = Avgift.Forseningsavgift(db);
            return View(registreringar);
        }

        //
        // GET: /RegAdmin/Invoice/5
        [Authorize]
        public ViewResult Invoice(int id)
        {
            Registreringar registreringar = db.Registreringar.Include("Klasser").Include("Kanoter").Include("Banor").Include("Invoice").Include("Deltagare").Single(r => r.ID == id);
            ViewData["Forseningsavgift"] = registreringar.Forseningsavgift;
            return View(registreringar);
        }

        //
        // GET: /RegAdmin/EditInvoice/5
        [Authorize]
        public ViewResult EditInvoice(int id)
        {
            Invoice invoice = db.InvoiceSet.SingleOrDefault(i => i.Id == id) ?? new Invoice();
            return View(invoice);
        }

        //
        // POST: /RegAdmin/EditInvoice/5
        [Authorize]
        [HttpPost]
        public ActionResult EditInvoice(Invoice invoice)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var origInvoice = db.InvoiceSet.FirstOrDefault(inv => inv.Id == invoice.Id);
                    if (origInvoice == null)
                    {
                        var registreringar = (Registreringar)TempData["reg"];
                        var origReg = db.Registreringar.FirstOrDefault(reg => reg.ID == registreringar.ID);
                        origReg.Invoice = invoice;
                        db.Registreringar.ApplyCurrentValues(registreringar);
                    }
                    else
                    {   
                        db.InvoiceSet.ApplyCurrentValues(invoice);
                    }
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return View(invoice);
            }
            catch (Exception exception)
            {
                return ShowError("Error in POST EditInvioce.", exception);
            }
        }

        //
        // GET: /RegAdmin/Edit/5
        [Authorize]
        public ActionResult Edit(int id)
        {
            try
            {
                FillViewData();
                Registreringar registreringar = db.Registreringar.Include("Invoice").Include("Deltagare").Single(r => r.ID == id);
                TempData["reg"] = registreringar;
                return View("Create", registreringar);
            }
            catch (Exception exception)
            {
                return ShowError("Error in GET Edit.", exception);
            }
        }

        //
        // POST: /RegAdmin/Edit/5
        [Authorize]
        [HttpPost]
        public ActionResult Edit(Registreringar registreringar, string sendReg, string Bana)
        {
            try
            {
                // Changed bana
                if (!string.IsNullOrEmpty(Bana) && string.IsNullOrEmpty(sendReg))
                {
                    TrimDeltagare(registreringar);
                    FillViewData();
                    Session["reg"] = registreringar;
                    return View("Create", registreringar);
                }

                TryValidateModel(registreringar);

                if (ModelState.IsValid)
                {   
                    var origReg = db.Registreringar.Include("Deltagare").FirstOrDefault(reg => reg.ID == registreringar.ID);
                
                    // Remove old deltagare
                    DeleteDeltagare(origReg);
                    db.SaveChanges();

                    // Add all new
                    foreach (var deltagare in registreringar.Deltagare)
                    {
                        db.DeltagareSet.AddObject(deltagare);
                    }
                    db.SaveChanges();
                    registreringar.Registreringstid = origReg != null ? origReg.Registreringstid : DateTime.Now;
                    db.Registreringar.ApplyCurrentValues(registreringar);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }

                FillViewData();

                return View("Create", registreringar);
            }
            catch (Exception exception)
            {
                return ShowError("Error in POST Edit.", exception);
            }
        }

        //
        // GET: /RegAdmin/Delete/5
        [Authorize]
        public ActionResult Delete(int id)
        {
            try
            {
            Registreringar registreringar;
            try
            {
                registreringar = db.Registreringar.Include("Banor").Include("Kanoter").Single(r => r.ID == id);
                ViewData["Forseningsavgift"] = registreringar.Forseningsavgift;
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "Det finns ingen redistrering med det ID";
                return View();
            }
            return View(registreringar);
            }
            catch (Exception exception)
            {
                return ShowError("Error in GET Delete.", exception);
            }
        }

        //
        // POST: /RegAdmin/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                DeleteRegistrering(id);
                return RedirectToAction("Index");
            }
            catch (Exception exception)
            {
                return ShowError("Error in POST create.", exception);
            }
        }

        //
        // GET: /Reg/Mail/3
        public ActionResult Mail(int id)
        {
            var regg = db.Registreringar.Include("Deltagare").SingleOrDefault(reg => reg.ID == id);
            FillViewData();
            return View("_MailView", regg);
        }

        //
        // GET: /Reg/Redirect
        public ActionResult Redirect()
        {
            return View();
        }

        //
        // GET: /Reg/RegClosed
        public ActionResult RegClosed()
        {
            return View();
        }

        private static void TrimDeltagare(Registreringar registrering)
        {
            var bana = db.Banor.FirstOrDefault(b => b.ID == registrering.Bana);
            if (registrering.Deltagare.Count > bana.AntalDeltagare)
            {
                for (var i = registrering.Deltagare.Count - 1; i > bana.AntalDeltagare - 1; i--)
                {
                    registrering.Deltagare.RemoveAt(i);
                }
            }
            else if (registrering.Deltagare.Count < bana.AntalDeltagare)
            {
                for (var i = registrering.Deltagare.Count; i < bana.AntalDeltagare; i++)
                {
                    registrering.Deltagare.Add(new Deltagare());
                }
            }
        }

        //public ActionResult ShowError(string logMessage, Exception exception = null)
        //{
        //    log.Error(logMessage, exception);
        //    if (exception != null)
        //    {
        //        SendMail.SendErrorMessage(logMessage + "\n\n" + exception.Message + "\n\n" + exception.StackTrace);
        //    }
        //    else
        //    {
        //        SendMail.SendErrorMessage(logMessage);
        //    }

        //    TempData["Error"] = "Fel vid anmälna. Administratör är kontaktad. Vill du veta när felet blivit åtgärdat skicka gärna ett meddelande till utmaningen@karlstadmultisport.se";

        //    return RedirectToAction("Create");
        //}
    }
}
