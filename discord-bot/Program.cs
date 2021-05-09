using System;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using discord_bot.Services;

using Microsoft.Extensions.DependencyInjection;

namespace discord_bot
{
    internal class Program
    {
        private static void Main()
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        private async Task MainAsync()
        {
            await using var services = ConfigureServices();

            var client = services.GetRequiredService<DiscordSocketClient>();

            client.Log                                        += LogAsync;
            services.GetRequiredService<CommandService>().Log += LogAsync;


            var token = "";
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

            await Task.Delay(Timeout.Infinite);
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());

            return Task.CompletedTask;
        }

        private static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection().AddSingleton<DiscordSocketClient>()
                                          .AddSingleton<CommandService>()
                                          .AddSingleton<CommandHandlingService>()
                                          .BuildServiceProvider();
        }
    }
}