/*
 * @file Atelier.cs
 * @name ArtSchool - Equipment Loan System (Project to subject IIS, FIT VUT)
 * @author Halva Jindřich (xhalva05)
 * @brief This file contains the class that represents the Atelier in the database.
 */

namespace ArtSchool.Models;

public class Atelier
{
    public int Id_atelier { get; set; }
    public string Name { get; set; }

    
    public ICollection<AtelierPersons> AtelierPersons { get; set; } = new List<AtelierPersons>();
    public ICollection<Equipment> Equipments { get; set; } = new List<Equipment>();

    public Atelier()
    {
    }
}