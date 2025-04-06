using donacionesWeb.Models;
using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace donacionesWeb.Services
{
    public class CampaniaService
    {
        private readonly HttpClient _httpClient;

        public CampaniaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("http://localhost:5097/api/");
        }

        public async Task<IEnumerable<Campania>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<Campania>>("Campanias");
        }

        public async Task<Campania> GetByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<Campania>($"Campanias/{id}");
        }

        public async Task CreateAsync(Campania campania)
        {
            try
            {
                var campaniaApi = new
                {
                    titulo = campania.Titulo,
                    descripcion = campania.Descripcion,
                    fechaInicio = DateOnly.FromDateTime(campania.FechaInicio),
                    fechaFin = campania.FechaFin.HasValue ?
                              DateOnly.FromDateTime(campania.FechaFin.Value) :
                              (DateOnly?)null,
                    metaRecaudacion = campania.MetaRecaudacion,
                    montoRecaudado = campania.MontoRecaudado ?? 0,
                    usuarioIdcreador = campania.UsuarioIdcreador,
                    activa = campania.Activa ?? true,
                    fechaCreacion = DateTime.Now
                };

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new DateOnlyJsonConverter() }
                };

                var json = JsonSerializer.Serialize(campaniaApi, options);
                Console.WriteLine($"Enviando a la API: {json}");

                var response = await _httpClient.PostAsJsonAsync("Campanias", campaniaApi, options);

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

        public async Task DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"Campanias/{id}");
            response.EnsureSuccessStatusCode();
        }

        public class DateOnlyJsonConverter : JsonConverter<DateOnly>
        {
            private const string Format = "yyyy-MM-dd";

            public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return DateOnly.Parse(reader.GetString());
            }

            public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString(Format));
            }
        }
    }
}
