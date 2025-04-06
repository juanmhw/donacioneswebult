using donacionesWeb.Models;
using System.Text.Json;
using System.Text;

namespace donacionesWeb.Services
{
    public class SaldosDonacionService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5097/api/SaldosDonaciones";

        public SaldosDonacionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<SaldosDonacione>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync(BaseUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<SaldosDonacione>>() ?? new List<SaldosDonacione>();
        }

        public async Task<SaldosDonacione> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<SaldosDonacione>();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
