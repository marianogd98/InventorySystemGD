// Inventory.Web/Program.cs
using Inventory.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de Razor Pages
builder.Services.AddRazorPages();

// 2. Registro del Typed HttpClient para el servicio de API
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5102";

builder.Services.AddHttpClient<IInventoryApiService, InventoryApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
