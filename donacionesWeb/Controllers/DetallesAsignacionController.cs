using donacionesWeb.Models;
using donacionesWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace donacionesWeb.Controllers
{
    public class DetallesAsignacionController : Controller
    {
        private readonly DetallesAsignacionService _detalleService;

        public DetallesAsignacionController(DetallesAsignacionService detalleService)
        {
            _detalleService = detalleService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var detalles = await _detalleService.GetDetallesAsync();
                return View(detalles);
            }
            catch (HttpRequestException ex)
            {
                ViewBag.ErrorMessage = "Error al cargar detalles: " + ex.Message;
                return View(new List<DetallesAsignacion>());
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DetallesAsignacion detalle)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _detalleService.CreateDetalleAsync(detalle);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error al crear detalle: {ex.Message}");
                }
            }
            return View(detalle);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _detalleService.DeleteDetalleAsync(id);
                if (!success)
                {
                    TempData["ErrorMessage"] = "No se pudo eliminar el detalle";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al eliminar detalle: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
