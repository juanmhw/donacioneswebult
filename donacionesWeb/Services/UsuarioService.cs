using donacionesWeb.Models;
using System.Text.Json;
using System.Text;

namespace donacionesWeb.Services
{
    public class UsuarioService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5097/api/Usuarios";

        public UsuarioService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Usuario>> GetUsuariosAsync()
        {
            var response = await _httpClient.GetAsync(BaseUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Usuario>>() ?? new List<Usuario>();
        }

        public async Task<Usuario> GetUsuarioByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Usuario>() ?? throw new Exception("Error al deserializar usuario");
        }

        public async Task<Usuario> CreateUsuarioAsync(Usuario usuario)
        {
            var json = JsonSerializer.Serialize(usuario);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Usuario>() ?? throw new Exception("Error al deserializar usuario");
        }

        public async Task<Usuario> UpdateUsuarioAsync(int id, Usuario usuario)
        {
            var json = JsonSerializer.Serialize(usuario);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{id}", content);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Usuario>() ?? throw new Exception("Error al deserializar usuario");
        }

        public async Task<bool> DeleteUsuarioAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<Usuario?> GetUsuarioByEmailAsync(string email)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/email/{email}");

                // Registrar detalles de la respuesta
                Console.WriteLine($"Respuesta de API para email {email}: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error al obtener usuario por email: {response.StatusCode}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Contenido de respuesta: {content}");

                return JsonSerializer.Deserialize<Usuario>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al obtener usuario por email: {ex.Message}");
                throw;
            }
        }
    }
}