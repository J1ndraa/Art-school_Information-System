/*
 * @file AtelierManagerPageController.cs
 * @name ArtSchool - Equipment Loan System (Project to subject IIS, FIT VUT)
 * @author Halva Jindřich (xhalva05)
 * @brief This file contains methods for AtelierManagerPage. 
 */

using ArtSchool.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;


namespace ArtSchool.Controllers;

[Authorize(Roles = "atelier_manager")]
public class AtelierManagerPageController : Controller
{
	private readonly MyDBContext _context;

	public AtelierManagerPageController(MyDBContext context)
	{
		_context = context;
	}

	public IActionResult Index()
	{
		var Id_user = User.FindFirst("Id_user")?.Value;

		if (Id_user == null)
		{
			return RedirectToAction("Index", "LoginPage");
		}

		//convert string id to int
		int Id_user_int = int.Parse(Id_user);

		//every manager is connected to one atelier so we can get it
		int Id_users_atelier = _context.AtelierPersons
						.Where(ap => ap.Id_user == Id_user_int)
						.Select(ap => ap.Id_atelier)
						.FirstOrDefault();

		//list of equipments from atelier, that manager is in
		var equipmentList = _context.AtelierPersons
							 .Where(ap => ap.Id_user == Id_user_int)
							 .SelectMany(ap => ap.Atelier.Equipments)
							 .Include(et => et.EquipmentType)
							 .Include(a => a.Atelier)
							 .ToList();

		//list of loans
		var loanList = _context.Loan
							 .Where(l => l.Id_user == Id_user_int)
							 .Include(e => e.Equipment)
							 .ToList();

		//list of reservations
		var reservationsList = _context.Reservation
							 .Where(r => r.Id_user == Id_user_int)
							 .Include(e => e.Equipment)
							 .ToList();

		//get users that are connected to atelier of the manager
		var userslist = _context.AtelierPersons
						.Where(ap => ap.Id_atelier == Id_users_atelier)
						.Include(p => p.Person)
						.ToList();

		var managers_atelierList = _context.Atelier
						.Where(a => a.Id_atelier == Id_users_atelier)
						.ToList();		

		var typesList = _context.EquipmentType
						.ToList();

		//send lists with data by ViewData
		ViewData["Equipments"] = equipmentList;
		ViewData["Loans"] = loanList;
		ViewData["Reservations"] = reservationsList;
		ViewData["Users"] = userslist;
		ViewData["Types"] = typesList;
		ViewData["Atelier"] = managers_atelierList;
		ViewData["AtelierName"] = _context.Atelier
								.Where(a => a.Id_atelier == Id_users_atelier)
								.Select(a => a.Name)
								.FirstOrDefault();
		ViewData["AtelierId"] = Id_users_atelier;

		return View();
	}
}

