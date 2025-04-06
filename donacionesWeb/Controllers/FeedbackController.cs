using donacionesWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace donacionesWeb.Controllers
{
    public class FeedbackController : Controller
    {
        // Lista para almacenar los feedbacks (en un proyecto real usarías una base de datos)
        private static List<Feedback> _feedbacks = new List<Feedback>();

        // GET: /Feedback/
        public IActionResult Index()
        {
            return View(_feedbacks);
        }

        // GET: /Feedback/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Feedback/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                feedback.Id = _feedbacks.Count > 0 ? _feedbacks.Max(f => f.Id) + 1 : 1;
                feedback.FechaCreacion = DateTime.Now;
                _feedbacks.Add(feedback);

                TempData["SuccessMessage"] = "¡Gracias por tu feedback!";
                return RedirectToAction(nameof(Index));
            }

            return View(feedback);
        }

        // GET: /Feedback/Details/5
        public IActionResult Details(int id)
        {
            var feedback = _feedbacks.FirstOrDefault(f => f.Id == id);

            if (feedback == null)
            {
                return NotFound();
            }

            return View(feedback);
        }

        // GET: /Feedback/ThankYou
        public IActionResult ThankYou()
        {
            return View();
        }

    }
}
