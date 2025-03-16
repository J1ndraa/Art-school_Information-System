/*
 * @file AtelierPersons.cs
 * @name ArtSchool - Equipment Loan System (Project to subject IIS, FIT VUT)
 * @author Halva Jindřich (xhalva05)
 * @brief This file contains the class that represents the M:N relation between Atelier and Person in the database.
 */

namespace ArtSchool.Models;

public class AtelierPersons
{
	public int Id_user { get; set; }
	public int Id_atelier { get; set; }

    public Person Person { get; set; }
    public Atelier Atelier { get; set; }
}
