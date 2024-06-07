using System.Text;

namespace Rpa.Curso.CrossCutting
{
    public enum TypeLog
    {
        Info, Error, Warning
    }
    public class Logging
    {
        private readonly string _logPath;
        private readonly string _fullFilePath;

        public Logging(string logPath, string logFileName = null)
        {
            this._logPath = logPath + (!logPath.EndsWith(@"\") ? @"\" : string.Empty);
            this._fullFilePath = this._logPath + (string.IsNullOrWhiteSpace(logFileName) ? "log_" + DateTime.Now.ToString("yyyMMdd_HHmmss") + ".log" : logFileName);
        }

        public void Info(string message)
        {
            this.Log(TypeLog.Info, message);
        }

        public void Error(string message)
        {
            this.Log(TypeLog.Error, message);
        }

        public void Warning(string message)
        {
            this.Log(TypeLog.Warning, message);
        }

        private void Log(TypeLog typeLog, string message)
        {
            if (!Directory.Exists(this._logPath))
                Directory.CreateDirectory(this._logPath);

            StringBuilder sb = new();
            sb.AppendLine(DateTime.Now + $" - [ {typeLog.ToString().ToUpper()} ] - {message}");

            File.AppendAllText(this._fullFilePath, sb.ToString());
            Console.Out.WriteLine(sb.ToString());
            sb.Clear();
        }
    }
}