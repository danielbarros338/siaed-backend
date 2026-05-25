using Serilog;
using Siaed.Api.DependencyInjection;
using Siaed.Api.Middlewares;
using Siaed.Application.DependencyInjection;
using Siaed.Infra.DependencyInjection;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/siaed-.log", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services
        .AddApiServices(builder.Configuration)
        .AddApplication()
        .AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "SIAED API v1");
            c.RoutePrefix = "swagger";
            c.DocumentTitle = "SIAED API — Documentação";
        });
    }

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseCors("AllowLocalhost");
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicação falhou ao inicializar");
}
finally
{
    Log.CloseAndFlush();
}
