/*
 * @file Loan.cs
 * @name ArtSchool - Equipment Loan System (Project to subject IIS, FIT VUT)
 * @author Halva Jindřich (xhalva05)
 * @brief This file contains the class that represents the Loan in the database.
 */

namespace ArtSchool.Models;
using System;

public class Loan
{
    public int Id_loan { get; set; }
    public DateTime DateOfLoan { get; set; }
    public DateTime DateOfReturn { get; set; }
    public int Id_equipment { get; set; }
    public int Id_atelier { get; set; }
    public int Id_user { get; set; }
    public string Status { get; set; } //active, returned, overdue

    public Person Person { get; set; }
    public Equipment Equipment { get; set; }

    public Loan()
    {
    }
}