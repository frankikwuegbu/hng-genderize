using Application.Common;
using Microsoft.AspNetCore.Diagnostics;
using hng_genderizeApp.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddDependencyInjection();

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.ContentType = "application/json";

        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var result = exception switch
        {
            ExternalApiException externalApiException => Result.Error(externalApiException.Message, StatusCodes.Status502BadGateway),
            _ => Result.Error("An unexpected server error occurred", StatusCodes.Status500InternalServerError)
        };

        context.Response.StatusCode = result.StatusCode;
        await context.Response.WriteAsJsonAsync(result);
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
