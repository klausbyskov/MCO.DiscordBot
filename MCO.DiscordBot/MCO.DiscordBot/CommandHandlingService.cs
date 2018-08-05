using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MCO.DiscordBot
{
    public class CommandHandlingService
    {
        private readonly CommandService commandService;
        private readonly DiscordSocketClient discordSocketClient;
        private readonly IServiceProvider serviceProvider;
        private readonly StatsService statsService;

        public CommandHandlingService(IServiceProvider services)
        {
            commandService = services.GetRequiredService<CommandService>();
            discordSocketClient = services.GetRequiredService<DiscordSocketClient>();
            serviceProvider = services;
            statsService = new StatsService();

            discordSocketClient.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync()
        {
            await commandService.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            // Ignore system messages, or messages from other bots
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            if (message.Content == null) return;

            if (message.Content.Trim().StartsWith(">stats", StringComparison.InvariantCultureIgnoreCase)
               || message.Content.Trim().StartsWith(">mcostats", StringComparison.InvariantCultureIgnoreCase))
                await HandleStats(message);
        }

        private async Task HandleStats(SocketUserMessage message)
        {
            var imageBytes = await this.statsService.GetStatsImageAsync();
            var context = new SocketCommandContext(discordSocketClient, message);

            using (var memStream = new MemoryStream(imageBytes))
            {
                await context.Channel.SendFileAsync(memStream, "MCO-stats.png");
            }
        }
    }
}
