using System;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using LiteDB;
using OpenDotaDotNet;

namespace DotaBotDiscord
{
    public class InteractiveModule : InteractiveBase
    {
        public static OpenDotaApi _openDota { get; set; }

        [Command("register", RunMode = RunMode.Async)]
        [Alias("signup", "su")]
        [Summary("Заполнение анкеты (пишите в личные сообщения боту)")]
        public async Task Register()
        {
            var user = Context.User;

            var channel = Context.Channel as IDMChannel;
            if (channel == null)
                return;

            using (var db = new LiteDatabase(@"BotData.db"))
            {
                //db.DropCollection("users");
                var users = db.GetCollection<UserSteamAccount>("users");
                UserSteamAccount existingUser = users.FindOne(x => x.DiscordID == user.Id);

                if(existingUser != null)
                {
                    await ReplyAsync("Такой аккаунт уже есть! Вы можете разрегистрироваться и пройти регистрацию ещё раз.");
                    return;
                }

                await user.SendMessageAsync("Здравствуйте! Давайте зарегистрируем Вас в системе. Введите, пожалуйста, Ваш Steam32 ID:");
            
            ParseResponse:

                var response = await NextMessageAsync();
                if (response != null)
                {
                    long steamID;
                    if (long.TryParse(response.Content, out steamID))
                    {
                        var playerInfo = await _openDota.Player.GetPlayerByIdAsync(steamID);
                        if (playerInfo == null || playerInfo.Profile == null)
                        {
                            await ReplyAsync("Профиль не найден, попробуйте ввести Steam32ID снова");
                            goto ParseResponse;
                        }
                        EmbedBuilder embedBuilder = new EmbedBuilder();
                        embedBuilder.AddField("Имя в Стиме:", playerInfo.Profile.Personaname);
                        embedBuilder.AddField("Ссылка на профиль", playerInfo.Profile.Profileurl);
                        embedBuilder.WithThumbnailUrl(playerInfo.Profile.Avatarfull.ToString());
                        await ReplyAsync("Это ваш профиль? д/н", false, embedBuilder.Build());
                        var ynResponce = await NextMessageAsync();
                        if (ynResponce != null)
                        {
                            var answer = ynResponce.Content;
                            answer = answer.ToLower();
                            var first = answer[0];
                            if (first == 'д')
                            {
                                var userSteamAccount = new UserSteamAccount
                                {
                                    DiscordID = user.Id,
                                    SteamID = steamID
                                };

                                users.Insert(userSteamAccount);
                                users.EnsureIndex(x => x.DiscordID);

                                await ReplyAsync("Вы были успешно зарегистрированы!");
                            }
                            else if (first == 'н')
                            {
                                await ReplyAsync("Попробуйте ввести Steam32ID снова");
                                goto ParseResponse;
                            }
                        }
                        else
                            await ReplyAsync("Прошло слишком много времени. Начните регистрацию заново.");
                    }
                    else
                    {
                        await ReplyAsync("Неправильный ввод, попробуйте ввести Steam32ID снова");
                        goto ParseResponse;
                    }

                }
                else
                    await ReplyAsync("Прошло слишком много времени. Начните регистрацию заново.");
            }
        }


        [Command("unregister", RunMode = RunMode.Async)]
        [Alias("signout", "signoff", "so")]
        [Summary("Удаление анкеты с сервера")]
        public async Task Unregister()
        {
            var channel = Context.Channel as IDMChannel;
            if (channel == null)
                return;

            IUser user = Context.User;
            using var db = new LiteDatabase(@"BotData.db");
            var users = db.GetCollection<UserSteamAccount>("users");

            if (!users.Exists(x => x.DiscordID == user.Id))
                await ReplyAsync("Вы не зарегистрированы в системе.");
            else
            {
                users.Delete(x => x.DiscordID == user.Id);
                await ReplyAsync("Вы были успешно разрегистрированы.");
            }
        }
    }

    public class UserSteamAccount
    {
        public int Id { get; set; }
        public ulong DiscordID { get; set; }
        public long SteamID { get; set; }
    }
}