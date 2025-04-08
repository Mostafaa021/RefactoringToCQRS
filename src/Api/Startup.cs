using System.Collections.Generic;
using System.Reflection;
using Api.Utils;
using FluentNHibernate.Testing.Values;
using Logic.Decorators;
using Logic.Dtos;
using Logic.Students;
using Logic.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<Config>(Configuration.GetSection("AppConfig"));
            services.AddMvc();

            services.AddSingleton(new SessionFactory(Configuration["ConnectionString"]));
            services.AddTransient<UnitOfWork>(); // version of this class hasn`t dispose method  so we use transient
            services.AddSingleton<Messages>();
            // Register all command and query handlers
            services.AddTransient<ICommandHandler<EditPersonalInfoCommand>>(provider =>
                new DatabaseRetryDecorator<EditPersonalInfoCommand>(//decorator pattern
                    new EditPersonalInfoCommandHandler(
                        provider.GetService<SessionFactory>())
                    , provider.GetService<IOptions<Config>>())
            );
            services.AddTransient<ICommandHandler<EnrollCommand>, EnrollCommandHandler>();
            services.AddTransient<ICommandHandler<DisEnrollCommand>, DisEnrollCommandHandler>();
            services.AddTransient<ICommandHandler<TransferCommand>, TransferCommandHandler>();
            services.AddTransient<ICommandHandler<RegisterCommand>, RegisterCommandHandler>();
            services.AddTransient<ICommandHandler<UnregisterCommand>, UnregisterCommandHandler>();
            services.AddTransient<IQueryHandler<GetListQuery, List<StudentDto>>, GetListQueryHandler>();
            // services.AddHandlersByReflection(Assembly.GetExecutingAssembly()); 
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionHandler>();
            app.UseMvc();
        }
    }
}
