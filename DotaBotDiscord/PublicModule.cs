using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using OpenDotaDotNet;
using OpenDotaDotNet.Dtos;

namespace DotaBotDiscord
{

    // Keep in mind your module **must** be public and inherit ModuleBase.
    // If it isn't, it will not be discovered by AddModulesAsync!
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        public static CommandService _commandService { get; set; }
        public static OpenDotaApi _openDota { get; set; }
        public static Dictionary<long, OpenDotaDotNet.Models.Heroes.Hero> heroes { get; set; }


        [Command("ping")]
        [Alias("pong", "hello")]
        [Summary("Отправляет pong в ответ")]
        public Task PingAsync()
            => ReplyAsync("pong!");


        // Get info on a user, or the user who invoked the command if one is not specified
        [Command("userinfo")]
        [Summary("Отправляет информацию о аккаунте Discord в ответ")]
        public async Task UserInfoAsync(IUser user = null)
        {
            user = user ?? Context.User;
            
            await ReplyAsync($"{user.Mention}: {user.ToString()}");
        }

        // Ban a user
        /*
        
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

        */


        // [Remainder] takes the rest of the command's arguments as one argument, rather than splitting every space
        [Command("echo")]
        [Summary("Повторяет сообщение, отправленное аргументом")]
        public Task EchoAsync([Remainder] string text)
            // Insert a ZWSP before the text to prevent triggering other bots!
            => ReplyAsync('\u200B' + text);

        // 'params' will parse space-separated elements into a list
        /*
        [Command("list")]
        public Task ListAsync(params string[] objects)
            => ReplyAsync("You listed: " + string.Join("; ", objects));
        */

        // Setting a custom ErrorMessage property will help clarify the precondition error
        /*
        [Command("guild_only")]
        [RequireContext(ContextType.Guild, ErrorMessage = "Sorry, this command must be ran from within a server, not a DM!")]
        public Task GuildOnlyCommand()
            => ReplyAsync("Nothing to see here!");
        */


        [Command("help")]
        [Summary("Вывод справки по командам")]
        public async Task Help()
        {
            List<CommandInfo> commands = _commandService.Commands.ToList();
            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.Title = "Список команд";

            foreach (CommandInfo command in commands)
            {
                string embedFieldText = command.Summary ?? "Без описания\n";

                embedBuilder.AddField(command.Name, embedFieldText);
            }

            await ReplyAsync("Вот список всех команд с описанием; все команды вводятся начиная с !: ", false, embedBuilder.Build());
        }

        [Summary("Вывод информации о профиле Стим по Steam32 ID ")]
        [Command("checkID", RunMode = RunMode.Async)]
        public async Task AddSteamID(long playerID_32)
        {
            var playerInfo = await _openDota.Player.GetPlayerByIdAsync(playerID_32);

            EmbedBuilder embedBuilder = new EmbedBuilder();

            embedBuilder.AddField("Имя в Стиме:", playerInfo.Profile.Personaname);
            embedBuilder.AddField("Ссылка на профиль", playerInfo.Profile.Profileurl);
            embedBuilder.AddField("MMR:", playerInfo.MmrEstimate.Estimate.HasValue ? playerInfo.MmrEstimate.Estimate.ToString() : "нет");
            //MMR может быть не актуален: add MMR to your profile card. 

            var playerWinLoss = await _openDota.Player.GetPlayerWinLossByIdAsync(playerID_32);
            embedBuilder.AddField("Всего игр сыграно:", playerWinLoss.Wins + playerWinLoss.Losses);
            embedBuilder.AddField("Побед:", playerWinLoss.Wins);
            embedBuilder.AddField("Поражений:", playerWinLoss.Losses);
            embedBuilder.WithThumbnailUrl(playerInfo.Profile.Avatarfull.ToString());
            var playerQueryParameters = new PlayerEndpointParameters
            {
                Limit = 20
            };
            var playerHeroes = await _openDota.Player.GetPlayerHeroesAsync(playerID_32, playerQueryParameters);

            var playerMostPlayedHeroLast20 = playerHeroes.FirstOrDefault();
            var hero = heroes[playerMostPlayedHeroLast20.HeroId];
            embedBuilder.AddField("Самый популярный герой за последние 20 матчей:", playerMostPlayedHeroLast20 != null ? $"{hero.LocalizedName} ({string.Join("; ", hero.Roles)}) с {playerMostPlayedHeroLast20.Win} победами" : "нет информации");
            await ReplyAsync("Информация об игроке: ", false, embedBuilder.Build());
        }


      

    }

}
