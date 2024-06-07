using Rpa.Curso.Application.Interfaces;
using Rpa.Curso.Application.Services;
using Rpa.Curso.CrossCutting;
using Rpa.Curso.Domain.Interfaces.Repositories;
using Rpa.Curso.Infrastructure.Context;
using Rpa.Curso.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Rpa.Curso.IoC.ServiceCollectionExtensions
{
    public static class DependecyInjection
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration config, Logging log)
        {
            log.Info("Captura de connection string");
            var connectionString = config.GetConnectionString("Cursos");

            log.Info("Injeção de dependencia de Repositórios");
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IKnowledgePlatformRepository, KnowledgePlatformRepository>();
            services.AddScoped<IInstructorRepository, InstructorRepository>();
            services.AddScoped<ICourseRepository, CourseRepository>();
            

            log.Info("Injeção de dependencia de Serviços");
            services.AddScoped<ICourseService, CourseService>();
            
            log.Info("Adição de contexto de Base de Dados SQL Server");
            services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            log.Info("Retorna todos os serviços configurados");
            return services;
        }
    }
}
