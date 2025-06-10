using donacionesWeb.Models;
using System.Text;
using System.Text.Json;

namespace donacionesWeb.Services
{
    public class AsignacionService
    {
        private readonly HttpClient _httpClient;

        public AsignacionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("http://localhost:5097/api/");
        }

        public async Task<IEnumerable<Asignacione>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<Asignacione>>("Asignaciones");
        }

        public async Task<Asignacione> GetByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<Asignacione>($"Asignaciones/{id}");
        }

        public async Task CreateAsync(Asignacione asignacion)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var response = await _httpClient.PostAsJsonAsync("Asignaciones", asignacion, options);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Error API: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en servicio: {ex.Message}");
                throw;
            }
        }

        public async Task<Asignacione> UpdateAsync(int id, Asignacione asignacion)
        {
            try
            {
                asignacion.FechaAsignacion ??= DateTime.Now;
                asignacion.Descripcion ??= "Asignación actualizada automáticamente";
                if (asignacion.CampaniaId == 0) asignacion.CampaniaId = 1;
                if (asignacion.UsuarioId == 0) asignacion.UsuarioId = 1;

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(asignacion, options);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"Asignaciones/{id}", content);

                // ✅ No hay contenido, pero es éxito
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                    return asignacion;

                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(responseString))
                    return asignacion;

                return JsonSerializer.Deserialize<Asignacione>(responseString, options)
                    ?? asignacion;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en UpdateAsync: {ex.Message}");
                throw;
            }
        }


        public async Task DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"Asignaciones/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
