using System;
using System.IO;
using System.Text;
using Discord;

namespace DalaranBot
{
    public class LoggingManager
    {
        public bool ToFile { get; set; } = false;
        public bool ToConsole { get; set; } = true;
        public bool ShowTimestamp { get; set; } = true;
        public bool ShowUser { get; set; } = true;
        public string FileName { get; set; } = string.Empty;

        public void Log(string message, User user = null)
        {
            var msgToLog = BuildMessage(message, user);

            if (ToConsole)
                LogToConsole(msgToLog);
            if (ToFile)
                LogToFile(msgToLog);
        }

        #region Private Methods

        private string BuildMessage(string msg, User user = null)
        {
            if (msg == null) return null;

            var sb = new StringBuilder();

            if (ShowTimestamp)
                sb.Append($"{{{DateTime.Now}}} ");
            if (ShowUser && user != null)
                sb.Append($"{user.Name}: ");
            sb.Append(msg);

            return sb.ToString();
        }

        private static void LogToConsole(string msg)
        {
            try
            {
                Console.WriteLine(msg);
            }
            catch
            {
                // Running as a service
            }
        }

        private void LogToFile(string msg)
        {
            // Check for invalid file name
            if (string.IsNullOrWhiteSpace(FileName))return;

            try
            {
                using (var sw = File.AppendText(FileName))
                {
                    sw.WriteLine(msg);
                }
            }
            catch (Exception ex)
            {
                if (ToConsole) LogToConsole("Unhandled Exception: " + ex.Message);
            }
        }

        #endregion
    }
}
