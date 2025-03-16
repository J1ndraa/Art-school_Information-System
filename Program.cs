/*
 * @file Program.cs
 * @name ArtSchool - Equipment Loan System (Project to subject IIS, FIT VUT)
 * @author Halva Jindřich (xhalva05), Čupr Marek (xcuprm01)
 * @brief This file contains the main logic of the whole project.
 */

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore.Design;

var builder = WebApplication.CreateBuilder(args);

//add services to the container.
builder.Services.AddControllersWithViews();

//add the database context to the project by using the connection string from appsettings.json
builder.Services.AddDbContext<MyDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//add authentication and authorization to the project
builder.Services.AddAuthentication("Cookies")
	.AddCookie(options =>
	{
		options.LoginPath = "/LoginPage";
		options.AccessDeniedPath = "/AccessDenied";
	});

builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("RequireAdmin", policy => policy.RequireRole("admin"));
	options.AddPolicy("RequireManager", policy => policy.RequireRole("atelier_manager"));
	options.AddPolicy("RequireTeacher", policy => policy.RequireRole("teacher"));
	options.AddPolicy("RequireUser", policy => policy.RequireRole("user"));
});

//build the application.
var app = builder.Build();

//configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    //the default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=LoginPage}/{action=Index}/{id?}");

app.Run();
