using Rpa.Curso.Application.Dto;
using Rpa.Curso.Application.Interfaces;
using Rpa.Curso.CrossCutting;
using Rpa.Curso.Domain.Interfaces.Repositories;
using Rpa.Curso.Domain.Models;
using AutoMapper;
using System.Text;

namespace Rpa.Curso.Application.Services
{
    public class CourseService : LoggingBase, ICourseService
    {
        private readonly IKnowledgePlatformRepository _knowledgePlatformRepository;
        private readonly IInstructorRepository _instructorRepository;
        private readonly ICourseRepository _cursoRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        public CourseService(IKnowledgePlatformRepository KnowledgePlatformRepository, IInstructorRepository InstructorRepository, ICourseRepository cursoRepository, IUnitOfWork uow, IMapper mapper)
        {
            _knowledgePlatformRepository = KnowledgePlatformRepository;
            _instructorRepository = InstructorRepository;
            _cursoRepository = cursoRepository;
            _uow = uow;
            _mapper = mapper;
        }
        public Course Register(CourseDto courseDto)
        {
            if(null == courseDto || (string.IsNullOrWhiteSpace(courseDto.CourseTitle) && string.IsNullOrWhiteSpace(courseDto.InstructorName) && string.IsNullOrWhiteSpace(courseDto.Description) && courseDto.Workload == 0))
            {
                string msg = "Não é posssível fazer persistencia de valores vazios";
                Log.Error(msg);
                throw new InvalidOperationException(msg);
            }

            StringBuilder notes = new StringBuilder();
            if(null != _cursoRepository.Get(courseDto.CourseTitle, courseDto.KnowledgePlatform, courseDto.InstructorName))
            {
                string msg = "O curso já está cadastrado na base de dados";
                Log.Error(msg);
                throw new InvalidOperationException(msg);
            }

            Course course = this._mapper.Map<Course>(courseDto);
            if (course.KnowledgePlatform == null)
            {
                string msg = "É necessário preencher a plataforma de ensino";
                Log.Error(msg);
                throw new InvalidOperationException(msg);
            }

            if (course.Instructor == null)
            {
                string msg = "É necessário ter um professor associado ao curso";
                Log.Error(msg);
                throw new InvalidOperationException(msg);
            }
                
            Log.Info("Verifica se já existe plataforma de ensino cadastrada");
            KnowledgePlatform KnowledgePlatform = _knowledgePlatformRepository.Get(course.KnowledgePlatform.Name);
            if(KnowledgePlatform == null)
            {
                Log.Info($"Não existe plataforma de ensino com Name {course.KnowledgePlatform.Name} cadastrada, será registrada uma nova");
                course.KnowledgePlatform.Id = Guid.NewGuid();
                course.KnowledgePlatform.CreatedDate = DateTime.Now;
                _knowledgePlatformRepository.Register(course.KnowledgePlatform);
                course.KnowledgePlatformId = course.KnowledgePlatform.Id;
            }
            else
            {
                Log.Info($"Já existe plataforma de ensino cadastrada com o Name dado: {course.KnowledgePlatform.Name}");
                course.KnowledgePlatformId = KnowledgePlatform.Id;
            }

            Log.Info("Verifica se já existe professor(a) cadastrado(a)");
            if (string.IsNullOrWhiteSpace(course.Instructor.CompleteName))
            {
                notes.Append("Ainda não existe professor para este curso.");
            }
            else if(_instructorRepository.Get(course.Instructor.CompleteName) == null)
            {
                Log.Info($"Não existe professor(a) com nome {course.Instructor.CompleteName} cadastrado(a), será registrado(a) um(a) novo(a)");
                course.Instructor.Id = Guid.NewGuid();
                course.Instructor.CreatedDate = DateTime.Now;
                _instructorRepository.Register(course.Instructor);
                course.InstructorId = course.Instructor.Id;
            }
            else
            {
                Log.Info($"Já existe Instructor(a) cadastrado(a) com o Name dado: {course.KnowledgePlatform.Name}");
                course.InstructorId = _instructorRepository.Get(course.Instructor.CompleteName).Id;
            }

            if (string.IsNullOrWhiteSpace(course.Description))
            {
                notes.Append("Ainda não existe descrição definida para este curso.");
            }

            if(course.Workload == null)
            {
                notes.Append("Ainda não existe carga-horária definida para este curso.");
            }

            if(notes.Length != 0)
            {
                course.Notes = notes.ToString();
            }

            Log.Info("Geração de Id e Data de Criação");
            course.Id = Guid.NewGuid();
            course.CreatedDate = DateTime.Now;
            _cursoRepository.Register(course);
            _uow.SaveAllAsync().GetAwaiter().GetResult();
            return course;
        }
    }
}