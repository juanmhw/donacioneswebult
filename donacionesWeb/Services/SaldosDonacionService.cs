using donacionesWeb.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace donacionesWeb.Services
{
    public class SaldosDonacionService
    {
        private readonly HttpClient _httpClient;

        public SaldosDonacionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("http://localhost:5097/api/"); // ✅ BaseAddress importante
        }

        public async Task<List<SaldosDonacione>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync("SaldosDonaciones");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<SaldosDonacione>>() ?? new List<SaldosDonacione>();
        }

        public async Task<SaldosDonacione> CreateAsync(SaldosDonacione nuevo)
        {
            var response = await _httpClient.PostAsJsonAsync("SaldosDonaciones", nuevo);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<SaldosDonacione>();
        }


        public async Task<SaldosDonacione> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"SaldosDonaciones/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<SaldosDonacione>();
        }

        public async Task<SaldosDonacione?> GetByDonacionIdAsync(int donacionId)
        {
            var response = await _httpClient.GetAsync($"SaldosDonaciones/donacion/{donacionId}");
            response.EnsureSuccessStatusCode();
            var lista = await response.Content.ReadFromJsonAsync<List<SaldosDonacione>>();
            return lista?.FirstOrDefault();
        }

        public async Task<SaldosDonacione> UpdateAsync(int id, SaldosDonacione saldo)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(saldo, options);
            Console.WriteLine($"📤 Enviando PUT SaldosDonaciones/{id} con saldo disponible: {saldo.SaldoDisponible}, utilizado: {saldo.MontoUtilizado}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"SaldosDonaciones/{id}", content);

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                return saldo; // la API no devuelve contenido

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(result))
                return saldo;

            return JsonSerializer.Deserialize<SaldosDonacione>(result, options)
                ?? throw new Exception("Error al deserializar saldo actualizado");
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"SaldosDonaciones/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
