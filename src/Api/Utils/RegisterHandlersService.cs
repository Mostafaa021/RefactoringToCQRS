using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Logic.AppServices;
using Logic.Decorators;
using Logic.Students;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Utils
{
    public static class RegisterHandlersService
    {
        public static void AddHandlers(this IServiceCollection services)
        {
            var handlerTypes = typeof(ICommand).Assembly.GetTypes()
                .Where(x => x.GetInterfaces().Any(y =>IsHandlerInterface(y)))
                .Where(x => x.Name.EndsWith("Handler"))
                .ToList();
            foreach (var type in handlerTypes)
            {
                AddHandler(services, type);
            }
        }

        private static void AddHandler(IServiceCollection services, Type type)
        {
            object [] attributes = type.GetCustomAttributes(false);
            
            List<Type> pipeline =attributes
                .Select(x => ToDecorator(x))
                .Concat(new[] { type })
                .Reverse()
                .ToList();
            
            Type interfaceType =  type.GetInterfaces().Single(y=> IsHandlerInterface(y));
            Func<IServiceProvider , object> factory = BuildPipeLine(pipeline , interfaceType);
            services.AddTransient(type, factory);
        }

        private static Func<IServiceProvider, object> BuildPipeLine(List<Type> pipeline, Type interfaceType)
        {
            List<ConstructorInfo> ctors = pipeline
                .Select(x =>
                {
                    Type type = x.IsGenericType
                        ? x.MakeGenericType(interfaceType.GenericTypeArguments)
                        : x;
                    return type.GetConstructors().FirstOrDefault();
                })
                .ToList();
            Func<IServiceProvider, object> func = provider =>
            {
                object result = null;
                foreach (var ctor in ctors)
                {
                    var parametersInfo = ctor.GetParameters().ToList();
                    object [] parameters = GetParameters(parametersInfo , result , provider ); 
                    result = ctor.Invoke(parameters);
                }
                return result;
            };
            return func;
        }

        private static object[] GetParameters(List<ParameterInfo> parametersInfo, object result, IServiceProvider provider)
        {
           var current = new object[parametersInfo.Count];
            for (int i = 0; i < parametersInfo.Count; i++)
            {
                current[i] = GetParameter(parametersInfo[i] ,result , provider );
            }
            return current;
        }
        private static object GetParameter(ParameterInfo parametersInfo, object result, IServiceProvider provider)
        {
            Type parameterType = parametersInfo.ParameterType;
            if (IsHandlerInterface(parameterType))
                return result;
            object service =  provider.GetService(parameterType);
            if(service is not null)
                return service;

            throw new ArgumentException($"Type {parameterType} not found");

        }

        private static Type ToDecorator(object attribute)
        {
            var type = attribute.GetType();
            
            if(type == typeof(DatabaseRetryAttribute))
                return typeof(DatabaseRetryDecorator<>);
            if (type == typeof(AuditLogAttribute))
                return typeof(AuditLoggingDecorator<>);
            
            // any additional custom attributes must be added here 
            throw new NotImplementedException(attribute.ToString());

        }

        private static bool IsHandlerInterface(Type type)
        {
            if(!type.IsGenericType)
                return false; 
            Type typeDefination = type.GetGenericTypeDefinition();
            return typeDefination == typeof(ICommandHandler<>)
                || typeDefination == typeof(IQueryHandler<,>);
        }
    }
}
