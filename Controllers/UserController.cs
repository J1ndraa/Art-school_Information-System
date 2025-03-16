/*
 * @file UserController.cs
 * @name ArtSchool - Equipment Loan System (Project to subject IIS, FIT VUT)
 * @author Halva Jindřich (xhalva05)
 * @brief This file contains methods that operate with users.
 */

using ArtSchool.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ArtSchool.Controllers
{
    public class UserController : Controller
    {
        //get context from database
        private readonly MyDBContext _context;

        public UserController(MyDBContext context)
        {
            _context = context;
        }

        public ActionResult Edit(int Id_User, string returnUrl)
        {
			//find user by id
			var user = _context.Person.FirstOrDefault(p => p.Id_user == Id_User);
            var name = user.Firstname + " " + user.Surname;

            ViewData["User"] = user;
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["UserName"] = name;

            return View(user);
        }

		//edit user role
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(int Id_User, Person edited_person, string returnUrl)
		{
			try	{
				//find user by id
				var user = _context.Person.FirstOrDefault(p => p.Id_user == Id_User);
				if (user != null)
				{
					//set new role
					user.User_role = edited_person.User_role;
					_context.Person.Update(user);
					_context.SaveChanges();

					//on success redirect to the previous page
					TempData["success"] = "Role byla úspěšně upravena.";
					return Redirect(returnUrl);
				}
			}
			catch {
				ModelState.AddModelError("", "Došlo k chybě při ukládání změn.");
			}

			ViewData["ReturnUrl"] = returnUrl;
			return View(edited_person);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Delete(int Id_User, string returnUrl)
		{
			try
			{
				//find user by id
				var user = _context.Person.FirstOrDefault(p => p.Id_user == Id_User);
				if (user == null)
				{
					TempData["Error"] = "Uživatel nebyl nalezen.";
					return Redirect(returnUrl);
				}

				//get all user's reservation
				var reservationList = _context.Reservation.Where(r => r.Id_user == Id_User).ToList();
				//get all user's loans
				var loanList = _context.Loan.Where(l => l.Id_user == Id_User).ToList();
				//get all user's atelier connections
				var atelierConnections = _context.AtelierPersons.Where(ap => ap.Id_user == Id_User).ToList();

				//delete all user's reservations if there are any
				if (reservationList != null){
					_context.Reservation.RemoveRange(reservationList);}

				//delete all user's loans if there are any
				if (loanList != null){
					_context.Loan.RemoveRange(loanList);}

				//delete all user's atelier connections if there are any
				if (atelierConnections != null){
					_context.AtelierPersons.RemoveRange(atelierConnections);}

				//delete user
				_context.Person.Remove(user);
				_context.SaveChanges();

				TempData["Success"] = "Uživatel byl úspěšně odebrán ze systému";
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Vyskytla se chyba při odebírání uživatele: " + ex.Message;
			}
			return Redirect(returnUrl);
		}
	}
}
