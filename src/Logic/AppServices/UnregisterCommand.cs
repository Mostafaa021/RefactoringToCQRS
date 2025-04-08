using CSharpFunctionalExtensions;
using Logic.AppServices;
using Logic.Utils;

namespace Logic.Students
{
    public sealed class UnregisterCommand : ICommand
    {
        public long Id { get;  }

        public UnregisterCommand(long id)
        {
            Id = id;
        }
        private sealed class UnregisterCommandHandler : ICommandHandler<UnregisterCommand>
        {
            private readonly UnitOfWork _unitOfWork;

            public UnregisterCommandHandler(SessionFactory sessionFactory)
            {
                _unitOfWork = new UnitOfWork(sessionFactory);
            }
            public Result Handle(UnregisterCommand command)
            {
                var student = new StudentRepository(_unitOfWork).GetById(command.Id);
                if (student == null)
                    return Result.Fail($"No student found for Id {command.Id}");

                new StudentRepository(_unitOfWork).Delete(student);
                _unitOfWork.Commit();
                return Result.Ok();
            }
        }

    }
}
