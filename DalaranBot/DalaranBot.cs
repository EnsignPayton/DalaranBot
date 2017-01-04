using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Discord;
using Discord.Audio;

namespace DalaranBot
{
    public class DalaranBot
    {
        #region Fields
        private readonly DateTime startTime = DateTime.Now;
        private readonly DiscordClient client = new DiscordClient();
        private readonly VotingManager voteMgr = new VotingManager();
        private readonly LoggingManager logMgr = new LoggingManager();
        private IAudioClient audio;
        #endregion

        #region Properties
        public string Token { get; }

        public TimeSpan Uptime => DateTime.Now - startTime;
        public string BotVersion => "DalaranBot Version " +
            Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

        public string HelpText => @"Useful Commands
`.version      Prints the version
.uptime       Prints the bot's uptime
.echo         Make me say stuff
.roll         Roll dice in the d20 syntax
.votestart    Starts a new vote
.vote         Votes for a category
.votestop     Stops an ongoing vote`";

        public string UptimeMessage
        {
            get
            {
                var sb = new StringBuilder("I've been up for ");

                if (Uptime.Days != 0)
                    sb.Append(Uptime.Days + "d ");

                if (Uptime.Hours != 0)
                    sb.Append(Uptime.Hours + "h ");

                if (Uptime.Minutes != 0)
                    sb.Append(Uptime.Minutes + "m ");

                if (Uptime.Seconds != 0)
                    sb.Append(Uptime.Seconds + "s ");

                sb.Append(Uptime.Milliseconds + "ms");

                return sb.ToString();
            }
        }
        #endregion Properties

        #region Constructor
        /// <summary>
        /// Creates a new Dalaran Bot
        /// </summary>
        /// <param name="token">OAuth Token</param>
        /// <param name="logFile">File for log output, leave null to disable logging to file</param>
        /// <param name="logTimestamp">Enable or disable timestamp logging</param>
        public DalaranBot(string token, string logFile = null, bool logTimestamp = true)
        {
            Token = token;

            // Configure Logging Manager
            if (!string.IsNullOrWhiteSpace(logFile))
            {
                logMgr.ToFile = true;
                logMgr.FileName = logFile;
            }

            logMgr.ShowTimestamp = logTimestamp;

            client.Ready += Client_Ready;
            client.MessageReceived += Client_MessageReceived;

            client.UsingAudio(x => x.Mode = AudioMode.Incoming);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Connect to Discord
        /// </summary>
        public void Connect()
        {
            client.Connect(Token, TokenType.Bot);
        }

        /// <summary>
        /// Disconnect from Discord
        /// </summary>
        public void Disconnect()
        {
            client.ExecuteAndWait(() => client.Disconnect());
        }
        #endregion

        #region Event Handlers
        private void Client_MessageReceived(object sender, MessageEventArgs e)
        {
            // Do nothing if we wrote the message, or if it doesn't start with the comnmand character
            if (e.Message.IsAuthor || !e.Message.Text.StartsWith(".")) return;

            var cmdUser = e.Message.User;
            var cmdChannel = e.Message.Channel;
            var cmdServer = e.Message.Server;
            var cmdType = e.Message.Text.Substring(1).Split(' ')[0];
            var cmdBody = e.Message.Text.Substring(1 + cmdType.Length).Trim();

            logMgr.Log($".{cmdType} {cmdBody}", cmdUser);

            switch (cmdType)
            {
                case "help":
                    SendMessage(cmdChannel, HelpText);
                    break;

                case "uptime":
                    SendMessage(cmdChannel, UptimeMessage);
                    break;

                case "version":
                    SendMessage(cmdChannel, BotVersion);
                    break;

                case "audiostart":
                    StartAudio(cmdServer);
                    break;

                case "audiostop":
                    StopAudio();
                    break;

                case "echo":
                    e.Message.Delete();
                    SendMessage(cmdChannel, cmdBody);
                    break;

                case "roll":
                    SendMessage(cmdChannel, GetRoll(cmdUser, cmdBody));
                    break;

                case "votestart":
                    SendMessage(cmdChannel, voteMgr.Start(cmdBody));
                    break;

                case "vote":
                    SendMessage(cmdChannel, voteMgr.AddVote(cmdBody));
                    break;

                case "votestop":
                    SendMessage(cmdChannel, voteMgr.Stop());
                    break;
            }
        }

        private void Client_Ready(object sender, EventArgs e)
        {
            Console.WriteLine("Connected to Discord");
        }
        #endregion

        #region Private Methods
        private void SendMessage(Channel channel, string msg)
        {
            if (string.IsNullOrEmpty(msg)) return;
            logMgr.Log(msg);
            channel.SendMessage(msg);
        }

        private async void StartAudio(Server serverToJoin, Channel channelToJoin = null)
        {
            if (channelToJoin == null)
            {
                foreach (var x in serverToJoin.VoiceChannels)
                {
                    if (!x.Name.Contains("General")) continue;
                    channelToJoin = x;
                    break;
                }
            }

            if (channelToJoin == null)
                channelToJoin = serverToJoin.VoiceChannels.First();

            audio = await client.GetService<AudioService>().Join(channelToJoin);
        }

        private async void StopAudio()
        {
            await audio.Disconnect();
        }

        private static string GetRoll(User callee, string command)
        {
            var isError = false;
            var rolls = command.Split('+');
            var results = new List<int>();

            foreach (var roll in rolls)
            {
                if (roll.Contains("d"))
                {
                    // We're rolling dice
                    uint numDice, typeDice;
                    var mods = roll.Split('d');

                    if (!uint.TryParse(mods[0], out numDice))
                    {
                        isError = true;
                        break;
                    }

                    if (!uint.TryParse(mods[1], out typeDice))
                    {
                        isError = true;
                        break;
                    }

                    // Actually roll the dice
                    var diceValues = RollDice(numDice, typeDice);
                    results.AddRange(diceValues);
                }
                else if (roll.Contains("-"))
                {
                    // Range-based rolling
                    int iFrom, iTo;
                    var nums = roll.Split('-');

                    if (!int.TryParse(nums[0], out iFrom))
                    {
                        isError = true;
                        break;
                    }

                    if (!int.TryParse(nums[1], out iTo))
                    {
                        isError = true;
                        break;
                    }

                    results.Add(RollRange(iFrom, iTo));
                }
                else
                {
                    // We're adding a modifier
                    int addMod;

                    if (!int.TryParse(roll, out addMod))
                    {
                        isError = true;
                        break;
                    }

                    results.Add(addMod);
                }
            }

            if (isError)
                return $"{callee.Name} used invalid syntax!";

            var sb = new StringBuilder();

            sb.Append($"{callee.Name} rolled ");

            if (results.Count > 1)
            {
                for (var i = 0; i < results.Count; i++)
                {
                    sb.Append(results[i]);
                    if (i != results.Count - 1) sb.Append("+");
                }

                sb.Append($" = {results.Sum()}");
            }
            else
            {
                sb.Append(results[0]);
            }

            return sb.ToString();
        }

        private static IEnumerable<int> RollDice(uint num, uint type)
        {
            var rand = new Random();
            var results = new List<int>();

            for (var i = 0; i < num; i++)
                results.Add(rand.Next((int)type) + 1);

            return results;
        }

        private static int RollRange(int iFrom, int iTo)
        {
            var rand = new Random();

            return rand.Next(iFrom, iTo);
        }
        #endregion
    }
}
