using Discord.Addons.Interactive;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotaBotDiscord
{
    class InteractiveModule : InteractiveBase
    {
        [Command("register")]
        [Summary("Заполнение анкеты")]
        public async Task Register()
        {
            var user = Context.User;
            var channel = await user.GetOrCreateDMChannelAsync();
            await channel.SendMessageAsync("Здравствуйте! Давайте зарегистрируем Вас в системе. Введите, пожалуйста, Ваш SteamID:");

            //var msg = await channel.GetMessagesAsync();


            Console.WriteLine("Hahahahhaaa");
        }


    }
}
