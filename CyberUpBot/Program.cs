using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using OpenDotaDotNet;
using OpenDotaDotNet.Dtos;

namespace CyberUpBot
{
    public class Program
    {
        private DiscordSocketClient _client;
        private OpenDotaApi openDota;

        public static void Main(string[] args)
            => new Program().MainAsync(args[0]).GetAwaiter().GetResult();

        public async Task MainAsync(string token)
        {
            using (CommandService service = new CommandService())
            {
                _client = new DiscordSocketClient();
                openDota = OpenDotaApi.GetInstance();

                _client.Log += Log;
                CommandHandler handler = new CommandHandler(_client, service);
                await handler.InstallCommandsAsync();
                await _client.LoginAsync(TokenType.Bot, token);
                await _client.StartAsync();

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
