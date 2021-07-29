using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;
using TjulfarBot.Net.Managers;

namespace TjulfarBot.Net.Commands
{
    public class Whois : Command
    {
        public Whois() : base("whois")
        {
        }

        public override void OnCommand(CommandContext ctx)
        {
            if (ctx.Arguments.Length == 0)
            {
                ctx.Channel.SendMessageAsync(null, false, GetInfo(ctx.Author)).GetAwaiter().GetResult();
            }
            else if (ctx.Arguments.Length == 1 && ctx.Message.MentionedUsers.Count == 1)
            {
                ctx.Channel.SendMessageAsync(null, false, GetInfo(ctx.Message.MentionedUsers.First() as SocketGuildUser)).GetAwaiter().GetResult();
            }
            else SendHelpMessage(ctx.Channel);
        }

        private Embed GetInfo(SocketGuildUser user)
        {
            var builder = new EmbedBuilder();
            builder.WithTitle($"Who is {user.Username} ?").WithThumbnailUrl(user.GetAvatarUrl()).WithColor(Color.Green);
            if (user.JoinedAt.HasValue)
            {
                builder.WithFields(new[]
                {
                    new EmbedFieldBuilder().WithName("Voller Nutzername").WithValue(user).WithIsInline(false),
                    new EmbedFieldBuilder().WithName($"Rollen ({user.Roles.Count - 1})")
                        .WithValue(String.Join(", ", GetRoles(user))).WithIsInline(false),
                    new EmbedFieldBuilder().WithName("Account erstellt am").WithValue($"{user.CreatedAt.DateTime.ToLongDateString()}  {user.CreatedAt.DateTime.ToLongTimeString()}").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Beigetretten am").WithValue($"{user.JoinedAt.Value.DateTime.ToLongDateString()}  {user.JoinedAt.Value.DateTime.ToLongTimeString()}").WithIsInline(true)
                });
            }
            else
            {
                builder.WithFields(new[]
                {
                    new EmbedFieldBuilder().WithName("Voller Nutzername").WithValue(user).WithIsInline(false),
                    new EmbedFieldBuilder().WithName($"Rollen ({user.Roles.Count - 1})")
                        .WithValue(String.Join(", ", GetRoles(user))).WithIsInline(false),
                    new EmbedFieldBuilder().WithName("Account erstellt am").WithValue($"{user.CreatedAt.DateTime.ToLongDateString()}  {user.CreatedAt.DateTime.ToLongTimeString()}").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Beigetretten am").WithValue("Nicht angegeben").WithIsInline(true)
                });
            }
            return builder.Build();
        }

        private List<string> GetRoles(SocketGuildUser user)
        {
            var list = new List<string>();
            if (user.Roles.Count == 1)
            {
                list.Add("Keine Rollen");
            }
            else
            {
                foreach (var role in user.Roles)
                {
                    if(!role.IsEveryone) list.Add(role.Mention);
                }
            }
            return list;
        }

        private void SendHelpMessage(ISocketMessageChannel channel)
        {
            var builder = new EmbedBuilder();
            builder.WithColor(Color.Blue);
            builder.WithTitle("whois Command");
            builder.WithDescription("Der whois Command ist dafür da um ein kleinen Steckbrief von verschiedensten Nutzern zu erstellen.");
            var fieldBuilders = new List<EmbedFieldBuilder>();
            fieldBuilders.Add(new EmbedFieldBuilder().WithName("Nutzung des Commands").WithValue("`+whois [@Member]`").WithIsInline(false));
            builder.WithFields(fieldBuilders);
            channel.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
        }
        
    }
}