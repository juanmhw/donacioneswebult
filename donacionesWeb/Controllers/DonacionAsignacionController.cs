using donacionesWeb.Models;
using donacionesWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace donacionesWeb.Controllers
{
    public class DonacionAsignacionController : Controller
    {
        private readonly DonacionAsignacionService _service;

        public DonacionAsignacionController(DonacionAsignacionService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var donacionesAsignaciones = await _service.GetAllAsync();
            return View(donacionesAsignaciones);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DonacionesAsignacione donacionAsignacion)
        {
            if (ModelState.IsValid)
            {
                donacionAsignacion.FechaAsignacion = DateTime.Now;
                await _service.CreateAsync(donacionAsignacion);
                return RedirectToAction(nameof(Index));
            }
            return View(donacionAsignacion);
        }

        public async Task<IActionResult> Details(int id)
        {
            var donacionAsignacion = await _service.GetByIdAsync(id);
            if (donacionAsignacion == null)
            {
                return NotFound();
            }
            return View(donacionAsignacion);
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
