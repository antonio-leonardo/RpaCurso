using Rpa.Curso.Domain.Models;

namespace Rpa.Curso.Domain.Interfaces.Repositories
{
    public interface ICourseRepository
    {
        Course Get(string courseTitle, string knowledgePlataformName, string instructorName);
        void Register(Course course);
    }
}