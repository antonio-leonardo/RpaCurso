using Rpa.Curso.Application.Dto;
using Rpa.Curso.Domain.Alura.Value.Object;
using Rpa.Curso.Repository;
using OpenQA.Selenium;
using System.Text;

namespace Rpa.Curso.Application.Alura.Services
{
    internal class AluraCoursesWebDriver : WebDriverRepository
    {
        private const string KNOWLEDGE_PLATFORM = "ALURA";
        internal AluraCoursesWebDriver(string courseUrl) : base(false)
        {
            Log.Info($"Navega na URL de destino: {courseUrl}");
            this.NavigateToUrl(courseUrl);
        }
        internal CourseDto WebScrapingData()
        {
            CourseDto result = new();
            if (this.CheckIfElementIsLoadedWithResult(AluraCoursePageElements.InstructorName))
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)this._driver;
                
                try
                {
                    result.CourseTitle = this.GetElementText(AluraCoursePageElements.CourseTitleFirstPart) + " " + this.GetElementText(AluraCoursePageElements.CourseTitleSecondPart);
                }
                catch (Exception ex)
                {
                    Log.Error($"Surgiu um erro ao tentar capturar a informação sobre o título do curso, vide mensagem: {ex.Message}");
                    return null;
                }
                finally
                {
                    js.ExecuteScript("window.scrollBy(0,500)", "");
                    Thread.Sleep(300);
                }

                int courseHour = 0;
                try
                {
                    string courseHoues = this.GetElementText(AluraCoursePageElements.CourseHours).Replace("h", "", StringComparison.InvariantCultureIgnoreCase);
                    if (!string.IsNullOrWhiteSpace(courseHoues) && !int.TryParse(courseHoues, out courseHour))
                    {
                        Log.Warning("A informação que deveria conter a carga-horária do curso não parece ser numérico");
                    }
                    else
                    {
                        result.Workload = courseHour;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Surgiu um erro ao tentar capturar a carga-horária do curso, vide mensagem: {ex.Message}");

                }
                finally
                {
                    js.ExecuteScript("window.scrollBy(0,500)", "");
                    Thread.Sleep(300);
                }

                try
                {
                    result.InstructorName = this.GetElementText(AluraCoursePageElements.InstructorName);
                }
                catch (Exception ex)
                {
                    Log.Error($"Surgiu um erro ao tentar capturar o nome do instrutor, vide mensagem: {ex.Message}");
                }

                try
                {
                    StringBuilder courseDescription = new();
                    var courseDescriptionParts = this.GetElementsByXPath(AluraCoursePageElements.CourseDescriptionPart);
                    foreach (var courseDescriptionPart in courseDescriptionParts)
                    {
                        courseDescription.AppendLine(courseDescriptionPart.Text);
                    }
                    if(courseDescription.Length != 0)
                    {
                        result.Description = courseDescription.ToString();
                    }
                    else
                    {
                        Log.Warning("Não existe descriição do curso");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Surgiu um erro ao tentar capturar a descrição do curso, vide mensagem: {ex.Message}");
                }
                finally
                {
                    js.ExecuteScript("window.scrollBy(0,500)", "");
                    Thread.Sleep(300);
                }

                if(string.IsNullOrWhiteSpace(result.CourseTitle) && string.IsNullOrWhiteSpace(result.InstructorName) && result.Workload == 0 && string.IsNullOrWhiteSpace(result.Description))
                {
                    Log.Error("Não foi possível coletar os elementos da página; provavelmente houve um redirect ou erro não esperado durante navegação");
                    return null;
                }
                                
                result.KnowledgePlatform = KNOWLEDGE_PLATFORM;
            }
            return result;
        }
    }
}