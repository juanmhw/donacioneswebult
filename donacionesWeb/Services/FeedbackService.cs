using donacionesWeb.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace donacionesWeb.Services
{
    public class FeedbackService
    {
        private readonly HttpClient _httpClient;

        public FeedbackService(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient("MongoApi");
        }

        public async Task<List<Feedback>> GetAllFeedbacksAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("Comentarios");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<Feedback>>() ?? new List<Feedback>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo feedbacks: {ex.Message}");
                return new List<Feedback>();
            }
        }

        public async Task<List<Feedback>> GetFeedbacksByUserIdAsync(int usuarioId)
        {
            try
            {
                var allFeedbacks = await GetAllFeedbacksAsync();
                return allFeedbacks.Where(f => f.UsuarioId == usuarioId).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo feedbacks por usuario: {ex.Message}");
                return new List<Feedback>();
            }
        }

        public async Task<Feedback?> GetFeedbackByIdAsync(string id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Comentarios/{id}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<Feedback>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo feedback por ID: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CreateFeedbackAsync(Feedback feedback)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("Comentarios", feedback);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creando feedback: {ex.Message}");
                return false;
            }
        }
    }
}
