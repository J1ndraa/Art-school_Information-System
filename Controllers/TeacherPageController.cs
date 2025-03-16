/*
 * @file TeacherPageController.cs
 * @name ArtSchool - Equipment Loan System (Project to subject IIS, FIT VUT)
 * @author Halva Jindřich (xhalva05)
 * @brief This file contains methods for the teacher page.
 */

using ArtSchool.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ArtSchool.Controllers;

[Authorize(Roles = "teacher")]
public class TeacherPageController : Controller
{
	private readonly MyDBContext _context;

	public TeacherPageController(MyDBContext context)
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

		//select teacher's ateliers
		var teacherAteliers = _context.AtelierPersons
							.Where(ap => ap.Id_user == Id_user_int)
							.Select(ap => ap.Id_atelier)
							.ToList();

		//list of equipments from ateliers, that user is in
		var equipmentList = _context.AtelierPersons
							 .Where(ap => ap.Id_user == Id_user_int)
							 .SelectMany(ap => ap.Atelier.Equipments)
							 .Include(et => et.EquipmentType)
							 .Include(a => a.Atelier)
							 .ToList();

		//list of user's loans
		var loanList = _context.Loan
							 .Where(l => l.Id_user == Id_user_int)
							 .Include(e => e.Equipment)
							 .ToList();

		//list of user's reservations
		var reservationList = _context.Reservation
							 .Where(r => r.Id_user == Id_user_int)
							 .Include(e => e.Equipment)
							 .ToList();

		//list of loans, that are from users, that are connected to at least one atelier same as the teacher
		var atelierLoansList = _context.Loan
							 .Where(l => l.Person.AtelierPersons.Any(ap => teacherAteliers.Contains(ap.Id_atelier)))
							 .Include(e => e.Equipment)
							 .Include(p => p.Person)
							 .ToList();

		//list of reservation requests, that are from users, that are connected to
		//at least one atelier same as the teacher
		var reservationRequestsList = _context.Reservation
								.Where(r => r.Person.AtelierPersons.Any(ap => teacherAteliers.Contains(ap.Id_atelier)))
								.Include(e => e.Equipment)
								.Include(p => p.Person)
								.ToList();

		//send lists with data by ViewData
		ViewData["Equipments"] = equipmentList;
		ViewData["Loans"] = loanList;
		ViewData["AtelierLoans"] = atelierLoansList;
		ViewData["Reservations"] = reservationList;
		ViewData["ReservationRequests"] = reservationRequestsList;

		return View();
	}
}
