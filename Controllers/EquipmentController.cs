/*
 * @file    EquipmentController.cs
 * @name    ArtSchool - Equipment Loan System (Project for the IIS subject, FIT VUT)
 * @author  Marek Čupr (xcuprm01)
 * @brief   Controller for managing the equipment actions.
 */

using ArtSchool.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ArtSchool.Controllers
{
    public class EquipmentController : Controller
    {
        // Database context
        private readonly MyDBContext _context;

        public EquipmentController(MyDBContext context)
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

        public async Task<IActionResult> Create()
        {
            // Get the current user from the database
            var user = await GetUserAsync();
            if (user == null)
            {
                TempData["error"] = "Přístup zamítnut, přihlaste se prosím.";
                return RedirectToAction("Index", "LoginPage");
            }

            // Retrieve the ateliers from the database
            List<Atelier> ateliers;
            if (user.User_role == "admin")
            {
                ateliers = await _context.Atelier.ToListAsync();
            }
            else
            {
                ateliers = await _context.AtelierPersons
                    .Where(ap => ap.Id_user == user.Id_user)
                    .Select(ap => ap.Atelier)
                    .ToListAsync();
            }

            // Retrieve the equipment types from the database
            var equipmentTypes = await _context.EquipmentType.ToListAsync();

            // Assign the corresponding ateliers and equipment types
            ViewData["Ateliers"] = ateliers;
            ViewData["EquipmentTypes"] = equipmentTypes;

            // Return the view
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Equipment obj)
        {
            // Get the current user from the database
            var user = await GetUserAsync();
            if (user == null)
            {
                TempData["error"] = "Přístup zamítnut, přihlaste se prosím.";
                return RedirectToAction("Index", "LoginPage");
            }

            // Check if the manufacturing date is later than the purchase date
            if (obj.DateOfManufacture > obj.DateOfPurchase)
            {
                ModelState.AddModelError("DateOfManufacture", "Datum výroby nemůže být pozdější než datum nákupu.");
            }

            // Check if the manufacturing date is in the past
            if (obj.DateOfManufacture > DateTime.UtcNow)
            {
                ModelState.AddModelError("DateOfManufacture", "Datum výroby musí být v minulosti.");
            }

            // Check if the purchase date is in the past
            if (obj.DateOfPurchase > DateTime.UtcNow)
            {
                ModelState.AddModelError("DateOfPurchase", "Datum nákupu musí být v minulosti.");
            }

            // Retrieve the equipment type from the database
            var equipmentType = await _context.EquipmentType.FirstOrDefaultAsync(et => et.Id_type == obj.Id_type);
            if (equipmentType == null)
            {
                ModelState.AddModelError("Id_type", "Vybraný typ zařízení není platný.");
            }

            // Retrieve the atelier from the database
            var atelier = await _context.Atelier.FirstOrDefaultAsync(a => a.Id_atelier == obj.Id_atelier);
            if (atelier == null)
            {
                ModelState.AddModelError("Id_atelier", "Vybraný ateliér není platný.");
            }

            // Assign the corresponding atelier and equipment type
            obj.EquipmentType = equipmentType;
            obj.Atelier = atelier;

            // Don't validate the equipment type and atelier
            ModelState.Remove("EquipmentType");
            ModelState.Remove("Atelier");

            // Validate the input model
            if (!ModelState.IsValid)
            {
                // Retrieve the equipment types from the database
                var equipmentTypes = await _context.EquipmentType.ToListAsync();

                // Retrieve the ateliers from the database
                var ateliers = await _context.AtelierPersons
                    .Where(ap => ap.Id_user == user.Id_user)
                    .Select(ap => ap.Atelier)
                    .ToListAsync();

                // Store the ateliers and equipment types
                ViewData["Ateliers"] = ateliers;
                ViewData["EquipmentTypes"] = equipmentTypes;

                // Return the view with the equipment
                TempData["error"] = "Došlo k chybě při přidávání zařízení.";
                return View(obj);
            }

            // Try to add the equipment to the database
            try
            {
                _context.Equipment.Add(obj);
                await _context.SaveChangesAsync();
                TempData["success"] = "Zařízení bylo úspěšně přidáno.";
            }
            catch (Exception e)
            {
                TempData["error"] = "Došlo k chybě při přidávání zařízení. " + e;
            }

            // Redirect based on the user's role
            return RedirectToRolePage(user);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // Get the current user from the database
            var user = await GetUserAsync();
            if (user == null)
            {
                TempData["error"] = "Přístup zamítnut, přihlaste se prosím.";
                return RedirectToAction("Index", "LoginPage");
            }

            // Retrieve the equipment from the database
            var equipment = await _context.Equipment.FirstOrDefaultAsync(et => et.Id_equipment == id);
            if (equipment == null)
            {
                TempData["error"] = "Zařízení nenalezeno.";
                return RedirectToRolePage(user);
            }

            // Retrieve the ateliers from the database
            List<Atelier> ateliers;
            if (user.User_role == "admin")
            {
                ateliers = await _context.Atelier.ToListAsync();
            }
            else
            {
                ateliers = await _context.AtelierPersons
                      .Where(ap => ap.Id_user == user.Id_user && ap.Id_atelier != equipment.Id_atelier)
                      .Select(ap => ap.Atelier)
                      .ToListAsync();
            }
            
            // Retrieve the equipment types from the database
            var equipmentTypes = await _context.EquipmentType
                .Where(ap => ap.Id_type != equipment.Id_type)
                .ToListAsync();

            // Retrieve the atelier and equipment type from the database
            var atelier = await _context.Atelier.FirstOrDefaultAsync(ap => ap.Id_atelier == equipment.Id_atelier);
            var type = await _context.EquipmentType.FirstOrDefaultAsync(ap => ap.Id_type == equipment.Id_type);

            // Assign the corresponding ateliers and equipment types
            ViewData["Ateliers"] = ateliers;
            ViewData["EquipmentTypes"] = equipmentTypes;
            ViewData["Atelier"] = atelier;
            ViewData["Type"] = type;

            // Return the view with the equipment
            return View(equipment);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Equipment obj)
        {
            // Get the current user from the database
            var user = await GetUserAsync();
            if (user == null)
            {
                TempData["error"] = "Přístup zamítnut, přihlaste se prosím.";
                return RedirectToAction("Index", "LoginPage");
            }

            // Retrieve the equipment from the database
            var equipment = await _context.Equipment.FirstOrDefaultAsync(et => et.Id_equipment == obj.Id_equipment);
            if (equipment == null)
            {
                TempData["error"] = "Zařízení nenalezeno.";
                return RedirectToRolePage(user);
            }

            // Retrieve the equipment type from the database
            var equipmentType = await _context.EquipmentType.FirstOrDefaultAsync(et => et.Id_type == obj.Id_type);
            if (equipmentType == null)
            {
                ModelState.AddModelError("Id_type", "Vybraný typ zařízení není platný.");
            }

            // Retrieve the atelier from the database
            var atelier = await _context.Atelier.FirstOrDefaultAsync(a => a.Id_atelier == obj.Id_atelier);
            if (atelier == null)
            {
                ModelState.AddModelError("Id_atelier", "Vybraný ateliér není platný.");
            }

            // Assign the corresponding atelier and equipment type
            equipment.EquipmentType = equipmentType;
            equipment.Atelier = atelier;

            // Don't validate the equipment type and atelier
            ModelState.Remove("EquipmentType");
            ModelState.Remove("Atelier");

            // Validate the input model
            if (!ModelState.IsValid)
            {
                // Return the view with the equipment
                TempData["error"] = "Došlo k chybě při aktualizaci údajů zařízení.";
                return View(obj.Id_equipment);
            }

            // Check if any equipment details have been changed
            bool isChanged = false;

            // Check if the equipment name has been changed
            if (equipment.Name != obj.Name)
            {
                equipment.Name = obj.Name;
                isChanged = true;
            }

            // Check if the purchase date has been changed
            if (equipment.DateOfPurchase != obj.DateOfPurchase)
            { 
                equipment.DateOfPurchase = obj.DateOfPurchase;
                isChanged = true;
            }

            // Check if the manufacture date has been changed
            if (equipment.DateOfManufacture != obj.DateOfManufacture)
            { 
                equipment.DateOfManufacture = obj.DateOfManufacture;
                isChanged = true;
            }

            // Check if the max loan has been changed
            if (equipment.MaxLoanDuration != obj.MaxLoanDuration)
            {
                equipment.MaxLoanDuration = obj.MaxLoanDuration;
                isChanged = true;
            }

            // Check if the atelier id has been changed
            if (equipment.Id_atelier != obj.Id_atelier)
            {
                equipment.Id_atelier = obj.Id_atelier;
                isChanged = true;
            }

            // Check if the equipment id has been changed
            if (equipment.Id_type != obj.Id_type)
            {
                equipment.Id_type = obj.Id_type;
            }

            // Check if any equipment details have been changed
            if (!isChanged)
            {
                TempData["info"] = "Žádné změny nebyly provedeny.";
                return RedirectToRolePage(user);
            }

            // Try to update the equipment in the database
            try
            {
                _context.Equipment.Update(equipment);
                await _context.SaveChangesAsync();
                TempData["success"] = "Zařízení bylo úspěšně aktualizováno.";
            }
            catch (Exception e)
            {
                TempData["error"] = "Došlo k chybě při aktualizaci zařízení. " + e;
            }

            // Redirect based on the user's role
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

            // Retrieve the equipment from the database
            var equipment = await _context.Equipment.FirstOrDefaultAsync(et => et.Id_equipment == id);
            if (equipment == null)
            {
                TempData["error"] = "Zařízení nenalezeno.";
                return RedirectToRolePage(user);
            }

            // Retrieve the related reservations and loans
            var reservations = _context.Reservation.Where(r => r.Id_equipment == id);
            var loans = _context.Loan.Where(l => l.Id_equipment == id);

            // Try to remove the equipment, reservations and loans from the database
            try
            {
                // Remove the reservations
                _context.Reservation.RemoveRange(reservations);

                // Remove the loans
                _context.Loan.RemoveRange(loans);

                // Remove the equipment
                _context.Equipment.Remove(equipment);

                // Save the changes to the database
                await _context.SaveChangesAsync();
                TempData["success"] = "Zařízení bylo úspěšně smazáno.";
            }
            catch (Exception e)
            {
                TempData["error"] = "Došlo k chybě při mazání zařízení. " + e;
            }

            // Redirect based on the user's role
            return RedirectToRolePage(user);
        }
    }
}

// End of EquipmentController.cs