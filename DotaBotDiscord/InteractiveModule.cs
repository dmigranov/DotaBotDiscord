using System;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;

namespace DotaBotDiscord
{
    public class InteractiveModule : InteractiveBase
    {

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
            //var channel = await user.GetOrCreateDMChannelAsync() as IPrivateChannel;
            /*if (channel == null)
                return;*/

            var channel = Context.Channel as IDMChannel;
            if (channel == null)
                return;


            await user.SendMessageAsync("Здравствуйте! Давайте зарегистрируем Вас в системе. Введите, пожалуйста, Ваш SteamID:");

            var response = await NextMessageAsync();

            if (response != null)
            {
                long steamID = long.Parse(response.Content);
                await ReplyAsync(steamID.ToString());
            }
            else
                await ReplyAsync("You did not reply before the timeout");
        }

    }
}