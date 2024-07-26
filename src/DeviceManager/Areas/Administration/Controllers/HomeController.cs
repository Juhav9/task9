using DeviceManager.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeviceManager.Areas.Administration.Controllers
{
	[Area("Administration")]
	[Authorize(Roles = ("Admins"))]
	public class HomeController : Controller
	{
		private readonly ApplicationDbContext _context;

		public HomeController(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index()
		{
			
            var user = await _context.AppUsers
									 .Include(e=>e.UserRoles)
									 .ToListAsync();
		
			return View(user);
		}
	}
}
