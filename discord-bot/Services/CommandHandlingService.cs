using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using discord_bot.Helpers;

using Microsoft.Extensions.DependencyInjection;

namespace discord_bot.Services
{
    public class CommandHandlingService
    {
        private const    string              Prefix = "!";
        private readonly CommandService      _commands;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider    _services;

        public CommandHandlingService(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _discord  = services.GetRequiredService<DiscordSocketClient>();
            _services = services;

            _commands.CommandExecuted += CommandExecutedAsync;

            _discord.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync()
        {
            // Register modules that are public and inherit ModuleBase<T>.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            if (rawMessage is not SocketUserMessage { Source: MessageSource.User } message) return;

            var argPos = 0;

            if (!message.HasMentionPrefix(_discord.CurrentUser, ref argPos)
             && !message.HasStringPrefix(Prefix, ref argPos)) return;

            var context = new SocketCommandContext(_discord, message);

            await _commands.ExecuteAsync(context, argPos, _services);
        }

        private async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            await context.Message.DeleteAsync();

            if (result.IsSuccess)
                return;

            var embed = new EmbedBuilder();
            embed.WithColor(Color.Red);
            embed.WithFooter(context.User.Username, context.User.GetAvatarUrl());
            embed.WithCurrentTimestamp();

            if (!command.IsSpecified)
            {
                embed.WithTitle("Comando não encontrado");

                var commands = new List<string>();
                foreach (var commandInfo in _commands.Commands) commands.AddRange(commandInfo.Aliases);


                var message = context.Message.Content[Prefix.Length..];

                if (message.Length == 0) return;

                var cmd = message.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];

                var list = commands.Where(botCommand => Levenshtein.Compare(botCommand, cmd) >= 125).ToList();

                var length = list.Count;

                switch (length)
                {
                    case 0:
                        embed.WithDescription($"O comando ``{cmd}`` não existe. Use **!ajuda** para ver a lista de comandos.");
                        await context.Channel.SendMessageAsync(embed: embed.Build());

                        return;
                    case 1:
                        message = $"O comando ``{cmd}`` não existe. Sugestão: '{list[0]}'";

                        break;
                    default:
                        message =  $"O comando ``{cmd}`` não existe. Comandos similares: ";
                        message += string.Join(", ", list);

                        break;
                }

                embed.WithDescription(message);
                await context.Channel.SendMessageAsync(embed: embed.Build());

                return;
            }


            if (result.Error is CommandError.BadArgCount or CommandError.ParseFailed)
            {
                embed.WithTitle("Sintaxe Inválida");

                embed.Description = $"Modo de uso: \n**{command.Value.Name}** ";

                var types = "";

                foreach (var parameter in command.Value.Parameters)
                {
                    embed.Description += $"{parameter.Name} ";
                    types             += $"{parameter.Name} = {parameter.Type.Name}\n";
                }

                embed.AddField("Tipos", types);


                await context.Channel.SendMessageAsync(embed: embed.Build());
            }

            else
            {
                embed.WithTitle(result.Error.ToString());
                embed.WithDescription(result.ErrorReason);

                await context.Channel.SendMessageAsync(embed: embed.Build());
            }
        }
    }
}