using Rpa.Curso.Domain.Models;

namespace Rpa.Curso.Domain.Interfaces.Repositories
{
    public interface IInstructorRepository
    {
        Instructor Get(string completeName);
        void Register(Instructor instructor);
    }
}