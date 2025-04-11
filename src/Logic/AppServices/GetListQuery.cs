using System.Collections.Generic;
using System.Linq;
using Logic.Dtos;
using Logic.Utils;

namespace Logic.Students
{
    public sealed class GetListQuery : IQuery<List<StudentDto>>
    {
        public string EnrolledIn { get;  }
        public int? NumberOfCourses { get;  }

        public GetListQuery(string enrolledIn, int? numberOfCourses)
        {
            EnrolledIn = enrolledIn;
            NumberOfCourses = numberOfCourses;
        }
        private sealed class GetListQueryHandler : IQueryHandler<GetListQuery,List<StudentDto>>
        {
            private readonly UnitOfWork _unitOfWork;

            public GetListQueryHandler(SessionFactory sessionFactory)
            {
                _unitOfWork = new UnitOfWork(sessionFactory);
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

    }
}
