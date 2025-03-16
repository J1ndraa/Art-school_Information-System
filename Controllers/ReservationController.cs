/*
 * @file    ReservationController.cs
 * @name    ArtSchool - Equipment Loan System (Project for the IIS subject, FIT VUT)
 * @author  Marek Čupr (xcuprm01)
 * @brief   Controller for managing the reservation actions.
 */

using ArtSchool.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArtSchool.Controllers
{
    public class ReservationController : Controller
    {
        // Database context
        private readonly MyDBContext _context;

        public ReservationController(MyDBContext context)
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

        private IActionResult RedirectToRolePage(Person user)
        {
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

        public async Task<IActionResult> Index(int id)
        {
            // Get the current user from the database
            var user = await GetUserAsync();
            if (user == null)
            {
                TempData["error"] = "Přístup zamítnut, přihlaste se prosím.";
                return RedirectToAction("Index", "LoginPage");
            }

            // Retrieve the equipment from the database
            var equipment = await _context.Equipment.FirstOrDefaultAsync(e => e.Id_equipment == id);
            if (equipment == null)
            {
                TempData["error"] = "Zařízení nenalezeno.";
                return RedirectToRolePage(user);
            }

            // Assign the corresponding equipment and id
            ViewData["Equipment"] = equipment;
            ViewData["Id"] = id;

            // Return the view
            return View();
        }

        [HttpGet]
        public IActionResult GetReservations(int id)
        {
            // Retrieve the reservations from the database
            var reservations = _context.Reservation
                .Where(r => r.Id_equipment == id)
                .Include(r => r.Equipment)
                .Include(r => r.Person)
                .ToList();

            // Create reservation events for the calendar
            var reservationEvents = reservations.Select(r => new
            {
                type = "reservation",
                title = $"Rezervace zařízení {r.Equipment.Name} - {r.Person.Firstname} {r.Person.Surname}",
                start = r.DateOfReservation.ToString("yyyy-MM-ddTHH:mm:ss"),
                end = r.DateOfEnd.ToString("yyyy-MM-ddTHH:mm:ss")
            }).ToList();

            // Retrieve the loans from the database
            var loans = _context.Loan
                .Where(l => l.Id_equipment == id)
                .Include(l => l.Equipment)
                .Include(l => l.Person)
                .ToList();

            // Create loan events for the calendar
            var loanEvents = loans.Select(l => new
            {
                type = "loan",
                title = $"Výpůjčka zařízení {l.Equipment.Name} - {l.Person.Firstname} {l.Person.Surname}",
                start = l.DateOfLoan.ToString("yyyy-MM-ddTHH:mm:ss"),
                end = l.DateOfReturn.ToString("yyyy-MM-ddTHH:mm:ss")
            }).ToList();

            // Combine the reservations and loans
            var allEvents = reservationEvents.Concat(loanEvents).ToList();

            // Return the calendar events as JSON
            return Json(allEvents);
        }

        [HttpPost]
        public async Task<IActionResult> AddReservation(Reservation obj)
        {
            // Get the user ID
            var userId = User.FindFirst("Id_user")?.Value;

            // Check if the user ID is valid
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return RedirectToAction("Index", "LoginPage");
            }

            // Retrieve the user from the database
            var user = await _context.Person.FirstOrDefaultAsync(u => u.Id_user == userIdInt);

            // Check if the user was found
            if (user == null)
            {
                return RedirectToAction("Index", "LoginPage");
            }

            // Retrieve the equipment from the database
            var equipment = await _context.Equipment.FirstOrDefaultAsync(et => et.Id_equipment == obj.Id_equipment);
            if (equipment == null)
            {
                TempData["error"] = "Zařízení nenalezeno.";
                return RedirectToRolePage(user);
            }

            // Check for overlapping reservations for the same equipment
            var existingReservation = await _context.Reservation
                .FirstOrDefaultAsync(r =>
                    r.Id_equipment == obj.Id_equipment &&
                    ((obj.DateOfReservation >= r.DateOfReservation && obj.DateOfReservation < r.DateOfEnd) ||
                     (obj.DateOfEnd > r.DateOfReservation && obj.DateOfEnd <= r.DateOfEnd))
                );

            // Check if overlapping reservation already exists
            if (existingReservation != null)
            {
                TempData["error"] = "Pro zařízení již existuje rezervace v této době.";
                return RedirectToAction("Index", new { id = obj.Id_equipment });
            }

            // Check for overlapping loans for the same equipment
            var existingLoans = await _context.Loan
                .FirstOrDefaultAsync(r =>
                    r.Id_equipment == obj.Id_equipment &&
                    ((obj.DateOfReservation >= r.DateOfLoan && obj.DateOfReservation < r.DateOfReturn) ||
                     (obj.DateOfEnd > r.DateOfLoan && obj.DateOfEnd <= r.DateOfReturn))
                );

            // Check if overlapping loan already exists
            if (existingLoans != null)
            {
                TempData["error"] = "Pro zařízení již existuje výpůjčka v této době.";
                return RedirectToAction("Index", new { id = obj.Id_equipment });
            }

            // Check if the reservation date is not in the past
            if (obj.DateOfReservation < DateTime.UtcNow)
            {
                TempData["error"] = "Rezervace nesmí být v minulosti.";
                return RedirectToAction("Index", new { id = obj.Id_equipment });
            }

            // Check if the loan durations isn't greater than the maximum equipment loan allowd
            TimeSpan difference = obj.DateOfEnd - obj.DateOfReservation;
            int daysDifference = difference.Days;
            if (daysDifference > equipment.MaxLoanDuration) 
            {
                TempData["error"] = $"Doba výpůjčky nesmí být delší než maximálně povlená doba: {equipment.MaxLoanDuration} dní.";
                return RedirectToAction("Index", new { id = obj.Id_equipment });
            }

            // Don't validate the person and equipment
            ModelState.Remove("Person");
            ModelState.Remove("Equipment");

            // Validate the input model
            if (ModelState.IsValid)
            {
                // Create a new reservation instance
                var reservation = new Reservation
                {
                    DateOfReservation = obj.DateOfReservation,
                    DateOfEnd = obj.DateOfEnd,
                    Id_equipment = obj.Id_equipment,
                    Id_user = userIdInt,
                    Person = user,
                    Equipment = equipment
                };

                // Add the reservation to the database
                _context.Reservation.Add(reservation);
                await _context.SaveChangesAsync();
                TempData["success"] = "Rezervace byla úspěšně přidána.";
            }
            else
            {
                TempData["error"] = "Došlo k chybě při přidávání rezervace.";
            }

            // Redirect back to the calendar
            return RedirectToAction("Index", new { id = obj.Id_equipment });
        }

        [HttpPost]
        public async Task<IActionResult> Accept(int id)
        {
            // Get the current user from the database
            var user = await GetUserAsync();
            if (user == null)
            {
                TempData["error"] = "Přístup zamítnut, přihlaste se prosím.";
                return RedirectToAction("Index", "LoginPage");
            }

            // Retrieve the reservation from the database
            var reservation = await _context.Reservation
                    .Include(r => r.Equipment)
                    .Include(r => r.Person)
                    .FirstOrDefaultAsync(r => r.Id_reservation == id);

            // Check if the reservation was found
            if (reservation == null)
            {
                TempData["error"] = "Rezervace nebyla nenalezena.";
                return RedirectToRolePage(user);
            }

            // Create a new loan instance
            var loan = new Loan
            {
                DateOfLoan = DateTime.UtcNow,
                DateOfReturn = reservation.DateOfEnd,
                Id_equipment = reservation.Id_equipment,
                Id_user = reservation.Id_user,
                Id_atelier = reservation.Equipment.Id_atelier,
                Status = "active"
            };

            // Try to remove the reservation to the database
            try
            {
                // Remove the reservation and the loan
                _context.Reservation.Remove(reservation);
                await _context.Loan.AddAsync(loan);
                await _context.SaveChangesAsync();
                TempData["success"] = "Rezervace byla schválena a byla převedena na půjčku.";
            }
            catch (Exception e)
            {
                TempData["error"] = "Došlo k chybě schvalování rezervace. " + e;
            }

            return RedirectToRolePage(user);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            // Get the current user from the database
            var user = await GetUserAsync();
            if (user == null)
            {
                TempData["error"] = "Přístup zamítnut, přihlaste se prosím.";
                return RedirectToAction("Index", "LoginPage");
            }

            // Retrieve the reservation from the database
            var reservation = await _context.Reservation.FirstOrDefaultAsync(et => et.Id_reservation == id);
            if (reservation == null)
            {
                TempData["error"] = "Rezerace nenalezena.";
                return RedirectToRolePage(user);
            }

            // Try to remove the reservation to the database
            try
            {
                // Remove the reservation
                _context.Reservation.Remove(reservation);
                await _context.SaveChangesAsync();
                TempData["success"] = "Rezervace byla úspěšně smazána.";
            }
            catch (Exception e)
            {
                TempData["error"] = "Došlo k chybě při mazání rezervace. " + e;
            }

            // Redirect based on the user's role
            return RedirectToRolePage(user);
        }
    }
}

// End of ReservationController.cs
