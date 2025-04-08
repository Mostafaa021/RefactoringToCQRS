using System;
using CSharpFunctionalExtensions;
using Logic.AppServices;
using Logic.Utils;

namespace Logic.Students
{
    public sealed class EnrollCommand : ICommand
    {
        public long Id { get;  }
        public string Course { get;  }
        public string Grade { get;  }

        public EnrollCommand(long id, string course, string grade)
        {
            Id = id;
            Course = course;
            Grade = grade;
        }
        private sealed class EnrollCommandHandler : ICommandHandler<EnrollCommand>
        {
            private readonly UnitOfWork _unitOfWork;

            public EnrollCommandHandler(SessionFactory sessionFactory)
            {
                _unitOfWork = new UnitOfWork(sessionFactory);
            }
            public Result Handle(EnrollCommand command)
            {
                Student student = new StudentRepository(_unitOfWork).GetById(command.Id);
                if (student == null)
                    return Result.Fail($"No student found for Id {command.Id}");

                Course course = new CourseRepository(_unitOfWork).GetByName(command.Course);
                if (course == null)
                    return Result.Fail($"No course found for name {command.Course}");

                bool sucess = Enum.TryParse<Grade>(command.Course , out Grade grade); 
                if(!sucess)
                    return Result.Fail($"Invalid grade {command.Course}");
            
                student.Enroll(course, grade);
            
                _unitOfWork.Commit();
                return Result.Ok();
            }
        }
    }
}
