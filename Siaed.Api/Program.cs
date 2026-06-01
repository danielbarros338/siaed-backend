using Microsoft.EntityFrameworkCore;
using Serilog;
using Siaed.Api.DependencyInjection;
using Siaed.Api.Middlewares;
using Siaed.Application.DependencyInjection;
using Siaed.Infra.DependencyInjection;
using Siaed.Infra.Persistence;

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

    if (args.Contains("--migrate-database", StringComparer.OrdinalIgnoreCase))
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        const int maxRetries = 10;
        Exception? lastException = null;

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                dbContext.Database.Migrate();
                Log.Information("Migrations do banco de dados aplicadas com sucesso");
                Log.CloseAndFlush();
                Environment.Exit(0);
                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
                Log.Warning(ex, "Falha ao aplicar migrations (tentativa {Attempt}/{MaxRetries}). Aguardando 5s...", attempt, maxRetries);

                if (attempt < maxRetries)
                    await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }

        Log.Fatal(lastException, "Todas as tentativas de migration falharam. Abortando.");
        Log.CloseAndFlush();
        Environment.Exit(1);
        return;
    }

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
catch (Exception ex) when (ex.GetType().Name != "HostAbortedException")
{
    Log.Fatal(ex, "Aplicação falhou ao inicializar");
}
finally
{
    Log.CloseAndFlush();
}
