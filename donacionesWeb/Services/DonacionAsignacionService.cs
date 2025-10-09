using donacionesWeb.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace donacionesWeb.Services
{
    public class DonacionAsignacionService
    {
        private readonly HttpClient _http;

        public DonacionAsignacionService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("SqlApi");
            // BaseAddress ya debe estar configurada en Program.cs → termina en /api/
        }

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        // GET /api/DonacionesAsignaciones
        public async Task<List<DonacionesAsignacione>> GetAllAsync(CancellationToken ct = default)
        {
            var res = await _http.GetAsync("DonacionesAsignaciones", ct);
            res.EnsureSuccessStatusCode();

            return await res.Content.ReadFromJsonAsync<List<DonacionesAsignacione>>(_jsonOptions, ct)
                   ?? new List<DonacionesAsignacione>();
        }

        // POST /api/DonacionesAsignaciones
        public async Task<DonacionesAsignacione> CreateAsync(DonacionesAsignacione model, CancellationToken ct = default)
        {
            var json = JsonSerializer.Serialize(model, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await _http.PostAsync("DonacionesAsignaciones", content, ct);

            if (!res.IsSuccessStatusCode)
            {
                var errorContent = await res.Content.ReadAsStringAsync(ct);
                throw new Exception($"Error al guardar la asignación de donación: {errorContent}");
            }

            // Devuelve la asignación creada si la API retorna JSON, 
            // si no, devolvemos el mismo modelo
            var created = await res.Content.ReadFromJsonAsync<DonacionesAsignacione>(_jsonOptions, ct);
            return created ?? model;
        }

        // DELETE /api/DonacionesAsignaciones/{id}
        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var res = await _http.DeleteAsync($"DonacionesAsignaciones/{id}", ct);
            return res.IsSuccessStatusCode;
        }
    }
}
