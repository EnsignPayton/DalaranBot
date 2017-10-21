using System.Threading.Tasks;
using Discord.Commands;

namespace DalaranBot.Modules
{
    [Name("Test")]
    public class TestModule : ModuleBase<SocketCommandContext>
    {
        [Command("echo")]
        [Summary("Echo some text")]
        public Task Echo([Remainder] string text) => ReplyAsync(text);
    }
}
