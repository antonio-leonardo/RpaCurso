namespace Rpa.Curso.CrossCutting
{
    public class LoggingSingleton
    {
        public static Logging Log { get; set; }

        public static void InitializeLog()
        {
            Log = new Logging(AppContext.BaseDirectory + @"\Log\", "log_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log");
        }

        public static Logging GetLogging()
        {
            return Log;
        }
    }
}