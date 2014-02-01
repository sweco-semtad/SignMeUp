using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UtmaningenReg;

namespace UtmaningenReg.Controllers
{
    [Authorize]
    public class RabattController : Controller
    {
        private UtmaningenEntities db = new UtmaningenEntities();

        //
        // GET: /Rabatt/

        public ViewResult Index()
        {
            return View(db.Rabatter.ToList());
        }

        //
        // GET: /Rabatt/Details/5

        public ViewResult Details(int id)
        {
            Rabatt rabatt = db.Rabatter.Single(r => r.Id == id);
            return View(rabatt);
        }

        //
        // GET: /Rabatt/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Rabatt/Create

        [HttpPost]
        public ActionResult Create(Rabatt rabatt)
        {
            if (ModelState.IsValid)
            {
                db.Rabatter.AddObject(rabatt);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(rabatt);
        }
        
        //
        // GET: /Rabatt/Edit/5
 
        public ActionResult Edit(int id)
        {
            Rabatt rabatt = db.Rabatter.Single(r => r.Id == id);
            return View(rabatt);
        }

        //
        // POST: /Rabatt/Edit/5

        [HttpPost]
        public ActionResult Edit(Rabatt rabatt)
        {
            if (ModelState.IsValid)
            {
                db.Rabatter.Attach(rabatt);
                db.ObjectStateManager.ChangeObjectState(rabatt, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(rabatt);
        }

        //
        // GET: /Rabatt/Delete/5
 
        public ActionResult Delete(int id)
        {
            Rabatt rabatt = db.Rabatter.Single(r => r.Id == id);
            return View(rabatt);
        }

        //
        // POST: /Rabatt/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            Rabatt rabatt = db.Rabatter.Single(r => r.Id == id);
            db.Rabatter.DeleteObject(rabatt);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}