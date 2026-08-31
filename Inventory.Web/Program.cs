// Inventory.Web/Program.cs
using Inventory.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de Razor Pages y mensajes de validación en español
builder.Services.AddRazorPages()
    .AddMvcOptions(options =>
    {
        var p = options.ModelBindingMessageProvider;
        p.SetValueMustNotBeNullAccessor(name => $"El campo '{name}' es obligatorio.");
        p.SetValueIsInvalidAccessor(value => $"El valor '{value}' no es válido.");
        p.SetValueMustBeANumberAccessor(name => $"El campo '{name}' debe ser un valor numérico (no se permiten letras ni caracteres).");
        p.SetMissingKeyOrValueAccessor(() => "Se requiere un valor.");
        p.SetNonPropertyAttemptedValueIsInvalidAccessor(value => $"El valor '{value}' no es válido.");
        p.SetNonPropertyUnknownValueIsInvalidAccessor(() => "El valor ingresado no es válido.");
        p.SetNonPropertyValueMustBeANumberAccessor(() => "El valor debe ser un número (no se permiten letras ni caracteres).");
        p.SetUnknownValueIsInvalidAccessor(name => $"El valor ingresado para '{name}' no es válido.");
        p.SetAttemptedValueIsInvalidAccessor((value, name) => $"El valor '{value}' no es válido para el campo '{name}' (no se permiten letras ni caracteres).");
        p.SetMissingBindRequiredValueAccessor(name => $"No se proporcionó un valor para el campo '{name}'.");
        p.SetMissingRequestBodyRequiredValueAccessor(() => "El cuerpo de la solicitud no puede estar vacío.");
    });

// 2. Registro del Typed HttpClient para el servicio de API
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5102";

builder.Services.AddHttpClient<IInventoryApiService, InventoryApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
