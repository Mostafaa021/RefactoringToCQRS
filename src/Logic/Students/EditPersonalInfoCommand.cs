using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using CSharpFunctionalExtensions;
using FluentNHibernate.Testing.Values;
using Logic.Dtos;
using Logic.Utils;

namespace Logic.Students
{
    public interface ICommand
    {
      
    }
    public interface IQuery<TResult> // we used parametrized interface to match dispatch method where return type of T 
    {
      
    }
    public interface ICommandHandler<in TCommand> 
        where TCommand : ICommand
    {
        Result Handle(TCommand command);
    }
    public interface IQueryHandler<in TQuery , out TResult>
        where TQuery : IQuery<TResult>
        where TResult : class
    {
        TResult Handle(TQuery query);
    }

    #region Query

    public sealed class GetListQuery : IQuery<List<StudentDto>>
    {
        public string EnrolledIn { get;  }
        public int? NumberOfCourses { get;  }

        public GetListQuery(string enrolledIn, int? numberOfCourses)
        {
            EnrolledIn = enrolledIn;
            NumberOfCourses = numberOfCourses;
        }
    }
    public sealed class GetListQueryHandler : IQueryHandler<GetListQuery,List<StudentDto>>
    {
        private readonly UnitOfWork _unitOfWork;

        public GetListQueryHandler(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public List<StudentDto> Handle(GetListQuery query)
        {
            return new StudentRepository(_unitOfWork)
                .GetList(query.EnrolledIn, query.NumberOfCourses)
                .Select(student => ConvertToDto(student)).ToList();
        }
        private StudentDto ConvertToDto(Student student)
        {
            return new StudentDto
            {
                Id = student.Id,
                Name = student.Name,
                Email = student.Email,
                Course1 = student.FirstEnrollment?.Course?.Name,
                Course1Grade = student.FirstEnrollment?.Grade.ToString(),
                Course1Credits = student.FirstEnrollment?.Course?.Credits,
                Course2 = student.SecondEnrollment?.Course?.Name,
                Course2Grade = student.SecondEnrollment?.Grade.ToString(),
                Course2Credits = student.SecondEnrollment?.Course?.Credits,
            };
        }
    }

    #endregion
    #region Command

    #region EditPersonalInfoCommand

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
    }
    public sealed class EditPersonalInfoCommandHandler : ICommandHandler<EditPersonalInfoCommand>
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

    #endregion
    
    #region EnrollCommand
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
    }
    public sealed class EnrollCommandHandler : ICommandHandler<EnrollCommand>
    {
        private readonly UnitOfWork _unitOfWork;

        public EnrollCommandHandler(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
    #endregion
    
    #region DisEnrollCommand
    public sealed class DisEnrollCommand : ICommand
    {
        public long Id { get;  }
        public int EnrollmentNumber { get;  }
        public string Comment { get;  }

        public DisEnrollCommand(long id, int enrollmentNumber, string comment)
        {
            Id = id;
            EnrollmentNumber = enrollmentNumber;
            Comment = comment;
        }
    }
    public sealed class DisEnrollCommandHandler : ICommandHandler<DisEnrollCommand>
    {
        private readonly UnitOfWork _unitOfWork;

        public DisEnrollCommandHandler(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
    #endregion
    
    #region TransferCommand
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
    }
    public sealed class TransferCommandHandler : ICommandHandler<TransferCommand>
    {
        private readonly UnitOfWork _unitOfWork;

        public TransferCommandHandler(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
    #endregion
    
    #region RegisterCommand
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
    }
    public sealed class RegisterCommandHandler : ICommandHandler<RegisterCommand>
    {
        private readonly UnitOfWork _unitOfWork;

        public RegisterCommandHandler(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
    #endregion
    
    #region UnregisterCommand
    
    public sealed class UnregisterCommand : ICommand
    {
        public long Id { get;  }

        public UnregisterCommand(long id)
        {
            Id = id;
        }
    }
    public sealed class UnregisterCommandHandler : ICommandHandler<UnregisterCommand>
    {
        private readonly UnitOfWork _unitOfWork;

        public UnregisterCommandHandler(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
    #endregion
    #endregion
}

