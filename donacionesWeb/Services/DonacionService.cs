using donacionesWeb.Models;
using System.Text.Json;
using System.Text;

namespace donacionesWeb.Services
{
    public class DonacionService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://apidonacionesbeni.somee.com/api/Donaciones";

        public DonacionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("http://apidonacionesbeni.somee.com/api/");
        }

        public async Task<List<Donacione>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync(BaseUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Donacione>>() ?? new List<Donacione>();
        }

        public async Task<Donacione> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Donacione>();
        }

        public async Task<Donacione> CreateAsync(Donacione donacion)
        {
            var json = JsonSerializer.Serialize(donacion);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Donacione>();
        }

        public async Task<Donacione> UpdateAsync(int id, Donacione donacion)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(donacion, options);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"Donaciones/{id}", content);

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return donacion; // ← retorna el mismo objeto si no hay JSON
            }

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(result))
                return donacion;

            return JsonSerializer.Deserialize<Donacione>(result, options)
                ?? throw new Exception("No se pudo deserializar la donación");
        }


        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
