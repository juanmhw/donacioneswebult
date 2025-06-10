using donacionesWeb.Models;
using donacionesWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace donacionesWeb.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly FeedbackService _feedbackService;
        private readonly ILogger<FeedbackController> _logger;

        public FeedbackController(FeedbackService feedbackService, ILogger<FeedbackController> logger)
        {
            _feedbackService = feedbackService;
            _logger = logger;
        }

        // GET: /Feedback - Muestra todos los feedbacks (puede requerir admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var feedbacks = await _feedbackService.GetAllFeedbacksAsync();
            return View(feedbacks);
        }

        // GET: /Feedback/Mis - Muestra los feedbacks del usuario actual
        [Authorize]
        public async Task<IActionResult> Mis()
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (usuarioIdClaim != null && int.TryParse(usuarioIdClaim.Value, out int usuarioId))
            {
                var feedbacks = await _feedbackService.GetFeedbacksByUserIdAsync(usuarioId);
                return View(feedbacks);
            }
            return RedirectToAction("Login", "Auth");
        }

        // GET: /Feedback/Create
        public IActionResult Create()
        {
            var feedback = new Feedback();

            // Si el usuario está autenticado, prellenar datos
            if (User.Identity.IsAuthenticated)
            {
                var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var emailClaim = User.FindFirst(ClaimTypes.Email);
                var nameClaim = User.FindFirst(ClaimTypes.Name);

                if (usuarioIdClaim != null && int.TryParse(usuarioIdClaim.Value, out int usuarioId))
                {
                    feedback.UsuarioId = usuarioId;
                }

                if (emailClaim != null)
                {
                    feedback.Email = emailClaim.Value;
                }

                if (nameClaim != null)
                {
                    feedback.Nombre = nameClaim.Value;
                }
            }

            return View(feedback);
        }

        // POST: /Feedback/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Si el usuario está autenticado, asignar su ID
                    if (User.Identity.IsAuthenticated)
                    {
                        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                        if (usuarioIdClaim != null && int.TryParse(usuarioIdClaim.Value, out int usuarioId))
                        {
                            feedback.UsuarioId = usuarioId;
                        }
                    }

                    feedback.FechaCreacion = DateTime.Now;

                    var result = await _feedbackService.CreateFeedbackAsync(feedback);
                    if (result)
                    {
                        TempData["SuccessMessage"] = "¡Gracias por tu comentario! Valoramos mucho tu opinión.";
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "No se pudo guardar el comentario. Por favor, intenta de nuevo.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear feedback");
                    ModelState.AddModelError("", "Ocurrió un error al enviar tu comentario.");
                }
            }
            return View(feedback);
        }

        // GET: /Feedback/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var feedback = await _feedbackService.GetFeedbackByIdAsync(id);
            if (feedback == null)
            {
                return NotFound();
            }

            return View(feedback);
        }

        // Método auxiliar para calificaciones promedio (puede mostrarse en el dashboard)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Estadisticas()
        {
            var feedbacks = await _feedbackService.GetAllFeedbacksAsync();

            var promedio = feedbacks.Count > 0 ? feedbacks.Average(f => f.Calificacion) : 0;
            var total = feedbacks.Count;
            var distribucion = new Dictionary<int, int>();

            for (int i = 1; i <= 5; i++)
            {
                distribucion[i] = feedbacks.Count(f => f.Calificacion == i);
            }

            ViewData["Promedio"] = promedio;
            ViewData["Total"] = total;
            ViewData["Distribucion"] = distribucion;

            return View();
        }
    }
}