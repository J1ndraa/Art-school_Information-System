/*
 * @file LoanController.cs
 * @name ArtSchool - Equipment Loan System (Project to subject IIS, FIT VUT)
 * @author Halva Jindřich (xhalva05)
 * @brief This file contains methods that operate with loans.
 */

using ArtSchool.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtSchool.Controllers
{
	public class LoanController : Controller
	{
		//get context from database
		private readonly MyDBContext _context;

		public LoanController(MyDBContext context)
		{
			_context = context;
		}

		public IActionResult Return(int id, string returnUrl)
		{
			//find loan by id
			var loan = _context.Loan.FirstOrDefault(l => l.Id_loan == id);

			try
			{
				//remove loan
				_context.Loan.Remove(loan);

				_context.SaveChanges();

				TempData["success"] = "Výpujčka byla úspěšně odstraněna.";
			}
			catch (Exception ex)
			{
				TempData["error"] = "Při odstraňování výpujčky došlo k chybě: " + ex;
			}

			return Redirect(returnUrl);
		}
	}
}
