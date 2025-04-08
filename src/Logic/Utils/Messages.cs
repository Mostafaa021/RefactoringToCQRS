using System;
using CSharpFunctionalExtensions;
using Logic.AppServices;
using Logic.Students;

namespace Logic.Utils
{
    public sealed class Messages // class that holds all messages
    {
        private readonly IServiceProvider _serviceProvider;

        public Messages(IServiceProvider serviceProvider)
        {
             _serviceProvider = serviceProvider; 
        }
        
        
        // Dispatch method is used to dispatch commands to the appropriate command handler
        public Result Dispatch(ICommand command)
        {
            // we have the provider which already know how to rsolve the command handler to particular ICommandHandler interface
            Type type = typeof(ICommandHandler<>); // get type of Handler => ICommandHandler<>
            Type [] commandType = {command.GetType()}; // get type of command => EditPersonalInfoCommand
            Type handlerType = type.MakeGenericType(commandType); //   ICommandHandler<EditPersonalInfoCommand>
            
            // we use dynamic to avoid casting   //we can use reflection to invoke the method Handle
            dynamic handler = _serviceProvider.GetService(handlerType);
            Result result =  handler.Handle((dynamic)command);
            
            
            if (handler == null)
                throw new InvalidOperationException($"No handler found for command type {commandType}");

            //Result result = (Result)handlerType.GetMethod("Handle").Invoke(handler, new object[] { command });

            return result;

        }
        public T Dispatch<T>(IQuery<T> query)
        {
            // we have the provider which already know how to rsolve the command handler to particular ICommandHandler interface
            Type type = typeof(IQueryHandler<,>); // get type of Handler => IQueryHandler<in, out>
            Type [] queryType = {query.GetType() , typeof(T)}; // get type of Query => GetlistQuery and return type => List<StudentDto>
            Type handlerType = type.MakeGenericType(queryType); //   IQueryHandler<GetlistQuery , List<StudentDto>>
            
            // we use dynamic to avoid casting   //we can use reflection to invoke the method Handle
            dynamic handler = _serviceProvider.GetService(handlerType);
            T result =  handler.Handle((dynamic)query);
            
            if (handler == null)
                throw new InvalidOperationException($"No handler found for Query type {queryType}");
            
            return result;

        }
    }
}
