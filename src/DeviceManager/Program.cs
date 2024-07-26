using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DeviceManager.Data;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Note! RequireConfirmedAccount is set to false only to allow easy testing of different users.
// In real apps confirmed account requirement is a good practice.
// TODO: configure identity to use the created DeviceManagerAppUser as the identity user and also to support roles.
builder.Services.AddDefaultIdentity<DeviceManagerAppUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// TODO: define route(s) to area(s)
app.MapAreaControllerRoute(
      name: "MyAreaManagement",
      areaName: "Management",
      pattern: "Management/{controller=Home}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
	  name: "MyAreaAdministration",
	  areaName: "Administration",
	  pattern: "Administration/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

// The Program class declaration below is needed for the automated tests. 
// DO NOT remove the following line!!!
public partial class Program { }