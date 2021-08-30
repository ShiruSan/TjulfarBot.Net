using Discord;
using TjulfarBot.Net.Commands;
using TjulfarBot.Net.Managers;

namespace TjulfarBot.Net.Economy
{
    public class EcoCommand : Command
    {
        
        public EcoCommand() : base("eco", null)
        {
        }

        public override void OnCommand(CommandContext ctx)
        {
            if (ctx.Arguments.Length == 0)
            {
                var builder = new EmbedBuilder();
                builder.WithTitle("eco Command").WithColor(Color.Blue);
                builder.WithDescription("Mit diesem Command kannst du dein Taschengeld und Bankkonto abrufen und deine :penny: verwalten.");
                builder.WithFields(new[]
                {
                    new EmbedFieldBuilder().WithName("Nutzung des Commands:").WithValue("`+eco money` Siehe wie viele :penny: du in deiner Tasche hast\n" +
                        "`+eco money give @Mitglied [Wert]` Gebe anderen Mitgliedern etwas von deinen :penny:\n" +
                        "`+eco daily` Bekomme täglich vom Bot 100 :penny: geschenkt\n" +
                        "`+eco bank` Siehe deine :penny: auf deinem Bankkonto\n" +
                        "`+eco money transfer [Wert]` Zahle :penny: auf dein Bankkonto ein\n" +
                        "`+eco bank transfer [Wert]` Hebe :penny: von deinem Bankkonto ab")
                });
            }
        }
        
    }
}