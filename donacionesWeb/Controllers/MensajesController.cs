using donacionesWeb.Models;
using donacionesWeb.Models.ViewModels;
using donacionesWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace donacionesWeb.Controllers
{
    [Authorize]
    public class MensajesController : Controller
    {
        private readonly MensajeService _mensajeService;
        private readonly RespuestaMensajeService _respuestaService;
        private readonly UsuarioService _usuarioService;
        private readonly UsuarioRolService _usuarioRolService;
        private readonly RolService _rolService;
        private readonly ILogger<MensajesController> _logger;

        public MensajesController(
            MensajeService mensajeService,
            RespuestaMensajeService respuestaService,
            UsuarioService usuarioService,
            UsuarioRolService usuarioRolService,
            RolService rolService,
            ILogger<MensajesController> logger)
        {
            _mensajeService = mensajeService;
            _respuestaService = respuestaService;
            _usuarioService = usuarioService;
            _usuarioRolService = usuarioRolService;
            _rolService = rolService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                int usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                bool isAdmin = User.IsInRole("Admin");

                if (isAdmin)
                {
                    // Vista para administradores - agrupar conversaciones por usuario
                    var todosLosMensajes = await _mensajeService.GetMensajesAsync();
                    var todasLasRespuestas = await _respuestaService.GetRespuestasAsync();

                    // Agrupar conversaciones por usuario (excluyendo al admin actual)
                    var conversacionesPorUsuario = new Dictionary<int, ConversacionViewModel>();

                    // Procesar mensajes
                    foreach (var mensaje in todosLosMensajes.Where(m => m.UsuarioOrigen != usuarioId || m.UsuarioDestino != usuarioId))
                    {
                        int otroUsuarioId = mensaje.UsuarioOrigen == usuarioId ?
                            (mensaje.UsuarioDestino ?? 0) : mensaje.UsuarioOrigen;

                        if (otroUsuarioId == 0) continue;

                        if (!conversacionesPorUsuario.ContainsKey(otroUsuarioId))
                        {
                            var otroUsuario = await _usuarioService.GetUsuarioByIdAsync(otroUsuarioId);
                            conversacionesPorUsuario[otroUsuarioId] = new ConversacionViewModel
                            {
                                UsuarioId = otroUsuarioId,
                                NombreUsuario = $"{otroUsuario.Nombre} {otroUsuario.Apellido}",
                                EmailUsuario = otroUsuario.Email,
                                MensajesYRespuestas = new List<object>()
                            };
                        }

                        conversacionesPorUsuario[otroUsuarioId].MensajesYRespuestas.Add(mensaje);
                    }

                    // Procesar respuestas
                    foreach (var respuesta in todasLasRespuestas)
                    {
                        var mensajeRelacionado = todosLosMensajes.FirstOrDefault(m => m.MensajeId == respuesta.MensajeId);
                        if (mensajeRelacionado == null) continue;

                        int otroUsuarioId = mensajeRelacionado.UsuarioOrigen == usuarioId ?
                            (mensajeRelacionado.UsuarioDestino ?? 0) : mensajeRelacionado.UsuarioOrigen;

                        if (otroUsuarioId == 0) continue;

                        if (conversacionesPorUsuario.ContainsKey(otroUsuarioId))
                        {
                            conversacionesPorUsuario[otroUsuarioId].MensajesYRespuestas.Add(respuesta);
                        }
                    }

                    // Ordenar conversaciones por último mensaje/respuesta
                    foreach (var conversacion in conversacionesPorUsuario.Values)
                    {
                        conversacion.MensajesYRespuestas = conversacion.MensajesYRespuestas.OrderByDescending(item =>
                        {
                            if (item is Mensaje mensaje)
                                return mensaje.FechaEnvio ?? DateTime.MinValue;
                            else if (item is RespuestaMensaje respuesta)
                                return respuesta.FechaRespuesta ?? DateTime.MinValue;
                            return DateTime.MinValue;
                        }).ToList();

                        // Establecer último mensaje y estado de lectura
                        var ultimo = conversacion.MensajesYRespuestas.FirstOrDefault();
                        if (ultimo is Mensaje ultimoMensaje)
                        {
                            conversacion.UltimoMensaje = ultimoMensaje.Contenido;
                            conversacion.EsLeido = ultimoMensaje.Leido ?? true;
                        }
                        else if (ultimo is RespuestaMensaje ultimaRespuesta)
                        {
                            conversacion.UltimoMensaje = ultimaRespuesta.Contenido;
                            conversacion.EsLeido = true; // Las respuestas se consideran leídas
                        }
                    }

                    var viewModelAdmin = new BandejaEntradaAdminViewModel
                    {
                        Conversaciones = conversacionesPorUsuario.Values.OrderByDescending(c =>
                            c.MensajesYRespuestas.Any() ?
                            (c.MensajesYRespuestas.First() is Mensaje m ? m.FechaEnvio :
                             ((RespuestaMensaje)c.MensajesYRespuestas.First()).FechaRespuesta) :
                            DateTime.MinValue).ToList()
                    };

                    return View("IndexAdmin", viewModelAdmin);
                }
                else
                {
                    // Vista original para usuarios normales
                    var viewModel = new BandejaEntradaViewModel
                    {
                        MensajesRecibidos = await _mensajeService.GetMensajesByUsuarioDestinoAsync(usuarioId),
                        MensajesEnviados = await _mensajeService.GetMensajesByUsuarioOrigenAsync(usuarioId)
                    };

                    foreach (var mensaje in viewModel.MensajesRecibidos.Concat(viewModel.MensajesEnviados))
                    {
                        try
                        {
                            var usuarioOrigen = await _usuarioService.GetUsuarioByIdAsync(mensaje.UsuarioOrigen);
                            mensaje.NombreUsuarioOrigen = $"{usuarioOrigen.Nombre} {usuarioOrigen.Apellido}";

                            if (mensaje.UsuarioDestino.HasValue)
                            {
                                var usuarioDestino = await _usuarioService.GetUsuarioByIdAsync(mensaje.UsuarioDestino.Value);
                                mensaje.NombreUsuarioDestino = $"{usuarioDestino.Nombre} {usuarioDestino.Apellido}";
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error al obtener información de usuarios para mensaje {MensajeId}", mensaje.MensajeId);
                        }
                    }

                    return View(viewModel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar bandeja de mensajes");
                TempData["ErrorMessage"] = "Ocurrió un error al cargar los mensajes.";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Detalle(int? id, int? usuarioId)
        {
            try
            {
                int currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                bool isAdmin = User.IsInRole("Admin");

                if (isAdmin && usuarioId.HasValue)
                {
                    // Vista de conversación para admin
                    return await DetalleConversacionAdmin(usuarioId.Value, currentUserId);
                }
                else if (id.HasValue)
                {
                    // Vista original de mensaje individual
                    return await DetalleMensajeIndividual(id.Value, currentUserId);
                }
                else
                {
                    TempData["ErrorMessage"] = "Parámetros inválidos.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar detalle");
                TempData["ErrorMessage"] = "Ocurrió un error al cargar el detalle.";
                return RedirectToAction("Index");
            }
        }

        private async Task<IActionResult> DetalleConversacionAdmin(int otroUsuarioId, int currentUserId)
        {
            // Obtener todos los mensajes entre el admin y el usuario
            var todosLosMensajes = await _mensajeService.GetMensajesAsync();
            var mensajesConversacion = todosLosMensajes.Where(m =>
                (m.UsuarioOrigen == currentUserId && m.UsuarioDestino == otroUsuarioId) ||
                (m.UsuarioOrigen == otroUsuarioId && m.UsuarioDestino == currentUserId)).ToList();

            // Obtener todas las respuestas relacionadas
            var todasLasRespuestas = await _respuestaService.GetRespuestasAsync();
            var respuestasConversacion = new List<RespuestaMensaje>();

            foreach (var mensaje in mensajesConversacion)
            {
                var respuestasMensaje = todasLasRespuestas.Where(r => r.MensajeId == mensaje.MensajeId).ToList();
                respuestasConversacion.AddRange(respuestasMensaje);
            }

            // Obtener información del otro usuario
            var otroUsuario = await _usuarioService.GetUsuarioByIdAsync(otroUsuarioId);

            // Crear lista combinada y ordenada
            var conversacionCompleta = new List<object>();
            conversacionCompleta.AddRange(mensajesConversacion.Cast<object>());
            conversacionCompleta.AddRange(respuestasConversacion.Cast<object>());

            // Ordenar por fecha
            conversacionCompleta = conversacionCompleta.OrderBy(item =>
            {
                if (item is Mensaje mensaje)
                    return mensaje.FechaEnvio ?? DateTime.MinValue;
                else if (item is RespuestaMensaje respuesta)
                    return respuesta.FechaRespuesta ?? DateTime.MinValue;
                return DateTime.MinValue;
            }).ToList();

            // Marcar mensajes como leídos si son dirigidos al admin
            foreach (var mensaje in mensajesConversacion.Where(m => m.UsuarioDestino == currentUserId && m.Leido != true))
            {
                await _mensajeService.MarcarComoLeidoAsync(mensaje.MensajeId);
            }

            // Agregar nombres de usuarios a los objetos
            foreach (var item in conversacionCompleta)
            {
                if (item is Mensaje mensaje)
                {
                    var usuarioOrigen = await _usuarioService.GetUsuarioByIdAsync(mensaje.UsuarioOrigen);
                    mensaje.NombreUsuarioOrigen = $"{usuarioOrigen.Nombre} {usuarioOrigen.Apellido}";

                    if (mensaje.UsuarioDestino.HasValue)
                    {
                        var usuarioDestino = await _usuarioService.GetUsuarioByIdAsync(mensaje.UsuarioDestino.Value);
                        mensaje.NombreUsuarioDestino = $"{usuarioDestino.Nombre} {usuarioDestino.Apellido}";
                    }
                }
                else if (item is RespuestaMensaje respuesta)
                {
                    var usuario = await _usuarioService.GetUsuarioByIdAsync(respuesta.UsuarioId);
                    respuesta.NombreUsuario = $"{usuario.Nombre} {usuario.Apellido}";
                }
            }

            var viewModel = new ConversacionDetalleViewModel
            {
                OtroUsuario = otroUsuario,
                MensajesYRespuestas = conversacionCompleta,
                UsuarioActualId = currentUserId
            };

            return View("DetalleConversacion", viewModel);
        }

        private async Task<IActionResult> DetalleMensajeIndividual(int mensajeId, int currentUserId)
        {
            var mensaje = await _mensajeService.GetMensajeByIdAsync(mensajeId);
            var respuestas = await _respuestaService.GetRespuestasByMensajeIdAsync(mensajeId);

            if (mensaje.UsuarioOrigen != currentUserId && mensaje.UsuarioDestino != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            if (mensaje.UsuarioDestino == currentUserId && mensaje.Leido != true)
            {
                await _mensajeService.MarcarComoLeidoAsync(mensajeId);
            }

            var usuarioOrigen = await _usuarioService.GetUsuarioByIdAsync(mensaje.UsuarioOrigen);
            mensaje.NombreUsuarioOrigen = $"{usuarioOrigen.Nombre} {usuarioOrigen.Apellido}";

            if (mensaje.UsuarioDestino.HasValue)
            {
                var usuarioDestino = await _usuarioService.GetUsuarioByIdAsync(mensaje.UsuarioDestino.Value);
                mensaje.NombreUsuarioDestino = $"{usuarioDestino.Nombre} {usuarioDestino.Apellido}";
            }

            foreach (var respuesta in respuestas)
            {
                var usuario = await _usuarioService.GetUsuarioByIdAsync(respuesta.UsuarioId);
                respuesta.NombreUsuario = $"{usuario.Nombre} {usuario.Apellido}";
            }

            var viewModel = new MensajeViewModel
            {
                Mensaje = mensaje,
                Respuestas = respuestas
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Responder(NuevaRespuestaViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    bool isAdmin = User.IsInRole("Admin");

                    var respuesta = new RespuestaMensaje
                    {
                        MensajeId = model.MensajeId,
                        UsuarioId = usuarioId,
                        Contenido = model.Contenido,
                        FechaRespuesta = DateTime.Now
                    };

                    await _respuestaService.CreateRespuestaAsync(respuesta);

                    TempData["SuccessMessage"] = "Respuesta enviada correctamente.";

                    // Redireccionar según el tipo de usuario
                    if (isAdmin && model.OtroUsuarioId.HasValue)
                    {
                        return RedirectToAction("Detalle", new { usuarioId = model.OtroUsuarioId.Value });
                    }
                    else
                    {
                        return RedirectToAction("Detalle", new { id = model.MensajeId });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al enviar respuesta a mensaje {MensajeId}", model.MensajeId);
                    TempData["ErrorMessage"] = "Ocurrió un error al enviar la respuesta.";
                }
            }

            // En caso de error, redireccionar apropiadamente
            if (User.IsInRole("Admin") && model.OtroUsuarioId.HasValue)
            {
                return RedirectToAction("Detalle", new { usuarioId = model.OtroUsuarioId.Value });
            }
            else
            {
                return RedirectToAction("Detalle", new { id = model.MensajeId });
            }
        }
    }
}