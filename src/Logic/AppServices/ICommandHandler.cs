using CSharpFunctionalExtensions;
using Logic.AppServices;

namespace Logic.Students
{
    public interface ICommandHandler<in TCommand> 
        where TCommand : ICommand
    {
        Result Handle(TCommand command);
    }
}
