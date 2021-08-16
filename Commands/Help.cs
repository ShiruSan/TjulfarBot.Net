using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using TjulfarBot.Net.Managers;

namespace TjulfarBot.Net.Commands
{
    public class Help : Command
    {
        public Help() : base("help")
        {
        }

        public override void OnCommand(CommandContext ctx)
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle("\u00dcber den Tjulfar Bot");
            builder.WithThumbnailUrl(ctx.SelfUser.GetAvatarUrl());
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Dieser Discord Bot ist, wie der Name schon sagt, f\u00FCr Tjulfars Discord Server.");
            stringBuilder.AppendLine("Der Bot wurde als kleines Fanprojekt von <@370553641263955970> gestartet und wird weiterentwickelt !");
            stringBuilder.AppendLine("Der Geburtstag dieses Botes ist der 29.03.2021.");
            stringBuilder.Append("Au\u00DFerdem findest du hier eine Auflistung aller Commands.");
            builder.WithDescription(stringBuilder.ToString());
            builder.WithColor(Color.Green);
            builder.WithFooter("Du m\u00F6chtest wissen wie ich funktioniere ? https://github.com/ShiruSan/TjulfarBot.Net");
            string[] commands = new string[Listener.CommandManager.Commands.Count];
            int i = 0;
            foreach (var command in Listener.CommandManager.Commands)
            {
                commands[i] = "  \u2022  " + command.name;
                i++;
            }

            var fieldBuilders = new List<EmbedFieldBuilder>();
            fieldBuilders.Add(new EmbedFieldBuilder().WithName("Alle Commands").WithValue(String.Join("\n", commands)).WithIsInline(false));
            fieldBuilders.Add(new EmbedFieldBuilder().WithName("Aktuelle Version").WithValue("TjulfarBot 1.0\nC# Rewrite (TjulfarBot.Net)").WithIsInline(false));
            builder.WithFields(fieldBuilders);
            ctx.Channel.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
        }
        
    }
}