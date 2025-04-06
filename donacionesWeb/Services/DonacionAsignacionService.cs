using donacionesWeb.Models;
using System.Text.Json;
using System.Text;

namespace donacionesWeb.Services
{
    public class DonacionAsignacionService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5097/api/DonacionesAsignaciones";

        public DonacionAsignacionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<DonacionesAsignacione>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync(BaseUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<DonacionesAsignacione>>() ?? new List<DonacionesAsignacione>();
        }

        public async Task<DonacionesAsignacione> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<DonacionesAsignacione>();
        }

        public async Task<DonacionesAsignacione> CreateAsync(DonacionesAsignacione donacionAsignacion)
        {
            var json = JsonSerializer.Serialize(donacionAsignacion);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<DonacionesAsignacione>();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
