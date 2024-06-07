using Rpa.Curso.CrossCutting;
using Rpa.Curso.Domain.Interfaces.Repositories;
using Rpa.Curso.Domain.Models;
using Rpa.Curso.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Rpa.Curso.Infrastructure.Repositories
{
    public class CourseRepository : LoggingBase, ICourseRepository
    {
        private readonly DataContext _context;
        public CourseRepository(DataContext context)
        {
            _context = context;
        }

        public Course Get(string courseTitle, string knowledgePlataformName, string instructorName)
        {
            return _context.Courses
                .Include(c => c.KnowledgePlatform)
                .Include(c => c.Instructor)
                .FirstOrDefault(c => c.Instructor.CompleteName.Replace(" ", "").ToLower() == instructorName.Replace(" ", "").ToLower()
                    && c.KnowledgePlatform.Name.Replace(" ", "").ToLower() == knowledgePlataformName.Replace(" ", "").ToLower()
                    && c.Title.Replace(" ", "").ToLower() == courseTitle.Replace(" ", "").ToLower());
        }

        public void Register(Course course)
        {
            Log.Info($"Persistencia de curso, Id: {course.Id}");
            _context.Courses.Add(course);
        }
    }
}