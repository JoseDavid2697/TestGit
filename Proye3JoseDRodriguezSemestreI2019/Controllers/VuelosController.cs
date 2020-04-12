using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Proye3JoseDRodriguezSemestreI2019.Controllers
{
    public class VuelosController : Controller
    {
        // GET: Vuelos
        public ActionResult Index()
        {
            return View();
        }

        // GET: Vuelos/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Vuelos/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Vuelos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Vuelos/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Vuelos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Vuelos/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Vuelos/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}