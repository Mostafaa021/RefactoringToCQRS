using System.Collections.Generic;
using Api.Utils;
using FluentNHibernate.Testing.Values;
using Logic.Dtos;
using Logic.Students;
using Logic.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            services.AddMvc();

            services.AddSingleton(new SessionFactory(Configuration["ConnectionString"]));
            services.AddTransient<UnitOfWork>(); // version of this class hasn`t dispose method  so we use transient
            services.AddSingleton<Messages>();
            services.AddTransient<ICommandHandler<EditPersonalInfoCommand>, EditPersonalInfoCommandHandler>();            services.AddTransient<ICommandHandler<EditPersonalInfoCommand>, EditPersonalInfoCommandHandler>();
            services.AddTransient<ICommandHandler<EnrollCommand>, EnrollCommandHandler>();
            services.AddTransient<ICommandHandler<DisEnrollCommand>, DisEnrollCommandHandler>();
            services.AddTransient<ICommandHandler<TransferCommand>, TransferCommandHandler>();
            services.AddTransient<ICommandHandler<RegisterCommand>, RegisterCommandHandler>();
            services.AddTransient<ICommandHandler<UnregisterCommand>, UnregisterCommandHandler>();
            
            services.AddTransient<IQueryHandler<GetListQuery,List<StudentDto>>, GetListQueryHandler>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionHandler>();
            app.UseMvc();
        }
    }
}
