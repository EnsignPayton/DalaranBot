using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DalaranBot
{
    public static class Program
    {
        private const string defaultTokenFile = "keys.txt";
        private static DalaranBot bot;

        /// <summary>
        /// Application Entry Point
        /// </summary>
        /// <param name="args">Command line argument array</param>
        public static void Main(string[] args)
        {
            string tokenFile = null;
            string logFile = null;

            // Check for parameters with associated data
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (args[i].Equals("-kf") && !string.IsNullOrWhiteSpace(args[i + 1]))
                    tokenFile = args[i + 1];
                if (args[i].Equals("-lf") && !string.IsNullOrWhiteSpace(args[i + 1]))
                    logFile = args[i + 1];
            }

            StartBot(tokenFile, logFile, args.Contains("-lts"));

            while (true)
            {
                var input = Console.ReadLine();

                switch (input?.ToUpper())
                {
                    case "EXIT":
                    case "QUIT":
                        bot.Disconnect();
                        Environment.Exit(0);
                        break;
                }
            }
        }

        #region Private Methods
        private static void StartBot(string tokenFile, string logFile, bool logTimeStamp)
        {
            if (string.IsNullOrWhiteSpace(tokenFile))
                tokenFile = defaultTokenFile;

            Console.WriteLine("Starting DalaranBot v{0}",
                Assembly.GetExecutingAssembly().GetName().Version.ToString(3));

            Console.WriteLine("Key File: {0}", tokenFile);

            if (!string.IsNullOrWhiteSpace(logFile))
                Console.WriteLine("Log File: {0}", logFile);

            if (logTimeStamp)
                Console.WriteLine("Timestamp Logging Enabled");

            try
            {
                bot = new DalaranBot(GetToken(tokenFile), logFile, logTimeStamp);
                bot.Connect();
            }
            catch (Discord.Net.HttpException ex)
            {
                if (ex.Message.Contains("401"))
                    Console.WriteLine(
                        "Invalid credentials. Your bot user's token should be placed in \"keys.txt\" or another file specified with -kf");
                else throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unhandled Exception: " + ex.Message);
            }
        }

        private static string GetToken(string tokenFilePath = defaultTokenFile)
        {
            var result = string.Empty;

            if (!File.Exists(tokenFilePath)) return result;

            var tokenFile = new FileInfo(tokenFilePath);

            // 64 MB is plenty
            if (tokenFile.Length >= 1024*1024*64) return result;

            using (var sr = tokenFile.OpenText())
                result = sr.ReadLine();

            return result;
        }
        #endregion
    }
}
