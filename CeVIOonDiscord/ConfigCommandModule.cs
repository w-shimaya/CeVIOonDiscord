using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CeVIO.Talk.RemoteService;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace CeVIOonDiscord
{
    [Group("config")]
    [Description("Configure talkers.")]
    public class ConfigCommandModule : BaseCommandModule
    {
        private static readonly Dictionary<string, string> CastColor = new Dictionary<string, string>()
        {
            {"さとうささら", "#e79dff" },
            {"すずきつづみ", "#41b3e9" },
            {"タカハシ", "#c2c2c2" },
        };

        [Command("volume")]
        [Description("Get/Set volume.")]
        public async Task GetVolume(CommandContext context)
        {
            var entry = TalkerConfigManager.getInstance().GetConfigEntry(context.Guild.Id, context.User.Id);
            await context.RespondAsync($"{context.User.Username}'s volume is {entry.Volume}.");
        }

        [Command("volume")]
        public async Task SetVolume(CommandContext context, uint value)
        {
            if (value > 100)
            {
                await context.RespondAsync($":x: {value} is not a valid value for volume (0-100).");
                return;
            }

            var entry = TalkerConfigManager.getInstance().GetConfigEntry(context.Guild.Id, context.User.Id);
            entry.Volume = value;

            await context.RespondAsync($"{context.User.Username}'s volume is now {value}.");
        }

        [Command("speed")]
        public async Task GetSpeed(CommandContext context)
        {
            var entry = TalkerConfigManager.getInstance().GetConfigEntry(context.Guild.Id, context.User.Id);
            await context.RespondAsync($"{context.User.Username}'s speed is {entry.Speed}.");
        }

        [Command("speed")]
        public async Task SetSpeed(CommandContext context, uint value)
        {
            if (value > 100)
            {
                await context.RespondAsync($":x: {value} is not a valid value for speed (0-100).");
                return;
            }

            var entry = TalkerConfigManager.getInstance().GetConfigEntry(context.Guild.Id, context.User.Id);
            entry.Speed = value;

            await context.RespondAsync($"{context.User.Username}'s speed is now {value}.");
        }

        [Command("tone")]
        public async Task GetTone(CommandContext context)
        {
            var entry = TalkerConfigManager.getInstance().GetConfigEntry(context.Guild.Id, context.User.Id);
            await context.RespondAsync($"{context.User.Username}'s tone is {entry.Tone}.");
        }

        [Command("tone")]
        public async Task SetTone(CommandContext context, uint value)
        {
            if (value > 100)
            {
                await context.RespondAsync($":x: {value} is not a valid value for tone (0-100).");
                return;
            }

            var entry = TalkerConfigManager.getInstance().GetConfigEntry(context.Guild.Id, context.User.Id);
            entry.Tone = value;

            await context.RespondAsync($"{context.User.Username}'s tone is now {value}.");
        }

        [Command("tonescale")]
        public async Task GetToneScale(CommandContext context)
        {
            var entry = TalkerConfigManager.getInstance().GetConfigEntry(context.Guild.Id, context.User.Id);
            await context.RespondAsync($"{context.User.Username}'s tonescale is {entry.Tonescale}.");
        }

        [Command("tonescale")]
        public async Task SetToneScale(CommandContext context, uint value)
        {
            if (value > 100)
            {
                await context.RespondAsync($":x: {value} is not a valid value for tonescale (0-100).");
                return;
            }

            var entry = TalkerConfigManager.getInstance().GetConfigEntry(context.Guild.Id, context.User.Id);
            entry.Tonescale = value;

            await context.RespondAsync($"{context.User.Username}'s tonescale is now {value}.");
        }

        [Command("alpha")]
        public async Task GetAlpha(CommandContext context)
        {
            var entry = TalkerConfigManager.getInstance().GetConfigEntry(context.Guild.Id, context.User.Id);
            await context.RespondAsync($"{context.User.Username}'s alpha is {entry.Alpha}.");
        }

        [Command("alpha")]
        public async Task SetAlpha(CommandContext context, uint value)
        {
            if (value > 100)
            {
                await context.RespondAsync($":x: {value} is not a valid value for alpha (0-100).");
                return;
            }

            var entry = TalkerConfigManager.getInstance().GetConfigEntry(context.Guild.Id, context.User.Id);
            entry.Alpha = value;

            await context.RespondAsync($"{context.User.Username}'s alpha is now {value}.");
        }

        [Command("cast")]
        public async Task GetCast(CommandContext context)
        {
            var entry = TalkerConfigManager.getInstance().GetConfigEntry(context.Guild.Id, context.User.Id);
            await context.RespondAsync($"{context.User.Username}'s cast is {entry.Cast}.");
        }

        [Command("cast")]
        public async Task SetCast(CommandContext context, string value)
        {
            if (!Talker.AvailableCasts.Contains(value))
            {
                var message = $":x: There is no cast named \"{value}\"\n**Available casts** are:\n";
                foreach (var castname in Talker.AvailableCasts)
                {
                    message += $"{castname}, ";
                }
                await context.RespondAsync(message);
            }

            var entry = TalkerConfigManager.getInstance().GetConfigEntry(context.Guild.Id, context.User.Id);
            entry.Cast = value;

            await context.RespondAsync($"{context.User.Username}'s cast is now {value}.");
        }

        [Command("comp")]
        [Description("Set/Get components.")]
        public async Task GetComponent(CommandContext context)
        {
            var entry = TalkerConfigManager.getInstance().GetConfigEntry(context.Guild.Id, context.User.Id);
            var talker = new Talker();
            talker.Cast = entry.Cast;

            var builder = new DiscordEmbedBuilder();
            builder.WithTitle($"{context.User.Username}'s cast components");
            builder.WithColor(new DiscordColor(CastColor[entry.Cast]));
            for (var i = 0; i < talker.Components.Count; ++i)
            {
                builder.AddField($"{talker.Components[i].Name}", $"{entry.Castspec[i]}", true);
            }
            await context.RespondAsync(embed: builder.Build());
        }

        [Command("comp")]
        public async Task SetComponent(CommandContext context, string name, uint value)
        {
            var entry = TalkerConfigManager.getInstance().GetConfigEntry(context.Guild.Id, context.User.Id);

            var talker = new Talker();
            talker.Cast = entry.Cast;

            // 
            name = name.First().ToString().ToUpper() + name.Substring(1).ToLower();

            for (var i = 0; i < talker.Components.Count; ++i)
            {
                if (talker.Components[i].Name == name)
                {
                    if (value > 100)
                    {
                        await context.RespondAsync($":x: {value} is not a valid value for alpha (0-100).");
                        return;
                    }
                    entry.Castspec[i] = value;
                    break;
                }
            }

            await context.RespondAsync($"{context.User.Username}'s cast has now components below:");
            var builder = new DiscordEmbedBuilder();
            for (var i = 0; i < talker.Components.Count; ++i)
            {
                builder.AddField($"{talker.Components[i].Name}", $"{entry.Castspec[i]}", true);
            }
            await context.RespondAsync(embed: builder.Build());
        }

        [Command("save")]
        [Description("Save your current cast config.")]
        public async Task SaveConfig(CommandContext context)
        {
            TalkerConfigManager.getInstance().NotifyUpdate();
            await context.RespondAsync("Your config has been saved :+1:");
        }

        [Command("available")]
        [Aliases("av")]
        public async Task ListAvailabelCasts(CommandContext context)
        {
            var message = "Available casts are:\n";
            foreach (var castname in Talker.AvailableCasts)
            {
                message += $"{castname}, ";
            }
            message = message.Substring(0, message.Length - 2);
            await context.RespondAsync(message);
        }

        [Command("show")]
        [Description("Show all parameters you can set.")]
        public async Task ShowConfig(CommandContext context)
        {
            var entry = TalkerConfigManager.getInstance().GetConfigEntry(context.Guild.Id, context.User.Id);

            var builder = new DiscordEmbedBuilder();
            builder.WithTitle($"{context.User.Username}'s Current cast is **{entry.Cast}**.");
            builder.AddField("Volume", $"{entry.Volume}", true);
            builder.AddField("Speed", $"{entry.Speed}", true);
            builder.AddField("Tone", $"{entry.Tone}", true);
            builder.AddField("ToneScale", $"{entry.Tonescale}", true);
            builder.AddField("Alpha", $"{entry.Alpha}", true);
            await context.RespondAsync(embed: builder.Build());

            var talker = new Talker();
            talker.Cast = entry.Cast;

            builder.ClearFields();
            builder.WithTitle($"{entry.Cast} has {talker.Components.Count} components");
            builder.WithColor(new DiscordColor(CastColor[entry.Cast]));
            for (var i = 0; i < talker.Components.Count; ++i)
            {
                builder.AddField($"{talker.Components[i].Name}", $"{entry.Castspec[i]}", true);
            }
            await context.RespondAsync(embed: builder.Build());
        }
    }
}
