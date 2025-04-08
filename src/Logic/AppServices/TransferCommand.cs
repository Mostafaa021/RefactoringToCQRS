using System;
using CSharpFunctionalExtensions;
using Logic.Students;
using Logic.Utils;

namespace Logic.AppServices
{
    public sealed class TransferCommand : ICommand
    {
        public long Id { get;  }
        public int EnrollmentNumber { get;  }
        public string Course { get;  }
        public string Grade { get;  }

        public TransferCommand(long id, int enrollmentNumber, string course, string grade)
        {
            Id = id;
            EnrollmentNumber = enrollmentNumber;
            Course = course;
            Grade = grade;
        }
        public sealed class TransferCommandHandler : ICommandHandler<TransferCommand>
        {
            private readonly UnitOfWork _unitOfWork;

            public TransferCommandHandler(SessionFactory sessionFactory)
            {
                _unitOfWork = new UnitOfWork(sessionFactory);
            }
            public Result Handle(TransferCommand command)
            {
                var student = new StudentRepository(_unitOfWork).GetById(command.Id);
                if (student == null)
                    return Result.Fail($"No student found for Id {command.Id}");

                Course course = new CourseRepository(_unitOfWork).GetByName(command.Course);
                if (course == null)
                    return Result.Fail($"No course found for name {command.Course}");

                bool sucess = Enum.TryParse(command.Grade , out Grade grade); 
                if(!sucess)
                    return Result.Fail($"Invalid grade {command.Grade}");
            
                var enrollment = student.GetEnrollment(command.EnrollmentNumber);
                if(enrollment == null)
                    return Result.Fail($"No enrollment found for number {command.EnrollmentNumber}");

                enrollment.Update(course, grade);
                _unitOfWork.Commit();
                return Result.Ok();
            }
        }
    }
}
