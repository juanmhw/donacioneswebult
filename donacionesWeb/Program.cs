using donacionesWeb.Controllers;
using donacionesWeb.Services;
using donacionesWeb.Services.Firebase;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using QuestPDF.Infrastructure;
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

// Configuración de autenticación con cookie persistente
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(30); // Cookie válida por 30 días
        options.SlidingExpiration = true; // Renueva el tiempo de expiración con cada actividad
    });

// Configuración específica para CampaniaService
builder.Services.AddHttpClient<CampaniaService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5097/api/");
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

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/var/www/dp-keys"))
    .SetApplicationName("DonacionesWeb");



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
builder.Services.AddScoped<RendicionCuentasController>();
builder.Services.AddScoped<FeedbackService>();
builder.Services.AddHttpClient<UsuarioRolService>();
builder.Services.AddHttpClient<RolService>();
builder.Services.AddHttpClient<MensajeService>();
builder.Services.AddHttpClient<RespuestaMensajeService>();
builder.Services.AddSingleton<FirebaseStorageService>();
builder.Services.AddHttpClient<SupabaseStorageService>();

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// Nota: MensajeService estaba duplicado, lo he quitado

// ... otros servicios
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseCors("PermitirFrontend");


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Asegúrate de que esto esté antes de UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();

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