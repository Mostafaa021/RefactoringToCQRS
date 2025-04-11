using CSharpFunctionalExtensions;
using Logic.AppServices;
using Logic.Utils;

namespace Logic.Students
{
    public sealed class DisEnrollCommand : ICommand
    {
        private  long Id { get; }
        private  int EnrollmentNumber { get;  }
        private string Comment { get;  }
        public DisEnrollCommand(long id, int enrollmentNumber, string comment)
        {
            Id = id;
            EnrollmentNumber = enrollmentNumber;
            Comment = comment;
        }
        private sealed class DisEnrollCommandHandler : ICommandHandler<DisEnrollCommand>
        {
            private readonly UnitOfWork _unitOfWork;

            public DisEnrollCommandHandler(SessionFactory sessionFactory)
            {
                _unitOfWork = new UnitOfWork(sessionFactory);
            }
            public Result Handle(DisEnrollCommand command)
            {
                var student = new StudentRepository(_unitOfWork).GetById(command.Id);
                if (student == null)
                    return Result.Fail($"No student found for Id {command.Id}");

                if (string.IsNullOrWhiteSpace(command.Comment))
                    return Result.Fail("DisEnrollment comment is required");

                var enrollment = student.GetEnrollment(command.EnrollmentNumber);
                if(enrollment == null)
                    return Result.Fail($"No enrollment found for number {command.EnrollmentNumber}");

                student.RemoveEnrollment(enrollment, command.Comment);
                _unitOfWork.Commit();
                return Result.Ok();
            }
        }

    }
}
