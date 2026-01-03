using Confluent.Kafka;
using CustomerService.Service;
using CustomerService.Utils;
using dbMovies.Models;
using helperMovies.constMovies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ========== CONFIG SERVICES ==========

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Movies API",
        Version = "v1"
    });

    // 🔐 JWT Bearer support
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "'Bearer {token}' Authorization"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// ===== Entity Framework =====
builder.Services.AddDbContext<dbMoviesContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MoviesConnection")));

// ===== File Settings =====
builder.Services.Configure<FileSettings>(builder.Configuration.GetSection("FileSettings"));

// ===== JWT CONFIG =====
var secretKey = builder.Configuration["jwt:Secret-Key"];
var issuer = builder.Configuration["jwt:Issuer"];
var audience = builder.Configuration["jwt:Audience"];

if (string.IsNullOrEmpty(secretKey))
    throw new InvalidOperationException("JWT Secret-Key not configured in appsettings.json");

var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };

    // Log event khi JWT lỗi
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("JWT Authentication Failed:");
            Console.WriteLine(context.Exception.ToString());
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine($"✅ JWT Token Validated for: {context.Principal?.Identity?.Name}");
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine("⚠️ JWT Challenge Triggered (token missing or invalid)");
            return Task.CompletedTask;
        },
        OnForbidden = context =>
        {
            Console.WriteLine("⛔ JWT Forbidden: insufficient permissions");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// ===== Register Services =====
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IUserCustomerRepository, UserCustomerRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IWatchHistoryRepository, WatchHistoryRepository> ();
builder.Services.AddScoped<ISubscriptionPackageRepository, SubscriptionPackageRepository>();
builder.Services.AddScoped<IMoviePricingRepository, MoviePricingRepository>();


builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<JwtAuthService>();

builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<ICustomerAuthService, CustomerAuthService>();
builder.Services.AddScoped<ICustomerService, CustomerDetailService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUserCustomerService, UserCustomerService>();
builder.Services.AddScoped<IWatchHistoryService, WatchHistoryService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<ISubscriptionPackageService, SubscriptionPackageService>();
builder.Services.AddScoped<IMoviePricingService, MoviePricingService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

// ========== BUILD APP ==========
var app = builder.Build();

// ========== PIPELINE ==========

// Swagger (Development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// === Middleware log request ===
//app.Use(async (context, next) =>
//{
//    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
//    Console.WriteLine("==== Incoming Request ====");
//    Console.WriteLine($"Method: {context.Request.Method}");
//    Console.WriteLine($"Path: {context.Request.Path}");
//    Console.WriteLine($"QueryString: {context.Request.QueryString}");
//    Console.WriteLine($"Authorization: {authHeader ?? "No token"}");

//    if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
//    {
//        var tokenStr = authHeader.Replace("Bearer ", "");
//        try
//        {
//            var handler = new JwtSecurityTokenHandler();
//            var jwtToken = handler.ReadJwtToken(tokenStr);
//            Console.WriteLine("Decoded JWT Claims:");
//            foreach (var claim in jwtToken.Claims)
//            {
//                Console.WriteLine($" - {claim.Type}: {claim.Value}");
//            }
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine("Failed to decode JWT: " + ex.Message);
//        }
//    }

//    await next.Invoke();
//});

app.Use(async (context, next) =>
{

    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
    Console.WriteLine("==== Incoming Request ====");
    Console.WriteLine($"Method: {context.Request.Method}");
    Console.WriteLine($"Path: {context.Request.Path}");
    Console.WriteLine("QueryString: " + context.Request.QueryString);
    Console.WriteLine($"Authorization: {authHeader ?? "No token"}");

    if (context.Request.Headers.TryGetValue("Authorization", out var stringsauthHeader))
    {
        var tokenStr = stringsauthHeader.ToString().Replace("Bearer ", "");
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(tokenStr);
            Console.WriteLine("Decoded JWT Claims:");
            foreach (var claim in jwtToken.Claims)
            {
                Console.WriteLine($"{claim.Type}: {claim.Value}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to decode JWT: " + ex.Message);
        }
    }

    // Log body nếu có
    if (context.Request.ContentLength > 0)
    {
        context.Request.EnableBuffering(); // cho phép đọc body nhiều lần
        using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0; // reset stream
        Console.WriteLine("Body:");
        Console.WriteLine(body);
    }

    await next.Invoke(); // gọi tiếp middleware / controller
});


// HTTPS redirect
app.UseHttpsRedirection();

// Static files (E:\File\movies)
using (var scope = app.Services.CreateScope())
{
    var options = scope.ServiceProvider.GetRequiredService<IOptions<FileSettings>>();
    var fileSettings = options.Value;
    var moviePath = Path.Combine(fileSettings.FilesPath, "movies");

    Console.WriteLine($"🧭 Static file path: {moviePath}");

    if (Directory.Exists(moviePath))
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(moviePath),
            RequestPath = "/movies",
            ServeUnknownFileTypes = true,
            OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
            }
        });
        Console.WriteLine("✅ Static file mapping added successfully!");
    }
    else
    {
        Console.WriteLine($"❌ Directory not found: {moviePath}");
    }
}

// Routing
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

