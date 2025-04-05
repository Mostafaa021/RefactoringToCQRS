using System.Windows.Input;
using CSharpFunctionalExtensions;
using Logic.Utils;

namespace Logic.Students
{
    public interface ICommand
    {
      
    }
    public interface ICommandHandler<in TCommand> 
        where TCommand : ICommand
    {
        Result Handle(TCommand command);
    }
   
    public sealed class EditPersonalInfoCommand : ICommand
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
    public sealed class EditPersonalInfoCommandHandler : ICommandHandler<EditPersonalInfoCommand>
    {
        private readonly UnitOfWork _unitOfWork;

        public EditPersonalInfoCommandHandler(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
