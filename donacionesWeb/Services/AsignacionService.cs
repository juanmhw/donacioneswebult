using donacionesWeb.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace donacionesWeb.Services
{
    public class AsignacionService
    {
        private readonly HttpClient _http;

        public AsignacionService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("SqlApi"); // BaseAddress termina en /api/
        }

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        // GET /api/Asignaciones
        public async Task<IEnumerable<Asignacione>> GetAllAsync(CancellationToken ct = default)
        {
            return await _http.GetFromJsonAsync<IEnumerable<Asignacione>>("Asignaciones", _jsonOptions, ct)
                   ?? Enumerable.Empty<Asignacione>();
        }

        // GET /api/Asignaciones/{id}
        public async Task<Asignacione?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _http.GetFromJsonAsync<Asignacione>($"Asignaciones/{id}", _jsonOptions, ct);
        }

        // POST /api/Asignaciones
        public async Task<Asignacione> CreateAsync(Asignacione asignacion, CancellationToken ct = default)
        {
            var res = await _http.PostAsJsonAsync("Asignaciones", asignacion, _jsonOptions, ct);

            if (!res.IsSuccessStatusCode)
            {
                var error = await res.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"Error API: {res.StatusCode} - {error}");
            }

            var created = await res.Content.ReadFromJsonAsync<Asignacione>(_jsonOptions, ct);
            return created ?? asignacion;
        }

        // PUT /api/Asignaciones/{id}
        public async Task<Asignacione> UpdateAsync(int id, Asignacione asignacion, CancellationToken ct = default)
        {
            // Defaults defensivos (opcional)
            asignacion.FechaAsignacion ??= DateTime.Now;
            asignacion.Descripcion ??= "Asignación actualizada automáticamente";
            if (asignacion.CampaniaId == 0) asignacion.CampaniaId = 1;
            if (asignacion.UsuarioId == 0) asignacion.UsuarioId = 1;

            var json = JsonSerializer.Serialize(asignacion, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await _http.PutAsync($"Asignaciones/{id}", content, ct);

            if (res.StatusCode == System.Net.HttpStatusCode.NoContent)
                return asignacion;

            res.EnsureSuccessStatusCode();

            var body = await res.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(body))
                return asignacion;

            return JsonSerializer.Deserialize<Asignacione>(body, _jsonOptions) ?? asignacion;
        }

        // DELETE /api/Asignaciones/{id}
        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var res = await _http.DeleteAsync($"Asignaciones/{id}", ct);
            res.EnsureSuccessStatusCode();
        }
    }
}
