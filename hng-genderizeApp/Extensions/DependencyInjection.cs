using Application.Common.Interfaces;
using Application.Profiles.Commands;
using Infrastructure;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace hng_genderizeApp.Extensions;

public static class DependencyInjection
{
    public static void AddDependencyInjection(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddAuthorization();
        builder.Services.AddOpenApi();
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
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
