using donacionesWeb.Models;
using System.Text;
using System.Text.Json;

namespace donacionesWeb.Services
{
    public class DonacionService
    {
        private readonly HttpClient _http;

        // Usamos la factory para obtener el cliente nombrado "SqlApi" (o el que definas).
        // Ese cliente debe tener BaseAddress terminando en /api/
        public DonacionService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("SqlApi");
        }

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        // GET /api/Donaciones
        public async Task<List<Donacione>> GetAllAsync(CancellationToken ct = default)
        {
            var res = await _http.GetAsync("Donaciones", ct);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<List<Donacione>>(_jsonOptions, ct)
                   ?? new List<Donacione>();
        }

        // GET /api/Donaciones/{id}
        public async Task<Donacione> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var res = await _http.GetAsync($"Donaciones/{id}", ct);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<Donacione>(_jsonOptions, ct)
                   ?? throw new Exception("No se pudo deserializar la donación");
        }

        // POST /api/Donaciones
        public async Task<Donacione> CreateAsync(Donacione donacion, CancellationToken ct = default)
        {
            var json = JsonSerializer.Serialize(donacion, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await _http.PostAsync("Donaciones", content, ct);
            res.EnsureSuccessStatusCode();

            return await res.Content.ReadFromJsonAsync<Donacione>(_jsonOptions, ct)
                   ?? throw new Exception("No se pudo deserializar la donación creada");
        }

        // PUT /api/Donaciones/{id}
        public async Task<Donacione> UpdateAsync(int id, Donacione donacion, CancellationToken ct = default)
        {
            var json = JsonSerializer.Serialize(donacion, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await _http.PutAsync($"Donaciones/{id}", content, ct);

            // Algunas APIs devuelven 204 NoContent en update:
            if (res.StatusCode == System.Net.HttpStatusCode.NoContent)
                return donacion;

            res.EnsureSuccessStatusCode();

            var body = await res.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(body))
                return donacion;

            return JsonSerializer.Deserialize<Donacione>(body, _jsonOptions)
                   ?? throw new Exception("No se pudo deserializar la donación actualizada");
        }

        // DELETE /api/Donaciones/{id}
        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var res = await _http.DeleteAsync($"Donaciones/{id}", ct);
            return res.IsSuccessStatusCode;
        }
    }
}
