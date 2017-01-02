using System;
using System.IO;
using System.Reflection;

namespace DalaranBot
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            string token = GetToken("../../keys.txt");

            if (string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine("Invalid Credentials. Shutting down...");
                return;
            }

            Console.WriteLine("Starting DalaranBot v{0}",
                Assembly.GetExecutingAssembly().GetName().Version.ToString(3));

            var bot = new DalaranBot(token);
            bot.Start();
        }

        private static string GetToken(string tokenFilePath)
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
    }
}
