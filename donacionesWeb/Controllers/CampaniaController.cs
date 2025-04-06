using donacionesWeb.Models;
using donacionesWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;

namespace donacionesWeb.Controllers
{
    public class CampaniaController : Controller
    {
        private readonly CampaniaService _service;

        public CampaniaController(CampaniaService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var campanias = await _service.GetAllAsync();
            return View(campanias);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Campania campania)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(campania);
                }

                campania.FechaCreacion = DateTime.Now;
                campania.Activa = true;
                campania.MontoRecaudado ??= 0;

                if (string.IsNullOrWhiteSpace(campania.Titulo))
                {
                    ModelState.AddModelError("Titulo", "El título no puede estar vacío");
                    return View(campania);
                }

                await _service.CreateAsync(campania);
                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al comunicarse con la API: {ex.Message}");
                return View(campania);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error inesperado: {ex.Message}");
                return View(campania);
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var campania = await _service.GetByIdAsync(id);
            if (campania == null)
            {
                return NotFound();
            }
            return View(campania);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var campania = await _service.GetByIdAsync(id);
            if (campania == null)
            {
                return NotFound();
            }
            return View(campania);
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
