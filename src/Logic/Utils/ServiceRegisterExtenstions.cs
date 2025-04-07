using System.Reflection;
using Logic.Students;
using Microsoft.Extensions.DependencyInjection;

namespace Logic.Utils
{
    public static class ServiceRegistrationExtensions
    {
        public static void AddHandlersByReflection(this IServiceCollection services, Assembly assembly)
        {
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                // Must be a concrete, non-abstract, public class
                if (!type.IsClass || type.IsAbstract || !type.IsPublic)
                    continue;

                var interfaces = type.GetInterfaces();

                foreach (var @interface in interfaces)
                {
                    // Only register handlers that match the desired generic interfaces
                    if (@interface.IsGenericType)
                    {
                        var genericDef = @interface.GetGenericTypeDefinition();

                        if (genericDef == typeof(ICommandHandler<>)
                            || genericDef == typeof(IQueryHandler<,>))
                        {
                            services.AddTransient(@interface, type);
                        }
                    }
                }
            }
        }
    }
}
