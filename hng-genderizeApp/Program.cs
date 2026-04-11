using hng_genderizeApp.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddOpenApi();

//genderize service
builder.Services.AddHttpClient<GenderizeServices>(client =>
{
    client.BaseAddress = new Uri("https://api.genderize.io/");
});

//builder.Services.AddScoped<GenderizeServices>

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
