using DeviceManager.Data;
using DeviceManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace DeviceManager.Areas.Management.Controllers
{
	[Area("Management")]
	[Authorize(Roles =("Managers"))]
	public class UserDevicesController : Controller
	{
		private readonly ApplicationDbContext _context;

		public UserDevicesController(ApplicationDbContext context)
		{
			_context = context;
		}

		public async  Task<IActionResult> Index(string userId)
		{
			var devices = await _context.Devices.Where(e=>e.UserId==userId).ToListAsync();
			return View(devices);
		}

		public async Task<IActionResult> Details(int id, string userId)
		{
			var device = await _context.Devices.FirstOrDefaultAsync(e => e.UserId == userId && e.Id == id);
			return View(device);
		}
		
		public async Task<IActionResult> Edit(int id, string userId)
		{
			var device = await _context.Devices.FirstOrDefaultAsync(e => e.UserId == userId && e.Id==id);
			var viewModel = getViewModel(device);
			return View(viewModel);
		}

		[HttpPost, ActionName("Edit")]
		public async Task<IActionResult> EditConfirmed(DeviceViewModel viewModel)
		{
			if(ModelState.IsValid)
			{
				var device = _context.Devices.FirstOrDefault(e => e.Id == viewModel.Id);
				device.Description= viewModel.Description;
				device.Name = viewModel.Name;
				_context.Devices.Update(device);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index), new { userId = device.UserId });
			}

			return RedirectToAction("/Management");
	
		}
		public async Task<IActionResult> Delete(int id, string userId)
		{
			var device = await _context.Devices.FirstOrDefaultAsync(e => e.UserId == userId && e.Id == id);
			var viewModel = getViewModel(device);
			return View(viewModel);
        }
		[HttpPost, ActionName("Delete")]
		public async Task<IActionResult> DeleteConfirmed(DeviceViewModel viewModel)
		{
			if (ModelState.IsValid)
			{
				var device = _context.Devices.FirstOrDefault(e => e.Id == viewModel.Id);
				_context.Devices.Remove(device);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index), new { userId = device.UserId });
			}

			return RedirectToAction("/Management");
		}
		public async Task<IActionResult> Create(string userId)
		{
			DeviceViewModel viewModel = new DeviceViewModel();
			viewModel.UserId = userId;	
			return View(viewModel);
		}
		[HttpPost, ActionName("Create")]
		public async Task<IActionResult> CreateConfirmed(DeviceViewModel viewModel)
		{
			if (ModelState.IsValid)
			{
				Device device = new Device();
				device.UserId = viewModel.UserId;
				device.Name = viewModel.Name;	
				device.Description = viewModel.Description;
				_context.Devices.Add(device);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index), new { userId = viewModel.UserId });
			}
			return RedirectToAction("/Management");
		}
		private DeviceViewModel getViewModel(Device device)
		{
			return new DeviceViewModel
			{
				Id = device.Id,
				Name = device.Name,
				Description = device.Description,
				UserId = device.UserId,
				DateAdded = device.DateAdded
			};
		}
	}
}
