using System.Linq;
using donacionesWeb.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace donacionesWeb.Services
{
    public class SaldosDonacionService
    {
        private readonly HttpClient _http;

        public SaldosDonacionService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("SqlApi");
            // BaseAddress ya configurada en Program.cs → debe terminar en /api/
        }

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        // GET /api/SaldosDonaciones
        public async Task<List<SaldosDonacione>> GetAllAsync(CancellationToken ct = default)
        {
            var res = await _http.GetAsync("SaldosDonaciones", ct);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<List<SaldosDonacione>>(_jsonOptions, ct)
                   ?? new List<SaldosDonacione>();
        }

        // POST /api/SaldosDonaciones
        public async Task<SaldosDonacione> CreateAsync(SaldosDonacione nuevo, CancellationToken ct = default)
        {
            var res = await _http.PostAsJsonAsync("SaldosDonaciones", nuevo, _jsonOptions, ct);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<SaldosDonacione>(_jsonOptions, ct)
                   ?? nuevo;
        }

        // GET /api/SaldosDonaciones/{id}
        public async Task<SaldosDonacione?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var res = await _http.GetAsync($"SaldosDonaciones/{id}", ct);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<SaldosDonacione>(_jsonOptions, ct);
        }

        // GET /api/SaldosDonaciones/donacion/{donacionId}
        public async Task<SaldosDonacione?> GetByDonacionIdAsync(int donacionId, CancellationToken ct = default)
        {
            var res = await _http.GetAsync($"SaldosDonaciones/donacion/{donacionId}", ct);
            res.EnsureSuccessStatusCode();

            var lista = await res.Content.ReadFromJsonAsync<List<SaldosDonacione>>(_jsonOptions, ct);
            return lista?.FirstOrDefault();
        }

        // PUT /api/SaldosDonaciones/{id}
        public async Task<SaldosDonacione> UpdateAsync(int id, SaldosDonacione saldo, CancellationToken ct = default)
        {
            var json = JsonSerializer.Serialize(saldo, _jsonOptions);
            Console.WriteLine($"📤 PUT SaldosDonaciones/{id} saldoDisponible:{saldo.SaldoDisponible} utilizado:{saldo.MontoUtilizado}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var res = await _http.PutAsync($"SaldosDonaciones/{id}", content, ct);

            if (res.StatusCode == System.Net.HttpStatusCode.NoContent)
                return saldo; // algunas APIs no devuelven body

            res.EnsureSuccessStatusCode();

            var body = await res.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(body))
                return saldo;

            return JsonSerializer.Deserialize<SaldosDonacione>(body, _jsonOptions)
                   ?? throw new Exception("Error al deserializar saldo actualizado");
        }

        // DELETE /api/SaldosDonaciones/{id}
        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var res = await _http.DeleteAsync($"SaldosDonaciones/{id}", ct);
            return res.IsSuccessStatusCode;
        }
    }
}
