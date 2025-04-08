using CSharpFunctionalExtensions;
using Logic.AppServices;
using Logic.Students;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Logic.Decorators
{
    public class AuditLoggingDecorator<TCommand> : ICommandHandler<TCommand> 
    where TCommand : ICommand
    {
        private readonly ICommandHandler<TCommand> _handler;
        private readonly ILogger _logger;

        public AuditLoggingDecorator(
            ICommandHandler<TCommand> handler 
            ,ILogger logger)
        {
            _handler = handler;
            _logger = logger;
        }
        public Result Handle(TCommand command)
        {
            var result = JsonConvert.SerializeObject(command);
            _logger.LogInformation(result);
            return _handler.Handle(command);
        }
    }
}
