namespace Rpa.Curso.Domain.Alura.Value.Object
{
    public static class AluraCoursePageElements
    {
        private const string COURSE_TITLE_FIRST_PART = "//h1[@class='curso-banner-course-title']";
        public static string CourseTitleFirstPart { get { return COURSE_TITLE_FIRST_PART; } }

        //-------------------------------------------------------------------------------//

        private const string COURSE_TITLE_SECOND_PART = "//p[@class='course--banner-text-category']";
        public static string CourseTitleSecondPart { get { return COURSE_TITLE_SECOND_PART; } }

        //-------------------------------------------------------------------------------//

        private const string COURSE_HOURS = "(//div[@class='couse-container--spacing']/div/p[@class='courseInfo-card-wrapper-infos'])[1]";
        public static string CourseHours { get { return COURSE_HOURS; } }

        //-------------------------------------------------------------------------------//

        private const string INSTRUCTOR_NAME = "//h3[@class='instructor-title--name']";
        public static string InstructorName { get { return INSTRUCTOR_NAME; } }

        //-------------------------------------------------------------------------------//

        private const string COURSE_DESCRIPTION_PART = "//li[@class='course-list--learn']";
        public static string CourseDescriptionPart { get { return COURSE_DESCRIPTION_PART; } }
    }
}