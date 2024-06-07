using Rpa.Curso.Domain.Alura.Value.Object;
using Rpa.Curso.Repository;

namespace Rpa.Curso.Application.Alura.Services
{
    internal class AluraSearchWebDriver : WebDriverRepository
    {
        internal AluraSearchWebDriver(string knowledgePlataformUrl) : base(true)
        {
            Log.Info($"Navega na URL de destino: {knowledgePlataformUrl}");
            this.NavigateToUrl(knowledgePlataformUrl);
        }

        internal List<string> GetCoursesLinks()
        {
            List<string> coursesLinks = null;
            if (this.CheckIfElementIsLoadedWithResult(AluraSearchPageElements.SearchInput, 300))
            {
                coursesLinks = this.GetElementsByXPath(AluraSearchPageElements.AnchorCouseResult).Select(a => a.GetAttribute("href")).ToList();
                List<string> coursesPagination = this.GetElementsByXPath(AluraSearchPageElements.AnchorLinksPagination).Select(a => a.GetAttribute("href")).ToList();
                if (coursesPagination.Count > 0)
                {
                    foreach (var coursePage in coursesPagination)
                    {
                        this.NavigateToUrl(coursePage);
                        if (this.CheckIfElementIsLoadedWithResult(AluraSearchPageElements.SearchInput, 300))
                        {
                            coursesLinks.AddRange(this.GetElementsByXPath(AluraSearchPageElements.AnchorCouseResult).Select(a => a.GetAttribute("href")).ToList());
                        }
                    }
                }
            }
            return coursesLinks;
        }
    }
}