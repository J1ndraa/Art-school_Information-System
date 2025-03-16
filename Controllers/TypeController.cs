/*
 * @file TypeController.cs
 * @name ArtSchool - Equipment Loan System (Project to subject IIS, FIT VUT)
 * @author Halva Jindřich (xhalva05)
 * @brief This file contains methods that operate equipment types.
 */

using ArtSchool.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Drawing2D;

namespace ArtSchool.Controllers
{
	[Authorize(Roles = "atelier_manager, admin")]
	public class TypeController : Controller
	{

		private readonly MyDBContext _context;

		public TypeController(MyDBContext context)
		{
			_context = context;
		}

		// GET: Type/Create
		[HttpGet]
		public ActionResult Create(string returnUrl)
		{
			ViewData["ReturnUrl"] = returnUrl;
			return View();
		}

		//create new type
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(EquipmentType new_type, string returnUrl)
		{
			if (ModelState.IsValid) {
				try
				{
					//add new type to database
					_context.EquipmentType.Add(new_type);
					_context.SaveChanges();
                    TempData["success"] = "Nový typ zařízení úspěšně přidán.";
                    return Redirect(returnUrl);
				}
				catch
				{
                    TempData["Error"] = "Vyskytla se chyba při vytváření ateliéru.";
                }
			}
			ViewData["ReturnUrl"] = returnUrl;
			return View(new_type);
		}

		[HttpGet]
		public ActionResult Edit(int id, string returnUrl)
		{
			//find type by id
			var type = _context.EquipmentType.FirstOrDefault(t => t.Id_type == id);
			//if type was not found
			if (type == null)
            {
				ModelState.AddModelError("", "Došlo k chybě při úpravě typu zařízení.");
                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(type);
        }

		//edit type 
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(int id, EquipmentType edited_type, string returnUrl)
		{
            if (ModelState.IsValid)
            {
                try
                {
                    _context.EquipmentType.Update(edited_type);
                    _context.SaveChanges();
                    TempData["success"] = "Typ zařízení byl úspěšně upraven.";
                    return Redirect(returnUrl);
                }
                catch
                {
                    TempData["Error"] = "Vyskytla se chyba při úpravě ateliéru.";
                }
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View(edited_type);
        }

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Delete(int id, string returnUrl)
		{
			//find type include equipments of this type
			var equipmentType = _context.EquipmentType
				   .Include(et => et.Equipments)
				   .FirstOrDefault(et => et.Id_type == id);

			try
			{
				//delete all equipments of this type
				foreach (var equipment in equipmentType.Equipments)
				{
					_context.Equipment.Remove(equipment);
				}

				//remove type
				_context.EquipmentType.Remove(equipmentType);

				_context.SaveChanges();

				TempData["success"] = "Typ zařízení byl úspěšně odstraněn.";
			}
			catch (Exception ex)
			{
				TempData["error"] = "Při odstraňování typu zařízení došlo k chybě: " + ex;
			}

			return Redirect(returnUrl);
		}
	}
}
