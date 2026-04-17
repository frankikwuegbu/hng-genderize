using System.Text.Json;
using Application.Common;
using Application.Common.Interfaces;
using Application.Profiles.Commands;
using Infrastructure;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hng_genderizeApp.Extensions;

public static class DependencyInjection
{
    public static void AddDependencyInjection(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        builder.Services.AddControllers();
        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var hasTypeConversionError = context.ModelState.Values
                    .SelectMany(value => value.Errors)
                    .Any(error =>
                        error.Exception is JsonException ||
                        error.ErrorMessage.Contains("could not be converted", StringComparison.OrdinalIgnoreCase));

                if (hasTypeConversionError)
                {
                    return new UnprocessableEntityObjectResult(Result.Error("Invalid type", StatusCodes.Status422UnprocessableEntity));
                }

                return new BadRequestObjectResult(Result.Error("Missing or empty name", StatusCodes.Status400BadRequest));
            };
        });

        builder.Services.AddAuthorization();
        builder.Services.AddOpenApi();
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                sqlServerOptions => sqlServerOptions.EnableRetryOnFailure());
        });
        builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        builder.Services.AddScoped<IProfileServices, ProfileServices>();
        builder.Services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssemblyContaining<CreateProfileCommand>());

        builder.Services.AddHttpClient("genderize", client =>
        {
            client.BaseAddress = new Uri("https://api.genderize.io/");
        });

        builder.Services.AddHttpClient("nationalize", client =>
        {
            client.BaseAddress = new Uri("https://api.nationalize.io/");
        });

        builder.Services.AddHttpClient("agify", client =>
        {
            client.BaseAddress = new Uri("https://api.agify.io/");
        });
    }
}
