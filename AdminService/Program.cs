using AdminService.Service;
using Confluent.Kafka;
using dbMovies.Models;
using helperMovies.constMovies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using Microsoft.OpenApi.Models;
using AdminService.Services;
using AdminService.Utils;



var builder = WebApplication.CreateBuilder(args);

// ========== CONFIG SERVICES ==========
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger + JWT Bearer Support
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Movies API",
        Version = "v1"
    });

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
            new string[] {}
        }
    });
});

// DbContext
builder.Services.AddDbContext<dbMoviesContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MoviesConnection")));

// File config
builder.Services.Configure<FileSettings>(builder.Configuration.GetSection("FileSettings"));

// ================= JWT CONFIG =================
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
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = key,
        ClockSkew = TimeSpan.FromDays(2),
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.Name,
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
            Console.WriteLine("JWT Token Validated for: " + context.Principal?.Identity?.Name);
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine("JWT Challenge Triggered (token missing or invalid)");
            return Task.CompletedTask;
        },
        OnForbidden = context =>
        {
            Console.WriteLine("JWT Forbidden: insufficient permissions");
            return Task.CompletedTask;
        }
    };
});


builder.Services.AddAuthorization();

// ================= DEPENDENCY INJECTION =================
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IUserEmployeeRepository, UserEmployeeRepository>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IMovieFileRepository, MovieFileRepository>();
builder.Services.AddScoped<IMoviePricingRepository, MoviePricingRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IPartnerRepository, PartnerRepository>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IRoleUserRepository, IRoleUsersService>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ISubscriptionPackageRepository, SubscriptionPackageRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<JwtAuthService>();
builder.Services.AddScoped<IUserEmployeeService, UserEmployeeService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IPartnerService, PartnerService>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IMovieFileService, MovieFileService>();
builder.Services.AddScoped<IMoviePricingService, MoviePricingService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ISubscriptionPackageService, SubscriptionPackageService>();


builder.Services.AddHttpContextAccessor();

// ========== BUILD APP ==========
var app = builder.Build();

// ========== PIPELINE ==========
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Log request Authorization header (Debug)
app.Use(async (context, next) =>
{

    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
    Console.WriteLine("==== Incoming Request ====");
    Console.WriteLine($"Method: {context.Request.Method}");
    Console.WriteLine($"Path: {context.Request.Path}");
    Console.WriteLine("QueryString: " + context.Request.QueryString);
    Console.WriteLine($"Authorization: {authHeader ?? "No token"}");
    //Console.WriteLine("Headers:");
    //foreach (var header in context.Request.Headers)
    //{
    //    Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
    //}

    // Decode Authorization token nếu có
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


// Đúng thứ tự middleware
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();


