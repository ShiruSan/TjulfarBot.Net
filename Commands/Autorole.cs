using System.Linq;
using Discord;
using TjulfarBot.Net.Managers;

namespace TjulfarBot.Net.Commands
{
    public class Autorole : Command
    {
        public Autorole() : base("autorole", GuildPermission.ManageRoles)
        {
        }

        public override void OnCommand(CommandContext ctx)
        {
            if (ctx.Arguments.Length == 1 && ctx.Message.MentionedRoles.Count == 1)
            {
                var autorole = ctx.Message.MentionedRoles.First();
                if (AutoroleManager.Get().IsAutorole(autorole.Id))
                {
                    AutoroleManager.Get().RemoveAutorole(autorole.Id);
                    ctx.Channel.SendMessageAsync(
                            ":white_check_mark: Diese Rolle wird nun nicht mehr jedem neuem Mitglied gegeben !", false, null, null,
                            null, new MessageReference(ctx.Message.Id, ctx.Channel.Id, ctx.Guild.Id)).GetAwaiter()
                        .GetResult();
                }
                else
                {
                    AutoroleManager.Get().AddAutorole(autorole.Id);
                    ctx.Channel.SendMessageAsync(
                            ":white_check_mark: Diese Rolle wird nun jedem neuem Mitglied gegeben !", false, null, null,
                            null, new MessageReference(ctx.Message.Id, ctx.Channel.Id, ctx.Guild.Id)).GetAwaiter()
                        .GetResult();
                }
            }
            else
            {
                var builder = new EmbedBuilder();
                builder.WithTitle("Autorole Command").WithColor(Color.Blue);
                builder.WithDescription(
                    "Der Level Command ist dafür da um eine Rolle automatisch jemanden geben zu lassen, der den Server gerade betretten hat.");
                builder.WithFields(new[]
                {
                    new EmbedFieldBuilder().WithName("Nutzung des Commands:").WithValue("`+autorole @Rolle`")
                        .WithIsInline(false)
                });
                ctx.Channel.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
            }
        }
        
    }
}