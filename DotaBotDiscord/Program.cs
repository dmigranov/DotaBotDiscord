using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using OpenDotaDotNet;
using OpenDotaDotNet.Dtos;
using Discord.Addons.Interactive;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DotaBotDiscord
{
    /*public class Program
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

    }*/

    class Program
    {
        public static void Main(string[] args)
                    => new Program().MainAsync(args[0]).GetAwaiter().GetResult();

        private DiscordSocketClient client;
        private CommandService commands;
        private IServiceProvider services;

        public async Task MainAsync(string token)
        {
            client = new DiscordSocketClient();

            client.Log += log =>
            {
                Console.WriteLine(log.ToString());
                return Task.CompletedTask;
            };

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            services = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton<InteractiveService>()
                .BuildServiceProvider();

            commands = new CommandService();
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);

            client.MessageReceived += HandleCommandAsync;

            await Task.Delay(-1);
        }

        public async Task HandleCommandAsync(SocketMessage m)
        {
            if (!(m is SocketUserMessage msg)) return;
            if (msg.Author.IsBot) return;

            int argPos = 0;
            if (!(msg.HasStringPrefix("i~>", ref argPos))) return;

            var context = new SocketCommandContext(client, msg);
            await commands.ExecuteAsync(context, argPos, services);
        }
    }
}
