using DeviceManager.Data;
using DeviceManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.ConstrainedExecution;
using System.Security.Claims;

namespace DeviceManager.Controllers
{
	[Authorize]
	public class DevicesController : Controller
	{
		private readonly ApplicationDbContext _context;
		public List<DeviceViewModel> deviceViewModels { get; set; }
		public DevicesController(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index(int id)
		{
			string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var userDevices = await _context.Devices.Where(d => d.UserId == userId).ToListAsync();
			deviceViewModels = new List<DeviceViewModel>();

			foreach (var device in userDevices)
			{
				var d = getViewModel(device);
				deviceViewModels.Add(d);
			}
			return View(deviceViewModels);
		}
		public async Task<IActionResult> Details(int id)
		{
			Device? device = await _context.Devices.SingleOrDefaultAsync(d => d.Id == id);

			if (device == null)
			{
				return Ok();
			}

			if (device.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
			{
				return RedirectToAction("Error", "Home");
			}
			var viewModel = getViewModel(device);
			return View(viewModel);
		}

		public async Task<IActionResult> Create(string id)
		{
			DeviceViewModel d = new DeviceViewModel();
			return View(d);
		}

		[HttpPost, ActionName("Create")]
		public async Task<IActionResult> CreateConfirm(DeviceViewModel deviceView)
		{
			if (ModelState.IsValid)
			{
				var device = getDevice(deviceView);
				device.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
				_context.Devices.Add(device);
				await _context.SaveChangesAsync();
				return RedirectToAction("Index");
			}
			return View();
		}

		public async Task<IActionResult> Edit(int id)
		{
			var device = await _context.Devices.SingleOrDefaultAsync(e => e.Id == id);
			var viewModel = getViewModel(device);
			return View(viewModel);
		}

		[HttpPost, ActionName("Edit")]
		public async Task<IActionResult> EditConfirmed(DeviceViewModel viewModel)
		{
			if (ModelState.IsValid)
			{
				var device = getDevice(viewModel);
				_context.Devices.Update(device);
				await _context.SaveChangesAsync();
				return RedirectToAction("Index");

			}
			return View(viewModel);
		}

		public async Task<IActionResult> Delete(int id)
		{
			
			var device = await _context.Devices.FirstOrDefaultAsync(e => e.Id == id);
			var viewModel = getViewModel(device);
			return View(viewModel);

		}

		[HttpPost, ActionName("Delete")]
		public async Task<IActionResult> DeleteConfirmed( DeviceViewModel viewModel)
		{
			if (viewModel.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
			{
				return RedirectToAction("Error", "Home");
			}
			var device = _context.Devices.FirstOrDefault(e => e.Id == viewModel.Id);
			if (device == null)
			{
				return RedirectToAction("Error", "Home");
			}
			_context.Devices.Remove(device);
			await _context.SaveChangesAsync();
			return RedirectToAction("Index");
			
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
		private Device getDevice(DeviceViewModel viewModel)
		{
			return new Device
			{
				Id = viewModel.Id,
				Name = viewModel.Name,
				Description = viewModel.Description,
				UserId = viewModel.UserId,
				DateAdded = DateTime.Parse(DateTime.Now.ToString("dd.MM.yyyy hh.mm.ss"))
			};
		}
	}
	
}
