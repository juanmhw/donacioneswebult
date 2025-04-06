using donacionesWeb.Models.ViewModels;
using donacionesWeb.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace donacionesWeb.Controllers
{
    public class RendicionCuentasController : Controller
    {
        private readonly DonacionService _donacionService;
        private readonly DonacionAsignacionService _donacionAsignacionService;
        private readonly AsignacionService _asignacionService;

        public RendicionCuentasController(
            DonacionService donacionService,
            DonacionAsignacionService donacionAsignacionService,
            AsignacionService asignacionService)
        {
            _donacionService = donacionService;
            _donacionAsignacionService = donacionAsignacionService;
            _asignacionService = asignacionService;
        }

        public async Task<IActionResult> Index()
        {
            // Obtener el ID del usuario actual desde los claims
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int usuarioId))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Obtener todas las donaciones del usuario
            var todasLasDonaciones = await _donacionService.GetAllAsync();
            var misDonaciones = todasLasDonaciones.Where(d => d.UsuarioId == usuarioId).ToList();

            // Obtener todas las asignaciones de donaciones
            var todasLasAsignaciones = await _donacionAsignacionService.GetAllAsync();

            // Crear el modelo para la vista
            var viewModel = new RendicionCuentasViewModel
            {
                MisDonaciones = misDonaciones,
                TotalDonado = misDonaciones.Sum(d => d.Monto),
                CampaniasApoyadas = misDonaciones.Select(d => d.CampaniaId).Distinct().Count()
            };

            // Construir la información detallada de donaciones con sus asignaciones
            foreach (var donacion in misDonaciones)
            {
                var donacionConAsignaciones = new DonacionConAsignacionesViewModel
                {
                    Donacion = donacion
                };

                // Buscar las asignaciones para esta donación
                var asignacionesDeDonacion = todasLasAsignaciones
                    .Where(da => da.DonacionId == donacion.DonacionId)
                    .ToList();

                foreach (var asignacionDonacion in asignacionesDeDonacion)
                {
                    try
                    {
                        // Obtener detalles de la asignación
                        var asignacion = await _asignacionService.GetByIdAsync(asignacionDonacion.AsignacionId);

                        donacionConAsignaciones.Asignaciones.Add(new AsignacionDetalleViewModel
                        {
                            Asignacion = asignacion,
                            MontoAsignado = asignacionDonacion.MontoAsignado,
                            FechaAsignacion = asignacionDonacion.FechaAsignacion ?? DateTime.Now
                        });
                    }
                    catch (Exception)
                    {
                        // Manejar el caso donde no se puede obtener la asignación
                        continue;
                    }
                }

                viewModel.DonacionesConDestino.Add(donacionConAsignaciones);
            }

            return View(viewModel);
        }

    }
}
