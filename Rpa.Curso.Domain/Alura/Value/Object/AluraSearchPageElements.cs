namespace Rpa.Curso.Domain.Alura.Value.Object
{
    public static class AluraSearchPageElements
    {
        private const string SEARCH_INPUT = "//input[@id='busca-form-input']";
        public static string SearchInput { get { return SEARCH_INPUT; } }

        //-------------------------------------------------------------------------------//

        private const string ANCHOR_COURSE_RESULT = "//a[@class='busca-resultado-link']";
        public static string AnchorCouseResult { get { return ANCHOR_COURSE_RESULT; } }

        //-------------------------------------------------------------------------------//

        private const string ANCHOR_LINKS_PAGINATION = "//a[@class='paginationLink']";
        public static string AnchorLinksPagination { get { return ANCHOR_LINKS_PAGINATION; } }
    }
}