/*
 * @file MyDBContext.cs
 * @name ArtSchool - Equipment Loan System (Project to subject IIS, FIT VUT)
 * @author Halva Jindřich (xhalva05)
 * @brief This file contains the class that is used to map the database tables.
 */

using Microsoft.EntityFrameworkCore;
using ArtSchool.Models;

//This class is used to map the database tables
public class MyDBContext : DbContext
{ 
    public MyDBContext(DbContextOptions<MyDBContext> options) : base(options) { }
    
    public DbSet<Person> Person { get; set; }
    public DbSet<Atelier> Atelier { get; set; }
    public DbSet<Equipment> Equipment { get; set; }
    public DbSet<EquipmentType> EquipmentType { get; set; }
    public DbSet<Loan> Loan { get; set; }
    public DbSet<Reservation> Reservation { get; set; } 
    public DbSet<AtelierPersons> AtelierPersons { get; set; }

    //mapping the database tables
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //primary keys//////////////////////////////////////////////////////////////////////////

        modelBuilder.Entity<Person>().HasKey(u => u.Id_user);
		modelBuilder.Entity<Atelier>().HasKey(a => a.Id_atelier);
        modelBuilder.Entity<Equipment>().HasKey(e => e.Id_equipment);
		modelBuilder.Entity<EquipmentType>().HasKey(et => et.Id_type);
        modelBuilder.Entity<Loan>().HasKey(l => l.Id_loan);
        modelBuilder.Entity<Reservation>().HasKey(r => r.Id_reservation);
		modelBuilder.Entity<AtelierPersons>().HasKey(ap => new { ap.Id_user, ap.Id_atelier });

        //relations/////////////////////////////////////////////////////////////////////////////

        modelBuilder.Entity<AtelierPersons>() //with person
           .HasOne(ap => ap.Person) 
           .WithMany(p => p.AtelierPersons)
           .HasForeignKey(ap => ap.Id_user);

        modelBuilder.Entity<AtelierPersons>() //with atelier
            .HasOne(ap => ap.Atelier)
            .WithMany(a => a.AtelierPersons)
            .HasForeignKey(ap => ap.Id_atelier);

        modelBuilder.Entity<Loan>() //with person
            .HasOne(l => l.Person) 
            .WithMany(p => p.Loans)
            .HasForeignKey(l => l.Id_user);

        modelBuilder.Entity<Loan>() //with equipment
            .HasOne(l => l.Equipment)
            .WithMany(e => e.Loans)
            .HasForeignKey(l => l.Id_equipment);

        modelBuilder.Entity<Reservation>() //with person
            .HasOne(r => r.Person)
            .WithMany(p => p.Reservations)
            .HasForeignKey(r => r.Id_user);

        modelBuilder.Entity<Reservation>() //with equipment
            .HasOne(r => r.Equipment)
            .WithMany(e => e.Reservations)
            .HasForeignKey(r => r.Id_equipment);

        modelBuilder.Entity<Equipment>() //with equipment type
            .HasOne(e => e.EquipmentType)
            .WithMany(et => et.Equipments)
            .HasForeignKey(e => e.Id_type);

        modelBuilder.Entity<Equipment>() //with equipment
            .HasOne(a => a.Atelier)
            .WithMany(e => e.Equipments)
            .HasForeignKey(a => a.Id_atelier);

        //unique constraints///////////////////////////////////////////////////////////////////

        //unique email
        modelBuilder.Entity<Person>()
            .HasIndex(p => p.Email)
            .IsUnique();
    }
}