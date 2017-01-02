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
        private readonly DiscordClient client = new DiscordClient();
        private readonly DateTime startTime = DateTime.Now;
        private readonly VotingManager voteMgr = new VotingManager();

        #region Properties
        public string Token { get; }

        public TimeSpan Uptime => DateTime.Now - startTime;
        public string BotVersion => "DalaranBot Version " +
            Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

        public string HelpText => @"Useful Commands
.version     Prints the version
.uptime      Prints the bot's uptime
.echo          Make me say stuff
.roll             Roll dice in the d20 syntax
.votestart  Starts a new vote
.vote           Votes for a category
.votestop   Stops an ongoing vote";

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

        /// <summary>
        /// Creates a new Dalaran Bot
        /// </summary>
        /// <param name="token">OAuth Token</param>
        public DalaranBot(string token)
        {
            Token = token;

            client.MessageReceived += Client_MessageReceived;
        }

        /// <summary>
        /// Connect to Discord
        /// </summary>
        public void Start()
        {
            client.ExecuteAndWait(async () =>
            {
                await client.Connect(Token, TokenType.Bot);
            });
        }

        #region Event Handlers
        private void Client_MessageReceived(object sender, MessageEventArgs e)
        {
            // Do nothing if we wrote the message, or if it doesn't start with the comnmand character
            if (e.Message.IsAuthor || !e.Message.Text.StartsWith(".")) return;

            var cmdUser = e.Message.User;
            var cmdChannel = e.Message.Channel;
            var cmdType = e.Message.Text.Substring(1).Split(' ')[0];
            var cmdBody = e.Message.Text.Substring(1 + cmdType.Length).Trim();

            switch (cmdType)
            {
                case "help":
                    cmdChannel.SendMessage(HelpText);
                    break;

                case "echo":
                    e.Message.Delete();
                    cmdChannel.SendMessage(cmdBody);
                    break;

                case "roll":
                    cmdChannel.SendMessage(GetRoll(cmdUser, cmdBody));
                    break;

                case "uptime":
                    cmdChannel.SendMessage(UptimeMessage);
                    break;

                case "version":
                    cmdChannel.SendMessage(BotVersion);
                    break;

                case "votestart":
                    cmdChannel.SendMessage(voteMgr.Start(cmdBody));
                    break;

                case "vote":
                    var msg = voteMgr.AddVote(cmdBody);
                    if (!string.IsNullOrEmpty(msg))
                        cmdChannel.SendMessage(msg);
                    break;

                case "votestop":
                    cmdChannel.SendMessage(voteMgr.Stop());
                    break;
            }
        }
        #endregion

        #region Command Methods
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
                    int numDice, typeDice;
                    var mods = roll.Split('d');

                    if (!int.TryParse(mods[0], out numDice))
                    {
                        isError = true;
                        break;
                    }

                    if (!int.TryParse(mods[1], out typeDice))
                    {
                        isError = true;
                        break;
                    }

                    // Actually roll the dice
                    var diceValues = RollDice(numDice, typeDice);
                    results.AddRange(diceValues);
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

        private static IEnumerable<int> RollDice(int num, int type)
        {
            var rand = new Random();
            var results = new List<int>();

            for (var i = 0; i < num; i++)
                results.Add(rand.Next(type) + 1);

            return results;
        }
        #endregion
    }
}
