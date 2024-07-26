using DeviceManager.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeviceManager.Areas.Management.Controllers
{
    [Area("Management")]
    [Authorize(Roles = "Managers")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private IEnumerable<DeviceManagerAppUser> users;
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            users = await _context.AppUsers.ToListAsync();
            return View(users);
        }
    }
}
