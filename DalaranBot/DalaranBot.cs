using Discord;

namespace DalaranBot
{
    class DalaranBot
    {
        private DiscordClient client;

        public string Token { get; }

        public DalaranBot(string token)
        {
            Token = token;

            client = new DiscordClient();
            client.MessageReceived += Client_MessageReceived;
        }

        public void Start()
        {
            client.ExecuteAndWait(async () =>
            {
                await client.Connect(Token, TokenType.Bot);
            });
        }

        private void Client_MessageReceived(object sender, MessageEventArgs e)
        {
            if (e.Message.IsAuthor) return;

            // Temp Echo
            e.Message.Channel.SendMessage("ECHO: " + e.Message.Text);
        }
    }
}
