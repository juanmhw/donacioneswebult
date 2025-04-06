using donacionesWeb.Models;
using donacionesWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace donacionesWeb.Controllers
{
    public class EstadoController : Controller
    {
        private readonly EstadoService _service;

        public EstadoController(EstadoService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var estados = await _service.GetAllAsync();
            return View(estados);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Estado estado)
        {
            if (ModelState.IsValid)
            {
                await _service.CreateAsync(estado);
                return RedirectToAction(nameof(Index));
            }
            return View(estado);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var estado = await _service.GetByIdAsync(id);
            if (estado == null)
            {
                return NotFound();
            }
            return View(estado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Estado estado)
        {
            if (id != estado.EstadoId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _service.UpdateAsync(id, estado);
                return RedirectToAction(nameof(Index));
            }
            return View(estado);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var estado = await _service.GetByIdAsync(id);
            if (estado == null)
            {
                return NotFound();
            }
            return View(estado);
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
