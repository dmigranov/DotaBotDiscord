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


        // NextMessageAsync will wait for the next message to come in over the gateway, given certain criteria
        // By default, this will be limited to messages from the source user in the source channel
        // This method will block the gateway, so it should be ran in async mode.
        [Command("next", RunMode = RunMode.Async)]
        public async Task Test_NextMessageAsync()
        {
            await ReplyAsync("What is 2+2?");
            var response = await NextMessageAsync();
            if (response != null)
                await ReplyAsync($"You replied: {response.Content}");
            else
                await ReplyAsync("Прошло слишком много времени. Начните регистрацию заново.");
        }


        [Command("register", RunMode = RunMode.Async)]
        [Summary("Заполнение анкеты (пишите в личные сообщения боту)")]
        public async Task Register()
        {
            var user = Context.User;

            var channel = Context.Channel as IDMChannel;
            if (channel == null)
                return;

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
                    if(ynResponce != null)
                    {
                        var answer = ynResponce.Content;
                        answer = answer.ToLower();
                        var first = answer[0];
                        if (first == 'д')
                        {
                            using (var db = new LiteDatabase(@"BotData.db"))
                            {

                                var users = db.GetCollection<UserSteamAccount>("users");
                                Console.WriteLine($"{users.Count()}");

                                var userSteamAccount = new UserSteamAccount
                                {
                                    DiscordID = user.Id,
                                    SteamID = steamID
                                };

                                //todo: вставка без повторений

                                users.Insert(userSteamAccount);


                                users.EnsureIndex(x => x.DiscordID);
                            }

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

    public class UserSteamAccount
    {
        public int Id { get; set; }
        public ulong DiscordID { get; set; }
        public long SteamID { get; set; }
    }
}