/*
 * @file AtelierUsersController.cs
 * @name ArtSchool - Equipment Loan System (Project to subject IIS, FIT VUT)
 * @author Halva Jindřich (xhalva05)
 * @brief This file contains methods that operate with concrete atelier users.
 */

using ArtSchool.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ArtSchool.Controllers
{
	public class AtelierUsersController : Controller
	{
		//get context from database
		private readonly MyDBContext _context;

		public AtelierUsersController(MyDBContext context)
		{
			_context = context;
		}

		public ActionResult Index(int atelier_id, string returnUrl)
		{
			//get all users that are not in the atelier
			var atelier_users = _context.Person
							.Where(p => p.AtelierPersons.All(ap => ap.Id_atelier != atelier_id))
							.ToList();

			ViewData["AtelierUsers"] = atelier_users;
			ViewData["AtelierId"] = atelier_id;
			ViewData["ReturnUrl"] = returnUrl;

			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(int atelier_id, int Id_User, string returnUrl)
		{
			try{
				var atelierPerson = new AtelierPersons { Id_atelier = atelier_id, Id_user = Id_User };

				//add user to the atelier and save changes in the database
				_context.AtelierPersons.Add(atelierPerson);
				_context.SaveChanges();

				TempData["Success"] = "Uživatel byl úspěšně přidán do ateliéru";

				return RedirectToAction("Index", new { atelier_id, returnUrl });
			}
			catch{
				TempData["Error"] = "Vyskytla se chyba při přidání uživatele do ateliéru";

				//Refresh data for Index view
				var atelier_users = _context.Person
								.Where(p => p.AtelierPersons.All(ap => ap.Id_atelier != atelier_id))
								.ToList();

				ViewData["AtelierUsers"] = atelier_users;
				ViewData["AtelierId"] = atelier_id;
				ViewData["ReturnUrl"] = returnUrl;

				return View("Index");
			}
		}

		public ActionResult Edit(int Id_User, string returnUrl)
		{
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
		public ActionResult Edit(int Id_User,Person edited_person, string returnUrl)
		{

            try{
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
            catch{
                TempData["Error"] = "Vyskytla se chyba při úpravě role uživatele.";
			}

            ViewData["ReturnUrl"] = returnUrl;
            return View(edited_person);
        }

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Delete(int Id_User, string returnUrl)
		{
			try{
				var atelierPerson = _context.AtelierPersons.FirstOrDefault(ap => ap.Id_user == Id_User);
				//get all user's reservation that are connected to the atelier's equipment
				var reservationList = _context.Reservation
								.Where(r => r.Equipment.Id_atelier == atelierPerson.Id_atelier && r.Id_user == Id_User)
								.ToList();

				//delete all user's reservations if there are any
				if (reservationList.Any())
				{
					foreach (var reservation in reservationList)
					{
						_context.Reservation.Remove(reservation);
					}
				}

				//remove user from the atelier
				_context.AtelierPersons.Remove(atelierPerson);
				_context.SaveChanges();

				TempData["Success"] = "Uživatel byl úspěšně odebrán z ateliéru";
			}
			catch(Exception ex){
				TempData["Error"] = "Vyskytla se chyba při odebírání uživatele z ateliéru" + ex.Message;
			}
			return Redirect(returnUrl);
		}
	}
}
