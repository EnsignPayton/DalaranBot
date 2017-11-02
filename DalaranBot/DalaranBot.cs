using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DalaranBot
{
    public class DalaranBot
    {
        private static readonly NLog.ILogger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly DiscordSocketClient _discordClient;
        private readonly CommandService _commandService;
        private readonly string _token;
        private readonly char _prefix = '.';

        public DalaranBot(string token)
        {
            _discordClient = new DiscordSocketClient();

            _commandService = new CommandService(new CommandServiceConfig
            {
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Verbose
            });

            _token = token;

            _discordClient.MessageReceived += DiscordClient_MessageReceived;
        }

        public async Task Connect()
        {
            await _discordClient.LoginAsync(TokenType.Bot, _token);
            await _discordClient.StartAsync();

            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task DiscordClient_MessageReceived(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            if (message == null) return;
            if (message.Author == _discordClient.CurrentUser ||
                message.Author.IsBot) return;

            int argPos = 0;
            if (message.HasCharPrefix(_prefix, ref argPos) ||
                message.HasMentionPrefix(_discordClient.CurrentUser, ref argPos))
            {
                Logger.Info("Responding to @" + message.Author.Username + ": " + message.Content);
                var context = new SocketCommandContext(_discordClient, message);
                await _commandService.ExecuteAsync(context, argPos);
            }

            await Task.Delay(1);
        }
    }
}
