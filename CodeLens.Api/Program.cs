using CodeLens.Api.Extensions;
using CodeLens.Api.Middleware;
using CodeLens.Application;
using CodeLens.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ?? Layer registrations ???????????????????????????????????????????????????????
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

// ?? Middleware pipeline ???????????????????????????????????????????????????????
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CodeLens API v1");
        c.RoutePrefix = string.Empty;
        c.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowBlazorOrigin");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
})).AllowAnonymous();

app.Run();

// Expose Program for integration test WebApplicationFactory
public partial class Program { }
