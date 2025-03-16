/*
 * @file UserPageController.cs
 * @name ArtSchool - Equipment Loan System (Project to subject IIS, FIT VUT)
 * @author Halva Jindřich (xhalva05)
 * @brief This file contains methods for UserPage.
 */

using ArtSchool.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ArtSchool.Controllers;

[Authorize(Roles = "user")]
public class UserPageController : Controller
{
	private readonly MyDBContext _context;

	public UserPageController(MyDBContext context)
	{
		_context = context;
	}

	public IActionResult Index()
    {
        //get user id from claims
        var Id_user = User.FindFirst("Id_user")?.Value;

        if (Id_user == null)
		{
			return RedirectToAction("Index", "LoginPage");
		}

        //convert string id to int
        int Id_user_int = int.Parse(Id_user);

        //list of equipments from ateliers, that user is in
        var equipmentList = _context.AtelierPersons
                             .Where(ap => ap.Id_user == Id_user_int) 
                             .SelectMany(ap => ap.Atelier.Equipments)
                             .Include(et => et.EquipmentType)
                             .Include(a => a.Atelier)
							 .ToList();
        
        var loanList = _context.Loan
							 .Where(l => l.Id_user == Id_user_int)
							 .Include(e => e.Equipment)
							 .ToList();

		var reservationList = _context.Reservation
							 .Where(r => r.Id_user == Id_user_int)
							 .Include(e => e.Equipment)
							 .ToList();

		//send lists with data by ViewData
		ViewData["Equipments"] = equipmentList;
		ViewData["Loans"] = loanList;
		ViewData["Reservations"] = reservationList;

		return View();
    }
}

