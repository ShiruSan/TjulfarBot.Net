using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;
using TjulfarBot.Net.Commands;
using TjulfarBot.Net.Managers;

namespace TjulfarBot.Net.Tempban
{
    public class TempbanCommand : Command
    {
        
        public TempbanCommand() : base("tempban", GuildPermission.BanMembers)
        {
        }

        public override void OnCommand(CommandContext ctx)
        {
            if (ctx.Arguments.Length == 2)
            {
                try
                {
                    int time = int.Parse(ctx.Arguments[0].Substring(0, ctx.Arguments[0].Length - 1));
                    char timeunit = ctx.Arguments[0].ToCharArray()[ctx.Arguments[0].Length - 1];
                    if (ctx.Message.MentionedUsers.Count == 1)
                    {
                        var member = ctx.Message.MentionedUsers.ToArray()[0] as SocketGuildUser;
                        if (timeunit == 'h')
                        {
                            member.BanAsync().GetAwaiter().GetResult();
                            TempbanManager.Get().Tempban(member, time, TempbanManager.TimeUnit.HOURS);
                            ctx.Channel.SendMessageAsync(":white_check_mark: " + member.Username + " wurde für `" + time + "` Stunde(n) gebannt !").GetAwaiter().GetResult();
                        }
                        else if(timeunit == 'd')
                        {
                            member.BanAsync().GetAwaiter().GetResult();
                            TempbanManager.Get().Tempban(member, time, TempbanManager.TimeUnit.DAYS);
                            ctx.Channel.SendMessageAsync(":white_check_mark: " + member.Username + " wurde für `" + time + "` Tag(e) gebannt !").GetAwaiter().GetResult();
                        }
                        else SendHelpMessage(ctx.Channel);
                    }
                    else SendHelpMessage(ctx.Channel);
                }
                catch (FormatException)
                {
                    ctx.Channel.SendMessageAsync(":x: `" + ctx.Arguments[0].Substring(0, ctx.Arguments[0].Length - 1) + " ist keine valide Zahl !").GetAwaiter().GetResult();
                }
            }
            else if (ctx.Arguments.Length > 2)
            {
                try
                {
                    int time = int.Parse(ctx.Arguments[0].Substring(0, ctx.Arguments[0].Length - 1));
                    char timeunit = ctx.Arguments[0].ToCharArray()[ctx.Arguments[0].Length - 1];
                    if (ctx.Message.MentionedUsers.Count == 1)
                    {
                        String reason = "";
                        for(int i = 2; i < ctx.Arguments.Length; i++) {
                            if((i + 1) == ctx.Arguments.Length) reason += ctx.Arguments[i];
                            else reason += ctx.Arguments[i] + " ";
                        }
                        var member = ctx.Message.MentionedUsers.ToArray()[0] as SocketGuildUser;
                        if (timeunit == 'h')
                        {
                            member.BanAsync(0, reason).GetAwaiter().GetResult();
                            TempbanManager.Get().Tempban(member, time, TempbanManager.TimeUnit.HOURS);
                            ctx.Channel.SendMessageAsync(":white_check_mark: " + member.Username + " wurde für `" + time + "` Stunde(n) gebannt !").GetAwaiter().GetResult();
                        }
                        else if(timeunit == 'd')
                        {
                            member.BanAsync(0, reason).GetAwaiter().GetResult();
                            TempbanManager.Get().Tempban(member, time, TempbanManager.TimeUnit.DAYS);
                            ctx.Channel.SendMessageAsync(":white_check_mark: " + member.Username + " wurde für `" + time + "` Tag(e) gebannt !").GetAwaiter().GetResult();
                        }
                        else SendHelpMessage(ctx.Channel);
                    }
                    else SendHelpMessage(ctx.Channel);
                }
                catch (FormatException)
                {
                    ctx.Channel.SendMessageAsync(":x: `" + ctx.Arguments[0].Substring(0, ctx.Arguments[0].Length - 1) + " ist keine valide Zahl !").GetAwaiter().GetResult();
                }
            }
            else SendHelpMessage(ctx.Channel);
        }

        private void SendHelpMessage(ISocketMessageChannel channel)
        {
            var builder = new EmbedBuilder();
            builder.WithTitle("Tempban Command").WithColor(Color.Blue);
            builder.WithDescription("Der Tempban Command bannt tempor\u00E4r Mitglieder vom Discord Server.");
            var fieldBuilders = new List<EmbedFieldBuilder>();
            fieldBuilders.Add(new EmbedFieldBuilder().WithName("Nutzung des Commands").WithValue("`+tempban [Zeit] [Member] (Grund)`").WithIsInline(false));
            var array = new string[] { "Die Zeitangabe kann nur in Stunden oder Tage stattfinden !", "Die Zeit gibt man in folgenden Schema an:", "`1h oder 1d`" };
            fieldBuilders.Add(new EmbedFieldBuilder().WithName("Info zur Zeitangabe").WithValue(String.Join("\n", array)).WithIsInline(false));
            builder.WithFields(fieldBuilders);
            channel.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
        }
        
    }
}