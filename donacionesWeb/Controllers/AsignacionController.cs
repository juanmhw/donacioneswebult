using donacionesWeb.Models;
using donacionesWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace donacionesWeb.Controllers
{
    public class AsignacionController : Controller
    {
        private readonly AsignacionService _service;

        public AsignacionController(AsignacionService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var asignaciones = await _service.GetAllAsync();
            return View(asignaciones);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Asignacione asignacion)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(asignacion);
                }

                asignacion.FechaAsignacion = DateTime.Now;

                await _service.CreateAsync(asignacion);
                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al comunicarse con la API: {ex.Message}");
                return View(asignacion);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error inesperado: {ex.Message}");
                return View(asignacion);
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var asignacion = await _service.GetByIdAsync(id);
            if (asignacion == null)
            {
                return NotFound();
            }
            return View(asignacion);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var asignacion = await _service.GetByIdAsync(id);
            if (asignacion == null)
            {
                return NotFound();
            }
            return View(asignacion);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
