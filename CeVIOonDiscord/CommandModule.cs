using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.VoiceNext;
using DSharpPlus.Entities;

namespace CeVIOonDiscord
{
    public class CommandModule : BaseCommandModule
    {
        /// <summary>
        /// Command to connect the bot to a voice channel
        /// that a user who triggered this command is in.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="channel">
        /// Voice channel to connect to. By default (null), The bot tries to connect to a voice channel that a user who triggered the command is in.
        /// </param>
        /// <returns></returns>
        [Command("connect")]
        [Aliases("c")]
        [Description("Connect this Bot to the voice channel where you are.")]
        public async Task Connect(CommandContext context, DiscordChannel channel = null)
        {
            var vnext = context.Client.GetVoiceNext();
            var connection = vnext?.GetConnection(context.Guild);
            if (connection != null)
            {
                await context.RespondAsync(":stop_sign: The bot is already connected.");
                return;
            }
            channel = channel ?? context.Member.VoiceState?.Channel;

            if (channel != null)
            {
                await channel.ConnectAsync();
                Program.channel2Read[context.Guild.Id] = context.Channel.Id;
                await context.RespondAsync($":loud_sound: The bot will read messages in #{context.Channel.Name}");
            } 
            else
            {
                await context.RespondAsync(":warning: You must be in a voice channel to connect the bot.");
            }
        }

        /// <summary>
        /// Command to disconnect the bot from a voice channel.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [Command("disconnect")]
        [Aliases("dc")]
        [Description("Disconnect this Bot from the voice channel.")]
        public async Task Disconnect(CommandContext context)
        {
            var vnext = context.Client.GetVoiceNext();
            var connection = vnext?.GetConnection(context.Guild);
            if (connection == null)
            {
                await context.RespondAsync(":stop_sign: The bot is not connected to any voice channels.");
            }
            else {
                connection.Disconnect();
                await context.RespondAsync("Good bye:wave:");
            }
        }

        [Command("usage")]
        public async Task Usage(CommandContext context)
        {
            var builder = new DiscordEmbedBuilder();
            builder.WithTitle("CeVIOよみあげの使い方");
            builder.WithColor(new DiscordColor("#e79dff"));
            builder.AddField("接続", "ボイスチャンネルに接続した状態で、`!connect`または`!c`");
            builder.AddField("さようなら", "`!disconnect`または`!dc`");
            builder.AddField("CeVIOキャストとできること", "`!config show`でCeVIOキャストの調整できるパラメーターが分かります");
            builder.AddField("キャストを変更したい", "`!config cast キャストの名前`． `!config av`で利用可能なキャストの名前をリストアップします");
            builder.AddField("パラメーターを変更するには？",
                             "`!config show`で変更したいパラメータの名前を探します。"
                             + "`!config NAME VALUE`のようにすると、指定したパラメーターを変更できます。\n"
                             + "例：`!config speed 70`");
            builder.AddField("もっと細かく調整したい",
                             "各キャストは固有の感情パラメーターを持っています（_components_といいます）。"
                             + "`!config show`でパラメーターの名前を探し、`!config comp NAME VALUE`のようにして値を変更できます。\n"
                             + "例：`!config comp angry 60`（さとうささら）");
            builder.AddField("セーブ", "キャスト/パラメーターを調整し終わったら`!config save`で保存してください。");

            await context.RespondAsync(embed: builder.Build());
        }

        /* English ver.
        [Command("usage")]
        public async Task Usage(CommandContext context)
        {
            var builder = new DiscordEmbedBuilder();
            builder.WithTitle("How to Use This CeVIO Bot");
            builder.WithColor(new DiscordColor("#e79dff"));
            builder.AddField("Connect", "Type `!connect` or `!c` after joining a voice channel.");
            builder.AddField("Bye-bye", "Type `!disconnect` or `!dc` to disconnect this bot.");
            builder.AddField("What you can do about your casts", "Type `!config show` to see what you can edit about CeVIO casts.");
            builder.AddField("How you can change your cast", "Type `!config cast [NAME]`. You can list available casts by just typing `!config av`");
            builder.AddField("How you can set some general parameters",
                             "Look for the parameter name by typing `!config show`. "
                             + "You can change values of the params by `!config NAME VALUE`. \n"
                             + "Example: `!config speed 70`");
            builder.AddField("Want to tune casts more finely?",
                             "Some params are specific to each cast (called _components_). You can get component names by "
                             + "`!config show` and change the values by `!config comp NAME VALUE`.\n"
                             + "Example: `!config comp angry 60` (for さとうささら)");

            await context.RespondAsync(embed: builder.Build());
        }
        */
    }
}
