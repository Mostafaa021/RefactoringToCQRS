using CSharpFunctionalExtensions;
using Logic.AppServices;
using Logic.Decorators;
using Logic.Utils;

namespace Logic.Students
{
    public sealed class EditPersonalInfoCommand : ICommand
    {
        public long Id { get;  }
        public string Name { get;  }
        public string Email { get;  }

        public EditPersonalInfoCommand(string email, string name, long id)
        {
            Email = email;
            Name = name;
            Id = id;
        }
        [AuditLog] // this attribute is used to log the command firstly
        [DatabaseRetry] //  retry the command in case of database failure secondly 
        private sealed class EditPersonalInfoCommandHandler : ICommandHandler<EditPersonalInfoCommand>
        {
            private readonly UnitOfWork _unitOfWork;

            public EditPersonalInfoCommandHandler(SessionFactory sessionFactory)
            {
                _unitOfWork = new UnitOfWork(sessionFactory);
            }
            public Result Handle(EditPersonalInfoCommand command)
            {
                var studentRepository = new StudentRepository(_unitOfWork);
                Student student = studentRepository.GetById(command.Id);
                if (student == null)
                    return Result.Fail($"No student found for Id {command.Id}");
            
                student.Name = command.Name;
                student.Email = command.Email;
            
                _unitOfWork.Commit();
                return Result.Ok();
            }
        }

    }
}

