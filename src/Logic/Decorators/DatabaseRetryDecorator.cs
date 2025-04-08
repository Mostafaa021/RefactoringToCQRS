using System;
using CSharpFunctionalExtensions;
using Logic.Students;
using Logic.Utils;
using Microsoft.Extensions.Options;

namespace Logic.Decorators
{
    public sealed class DatabaseRetryDecorator <TCommand> : ICommandHandler<TCommand>
    where TCommand : ICommand
    {
        //  we use decorator pattern to add additional behaviour without need to change the original code
        // reference from handler this decorator decorate 
        private readonly ICommandHandler<TCommand> _handler;
        private readonly Config _config;

        public DatabaseRetryDecorator(ICommandHandler<TCommand> handler 
            , IOptions<Config> options)
        {
            _handler = handler;
            _config = options.Value;
        }
        public Result Handle(TCommand command)
        {
            for (int i = 0; i < _config.NumberOfRetries; i++)
            {
                try
                {
                  var result =   _handler.Handle(command);
                  return result; 
                }
                catch (Exception e)
                {
                    if (i >= _config.NumberOfRetries || IsDataBaseExcptionOccur(e))
                        throw e;
                }
            } 
            throw new InvalidProgramException("Should never reach here");
        }

        private bool IsDataBaseExcptionOccur(Exception ex)
        {
            var message =  ex.InnerException.Message;
            if (message == null)
                return false;

            return message.Contains("The Connection is broken and recovery is not possible") 
                || message.Contains("error Occur while Establishing Connection");
            

        }
    }
}
