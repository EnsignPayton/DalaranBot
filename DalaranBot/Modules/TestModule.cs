using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace DalaranBot.Modules
{
    [Name("Test")]
    public class TestModule : ModuleBase<SocketCommandContext>
    {
        private static readonly Random Random = new Random();

        private static readonly IList<string> Memes = new[]
        {
            "Program received signal SIGSEGV, Segmentation fault.",
            "Robert'); DROP TABLE Students; -- ",
            "Object reference not set to an instance of an object.",
            "java.lang.NullPointerException: Unhandled exception."
        };

        [Command("echo"), Alias("say")]
        [Summary("Echo some text")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Echo([Remainder] string text) => await ReplyAsync(text);

        [Command("meme")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Meme() => await ReplyAsync(Memes[Random.Next(Memes.Count)]);
    }
}
