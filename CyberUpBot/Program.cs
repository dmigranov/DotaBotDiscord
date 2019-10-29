using Discord;
using Discord.WebSocket;
using System;

using System.Threading.Tasks;

namespace DiscordBot
{
    public class Program
    {
        private DiscordSocketClient _client;

        public static void Main(string[] args)
            => new Program().MainAsync(args[0]).GetAwaiter().GetResult();

        public async Task MainAsync(string token)
        {
            _client = new DiscordSocketClient();

            _client.Log += Log;
            _client.MessageReceived += MessageReceived;

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private async Task MessageReceived(SocketMessage message)
        {
            if (message.Content == "!ping")
            {
                await message.Channel.SendMessageAsync("Pong!");
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
