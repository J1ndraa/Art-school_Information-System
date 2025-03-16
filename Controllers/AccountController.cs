/*
 * @file    AccountController.cs
 * @name    ArtSchool - Equipment Loan System (Project for the IIS subject, FIT VUT)
 * @author  Marek Čupr (xcuprm01)
 * @brief   Controller for managing the user account actions.
 */

using ArtSchool.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtSchool.Controllers
{
    public class AccountController : Controller
    {
        // Database context
        private readonly MyDBContext _context;

        public AccountController(MyDBContext context) 
        {
            _context = context;
        }

        private async Task<Person> GetUserAsync()
        {
            // Get the user ID
            var userId = User.FindFirst("Id_user")?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return null;
            }

            // Retrieve the user from the database
            return await _context.Person.FirstOrDefaultAsync(u => u.Id_user == userIdInt);
        }

        public async Task<IActionResult> Index()
        {
            // Get the current user from the database
            var user = await GetUserAsync();
            if (user == null)
            {
                TempData["error"] = "Přístup zamítnut, přihlaste se prosím.";
                return RedirectToAction("Index", "LoginPage");
            }

            // Return the view with the user details
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> RedirectToDashboard()
        {
            // Get the current user from the database
            var user = await GetUserAsync();
            if (user == null)
            {
                return RedirectToAction("Index", "LoginPage");
            }

            // Redirect the user based on his role
            return user.User_role switch
            {
                "admin" => RedirectToAction("Index", "AdminPage"),
                "atelier_manager" => RedirectToAction("Index", "AtelierManagerPage"),
                "teacher" => RedirectToAction("Index", "TeacherPage"),
                "user" => RedirectToAction("Index", "UserPage"),
                _ => RedirectToAction("Index", "LoginPage"),
            };
        }

            [HttpGet]
        public async Task<IActionResult> Edit()
        {
            // Get the current user from the database
            var user = await GetUserAsync();
            if (user == null)
            {
                TempData["error"] = "Přístup zamítnut, přihlaste se prosím.";
                return RedirectToAction("Index", "LoginPage");
            }

            // Return the view with the user details
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Person obj)
        {
            // Get the current user from the database
            var user = await GetUserAsync();
            if (user == null)
            {
                TempData["error"] = "Přístup zamítnut, přihlaste se prosím.";
                return RedirectToAction("Index", "LoginPage");
            }

            // Check if the email is unique
            if (user.Email != obj.Email)
            {
                var emailExists = await _context.Person.AnyAsync(u => u.Email == obj.Email && u.Id_user != obj.Id_user);
                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Tento e-mail je již používán jiným uživatelem.");
                }
            }

            // Don't validate the password
            ModelState.Remove("Pwd");

            // Validate the input model
            if (!ModelState.IsValid)
            {
                // Return the view with user details
                TempData["error"] = "Došlo k chybě při aktualizaci údajů.";
                return View(obj);
            }

            // Check if any user details have been changed
            if (user.Firstname == obj.Firstname && user.Surname == obj.Surname && user.Email == obj.Email)
            {
                TempData["info"] = "Žádné změny nebyly provedeny.";
                return RedirectToAction("Index", "Account");
            }

            // Update the user details
            user.Firstname = obj.Firstname;
            user.Surname = obj.Surname;
            user.Email = obj.Email;

            // Try to update the user in the database
            try
            {
                _context.Person.Update(user);
                await _context.SaveChangesAsync();
                TempData["success"] = "Osobní údaje byly úspěšně aktualizovány.";
            }
            catch (Exception)
            {
                TempData["error"] = "Došlo k chybě při aktualizaci údajů, zkuste to znovu.";
            }

            // Redirect after successfully updating user details
            return RedirectToAction("Index", "Account");
        }
    }
}

// End of AccountController.cs