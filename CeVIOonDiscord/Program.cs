using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CeVIO.Talk.RemoteService;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.CommandsNext;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace CeVIOonDiscord
{
    public static class CeVIOEvents
    {
        public static EventId SendVoice { get; } = new EventId(201, nameof(SendVoice));
        public static EventId CeVIOError { get; } = new EventId(202, nameof(CeVIOError));
    }

    class Program
    {
        public static Talker talker;
        /// <summary>
        /// Dictionary from the ID of a guild to the ID of a channel in which the bot will process messages.
        /// </summary>
        public static Dictionary<ulong, ulong> channel2Read;

        static void Main(string[] args)
        {
            channel2Read = new Dictionary<ulong, ulong>();

            new Program().MainAsync().GetAwaiter().GetResult();

            // Close CS7
            ServiceControl.CloseHost();
        }

        public async Task MainAsync()
        {
            // 【CeVIO Creative Studio】起動
            ServiceControl.StartHost(false);
            // Talkerインスタンス生成
            talker = new Talker();

            var token = GetDiscordTokenFromFile("discord.txt");
             
            // Create client
            var client = new DiscordClient(new DiscordConfiguration()
            {
                Token = token,
                TokenType = TokenType.Bot,
#if DEBUG
                MinimumLogLevel = LogLevel.Debug,
#else
                MinimumLogLevel = LogLevel.Information,
#endif
            });
            var commands = client.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { "!" },
            });
            client.UseVoiceNext();
            // Register commands
            commands.RegisterCommands<CommandModule>();
            commands.RegisterCommands<ConfigCommandModule>();

            // Events
            client.MessageCreated += MessageRecieved;
            client.VoiceStateUpdated += VoiceStateUpdated;

            await client.ConnectAsync();
            await Task.Delay(-1);
        }

        /// <summary>
        /// Handles MessageRecieved event.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task MessageRecieved(DiscordClient c, MessageCreateEventArgs e)
        {
            // If the bot received a message from a guild for the first time,
            // add the ID of the guild to Dictionary
            if (!channel2Read.ContainsKey(e.Guild.Id))
            {
                channel2Read.Add(e.Guild.Id, 0);
            }

            var vnext = c.GetVoiceNext();
            var connection = vnext?.GetConnection(e.Guild);
            var isconnected = connection != null;
            // Read all messages that is not command and is not from bots if the bot is activated.
            if (isconnected && !e.Author.IsBot
                && !e.Message.Content.StartsWith("!")
                && e.Channel.Id == channel2Read[e.Guild.Id])
            {
                _ = Task.Run(async () => await SendWavSoundAsync(c, e));
                c.Logger.LogDebug(CeVIOEvents.SendVoice, "Generating and sending wav to the guild (ID: {id})", e.Guild.Id);
                
            }
        }

        /// <summary>
        /// Send voice to a voice channel.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task SendWavSoundAsync(DiscordClient c, MessageCreateEventArgs e)
        {
            var manager = TalkerConfigManager.getInstance();
            var entry = manager.GetConfigEntry(e.Guild.Id, e.Author.Id);
            // General configs
            if (Talker.AvailableCasts.Contains(entry.Cast))
            {
                talker.Cast = entry.Cast;
            }
            else
            {
                c.Logger.LogError(CeVIOEvents.CeVIOError, "Cast has an invalid value: {cast}", entry.Cast);
                return;
            }
            talker.Volume = entry.Volume;
            talker.Tone = entry.Tone;
            talker.ToneScale = entry.Tonescale;
            talker.Speed = entry.Speed;
            talker.Alpha = entry.Alpha;
            // Cast specific configs
            if (talker.Components.Count != entry.Castspec.Count)
            {
                // Fill with default values
                entry.Castspec = new List<uint> { 100 };

                for (int i = 0; i < talker.Components.Count - 1; ++i)
                {
                    entry.Castspec.Add(0);
                }
            }
            for (int i = 0; i < talker.Components.Count; ++i)
            {
                talker.Components[i].Value = entry.Castspec[i];
            }

            // Preprocess the message.
            c.Logger.LogDebug(CeVIOEvents.SendVoice, "The raw message to read is: {msg}", e.Message.Content);
            var content = GetContentBody(e.Message.Content, e.Message.MentionedUsers, e.Message.MentionedChannels, e.Message.MentionedRoles);
            c.Logger.LogDebug(CeVIOEvents.SendVoice, "The processed message to read is: {msg}", content);

            // Output a temporary audio file.
            var wavname = $"{e.Guild.Id}.{e.Author.Id}.{DateTime.Now:yyyyMMddHHmmss}.wav";
            talker.OutputWaveToFile(content, wavname);
            
            var vnext = c.GetVoiceNext();
            var connection = vnext?.GetConnection(e.Guild);
            var transmit = connection.GetTransmitSink();
            var stream = CreateStream(wavname);
            await stream.CopyToAsync(transmit);
            stream.Dispose();
            File.Delete(wavname);
        }

        private Stream CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            }).StandardOutput.BaseStream;
        }

        private async Task VoiceStateUpdated(DiscordClient c, VoiceStateUpdateEventArgs e)
        {
            var vnext = c.GetVoiceNext();
            var connection = vnext?.GetConnection(e.Guild);

            // Check if the bot is connected.
            if (connection == null)
            {
                return;
            }

            if (connection.TargetChannel.Users.Count() <= 1)
            {
                connection.Disconnect();
                var textch = await c.GetChannelAsync(channel2Read[e.Guild.Id]);
                await c.SendMessageAsync(textch, "Disconnected because of loneliness :wave:");
            }
        }

        private string GetContentBody(string content, IReadOnlyList<DiscordUser> users, IReadOnlyList<DiscordChannel> channels, IReadOnlyList<DiscordRole> roles)
        {
            var msg = "";
            var i = 0;
            // User mention
            var match = Regex.Match(content, @"<(@!|#|@&)[0-9]+>");
            while (match.Success)
            {
                msg += content.Substring(i, match.Index - i);
                i = match.Index + match.Length;

                // Replace string according to its mention type (User, Channel, Role)
                if (match.Value[1] == '#')
                {
                    var id = UInt64.Parse(match.Value.Substring(2, match.Length - 3));
                    var matched_channel = channels.First(c => c.Id == id);
                    msg += $"#{matched_channel.Name}、";
                }
                else if (match.Value[2] == '!')
                {
                    var id = UInt64.Parse(match.Value.Substring(3, match.Length - 4));
                    var matched_user = users.First(u => u.Id == id);
                    msg += $"@{matched_user.Username}、";
                }
                else if (match.Value[2] == '&')
                {
                    var id = UInt64.Parse(match.Value.Substring(3, match.Length - 4));
                    var matched_role = roles.First(r => r.Id == id);
                    msg += $"@{matched_role.Name}、";
                }
                match = match.NextMatch();
            }
            msg += content.Substring(i);

            // remove urls
            msg = Regex.Replace(msg, @"https?://([\w-]+\.)+[\w-]+(/[\w-./?%&=~]*)?", "");

            return msg;
        }

        private string GetDiscordTokenFromFile(string path)
        {
            string token;
            using (var reader = new StreamReader(path))
            {
                token = reader.ReadToEnd();
            }

            var match = Regex.Match(token, @"^DISCORD_TOKEN\s*=\s*(.+)");
            if (!match.Success)
            {
                throw new IOException("Failed to get a discord token. Check if discord.txt is properly formatted.");
            }

            token = match.Groups[1].Value;
            return token;
        }
    }
}
