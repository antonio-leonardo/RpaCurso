using Rpa.Curso.Application.Dto;
using Rpa.Curso.Domain.Models;

namespace Rpa.Curso.Application.Interfaces
{
    public interface ICourseService
    {
        Course Register(CourseDto courseDto);
    }
}