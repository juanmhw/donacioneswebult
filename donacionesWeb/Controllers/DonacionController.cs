using donacionesWeb.Models;
using donacionesWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace donacionesWeb.Controllers
{
    public class DonacionController : Controller
    {
        private readonly DonacionService _service;

        public DonacionController(DonacionService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var donaciones = await _service.GetAllAsync();
            return View(donaciones);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Donacione donacion)
        {
            if (ModelState.IsValid)
            {
                donacion.FechaDonacion = DateTime.Now;
                await _service.CreateAsync(donacion);
                return RedirectToAction(nameof(Index));
            }
            return View(donacion);
        }

        public async Task<IActionResult> Details(int id)
        {
            var donacion = await _service.GetByIdAsync(id);
            if (donacion == null)
            {
                return NotFound();
            }
            return View(donacion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
