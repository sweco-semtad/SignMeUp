using System;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using log4net;
using UtmaningenReg.Helpers;

namespace UtmaningenReg.Controllers
{
    public class RegBaseController : Controller
    {
        protected static readonly UtmaningenEntities db = new UtmaningenEntities();
        private readonly ILog log;

        public RegBaseController()
        {   
            log = LogManager.GetLogger(GetType());
        }

        protected string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        protected void SaveNewRegistration(Registreringar reg = null)
        {
            if (reg == null)
            {
                reg = (Registreringar)Session["reg"];
            }

            reg.Banor = null;
            reg.Kanoter = null;
            reg.Klasser = null;
            
            try
            {
                db.Registreringar.AddObject(reg);
                db.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    log.Error(string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State));
                    foreach (var ve in eve.ValidationErrors)
                    {
                        log.Error(string.Format("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage));
                    }
                }
                throw;
            }
            Session["reg"] = null;
            Session["checkout"] = null;
        }

        protected void SaveChanges(Registreringar reg)
        {
            db.Registreringar.ApplyCurrentValues(reg);
            db.SaveChanges();
        }

        protected void FillRegistrering(Registreringar registrering)
        {
            registrering.Kanoter = db.Kanoter.First(kanot => kanot.ID == registrering.Kanot);
            registrering.Banor = db.Banor.First(bana => bana.ID == registrering.Bana);
            registrering.Klasser = db.Klasser.First(klass => klass.ID == registrering.Klass);
        }

        protected void FillViewData()
        {
            ViewData["Kanoter"] =
                from kanot in db.Kanoter.ToList()
                select new SelectListItem
                {
                    Text = kanot.Namn + " (" + kanot.Avgift + " kr)",
                    Value = kanot.ID.ToString()
                };

            ViewData["Banor"] =
                from bana in db.Banor.ToList()
                select new SelectListItem
                {
                    Text = bana.Namn + " (" + bana.Avgift + " kr)",
                    Value = bana.ID.ToString()
                };

            ViewData["Klasser"] = new SelectList(db.Klasser.ToList(), "ID", "Namn");
            ViewData["Forseningsavgift"] = Avgift.Forseningsavgift(db);
        }

        protected void DeleteRegistrering(int id)
        {
            Registreringar registreringar = db.Registreringar.Include("Invoice").Include("Deltagare").Single(r => r.ID == id);
            if (registreringar.Invoice != null)
            {
                // Cascade delete...
                db.InvoiceSet.DeleteObject(registreringar.Invoice);
            }
            DeleteDeltagare(registreringar);
            db.Registreringar.DeleteObject(registreringar);
            db.SaveChanges();
        }

        protected static void DeleteDeltagare(Registreringar registrering)
        {
            var deltagareToDelete = db.DeltagareSet.Where(delt => delt.RegistreringarID == registrering.ID);
            foreach (var deltagare in deltagareToDelete)
            {
                db.DeltagareSet.DeleteObject(deltagare);
            }
        }

        protected ActionResult ShowError(string logMessage, Exception exception = null)
        {
            log.Error(logMessage, exception);
            if (exception != null)
            {
                SendMail.SendErrorMessage(logMessage + "\n\n" + exception.Message + "\n\n" + exception.StackTrace);
            }
            else
            {
                SendMail.SendErrorMessage(logMessage);
            }

            //TempData["Error"] = "Fel vid anmälna. Administratör är kontaktad. Vill du veta när felet blivit åtgärdat skicka gärna ett meddelande till utmaningen@karlstadmultisport.se";

            return View("Error");
        }
    }
}