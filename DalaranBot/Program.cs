using System;
using System.IO;
using System.Threading.Tasks;

namespace DalaranBot
{
    public class Program
    {
        private const string DefaultTokenFile = "keys.txt";
        private static DalaranBot _dalaranBot;

        // The Discord API is fully asynchronous, so this app will be as well
        public static void Main(string[] args) => StartAsync().GetAwaiter().GetResult();

        public static async Task StartAsync()
        {
            try
            {
                _dalaranBot = new DalaranBot(File.ReadAllText(DefaultTokenFile));

                await _dalaranBot.Connect();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            await Task.Delay(-1);
        }
    }
}
