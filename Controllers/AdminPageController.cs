/*
 * @file AdminPageController.cs
 * @name ArtSchool - Equipment Loan System (Project to subject IIS, FIT VUT)
 * @author Halva Jindřich (xhalva05)
 * @brief This file contains methods for the admin page.
 */

using ArtSchool.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ArtSchool.Controllers;

[Authorize(Roles = "admin")]
public class AdminPageController : Controller
{
	private readonly MyDBContext _context;

	public AdminPageController(MyDBContext context)
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

		//list of all users
		var userlist = _context.Person.ToList();

		//list of equipments, admin sees all the equipment
		var equipmentList = _context.Equipment
							.Include(et => et.EquipmentType)
							.Include(a => a.Atelier)
							.ToList();

		//list of all equipment types
		var typesList = _context.EquipmentType.ToList();
		
		var ateliersList = _context.Atelier.ToList();

		//list of admin's loans
		var reservationList = _context.Reservation
							 .Where(r => r.Id_user == Id_user_int)
							 .Include(e => e.Equipment)
							 .ToList();

        //list of all loans in the system
        var allLoans = _context.Loan
                             .Include(e => e.Equipment)
                             .ToList();

        //list of admin's reservations
        var loanList = _context.Loan
							 .Where(l => l.Id_user == Id_user_int)
							 .Include(e => e.Equipment)
							 .ToList();

		//list of reservation requests
		var reservationRequestsList = _context.Reservation
							 .Include(e => e.Equipment)
							 .Include(p => p.Person)
							 .ToList();

		//send lists with data by ViewData
		ViewData["Users"] = userlist;
		ViewData["Equipments"] = equipmentList;
		ViewData["Loans"] = loanList;
		ViewData["Reservations"] = reservationList;
		ViewData["Types"] = typesList;
		ViewData["Ateliers"] = ateliersList;
		ViewData["ReservationRequests"] = reservationRequestsList;
        ViewData["AllLoans"] = allLoans;

        return View();
	}

	//show atelier users on new page
	public IActionResult AtelierDetails(int Id_atelier, string returnUrl)
	{
		//get atelier by id
		var atelier = _context.Atelier
						.Include(e => e.Equipments)
						.FirstOrDefault(a => a.Id_atelier == Id_atelier);

		//get all users in atelier
		var users = _context.AtelierPersons
					.Where(ap => ap.Id_atelier == Id_atelier)
					.Include(p => p.Person)
					.ToList();


		if (atelier == null) {
			return RedirectToAction("Index", "AdminPage");
		}

		//send data to view
		ViewData["ReturnUrl"] = returnUrl;
		ViewData["Atelier"] = atelier;
		ViewData["AtelierId"] = atelier.Id_atelier;
		ViewData["AtelierName"] = atelier.Name;
		ViewData["Users"] = users;
		return View();
	}
}
