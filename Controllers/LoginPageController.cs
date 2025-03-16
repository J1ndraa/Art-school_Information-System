/*
 * @file LoginPageController.cs
 * @name ArtSchool - Equipment Loan System (Project to subject IIS, FIT VUT)
 * @author Halva Jindøich (xhalva05)
 * @brief This file contains methods for authentication.
 */

using ArtSchool.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using BCrypt.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using NuGet.Common;


namespace ArtSchool.Controllers;

public class LoginPageController : Controller
{
    private readonly MyDBContext _context;

    public LoginPageController(MyDBContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Register(Person user)
    {
        //set user role to user
        user.User_role = "user";

        //check if the form is filled corecctly
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Údaje v registraèním formuláøi nebyly vyplnìny správnì";
            return RedirectToAction("Index", "LoginPage");
        }
        
        //check if the user already exists
        if (await _context.Person.AnyAsync(u => u.Email == user.Email))
        {
            TempData["Error"] = "Uživatel s tímto emailem již existuje";
            return RedirectToAction("Index", "LoginPage");
        }

		//hash the password
		user.Pwd = BCrypt.Net.BCrypt.HashPassword(user.Pwd);

		//add user to the database
		_context.Person.Add(user);
        await _context.SaveChangesAsync();

		var claims = new List<Claim>{
		    new Claim(ClaimTypes.Email, user.Email),
		    new Claim(ClaimTypes.Role, user.User_role),
		    new Claim("Id_user", user.Id_user.ToString())
        };

		var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
		var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

		await HttpContext.SignInAsync("Cookies", claimsPrincipal);

		//get to admin page
		return RedirectToAction("Index", "UserPage");
    }

    [HttpPost]
    public async Task<IActionResult> Login(Person user)
    {
        //get user from the database
        var db_user = _context.Person.FirstOrDefault(u => u.Email == user.Email);

        //check if the user exists
        if (db_user == null)
        {
            TempData["Error"] = "Uživatel s daným emailem neexistuje";
            return RedirectToAction("Index", "LoginPage");
        }

        //check if the password is correct
        if (!BCrypt.Net.BCrypt.Verify(user.Pwd, db_user.Pwd))
        {
            TempData["Error"] = "Nesprávné heslo";
            return RedirectToAction("Index", "LoginPage");
        }

		var claims = new List<Claim>{
		    new Claim(ClaimTypes.Email, db_user.Email),
		    new Claim(ClaimTypes.Role, db_user.User_role),
            new Claim("Id_user", db_user.Id_user.ToString())
        };

		var identity = new ClaimsIdentity(claims, "Cookies");
		var principal = new ClaimsPrincipal(identity);

		await HttpContext.SignInAsync("Cookies", principal);

		switch (db_user.User_role)
        {
            case "admin":
                return RedirectToAction("Index", "AdminPage");
			case "atelier_manager":
				return RedirectToAction("Index", "AtelierManagerPage");
			case "teacher":
                return RedirectToAction("Index", "TeacherPage");
            case "user":
                return RedirectToAction("Index", "UserPage");
        }
        
        return Error();
    }

	//logout user method
	[HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("Cookies");
        return RedirectToAction("Index", "LoginPage");
    }

    public IActionResult Index()
    {
     return View();
    }

    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
