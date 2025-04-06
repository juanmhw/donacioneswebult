using donacionesWeb.Services;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Configuración específica para CampaniaService
builder.Services.AddHttpClient<CampaniaService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5097/api/"); // Nota el /api/ al final
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

// Configuración general para otros servicios
builder.Services.AddHttpClient("DonacionesApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5097/");
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

// Registra tus servicios
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<MensajeService>();
builder.Services.AddScoped<SaldosDonacionService>();
builder.Services.AddScoped<RespuestaMensajeService>();
builder.Services.AddScoped<EstadoService>();
builder.Services.AddScoped<DonacionService>();
builder.Services.AddScoped<DonacionAsignacionService>();
builder.Services.AddScoped<DetallesAsignacionService>();
builder.Services.AddScoped<AsignacionService>();

// Nota: MensajeService estaba duplicado, lo he quitado

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Mover el conversor DateOnly aquí para que esté disponible globalmente
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