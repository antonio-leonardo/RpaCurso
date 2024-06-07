namespace Rpa.Curso.CrossCutting
{
    public class LoggingBase
    {
        protected readonly Logging Log = LoggingSingleton.GetLogging();
    }
}