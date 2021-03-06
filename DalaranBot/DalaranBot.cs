﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Discord;

namespace DalaranBot
{
    public class DalaranBot
    {
        #region Fields

        private readonly DateTime _startTime = DateTime.Now;
        private readonly DiscordClient _discordClient = new DiscordClient();
        private readonly VotingManager _votingManager = new VotingManager();
        private readonly LoggingManager _loggingManager = new LoggingManager();

        #endregion

        #region Properties

        public string Token { get; }

        public TimeSpan Uptime => DateTime.Now - _startTime;
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
                _loggingManager.ToFile = true;
                _loggingManager.FileName = logFile;
            }

            _loggingManager.ShowTimestamp = logTimestamp;

            _discordClient.Ready += DiscordClientReady;
            _discordClient.MessageReceived += DiscordClientMessageReceived;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Connect to Discord
        /// </summary>
        public void Connect()
        {
            _discordClient.Connect(Token, TokenType.Bot);
        }

        /// <summary>
        /// Disconnect from Discord
        /// </summary>
        public void Disconnect()
        {
            _discordClient.ExecuteAndWait(() => _discordClient.Disconnect());
        }

        #endregion

        #region Event Handlers

        private void DiscordClientMessageReceived(object sender, MessageEventArgs e)
        {
            // Do nothing if we wrote the message, or if it doesn't start with the comnmand character
            if (e.Message.IsAuthor || !e.Message.Text.StartsWith(".")) return;

            var cmdUser = e.Message.User;
            var cmdChannel = e.Message.Channel;
            var cmdType = e.Message.Text.Substring(1).Split(' ')[0];
            var cmdBody = e.Message.Text.Substring(1 + cmdType.Length).Trim();

            _loggingManager.Log($".{cmdType} {cmdBody}", cmdUser);

            switch (cmdType)
            {
                case "help":
                    SendMessage(cmdChannel, HelpText);
                    break;

                case "echo":
                    e.Message.Delete();
                    SendMessage(cmdChannel, cmdBody);
                    break;

                case "roll":
                    SendMessage(cmdChannel, GetRoll(cmdUser, cmdBody));
                    break;

                case "uptime":
                    SendMessage(cmdChannel, UptimeMessage);
                    break;

                case "version":
                    SendMessage(cmdChannel, BotVersion);
                    break;

                case "votestart":
                    SendMessage(cmdChannel, _votingManager.Start(cmdBody));
                    break;

                case "vote":
                    SendMessage(cmdChannel, _votingManager.AddVote(cmdBody));
                    break;

                case "votestop":
                    SendMessage(cmdChannel, _votingManager.Stop());
                    break;
            }
        }

        private void DiscordClientReady(object sender, EventArgs e)
        {
            Console.WriteLine("Connected to Discord");
        }

        #endregion

        #region Private Methods

        private void SendMessage(Channel channel, string msg)
        {
            if (string.IsNullOrEmpty(msg)) return;
            _loggingManager.Log(msg);
            channel.SendMessage(msg);
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
