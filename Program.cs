using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using SimpleCheckout.API;
using SimpleCheckout.Application;
using SimpleCheckout.Application.Rules;
using SimpleCheckout.Application.Validators;
using SimpleCheckout.Domain;
using SimpleCheckout.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            []
        }
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secret = builder.Configuration["Jwt:Secret"]!;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddProblemDetails(options =>
    options.CustomizeProblemDetails = ctx =>
    {
        if (ctx.Exception is BadHttpRequestException ex)
        {
            ctx.ProblemDetails.Status = ex.StatusCode;
            ctx.ProblemDetails.Detail = ex.Message;
            ctx.HttpContext.Response.StatusCode = ex.StatusCode;
        }
    });

builder.Services.AddDbContext<SimpleCheckoutDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ICheckoutRepository, EfCheckoutRepository>();
builder.Services.AddScoped<IPricingRuleRepository, EfPricingRuleRepository>();
builder.Services.AddScoped<CheckoutService>();
builder.Services.AddScoped<TokenService>();

builder.Services.AddScoped<IDiscountCalculator, PercentageDiscountCalculator>();
builder.Services.AddScoped<ITaxCalculator, PercentageTaxCalculator>();

builder.Services.AddScoped<IValidator<CheckoutRequest>, CheckoutRequestValidator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapCheckoutEndpoints();

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<SimpleCheckoutDbContext>().Database.EnsureCreated();

app.Run();
