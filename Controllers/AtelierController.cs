/*
 * @file AtelierController.cs
 * @name ArtSchool - Equipment Loan System (Project to subject IIS, FIT VUT)
 * @author Halva Jindřich (xhalva05)
 * @brief This file contains methods that operate with ateliers.
 */

using ArtSchool.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace ArtSchool.Controllers
{
	public class AtelierController : Controller
	{
		//get context from database
		private readonly MyDBContext _context;

		public AtelierController(MyDBContext context)
		{
			_context = context;
		}

        // GET: Atelier/Create
        public ActionResult Create(string returnUrl)
		{
            ViewData["ReturnUrl"] = returnUrl;
            return View();
		}

        //create new atelier
        [HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(Atelier atelier, string returnUrl)
		{
			try
			{
				//add atelier to the database
				_context.Atelier.Add(atelier);
				_context.SaveChanges();

				TempData["Success"] = "Ateliér byl úspěšně vytvořen.";

				return Redirect(returnUrl);
			}
			catch
			{
				TempData["Error"] = "Vyskytla se chyba při vytváření ateliéru.";
				return View(atelier);
			}
		}

        // GET: Atelier/Edit
        public ActionResult Edit(int Id_atelier, string returnUrl)
        {
            //find atelier by id
            var atelier = _context.Atelier.FirstOrDefault(a => a.Id_atelier == Id_atelier);

            ViewData["ReturnUrl"] = returnUrl;
            return View(atelier);
        }

        //edit atelier
        [HttpPost]
		[ValidateAntiForgeryToken]
        public ActionResult Edit(int Id_atelier, Atelier edited_atelier, string returnUrl)
		{
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Atelier.Update(edited_atelier);
                    _context.SaveChanges();
                    TempData["success"] = "Ateliér byl úspěšně upraven.";
                    return Redirect(returnUrl);
                }
                catch
                {
                    TempData["Error"] = "Vyskytla se chyba při úpravě ateliéru.";
                }
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View(edited_atelier);
        }

        [HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Delete(int Id_atelier, string returnUrl)
		{
			try
			{
				//find atelier by id
				var atelier = _context.Atelier.FirstOrDefault(a => a.Id_atelier == Id_atelier);
				if (atelier == null)
				{
					TempData["Error"] = "Ateliér nebyl nalezen.";
					return Redirect(returnUrl);
				}

				//get all equipment that is connected to the atelier
				var equipmentList = _context.Equipment
								.Where(e => e.Id_atelier == Id_atelier)
								.ToList();

				//get all reservations that are connected to the atelier's equipment
				var reservationList = _context.Reservation
								.Where(r => r.Equipment.Id_atelier == Id_atelier)
								.ToList();

				//get all loans that are connected to the atelier's equipment
				var loanList = _context.Loan
								.Where(l => l.Equipment.Id_atelier == Id_atelier)
								.ToList();

				 var person_connections = _context.AtelierPersons
								.Where(ap => ap.Id_atelier == Id_atelier)
								.ToList();

				//delete all equipment that is connected to the atelier
				if (equipmentList != null){
					_context.Equipment.RemoveRange(equipmentList);}

				//delete all reservations
				if (reservationList != null){
					_context.Reservation.RemoveRange(reservationList);}

				//delete all loans
				if (loanList != null){
					_context.Loan.RemoveRange(loanList);}

				//delete connections between atelier and persons
				if (person_connections != null){
					_context.AtelierPersons.RemoveRange(person_connections);}

				//delete atelier
				_context.Atelier.Remove(atelier);
				_context.SaveChanges();

				TempData["Success"] = "Ateliér byl úspěšně smazán.";
				return Redirect(returnUrl);
			}
			catch
			{
				TempData["Error"] = "Vyskytla se chyba při mazání ateliéru.";
				return Redirect(returnUrl);
			}
		}
	}
}
