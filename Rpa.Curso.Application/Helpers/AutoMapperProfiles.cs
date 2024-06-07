using Rpa.Curso.Application.Dto;
using Rpa.Curso.Domain.Models;
using AutoMapper;

namespace Rpa.Curso.Application.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<CourseDto, Course>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.CourseTitle))
                .ForMember(dest => dest.Workload, opt => opt.MapFrom(src => src.Workload))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForPath(dest => dest.Instructor.CompleteName, opt => opt.MapFrom(src => src.InstructorName))
                .ForPath(dest => dest.KnowledgePlatform.Name, opt => opt.MapFrom(src => src.KnowledgePlatform));
        }
    }
}