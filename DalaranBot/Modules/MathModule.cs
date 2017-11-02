using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace DalaranBot.Modules
{
    [Name("Math")]
    public class MathModule : ModuleBase<SocketCommandContext>
    {
        private static readonly Random Random = new Random();

        [Command("square")]
        public async Task Square(double x)
        {
            await ReplyAsync($"{x * x}");
        }

        [Command("add"), Alias("sum")]
        public async Task Sum(params double[] numbers)
        {
            await ReplyAsync(numbers.Sum().ToString());
        }

        [Command("random"), Alias("rand")]
        public async Task Rand(string range)
        {
            // Parse input
            int lower, upper;
            var bounds = range.Split('-');

            if (bounds.Length != 2) return;
            if (!int.TryParse(bounds[0], out lower)) return;
            if (!int.TryParse(bounds[1], out upper)) return;
            if (lower > upper) return;

            var result = Random.Next(lower, upper);

            await ReplyAsync(result.ToString());
        }

        [Command("roll")]
        public async Task Roll(string roll)
        {
            // Parse input
            int result = 0;
            var parts = roll.Split('+');

            foreach (var part in parts)
            {
                // Could be dice (Ex. 2d4) or modifier (Ex. 2)
                var subparts = part.Split('d');

                if (subparts.Length == 1)
                {
                    int value;
                    if (!int.TryParse(subparts[0], out value)) return;

                    result += value;
                    continue;
                }

                if (subparts.Length == 2)
                {
                    int count, type;
                    if (!int.TryParse(subparts[0], out count)) return;
                    if (!int.TryParse(subparts[1], out type)) return;

                    for (int i = 0; i < count; i++)
                    {
                        result += Random.Next(1, type);
                    }

                    continue;
                }

                return;
            }

            await ReplyAsync(result.ToString());
        }
    }
}
