using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace discord_bot.Modules
{
    public class PingCommand : ModuleBase<SocketCommandContext>
    {
        [Command("ping", true, RunMode = RunMode.Async)]
        [Summary("Retorna a latência atual.")]
        public async Task PingAsync()
        {
            var embed = new EmbedBuilder();
            embed.WithColor(185, 94, 255);
            embed.WithTitle("Teste de Latência :ping_pong:");
            embed.WithDescription("_Calculando..._");
            embed.WithFooter(Context.User.Username, Context.User.GetAvatarUrl());
            embed.WithCurrentTimestamp();

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var message = await ReplyAsync(embed: embed.Build());
            stopWatch.Stop();

            embed.AddField(":robot: Bot", $"{stopWatch.ElapsedMilliseconds} ms", true);
            embed.AddField(":desktop: Gateway", $"{Context.Client.Latency} ms", true);
            embed.Description = null;

            await message.ModifyAsync(msg => { msg.Embed = embed.Build(); });
        }
    }
}