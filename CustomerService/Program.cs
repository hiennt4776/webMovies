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
        Description = "Nhập token vào theo định dạng: Bearer {token}"
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

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IUserCustomerRepository, UserCustomerRepository>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<JwtAuthService>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<ICustomerAuthService, CustomerAuthService>();
builder.Services.AddScoped<IUserCustomerService, UserCustomerService>();

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
app.Use(async (context, next) =>
{
    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
    Console.WriteLine("==== Incoming Request ====");
    Console.WriteLine($"Method: {context.Request.Method}");
    Console.WriteLine($"Path: {context.Request.Path}");
    Console.WriteLine($"QueryString: {context.Request.QueryString}");
    Console.WriteLine($"Authorization: {authHeader ?? "No token"}");

    if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
    {
        var tokenStr = authHeader.Replace("Bearer ", "");
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(tokenStr);
            Console.WriteLine("Decoded JWT Claims:");
            foreach (var claim in jwtToken.Claims)
            {
                Console.WriteLine($" - {claim.Type}: {claim.Value}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to decode JWT: " + ex.Message);
        }
    }

    await next.Invoke();
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

// Map Controllers
app.MapControllers();


app.Run();
