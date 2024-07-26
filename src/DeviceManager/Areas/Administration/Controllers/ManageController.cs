using DeviceManager.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Linq;

namespace DeviceManager.Areas.Administration.Controllers
{
	[Area("Administration")]
	[Authorize(Roles = ("Admins"))]
	public class ManageController : Controller
	{
		private readonly ApplicationDbContext _context;

		public ManageController(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> UserDetails(string id)
		{
			var userDetails = await _context.Users
											.Where(e=>e.Id==id)
											.Include(e=>e.UserRoles)
											.FirstAsync();
			
			return View(userDetails);
		}

		public async Task<IActionResult> DeleteUser(string id)
		{
			var deletedUser = await _context.Users
											.Where(e => e.Id == id)
											.Include(e=>e.UserRoles)
										    .FirstAsync();
			return View(deletedUser);
		}
		[HttpPost, ActionName("Delete")]
		public async Task<IActionResult> DeleteConfirmed(DeviceManagerAppUser user)
		{
			var deleted = await _context.Users
										.Where(e => e.Id==user.Id)
										.Include(e =>e.UserRoles)
										.Include(e=>e.Devices)
										.FirstAsync();	

			_context.AppUsers.Remove(deleted);
			await _context.SaveChangesAsync();	
			return RedirectToAction("Index","Home");
		}
		public async Task<IActionResult> UserRoles(string id)
		{
			var user = await _context.Users
									 .Where(u=>u.Id==id)
									 .Include(r=>r.UserRoles)
									 .FirstAsync();
			return View(user);
		}
		[HttpPost, ActionName("Update")]
		public async Task<IActionResult> ChangeUserRole(DeviceManagerAppUser user)
		{
			var updated = await _context.Users
										.Where(r => r.Id==user.Id)
										.Include (r=>r.UserRoles)
										.FirstAsync();

			foreach (var item in updated.UserRoles)
			{
				if (item.RoleId == "Managers")
				{
					var role = await _context.UserRoles
											 .FirstOrDefaultAsync(e => e.UserId == user.Id);
					_context.UserRoles.Remove(role);
					await _context.SaveChangesAsync();
					return RedirectToAction("UserDetails", new { id = user.Id });
				}
			}
			var newRole = await _context.Roles.Where(e => e.Name == "Managers").FirstAsync();
			IdentityUserRole<string> userRole = new IdentityUserRole<string>();
			userRole.RoleId = newRole.Id;
			userRole.UserId = user.Id;
			_context.UserRoles.Add(userRole);
			await _context.SaveChangesAsync();
			return RedirectToAction("UserDetails", new { id = user.Id });
		}
	}
}
