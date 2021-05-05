using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace discord_bot.Modules
{
    public class HelpCommand : ModuleBase<SocketCommandContext>
    {
        public CommandService CommandService { get; set; }

        [Command("help", true, RunMode = RunMode.Async)]
        [Alias("ajuda", "comandos", "commands", "h")]
        [Summary("Mostra essa mensagem.")]
        public async Task Help()
        {
            var commands = CommandService.Commands.ToList();
            var embedBuilder = new EmbedBuilder();

            embedBuilder.WithTitle("Lista de Comandos");
            embedBuilder.WithColor(185, 94, 255);
            embedBuilder.WithFooter(Context.User.Username, Context.User.GetAvatarUrl());
            embedBuilder.WithCurrentTimestamp();

            foreach (var command in commands)
            {
                var embedFieldText = command.Summary ?? "Sem descrição";
                if (command.Aliases.Count > 1)
                    embedFieldText += "\nAliases: " + string.Join(", ",
                        command.Aliases.ToList().GetRange(1, command.Aliases.Count - 1));

                embedBuilder.AddField(command.Name, embedFieldText);
            }

            await ReplyAsync(embed: embedBuilder.Build());
        }
    }
}