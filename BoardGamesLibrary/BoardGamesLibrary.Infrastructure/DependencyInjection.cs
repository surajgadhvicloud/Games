using BoardGamesLibrary.Application.Interfaces;
using BoardGamesLibrary.Infrastructure.Configuration;
using BoardGamesLibrary.Infrastructure.Data;
using BoardGamesLibrary.Infrastructure.Services;
using BoardGamesLibrary.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BoardGamesLibrary.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BoardGamesDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.Configure<BusinessRulesOptions>(options =>
            configuration.GetSection(BusinessRulesOptions.SectionName).Bind(options));
        services.Configure<JwtOptions>(options =>
            configuration.GetSection(JwtOptions.SectionName).Bind(options));
        services.Configure<SeederOptions>(options =>
            configuration.GetSection(SeederOptions.SectionName).Bind(options));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IBoardGameService, BoardGameService>();
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IGameIssueService, GameIssueService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        return services;
    }
}