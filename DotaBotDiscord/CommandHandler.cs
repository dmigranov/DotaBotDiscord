using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using OpenDotaDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace DotaBotDiscord
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;
        private readonly OpenDotaApi _openDota;
        private Dictionary<long, OpenDotaDotNet.Models.Heroes.Hero> heroesMap = null;


        public CommandHandler(DiscordSocketClient client, CommandService commands, InteractiveService interactiveService, OpenDotaApi openDota)
        {
            _commandService = commands;
            _client = client;
            _openDota = openDota;

        }

        public async Task InstallCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;

            // Here we discover all of the command modules in the entry 
            // assembly and load them. Starting from Discord.NET 2.0, a
            // service provider is required to be passed into the
            // module registration method to inject the 
            // required dependencies.
            //
            // If you do not use Dependency Injection, pass null.
            // See Dependency Injection guide for more information.

            await _commandService.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);

            PublicModule._commandService = _commandService;
            PublicModule._openDota = _openDota;

            /*await _commandService.CreateModuleAsync("HelpModule", builder => {
                builder.AddCommand("help", null, async commandBuilder => { commandBuilder. });
            });

            foreach (ModuleInfo module in _commandService.Modules)
            {
                System.Console.WriteLine(module.Name);
            }*/




            List<OpenDotaDotNet.Models.Heroes.Hero> heroes =  await _openDota.Hero.GetHeroesAsync();
            heroesMap = heroes.ToDictionary(x => x.Id, x => x);
            PublicModule.heroes = heroesMap;


        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasCharPrefix('!', ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(_client, message);

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.

            // Keep in mind that result does not indicate a return value
            // rather an object stating if the command executed successfully.
            var result = await _commandService.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: null);

            // Optionally, we may inform the user if the command fails
            // to be executed; however, this may not always be desired,
            // as it may clog up the request queue should a user spam a
            // command.
            // if (!result.IsSuccess)
            // await context.Channel.SendMessageAsync(result.ErrorReason);
        }
    }
}
