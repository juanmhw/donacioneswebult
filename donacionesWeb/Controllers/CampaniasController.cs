using donacionesWeb.Models;
using donacionesWeb.Models.ViewModels;
using donacionesWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace donacionesWeb.Controllers
{
    [Authorize]
    public class CampaniasController : Controller
    {
        private readonly CampaniaService _campaniaService;
        private readonly SupabaseStorageService _supabaseStorageService;

        public CampaniasController(CampaniaService campaniaService, SupabaseStorageService supabaseStorageService)
        {
            _campaniaService = campaniaService;
            _supabaseStorageService = supabaseStorageService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var campañas = await _campaniaService.GetAllAsync();
            return View(campañas);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CampaniaViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var campania = new Campania
            {
                Titulo = model.Titulo,
                Descripcion = model.Descripcion,
                MetaRecaudacion = model.MetaRecaudacion,
                UsuarioIdcreador = usuarioId,
                FechaCreacion = DateTime.Now,
                MontoRecaudado = 0,
                Activa = true
            };

            if (model.Imagen != null && model.Imagen.Length > 0)
            {
                campania.ImagenUrl = await _supabaseStorageService.SubirImagenAsync(model.Imagen, "campanias");
            }
            else
            {
                campania.ImagenUrl = "https://tjuafoiemlxssyyfhden.supabase.co/storage/v1/object/public/transparencia-bucket/campaign-default.jpg";
            }

            await _campaniaService.CreateAsync(campania);
            TempData["SuccessMessage"] = "Campaña creada correctamente.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var campania = await _campaniaService.GetByIdAsync(id);
            if (campania == null)
                return NotFound();

            var viewModel = new CampaniaViewModel
            {
                Titulo = campania.Titulo,
                Descripcion = campania.Descripcion,
                MetaRecaudacion = campania.MetaRecaudacion
            };

            ViewBag.CampaniaId = id;
            ViewBag.ImagenUrl = campania.ImagenUrl;
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, CampaniaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CampaniaId = id;
                return View(model);
            }

            var campania = await _campaniaService.GetByIdAsync(id);
            if (campania == null)
                return NotFound();

            campania.Titulo = model.Titulo;
            campania.Descripcion = model.Descripcion;
            campania.MetaRecaudacion = model.MetaRecaudacion;

            if (model.Imagen != null && model.Imagen.Length > 0)
            {
                campania.ImagenUrl = await _supabaseStorageService.SubirImagenAsync(model.Imagen, "campanias");
            }

            await _campaniaService.UpdateAsync(id, campania);

            TempData["SuccessMessage"] = "Campaña actualizada correctamente.";
            return RedirectToAction("Index");
        }
    }
}
