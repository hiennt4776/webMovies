
using Blazored.LocalStorage;
using BlazorWebAppCustomer;
using BlazorWebAppCustomer.Data;
using BlazorWebAppCustomer.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.FileProviders;
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
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<InvoiceService>();
//builder.Services.AddScoped<AuthorizationMessageHandler>();

////////////
//// ApiSettings
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

// HttpClient với BaseAddress động
//builder.Services.AddHttpClient<IUserCustomerService, UserCustomerService>((sp, client) =>
//{
//    var settings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
//    client.BaseAddress = new Uri(settings.BaseUrl);
//    Console.WriteLine($"[DEBUG] API BaseUrl: {settings.BaseUrl}");
//});

builder.Services.AddHttpClient<IMovieService, MovieService>((sp, client) =>
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


builder.Services.AddHttpClient<IUserCustomerService, UserCustomerService>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
    Console.WriteLine($"[DEBUG] API BaseUrl: {settings.BaseUrl}");
});


builder.Services.AddHttpClient<ICustomerService, CustomerService>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
    Console.WriteLine($"[DEBUG] API BaseUrl: {settings.BaseUrl}");
});

builder.Services.AddHttpClient<ICategoryService, CategoryService>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
    Console.WriteLine($"[DEBUG] API BaseUrl: {settings.BaseUrl}");
});

builder.Services.AddHttpClient<IWatchHistoryClientService, WatchHistoryClientService>((sp, client) =>
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


builder.Services.AddHttpClient<IPaymentClientService, PaymentClientService>((sp, client) =>
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

builder.Services.AddHttpClient<IInvoiceService, InvoiceService>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
    Console.WriteLine($"[DEBUG] API BaseUrl: {settings.BaseUrl}");
});


builder.Services.AddHttpClient<WatchHistoryClientService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]);
});


builder.Services.AddHttpClient<MovieService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]);
});

builder.Services.AddHttpClient<SubscriptionPackageService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]);
});



builder.Services.AddHttpClient<PaymentClientService> (client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]);
});


builder.Services.AddHttpClient<MoviePricingService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]);
});
builder.Services.AddHttpClient<InvoiceService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]);
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.Run();