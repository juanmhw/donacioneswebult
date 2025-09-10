using donacionesWeb.Models;
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
        private readonly SaldosDonacionService _saldosDonacionService;
        private readonly CampaniaService _campaniaService;
        private readonly DetallesAsignacionService _detallesAsignacionService;

        public RendicionCuentasController(
            DonacionService donacionService,
            DonacionAsignacionService donacionAsignacionService,
            AsignacionService asignacionService,
            SaldosDonacionService saldosDonacionService,
            CampaniaService campaniaService,
            DetallesAsignacionService detallesAsignacionService)
        {
            _donacionService = donacionService;
            _donacionAsignacionService = donacionAsignacionService;
            _asignacionService = asignacionService;
            _saldosDonacionService = saldosDonacionService;
            _campaniaService = campaniaService;
            _detallesAsignacionService = detallesAsignacionService;
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

            // Obtener los saldos de las donaciones
            var todosSaldos = await _saldosDonacionService.GetAllAsync();

            // Obtener todas las campañas
            var todasCampanias = await _campaniaService.GetAllAsync();

            // Obtener todos los detalles de asignaciones
            var todosDetallesAsignaciones = await _detallesAsignacionService.GetDetallesAsync();

            // Crear el modelo para la vista
            var viewModel = new RendicionCuentasViewModel
            {
                MisDonaciones = misDonaciones,
                TotalDonado = misDonaciones.Sum(d => d.Monto),
                CampaniasApoyadas = misDonaciones.Select(d => d.CampaniaId).Distinct().Count(),
                TotalDonacionesCompletadas = misDonaciones.Count(d => d.EstadoId == 2), // Suponiendo que 2 es el ID para completadas
                UltimaDonacion = misDonaciones.OrderByDescending(d => d.FechaDonacion).FirstOrDefault()?.FechaDonacion
            };

            // Construir la información detallada de donaciones con sus asignaciones
            foreach (var donacion in misDonaciones)
            {
                // Obtener el saldo de esta donación
                var saldoDonacion = todosSaldos.FirstOrDefault(s => s.DonacionId == donacion.DonacionId);

                // Obtener la campaña asociada
                var campania = todasCampanias.FirstOrDefault(c => c.CampaniaId == donacion.CampaniaId);

                var donacionConAsignaciones = new DonacionConAsignacionesViewModel
                {
                    Donacion = donacion,
                    SaldoDonacion = saldoDonacion,
                    Campania = campania,
                    MontoOriginal = saldoDonacion?.MontoOriginal ?? donacion.Monto,
                    MontoUtilizado = saldoDonacion?.MontoUtilizado ?? 0,
                    SaldoDisponible = saldoDonacion?.SaldoDisponible ?? donacion.Monto
                };

                // Calcular el porcentaje utilizado
                donacionConAsignaciones.PorcentajeUtilizado = donacionConAsignaciones.MontoOriginal > 0
                    ? (donacionConAsignaciones.MontoUtilizado / donacionConAsignaciones.MontoOriginal) * 100
                    : 0;

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

                        // Obtener los detalles específicos de esta asignación
                        var detallesDeAsignacion = todosDetallesAsignaciones
                            .Where(d => d.AsignacionId == asignacion.AsignacionId)
                            .ToList();

                        var asignacionDetalle = new AsignacionDetalleViewModel
                        {
                            Asignacion = asignacion,
                            MontoAsignado = asignacionDonacion.MontoAsignado,
                            FechaAsignacion = asignacionDonacion.FechaAsignacion ?? DateTime.Now,
                            DetallesAsignacion = detallesDeAsignacion,
                            ComprobanteUrl = asignacion.Comprobante
                        };

                        donacionConAsignaciones.Asignaciones.Add(asignacionDetalle);
                    }
                    catch (Exception ex)
                    {
                        // Manejar el caso donde no se puede obtener la asignación
                        continue;
                    }
                }

                viewModel.DonacionesConDestino.Add(donacionConAsignaciones);
            }

            // Estadísticas adicionales
            viewModel.PromedioMontoDonado = misDonaciones.Any() ? misDonaciones.Average(d => d.Monto) : 0;
            viewModel.DonacionMasAlta = misDonaciones.Any() ? misDonaciones.Max(d => d.Monto) : 0;
            viewModel.TotalAsignado = viewModel.DonacionesConDestino.Sum(d => d.MontoUtilizado);
            viewModel.TotalPendienteAsignacion = viewModel.TotalDonado - viewModel.TotalAsignado;

            return View(viewModel);
        }

        public async Task<IActionResult> DetallesDonacion(int id)
        {
            try
            {
                if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int usuarioId))
                {
                    return RedirectToAction("Login", "Auth");
                }

                // Obtener la donación
                var donacion = await _donacionService.GetByIdAsync(id);
                if (donacion == null)
                {
                    Console.WriteLine($"No se encontró la donación con ID: {id}");
                    return NotFound("Donación no encontrada");
                }

                // Verificar que la donación pertenezca al usuario
                if (donacion.UsuarioId != usuarioId)
                {
                    return Forbid();
                }

                // Obtener campaña asociada
                Campania campania = null;
                try
                {
                    campania = await _campaniaService.GetByIdAsync(donacion.CampaniaId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al obtener campaña: {ex.Message}");
                    // Continuamos sin la campaña
                }

                // Obtener saldo de la donación
                var todosSaldos = await _saldosDonacionService.GetAllAsync();
                var saldoDonacion = todosSaldos.FirstOrDefault(s => s.DonacionId == donacion.DonacionId);

                // Obtener asignaciones
                var todasAsignaciones = await _donacionAsignacionService.GetAllAsync();
                var asignacionesDonacion = todasAsignaciones
                    .Where(a => a.DonacionId == donacion.DonacionId)
                    .ToList();

                var viewModel = new DetalleDonacionViewModel
                {
                    Donacion = donacion,
                    Campania = campania,
                    SaldoDonacion = saldoDonacion,
                    Asignaciones = new List<AsignacionDetalleViewModel>()
                };

                foreach (var asignacionDonacion in asignacionesDonacion)
                {
                    try
                    {
                        var asignacion = await _asignacionService.GetByIdAsync(asignacionDonacion.AsignacionId);
                        if (asignacion == null)
                        {
                            Console.WriteLine($"No se encontró la asignación con ID: {asignacionDonacion.AsignacionId}");
                            continue;
                        }

                        var todosDetalles = await _detallesAsignacionService.GetDetallesAsync();
                        var detalles = todosDetalles
                            .Where(d => d.AsignacionId == asignacion.AsignacionId)
                            .ToList();

                        viewModel.Asignaciones.Add(new AsignacionDetalleViewModel
                        {
                            Asignacion = asignacion,
                            MontoAsignado = asignacionDonacion.MontoAsignado,
                            FechaAsignacion = asignacionDonacion.FechaAsignacion ?? DateTime.Now,
                            DetallesAsignacion = detalles,
                            ComprobanteUrl = asignacion.Comprobante
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al procesar asignación {asignacionDonacion.AsignacionId}: {ex.Message}");
                        // Continuamos con la siguiente asignación
                    }
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general en DetallesDonacion: {ex.Message}");
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
