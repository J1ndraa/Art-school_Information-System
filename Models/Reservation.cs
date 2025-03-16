/*
 * @file Reservation.cs
 * @name ArtSchool - Equipment Loan System (Project to subject IIS, FIT VUT)
 * @author Halva Jindřich (xhalva05)
 * @brief This file contains the class that represents the Reservation in the database.
 */

using System.ComponentModel;

namespace ArtSchool.Models;

public class Reservation
{
    public int Id_reservation { get; set; }
    public DateTime DateOfReservation { get; set; }
    public DateTime DateOfLoanStart { get; set; }
    public DateTime DateOfEnd { get; set; }
    public int Id_equipment { get; set; }
    public int Id_user { get; set; }
    public Person Person { get; set; }
    public Equipment Equipment { get; set; }
    public Reservation()
    {
    }
}