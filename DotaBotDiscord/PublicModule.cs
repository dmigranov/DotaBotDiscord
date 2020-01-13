using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using OpenDotaDotNet;
using OpenDotaDotNet.Dtos;
using LiteDB;
using System.Globalization;

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

        // [Remainder] takes the rest of the command's arguments as one argument, rather than splitting every space
        [Command("echo")]
        [Summary("Повторяет сообщение, отправленное аргументом")]
        public Task EchoAsync([Remainder] string text)
            // Insert a ZWSP before the text to prevent triggering other bots!
            => ReplyAsync('\u200B' + text);


        [Command("help")]
        [Summary("Вывод справки по командам")]
        public async Task Help()
        {
            List<CommandInfo> commands = _commandService.Commands.ToList();
            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.Title = "Список команд";
            embedBuilder.Color = Color.Green;

            foreach (CommandInfo command in commands)
            {
                string embedFieldText = command.Summary ?? "Без описания\n";

                embedBuilder.AddField(command.Name, embedFieldText);
            }

            await ReplyAsync("Вот список всех команд с описанием; все команды вводятся начиная с !: ", false, embedBuilder.Build());
        }

        [Summary("Вывод информации о профиле Стим по Steam32 ID ")]
        [Command("check_steam", RunMode = RunMode.Async)]
        public async Task AddSteamID(long playerID_32)
        {
            var emoji = new Emoji("\uD83D\uDC4C");

            var msg = await ReplyAsync("Информация об игроке: ", false, await BuildUserStatsEmbedAsync(playerID_32));
            await msg.AddReactionAsync(emoji);

        }


        [Summary("Вывод информации о профиле Стим и игровой статистики по юзеру. Если юзер не указан, то об авторе сообщения")]
        [Command("get_stats", RunMode = RunMode.Async)]
        public async Task GetUserStats(IUser user = null)
        {
            user = user ?? Context.User;

            using var db = new LiteDatabase(@"BotData.db");
            var users = db.GetCollection<UserSteamAccount>("users");

            UserSteamAccount userSteamAccount = users.FindOne(x => x.DiscordID == user.Id);

            if (user == null)
                await ReplyAsync("Такого аккаунта нет в системе.");
            else
            {
                var emoji = new Emoji("\uD83D\uDC4C");

                var msg = await ReplyAsync("Информация об игроке: ", false, await BuildUserStatsEmbedAsync(userSteamAccount.SteamID));
                await msg.AddReactionAsync(emoji);
            }
        }


        [Summary("Вывод дополнительной информации о профиле Стим и игровой статистики по юзеру. Если юзер не указан, то об авторе сообщения")]
        [Command("get_stats_extra", RunMode = RunMode.Async)]
        public async Task GetUserStatsExtra(IUser user = null)
        {
            user = user ?? Context.User;

            using var db = new LiteDatabase(@"BotData.db");
            var users = db.GetCollection<UserSteamAccount>("users");

            UserSteamAccount userSteamAccount = users.FindOne(x => x.DiscordID == user.Id);

            if (user == null)
                await ReplyAsync("Такого аккаунта нет в системе.");
            else
            {
                var emoji = new Emoji("\uD83D\uDC4C");

                var msg = await ReplyAsync("Информация об игроке: ", false, await BuildUserStatsExtraEmbedAsync(userSteamAccount.SteamID));
                await msg.AddReactionAsync(emoji);
            }
        }

        private async Task<Embed> BuildUserStatsEmbedAsync(long playerID_32)
        {
            var playerInfo = await _openDota.Player.GetPlayerByIdAsync(playerID_32);
            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.Color = Color.Green;

            embedBuilder.AddField("Имя в Стиме:", playerInfo.Profile.Personaname, true);
            embedBuilder.AddField("Последний раз в сети:", playerInfo.Profile.LastLogin.HasValue ? playerInfo.Profile.LastLogin?.ToString("dd.mm.yyyy, HH:mm", CultureInfo.InvariantCulture) : "неизвестно", true);
            embedBuilder.AddField("Ссылка на профиль", playerInfo.Profile.Profileurl);
            embedBuilder.AddField("Ссылка на OpenDota: ", $"https://www.opendota.com/players/{playerID_32}");
            embedBuilder.AddField("MMR:", playerInfo.MmrEstimate.Estimate.HasValue ? playerInfo.MmrEstimate.Estimate.ToString() : "нет", true);
            //MMR может быть не актуален: add MMR to your profile card. 
            embedBuilder.AddField("Ранг: ", playerInfo.LeaderboardRank.HasValue ? playerInfo.LeaderboardRank.ToString() : "нет", true);
            embedBuilder.AddField("Страна: ", playerInfo.Profile.Loccountrycode != null ? GetFlag(playerInfo.Profile.Loccountrycode) : "неизвестно");

            embedBuilder.WithTimestamp(DateTimeOffset.Now);

            var playerWinLoss = await _openDota.Player.GetPlayerWinLossByIdAsync(playerID_32);

            int matches = playerWinLoss.Wins + playerWinLoss.Losses;
            embedBuilder.AddField("Всего игр сыграно:", matches);

            embedBuilder.AddField("Побед:", playerWinLoss.Wins, true);
            embedBuilder.AddField("Поражений:", playerWinLoss.Losses, true);
            embedBuilder.AddField("Винрейт:", matches != 0 ? ((double)playerWinLoss.Wins / matches).ToString("0.##") : "неизвестно", true);
            embedBuilder.WithThumbnailUrl(playerInfo.Profile.Avatarfull.ToString());
            var playerQueryParameters = new PlayerEndpointParameters
            {
                Limit = 20
            };
            var playerHeroes = await _openDota.Player.GetPlayerHeroesAsync(playerID_32, playerQueryParameters);

            var playerMostPlayedHeroLast20 = playerHeroes.FirstOrDefault();
            var hero = heroes[playerMostPlayedHeroLast20.HeroId];
            embedBuilder.AddField("Самый популярный герой за последние 20 матчей:", playerMostPlayedHeroLast20 != null ? $"{hero.LocalizedName} ({string.Join("; ", hero.Roles)}) с {playerMostPlayedHeroLast20.Win} победами" : "нет информации");
            embedBuilder.WithFooter(new EmbedFooterBuilder().WithText("Чтобы получить больше информации, воспользуйтесь командой !get_stats_extra"));

            return embedBuilder.Build();
        }

        private async Task<Embed> BuildUserStatsExtraEmbedAsync(long playerID_32)
        {
            var playerInfo = await _openDota.Player.GetPlayerByIdAsync(playerID_32);
            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.Color = Color.Green;

            embedBuilder.AddField("Имя в Стиме:", playerInfo.Profile.Personaname, true);
            embedBuilder.AddField("Последний раз в сети:", playerInfo.Profile.LastLogin.HasValue ? playerInfo.Profile.LastLogin?.ToString("dd.mm.yyyy, HH:mm", CultureInfo.InvariantCulture) : "неизвестно", true);
            embedBuilder.AddField("Ссылка на профиль", playerInfo.Profile.Profileurl);

            var param = new PlayerEndpointParameters();

            var playerTotals = await _openDota.Player.GetPlayerTotalsAsync(playerID_32);
            
            for (int i = 0; i < playerTotals.Count - 1; i++)
            {
                OpenDotaDotNet.Models.Players.PlayerTotal playerTotal = playerTotals[i];
                Console.WriteLine(playerTotal.Field + " " + playerTotal?.Sum);
                //embedBuilder.AddField(playerTotal.Field, playerTotal.Sum.ToString(), true);
                switch(playerTotal.Field)
                {
                    case "kills":
                        embedBuilder.AddField("Убийств:", playerTotal.Sum.ToString(), true);
                        break;
                    case "deaths":
                        embedBuilder.AddField("Смертей:", playerTotal.Sum.ToString(), true);
                        break;
                    case "assists":
                        embedBuilder.AddField("Помощи:", playerTotal.Sum.ToString(), true);
                        break;
                    case "duration":
                        embedBuilder.AddField("Играет уже:", 
                            new DateTime(TimeSpan.FromSeconds(playerTotal.Sum).Ticks).ToString("ddд hhч mmм ssс"), true);
                        break;
                }
            }


            embedBuilder.WithTimestamp(DateTimeOffset.Now);

            embedBuilder.WithThumbnailUrl(playerInfo.Profile.Avatarfull.ToString());

            embedBuilder.WithFooter(new EmbedFooterBuilder().WithText("Чтобы получить основную информацию, воспользуйтесь командой !get_stats"));

            return embedBuilder.Build();
        }

        private string GetFlag(string country)
        {
            int flagOffset = 0x1F1E6;
            int asciiOffset = 0x41;


            int firstChar = country[0] - asciiOffset + flagOffset;
            int secondChar = country[1] - asciiOffset + flagOffset;

            return new string(char.ConvertFromUtf32(firstChar))
                        + new string(char.ConvertFromUtf32(secondChar));
        }





    }
}
