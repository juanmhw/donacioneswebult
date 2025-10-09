using donacionesWeb.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace donacionesWeb.Services
{
    public class DetallesAsignacionService
    {
        private readonly HttpClient _http;

        public DetallesAsignacionService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("SqlApi");
            // BaseAddress ya configurada en Program.cs y debe terminar en /api/
        }

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        // GET /api/DetallesAsignacions
        public async Task<List<DetallesAsignacion>> GetDetallesAsync(CancellationToken ct = default)
        {
            var res = await _http.GetAsync("DetallesAsignacions", ct);
            res.EnsureSuccessStatusCode();

            return await res.Content.ReadFromJsonAsync<List<DetallesAsignacion>>(_jsonOptions, ct)
                   ?? new List<DetallesAsignacion>();
        }

        // GET /api/DetallesAsignacions/asignacion/{asignacionId}
        public async Task<List<DetallesAsignacion>> GetByAsignacionAsync(int asignacionId, CancellationToken ct = default)
        {
            var res = await _http.GetAsync($"DetallesAsignacions/asignacion/{asignacionId}", ct);
            res.EnsureSuccessStatusCode();

            return await res.Content.ReadFromJsonAsync<List<DetallesAsignacion>>(_jsonOptions, ct)
                   ?? new List<DetallesAsignacion>();
        }

        // POST /api/DetallesAsignacions
        public async Task<DetallesAsignacion> CreateDetalleAsync(DetallesAsignacion detalle, CancellationToken ct = default)
        {
            var json = JsonSerializer.Serialize(detalle, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await _http.PostAsync("DetallesAsignacions", content, ct);

            if (!res.IsSuccessStatusCode)
            {
                var error = await res.Content.ReadAsStringAsync(ct);
                throw new Exception($"Error al crear el detalle: {error}");
            }

            // Devuelve el detalle creado si la API responde con JSON, si no, devolvemos el payload original
            var created = await res.Content.ReadFromJsonAsync<DetallesAsignacion>(_jsonOptions, ct);
            return created ?? detalle;
        }

        // DELETE /api/DetallesAsignacions/{id}
        public async Task<bool> DeleteDetalleAsync(int id, CancellationToken ct = default)
        {
            var res = await _http.DeleteAsync($"DetallesAsignacions/{id}", ct);
            return res.IsSuccessStatusCode;
        }
    }
}
