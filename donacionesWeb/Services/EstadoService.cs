using donacionesWeb.Models;
using System.Text.Json;
using System.Text;

namespace donacionesWeb.Services
{
    public class EstadoService
    {
        private readonly HttpClient _http;

        // Usamos IHttpClientFactory para el cliente nombrado "SqlApi"
        public EstadoService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("SqlApi"); // BaseAddress = .../api/
        }

        // GET /api/Estados
        public async Task<List<Estado>> GetAllAsync()
        {
            var res = await _http.GetAsync("Estados");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<List<Estado>>() ?? new List<Estado>();
        }

        // GET /api/Estados/{id}
        public async Task<Estado> GetByIdAsync(int id)
        {
            var res = await _http.GetAsync($"Estados/{id}");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<Estado>()
                   ?? throw new Exception("Error al deserializar Estado");
        }

        // POST /api/Estados
        public async Task<Estado> CreateAsync(Estado estado)
        {
            var json = JsonSerializer.Serialize(estado);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var res = await _http.PostAsync("Estados", content);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<Estado>()
                   ?? throw new Exception("Error al deserializar Estado");
        }

        // PUT /api/Estados/{id}
        public async Task<Estado> UpdateAsync(int id, Estado estado)
        {
            var json = JsonSerializer.Serialize(estado);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var res = await _http.PutAsync($"Estados/{id}", content);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<Estado>()
                   ?? throw new Exception("Error al deserializar Estado");
        }

        // DELETE /api/Estados/{id}
        public async Task<bool> DeleteAsync(int id)
        {
            var res = await _http.DeleteAsync($"Estados/{id}");
            return res.IsSuccessStatusCode;
        }
    }
}
