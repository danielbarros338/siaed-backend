using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Siaed.Application.Interfaces;
using Siaed.Infra.Identity;
using Siaed.Infra.OpenAI;
using Siaed.Infra.Persistence;
using Siaed.Infra.Repositories;

namespace Siaed.Infra.DependencyInjection;

public static class InfraServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não configurada.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0))));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ILessonPlanRepository, LessonPlanRepository>();
        services.AddScoped<IActivityRepository, ActivityRepository>();
        services.AddScoped<IPedagogicalReportRepository, PedagogicalReportRepository>();
        services.AddScoped<IAIRequestRepository, AIRequestRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<ISchoolClassRepository, SchoolClassRepository>();
        services.AddScoped<IGradeRepository, GradeRepository>();

        services.AddScoped<IOpenAIService, OpenAIService>();
        services.AddScoped<IPromptBuilder, PromptBuilderService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddSingleton<IPasswordHasher, PasswordHasherService>();

        return services;
    }
}
