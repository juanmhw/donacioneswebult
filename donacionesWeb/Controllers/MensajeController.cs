using donacionesWeb.Models;
using donacionesWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace donacionesWeb.Controllers
{
    public class MensajeController : Controller
    {
        private readonly MensajeService _service;

        public MensajeController(MensajeService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var mensajes = await _service.GetAllAsync();
            return View(mensajes);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Mensaje mensaje)
        {
            if (ModelState.IsValid)
            {
                mensaje.FechaEnvio = DateTime.Now;
                mensaje.Leido = false;
                mensaje.Respondido = false;

                await _service.CreateAsync(mensaje);
                return RedirectToAction(nameof(Index));
            }
            return View(mensaje);
        }

        public async Task<IActionResult> Details(int id)
        {
            var mensaje = await _service.GetByIdAsync(id);
            if (mensaje == null)
            {
                return NotFound();
            }
            return View(mensaje);
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
