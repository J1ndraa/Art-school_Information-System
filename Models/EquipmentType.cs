/*
 * @file EquipmentType.cs
 * @name ArtSchool - Equipment Loan System (Project to subject IIS, FIT VUT)
 * @author Halva Jindřich (xhalva05)
 * @brief This file contains the class that represents the EquipmentType in the database.
 */

using System.ComponentModel.DataAnnotations;

namespace ArtSchool.Models;

public class EquipmentType
{
    public int Id_type { get; set; }

	[Required(ErrorMessage = "Název typu zařízení je povinný.")]
	[StringLength(30, ErrorMessage = "Název může obsahovat maximálně 30 znaků.")]
	public string Name { get; set; }

    public ICollection<Equipment> Equipments { get; set; } = new List<Equipment>();


    public EquipmentType()
    {
    }
}