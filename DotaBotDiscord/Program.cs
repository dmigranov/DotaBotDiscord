using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using OpenDotaDotNet;
using OpenDotaDotNet.Dtos;
using Discord.Addons.Interactive;
using Microsoft.Extensions.DependencyInjection;

namespace DotaBotDiscord
{
    public class Program
    {
        private DiscordSocketClient _client;
        private OpenDotaApi openDota;

        public static void Main(string[] args)
            => new Program().MainAsync(args[0]).GetAwaiter().GetResult();

        public async Task MainAsync(string token)
        {
            using (CommandService commandService = new CommandService())
            {
                _client = new DiscordSocketClient();
                openDota = OpenDotaApi.GetInstance();

                _client.Log += Log;

                await _client.LoginAsync(TokenType.Bot, token);
                await _client.StartAsync();

                var services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton<InteractiveService>()
                .BuildServiceProvider();

                CommandHandler handler = new CommandHandler(_client, commandService, services, openDota);
                await handler.InstallCommandsAsync();

                // Block this task until the program is closed.
                await Task.Delay(-1);
            }
            
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

    }
}
