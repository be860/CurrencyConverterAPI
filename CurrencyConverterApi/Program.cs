using CurrencyConverterApi.Data;
using CurrencyConverterApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure EF Core ? SQL Server
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// 2. Core Web API services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 3. Swagger / OpenAPI with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CurrencyConverterApi",
        Version = "v1"
    });

    // Define “Bearer” scheme, but only ask for the token itself
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description =
            "Paste your JWT token **only** (without the \"Bearer \" prefix).\n" +
            "Swagger will add the prefix automatically.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Require Bearer for all endpoints marked [Authorize]
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 4. Currency conversion service (calls external API)
builder.Services.AddHttpClient<ICurrencyConverterService, CurrencyConverterService>();

// 5. Email & OTP services
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services
  .AddHttpClient<ICurrencyConverterService, CurrencyConverterService>(client =>
  {
      client.BaseAddress = new Uri(builder.Configuration["ExchangeRates:BaseUrl"]);
      client.DefaultRequestHeaders.Add("apikey",
          builder.Configuration["ExchangeRates:ApiKey"]);
  });


// 6. JWT Authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
var keyBytes = Encoding.UTF8.GetBytes(jwtSection["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opts =>
{
    opts.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
    };
});

var app = builder.Build();

// 7. HTTP request pipeline

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CurrencyConverterApi v1");
        c.EnablePersistAuthorization(); // keep token in UI between calls
    });
}

app.UseHttpsRedirection();

// Order is important: Authentication first, then Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
