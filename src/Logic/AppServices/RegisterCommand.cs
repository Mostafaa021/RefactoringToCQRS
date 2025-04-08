using System;
using CSharpFunctionalExtensions;
using Logic.AppServices;
using Logic.Utils;

namespace Logic.Students
{
    public sealed class RegisterCommand : ICommand
    {
        public string Name { get;  }
        public string Email { get;  }
        public string Course1 { get;  }
        public string Course1Grade { get;  }
        public string Course2 { get;  }
        public string Course2Grade { get;  }

        public RegisterCommand(string name, string email, string course1, string course1Grade, string course2, string course2Grade)
        {
            Name = name;
            Email = email;
            Course1 = course1;
            Course1Grade = course1Grade;
            Course2 = course2;
            Course2Grade = course2Grade;
        }
        private sealed class RegisterCommandHandler : ICommandHandler<RegisterCommand>
        {
            private readonly UnitOfWork _unitOfWork;

            public RegisterCommandHandler(SessionFactory sessionFactory)
            {
                _unitOfWork = new UnitOfWork(sessionFactory);
            }
            public Result Handle(RegisterCommand command)
            {
                var student = new Student(command.Name, command.Email);
                if (command.Course1 != null && command.Course1Grade != null)
                {
                    Course course = new CourseRepository(_unitOfWork).GetByName(command.Course1);
                    student.Enroll(course, Enum.Parse<Grade>(command.Course1Grade));
                }

                if (command.Course2 != null && command.Course2Grade != null)
                {
                    Course course = new CourseRepository(_unitOfWork).GetByName(command.Course2);
                    student.Enroll(course, Enum.Parse<Grade>(command.Course2Grade));
                }

                new StudentRepository(_unitOfWork).Save(student);
                _unitOfWork.Commit();
                return Result.Ok();
            }
        }

    }
}
