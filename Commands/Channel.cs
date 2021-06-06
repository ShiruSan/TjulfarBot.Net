using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;
using TjulfarBot.Net.Managers;

namespace TjulfarBot.Net.Commands
{
    public class Channel : Command
    {
        public Channel() : base("channel")
        {
        }

        public override void OnCommand(CommandContext ctx)
        {
            if (ctx.Arguments.Length == 2)
            {
                if(ctx.Arguments[0].Equals("remove"))
                {
                    switch(ctx.Arguments[1]) {
                        case "welcomeChannel":
                            Program.instance.Settings.welcomeChannel = -1;
                            ctx.Channel.SendMessageAsync(":white_check_mark: Welcome Channel entfernt !").GetAwaiter().GetResult();
                            break;
                        case "serverLog":
                            Program.instance.Settings.serverLogChannel = -1;
                            ctx.Channel.SendMessageAsync(":white_check_mark: Server Log entfernt !").GetAwaiter().GetResult();
                            break;
                        case "announcement":
                            Program.instance.Settings.announcementChannel = -1;
                            ctx.Channel.SendMessageAsync(":white_check_mark: Announcement entfernt !").GetAwaiter().GetResult();
                            break;
                        case "ruleChannel":
                            Program.instance.Settings.ruleChannel = -1;
                            ctx.Channel.SendMessageAsync(":white_check_mark: Rule Channel entfernt !").GetAwaiter().GetResult();
                            break;
                        case "levelup":
                            Program.instance.Settings.levelupChannel = -1;
                            ctx.Channel.SendMessageAsync(":white_check_mark: Level Up Channel entfernt !").GetAwaiter().GetResult();
                            break;
                        case "videoUpload":
                            Program.instance.Settings.videoUploadChannel = 0;
                            ctx.Channel.SendMessageAsync(":white_check_mark: Video Upload Channel entfernt !").GetAwaiter().GetResult();
                            break;
                        default:
                            ctx.Channel.SendMessageAsync(":x: Diese Funktion existiert nicht !").GetAwaiter().GetResult();
                            break;
                    }
                }
                else SendHelpMessage(ctx.Channel);
            }
            else if (ctx.Arguments.Length > 2)
            {
                if (ctx.Arguments[0].Equals("set"))
                {
                    if (ctx.Message.MentionedChannels.Count == 1)
                    {
                        SocketGuildChannel guildChannel = ctx.Message.MentionedChannels.ToArray()[0];
                        switch(ctx.Arguments[1]) {
                            case "welcomeChannel":
                                Program.instance.Settings.welcomeChannel = (long) guildChannel.Id;
                                ctx.Channel.SendMessageAsync(":white_check_mark: Welcome Channel gesetzt !").GetAwaiter().GetResult();
                                break;
                            case "serverLog":
                                Program.instance.Settings.serverLogChannel = (long) guildChannel.Id;
                                ctx.Channel.SendMessageAsync(":white_check_mark: Server Log gesetzt !").GetAwaiter().GetResult();
                                break;
                            case "announcement":
                                Program.instance.Settings.announcementChannel = (long) guildChannel.Id;
                                ctx.Channel.SendMessageAsync(":white_check_mark: Announcement gesetzt !").GetAwaiter().GetResult();
                                break;
                            case "ruleChannel":
                                Program.instance.Settings.ruleChannel = (long) guildChannel.Id;
                                ctx.Channel.SendMessageAsync(":white_check_mark: Rule Channel gesetzt !").GetAwaiter().GetResult();
                                break;
                            case "levelup":
                                Program.instance.Settings.levelupChannel = (long) guildChannel.Id;
                                ctx.Channel.SendMessageAsync(":white_check_mark: Level Up Channel gesetzt !").GetAwaiter().GetResult();
                                break;
                            case "videoUpload":
                                Program.instance.Settings.videoUploadChannel = (long) guildChannel.Id;
                                ctx.Channel.SendMessageAsync(":white_check_mark: Video Upload Channel gesetzt !").GetAwaiter().GetResult();
                                break;
                            default:
                                ctx.Channel.SendMessageAsync(":x: Diese Funktion existiert nicht !").GetAwaiter().GetResult();
                                break;
                        }
                    }
                    else SendHelpMessage(ctx.Channel);
                }
                else SendHelpMessage(ctx.Channel);
            }
        }

        private void SendHelpMessage(ISocketMessageChannel channel)
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithColor(Color.Blue);
            builder.WithTitle("Channel Command");
            builder.WithDescription("Mit dem `+channel` Command kannst du elementaren Funktionen, die einen Channel ben\u00F6tigen, einen hinzuf\u00FCgen.");
            List<EmbedFieldBuilder> fieldBuilders = new List<EmbedFieldBuilder>();
            fieldBuilders.Add(new EmbedFieldBuilder().WithName("Nutzung des Commands").WithValue("`+channel [Aktion] [Funktion] (Channel)`").WithIsInline(false));
            fieldBuilders.Add(new EmbedFieldBuilder().WithName("Aktionen").WithValue("set, remove").WithIsInline(false));
            fieldBuilders.Add(new EmbedFieldBuilder().WithName("Funktionen").WithValue("welcomeChannel, serverLog, announcement, ruleChannel, levelup, videoUpload").WithIsInline(false));
            builder.WithFields(fieldBuilders);
            channel.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
        }
    }
}