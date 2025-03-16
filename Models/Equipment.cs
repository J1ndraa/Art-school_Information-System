/*
 * @file Equipment.cs
 * @name ArtSchool - Equipment Loan System (Project to subject IIS, FIT VUT)
 * @author Halva Jindřich (xhalva05)
 * @brief This file contains the class that represents the Equipment in the database.
 */

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ArtSchool.Models;

public class Equipment
{
    public int Id_equipment { get; set; }

    [Required(ErrorMessage = "Název zařízení je povinný.")]
    [StringLength(100, ErrorMessage = "Název zařízení musí být mezi 2 a 100 znaky.", MinimumLength = 2)]
    [DisplayName("Název zařízení")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Datum výroby je povinné.")]
    [DataType(DataType.Date)]
    [DisplayName("Datum výroby")]
    public DateTime DateOfManufacture { get; set; }

    [Required(ErrorMessage = "Datum nákupu je povinné.")]
    [DataType(DataType.Date)]
    [DisplayName("Datum nákupu")]
    public DateTime DateOfPurchase { get; set; }

    [Required(ErrorMessage = "Název ateliéru je povinný.")]
    [DisplayName("Název ateliéru")]
    public int Id_atelier { get; set; }

    [Required(ErrorMessage = "Typ zařízení je povinný.")]
    [DisplayName("Typ zařízení")]
    public int Id_type { get; set; }

    [Required(ErrorMessage = "Maximální doba výpůjčky je povinná.")]
    [Range(1, 365, ErrorMessage = "Maximální doba výpůjčky musí být mezi 1 a 365 dny.")]
    [DisplayName("Maximální doba výpůjčky (dny)")]
    public int MaxLoanDuration { get; set; }

    public Atelier Atelier { get; set; }

    public EquipmentType EquipmentType { get; set; }

    public ICollection<Loan> Loans { get; set; } = new List<Loan>();

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    public Equipment()
    {
    }
}