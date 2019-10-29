using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace CyberUpBot
{

    // Keep in mind your module **must** be public and inherit ModuleBase.
    // If it isn't, it will not be discovered by AddModulesAsync!
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        [Alias("pong", "hello")]
        [Summary("Says pong.")]
        public Task PingAsync()
            => ReplyAsync("pong!");


        // Get info on a user, or the user who invoked the command if one is not specified
        [Command("userinfo")]
        public async Task UserInfoAsync(IUser user = null)
        {
            user = user ?? Context.User;

            await ReplyAsync(user.ToString());
        }

        // Ban a user
        [Command("ban")]
        [RequireContext(ContextType.Guild)]
        // make sure the user invoking the command can ban
        [RequireUserPermission(GuildPermission.BanMembers)]
        // make sure the bot itself can ban
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanUserAsync(IGuildUser user, [Remainder] string reason = null)
        {
            await user.Guild.AddBanAsync(user, reason: reason);
            await ReplyAsync("ok!");
        }

        // [Remainder] takes the rest of the command's arguments as one argument, rather than splitting every space
        [Command("echo")]
        public Task EchoAsync([Remainder] string text)
            // Insert a ZWSP before the text to prevent triggering other bots!
            => ReplyAsync('\u200B' + text);

        // 'params' will parse space-separated elements into a list
        [Command("list")]
        public Task ListAsync(params string[] objects)
            => ReplyAsync("You listed: " + string.Join("; ", objects));

        // Setting a custom ErrorMessage property will help clarify the precondition error
        [Command("guild_only")]
        [RequireContext(ContextType.Guild, ErrorMessage = "Sorry, this command must be ran from within a server, not a DM!")]
        public Task GuildOnlyCommand()
            => ReplyAsync("Nothing to see here!");



        [Command("help")]
        public async Task Help()
        {
            

            await ReplyAsync("Here's a list of commands and their description: ");
        }

        [Command("add_steamid")]
        public async Task AdSteamID()
        {
            

            await ReplyAsync("Here's a list of commands and their description: ");
        }
    }
}
