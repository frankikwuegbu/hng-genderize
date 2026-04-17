using Application.Common;
using hng_genderizeApp.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddDependencyInjection();

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(Result.Error("An unexpected server error occurred", StatusCodes.Status500InternalServerError));
    });
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
