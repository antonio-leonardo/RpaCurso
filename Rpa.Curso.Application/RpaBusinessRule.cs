using Rpa.Curso.Application.Alura.Services;
using Rpa.Curso.Application.Dto;
using Rpa.Curso.Application.Interfaces;
using Rpa.Curso.CrossCutting;
using System.Collections.Concurrent;

namespace Rpa.Curso.Application
{
    public class RpaBusinessRule
    {
        public static void InitFlow(ICourseService coursesService, string knowledgePlataformUrl)
        {
            Logging log = LoggingSingleton.GetLogging();
            List<string> coursesLinks = null;
            AluraSearchWebDriver aluraSearchWebDriver = null;
            try
            {
                log.Info("Irá obter a relação de URLs exclusívas de cada curso");
                aluraSearchWebDriver = new(knowledgePlataformUrl);
                coursesLinks = aluraSearchWebDriver.GetCoursesLinks();
            }
            catch (Exception ex)
            {
                log.Error($"Surgiu um erro ao tentar utilizar o Web Driver, vide mensagem: {ex.Message}");
            }
            finally
            {
                aluraSearchWebDriver?.Dispose();
            }

            ConcurrentBag<CourseDto> searchedCourses = new();
            foreach (string courseLink in coursesLinks)
            {
                CourseDto courseDto = null;
                AluraCoursesWebDriver aluraCoursesWebDriver = null;
                try
                {
                    aluraCoursesWebDriver = new(courseLink);
                    courseDto = aluraCoursesWebDriver.WebScrapingData();
                    if (null != courseDto && !string.IsNullOrWhiteSpace(courseDto.CourseTitle) && !string.IsNullOrWhiteSpace(courseDto.InstructorName) && courseDto.Workload > 0 && !string.IsNullOrWhiteSpace(courseDto.Description))
                        searchedCourses.Add(courseDto);
                    else
                        log.Warning($"Houve algum problema ao tentar fazer web-scraping da URL do curso {courseLink}");
                }
                catch (Exception ex)
                {
                    log.Error($"Surgiu um erro ao tentar fazer web-scraping da URL do curso {courseLink}, vide mensagem: {ex.Message}");
                    continue;
                }
                finally
                {
                    aluraCoursesWebDriver?.Dispose();
                }
            }

            while (!searchedCourses.IsEmpty)
            {
                CourseDto searchedCourse = null;
                try
                {
                    searchedCourses.TryTake(out searchedCourse);
                    var persitedCourse = coursesService.Register(searchedCourse);
                    log.Info($"Curso {persitedCourse.Id} persistido com êxito: Título: {persitedCourse.Title}, Professor: {persitedCourse.Instructor.CompleteName}, Carga-horária: {persitedCourse.Workload}h");
                }
                catch
                {
                    log.Warning($"Houve algum problema ao tentar persistir o curso corrente no Banco de Dados, titulo do curso: {searchedCourse?.CourseTitle}");
                }
            }
        }
    }
}