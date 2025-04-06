using donacionesWeb.Models;
using System.Text.Json;
using System.Text;

namespace donacionesWeb.Services
{
    public class DonacionService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5097/api/Donaciones";

        public DonacionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
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

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
