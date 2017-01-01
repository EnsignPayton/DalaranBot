using System.IO;

namespace DalaranBot
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            string token = GetToken("../../keys.txt");

            if (string.IsNullOrWhiteSpace(token)) return;

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
