using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PandesalExpress.Auth;
using PandesalExpress.Cashier;
using PandesalExpress.Commissary;
using PandesalExpress.Host.EventHandlers;
using PandesalExpress.Host.Hubs;
using PandesalExpress.Host.Services;
using PandesalExpress.Infrastructure;
using PandesalExpress.Infrastructure.Configs;
using PandesalExpress.Infrastructure.Context;
using PandesalExpress.Infrastructure.Models;
using PandesalExpress.Infrastructure.Seeding.Extensions;
using PandesalExpress.Infrastructure.Seeding.Seeders;
using PandesalExpress.Infrastructure.Services;
using PandesalExpress.Management;
using PandesalExpress.PDND;
using PandesalExpress.Products;
using PandesalExpress.Stores;
using PandesalExpress.Transfers;
using Shared.Events;
using StackExchange.Redis;
using AssemblyReference = PandesalExpress.Stores.AssemblyReference;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("PandesalExpress.Infrastructure")
    )
);
builder.Services.AddIdentity<Employee, AppRole>(options =>
           {
               options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2);
               options.User.RequireUniqueEmail = true;
           }
       )
       .AddEntityFrameworkStores<AppDbContext>()
       .AddDefaultTokenProviders();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    }
).AddJwtBearer( // keep this scheme for testing API endpoints
    JwtBearerDefaults.AuthenticationScheme,
    options =>
    {
        JwtOptions jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtOptions>()!;

        options.SaveToken = true;
        options.RequireHttpsMetadata = builder.Environment.IsProduction();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue("jwt_token", out string? token)) context.Token = token;

                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                context.HandleResponse();

                // Return 401 with JSON response
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                return context.Response.WriteAsJsonAsync(
                    new
                    {
                        status = 401,
                        message = "You are not authorized to access this resource"
                    }
                );
            },
            OnForbidden = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                string result = JsonSerializer.Serialize(
                    new
                    {
                        status = 403,
                        message = "You do not have permission to access this resource"
                    }
                );

                return context.Response.WriteAsync(result);
            }
        };
    }
).AddJwtBearer(
    "FaceAuthScheme",
    options =>
    {
        JwtOptions jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtOptions>()!;

        options.SaveToken = false;
        options.RequireHttpsMetadata = builder.Environment.IsProduction();

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.FaceIssuer,
            ValidAudience = jwtSettings.FaceAudience,
            RequireSignedTokens = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue("jwt_token", out string? token))
                    context.Token = token;
                if (context.Request.Headers.TryGetValue("Authorization", out StringValues authHeader))
                    context.Token = authHeader.ToString()["Bearer ".Length..].Trim();

                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                FacePublicKeyService keyProvider = context.HttpContext.RequestServices.GetRequiredService<FacePublicKeyService>();
                TokenValidationParameters validationParams = context.Options.TokenValidationParameters;

                validationParams.IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
                {
                    IEnumerable<SecurityKey> keys = keyProvider.GetSigningKeysAsync().GetAwaiter().GetResult();
                    return keys;
                };

                return Task.CompletedTask;
            },
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(
                    new
                    {
                        status = 401,
                        message = "Invalid or missing Face auth token"
                    }
                );
            },
            OnAuthenticationFailed = async context =>
            {
                context.NoResult();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(
                    new
                    {
                        status = 401,
                        message = "Invalid or expired token."
                    }
                );
            }
        };
    }
).AddCookie(
    CookieAuthenticationDefaults.AuthenticationScheme,
    options =>
    {
        options.LoginPath = "/auth/login";
        options.LogoutPath = "/auth/logout";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(24);

        // don't use this scheme for API endpoints
        options.ForwardDefaultSelector = ctx =>
            ctx.Request.Path.StartsWithSegments("/api")
                ? JwtBearerDefaults.AuthenticationScheme
                : CookieAuthenticationDefaults.AuthenticationScheme;
    }
).AddCookie(
    "FaceAuthCookie",
    options =>
    {
        options.LoginPath = "/auth/face-login";
        options.LogoutPath = "/auth/logout";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(24);

        options.ForwardDefaultSelector = ctx =>
            ctx.Request.Path.StartsWithSegments("/api")
                ? JwtBearerDefaults.AuthenticationScheme
                : CookieAuthenticationDefaults.AuthenticationScheme;
    }
);

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthorizationBuilder()
       .SetFallbackPolicy(
           new AuthorizationPolicyBuilder()
               .RequireAuthenticatedUser()
               .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
               .Build()
       )
       .AddPolicy("StoreOperationsOnly", policy => policy.RequireRole("Store Operations"))
       .AddPolicy("StocksAndInventoryOnly", policy => policy.RequireRole("Stocks and Inventory"));

