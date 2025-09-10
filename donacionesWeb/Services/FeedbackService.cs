using donacionesWeb.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace donacionesWeb.Services
{
    public class FeedbackService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = "http://www.apimongo.somee.com/api/Comentarios";

        public FeedbackService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Feedback>> GetAllFeedbacksAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_apiUrl);
                response.EnsureSuccessStatusCode();
                var feedbacks = await response.Content.ReadFromJsonAsync<List<Feedback>>();
                return feedbacks ?? new List<Feedback>();
            }
            catch (Exception ex)
            {
                // En producción debería registrarse el error
                Console.WriteLine($"Error obteniendo feedbacks: {ex.Message}");
                return new List<Feedback>();
            }
        }

        public async Task<List<Feedback>> GetFeedbacksByUserIdAsync(int usuarioId)
        {
            try
            {
                var response = await _httpClient.GetAsync(_apiUrl);
                response.EnsureSuccessStatusCode();
                var allFeedbacks = await response.Content.ReadFromJsonAsync<List<Feedback>>();

                // Filtrar por usuario ID
                return allFeedbacks?.Where(f => f.UsuarioId == usuarioId).ToList() ?? new List<Feedback>();
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
                var response = await _httpClient.GetAsync($"{_apiUrl}/{id}");
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
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(feedback),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(_apiUrl, jsonContent);
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