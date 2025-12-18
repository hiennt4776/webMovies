using Blazored.LocalStorage;
using BlazorWebAppAdmin;
using BlazorWebAppAdmin.Data;
using BlazorWebAppAdmin.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// ===== Add services =====
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();


// Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();


////////////
//// ApiSettings
///

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 1024 * 1024 * 500; // 500MB
});

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

// HttpClient với BaseAddress động
builder.Services.AddHttpClient<IUserEmployeeService, UserEmployeeService>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
    Console.WriteLine($"[DEBUG] API BaseUrl: {settings.BaseUrl}");
});
builder.Services.AddHttpClient<IMoviePricingService, MoviePricingService>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
    Console.WriteLine($"[DEBUG] API BaseUrl: {settings.BaseUrl}");
});

builder.Services.AddHttpClient<IMoviePricingService, MoviePricingService>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
    Console.WriteLine($"[DEBUG] API BaseUrl: {settings.BaseUrl}");
});
builder.Services.AddHttpClient<ISubscriptionPackageService, SubscriptionPackageService>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
    Console.WriteLine($"[DEBUG] API BaseUrl: {settings.BaseUrl}");
});


builder.Services.AddHttpClient<ApiClient>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
    Console.WriteLine($"[DEBUG] API BaseUrl: {settings.BaseUrl}");
});

builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BaseUrl"]);
});



builder.Services.AddScoped<AuthService>();


// Thêm các service khác
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IPartnerService, PartnerService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IPartnerService, PartnerService>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IMovieFileService, MovieFileService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IMoviePricingService, MoviePricingService>();
builder.Services.AddScoped<ISubscriptionPackageService, SubscriptionPackageService>();



var app = builder.Build();

// ===== Middleware =====
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
