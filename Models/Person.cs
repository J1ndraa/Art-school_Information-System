/*
 * @file User.cs
 * @name ArtSchool - Equipment Loan System (Project to subject IIS, FIT VUT)
 * @author Halva Jindřich (xhalva05)
 * @brief This file contains the class that represents the User in the database.
 */

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace ArtSchool.Models;

public class Person
{
    public int Id_user { get; set; }

    [Required(ErrorMessage = "Jméno je povinné.")]
    [StringLength(50, ErrorMessage = "Jméno musí mít mezi 2 a 50 znaky.", MinimumLength = 2)]
    [DisplayName("Křestní Jméno")]
    public string Firstname { get; set; }

    [Required(ErrorMessage = "Příjmení je povinné.")]
    [StringLength(50, ErrorMessage = "Příjmení musí mít mezi 2 a 50 znaky.", MinimumLength = 2)]
    [DisplayName("Příjmení")]
    public string Surname { get; set; }

    [Required(ErrorMessage = "Email je povinný.")]
    [EmailAddress(ErrorMessage = "Zadejte platnou emailovou adresu.")]
    [DisplayName("Email")]
    public string Email { get; set; }

    public string? User_role { get; set; }

    [Required(ErrorMessage = "Heslo je povinné")]
    [StringLength(50, MinimumLength = 4, ErrorMessage = "Heslo musí mít alespoň 4 znaky a maximálně 50 znaků.")]
    public string Pwd { get; set; }
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public ICollection<AtelierPersons> AtelierPersons { get; set; } = new List<AtelierPersons>();

    public Person()
    {
    }
}