builder.Services.AddControllers()
       .AddJsonOptions(options =>
           {
               options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
               options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
           }
       );

string? redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection");
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString ?? "localhost:6379"));
builder.Services.AddSignalR();

// Modules
builder.Services.AddAuthModule();
builder.Services.AddStoresModule();
builder.Services.AddCommissaryModule();
builder.Services.AddCashierModule();
builder.Services.AddProductsModule();
builder.Services.AddManagementModule();
builder.Services.AddPdndModule();
builder.Services.AddTransfersModule();

// Seeding
builder.Services.AddDatabaseSeeding();
builder.Services.AddSeeder<DepartmentSeeder>();
builder.Services.AddSeeder<RoleSeeder>();
builder.Services.AddSeeder<StoreSeeder>();
builder.Services.AddSeeder<ProductSeeder>();

// Services
builder.Services.AddSingleton<ICacheService, RedisCacheService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();
builder.Services.AddTransient<INotificationService, NotificationService>();
builder.Services.AddMediator();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<FacePublicKeyService>();
builder.Services.AddHostedService<JwksRefreshService>();

// Event Handlers
builder.Services.AddScoped<IEventHandler<PdndRequestEvent>, PdndRequestEventHandler>();
builder.Services.AddScoped<IEventHandler<TransferRequestCreatedEvent>, TransferRequestEventHandler>();
builder.Services.AddScoped<IEventHandler<TransferRequestStatusUpdatedEvent>, TransferRequestEventHandler>();

// Controllers from all assemblies
builder.Services.AddControllers()
       .AddApplicationPart(typeof(AssemblyReference).Assembly)
       .AddApplicationPart(typeof(PandesalExpress.Commissary.AssemblyReference).Assembly)
       .AddApplicationPart(typeof(PandesalExpress.Cashier.AssemblyReference).Assembly)
       .AddApplicationPart(typeof(PandesalExpress.Products.AssemblyReference).Assembly)
       .AddApplicationPart(typeof(PandesalExpress.Auth.AssemblyReference).Assembly)
       .AddApplicationPart(typeof(PandesalExpress.Management.AssemblyReference).Assembly)
       .AddApplicationPart(typeof(PandesalExpress.Transfers.AssemblyReference).Assembly)
       .AddApplicationPart(typeof(PandesalExpress.PDND.AssemblyReference).Assembly);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    {
        var securitySchema = new OpenApiSecurityScheme
        {
            Name = "JWT Authentication",
            Description = "Enter a valid JWT token",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            BearerFormat = "JWT",
            Reference = new OpenApiReference
            {
                Id = JwtBearerDefaults.AuthenticationScheme,
                Type = ReferenceType.SecurityScheme
            }
        };

        var securityRequirement = new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                },
                []
            }
        };

        options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securitySchema);
        options.AddSecurityRequirement(securityRequirement);
    }
);

builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
            {
                policy.SetIsOriginAllowed(origin =>
                          {
                              if (string.IsNullOrWhiteSpace(origin)) return false;

                              // Allow localhost with any port for development
                              if (Uri.TryCreate(origin, UriKind.Absolute, out Uri? uri))
                                  return uri.Host is "localhost" or "127.0.0.1";

                              return false;
                          }
                      )
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            }
        );
    }
);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    await app.Services.SeedDatabaseAsync();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.MapControllers();

// Hubs
app.MapHub<NotificationHub>("/notificationHub");

await app.RunAsync();
