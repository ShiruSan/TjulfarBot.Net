using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using Discord.WebSocket;
using TjulfarBot.Net.Managers;

namespace TjulfarBot.Net.Commands
{
    public class Announcement : Command
    {
        public Announcement() : base("announcement", GuildPermission.ManageGuild)
        {
        }

        public override void OnCommand(CommandContext ctx)
        {
            if (ctx.Arguments.Length > 1)
            {
                try
                {
                    int id = int.Parse(ctx.Arguments[0]);
                    AnnouncementType type = AnnouncementType.GetAnnouncementById(id);
                    if (type != null)
                    {
                        if (Program.instance.Settings.announcementChannel != -1)
                        {
                            SocketTextChannel announcementChannel =
                                ctx.Guild.GetTextChannel((ulong) Program.instance.Settings.announcementChannel);
                            StringBuilder builder = new StringBuilder();
                            for (int i = 1; i < ctx.Arguments.Length; i++)
                            {
                                if ((i + 1) != ctx.Arguments.Length)
                                {
                                    builder.Append(ctx.Arguments[i] + " ");
                                }
                                else builder.Append(ctx.Arguments[i]);
                            }

                            string[] array = builder.ToString().Split("\u00A7");
                            string title = array[0];
                            string text = "";
                            if (ctx.Arguments.Length > 2)
                            {
                                string[] array2 = new string[ctx.Arguments.Length - 1];
                                Array.Copy(array, 1, array2, 0, ctx.Arguments.Length - 1);
                                text = String.Join("\u00A7", array2);
                            }
                            else text = array[1];

                            EmbedBuilder builder2 = new EmbedBuilder();
                            builder2.WithAuthor(type.Title, ctx.SelfUser.GetAvatarUrl()).WithTitle(title);
                            builder2.WithColor(type.AnnouncementColor).WithDescription(text);
                            //Can be null because its by default null
                            announcementChannel
                                .SendMessageAsync(ctx.Guild.Roles.FirstOrDefault(x => x.IsEveryone).Mention, false,
                                    builder2.Build()).GetAwaiter()
                                .GetResult();
                        }
                        else
                        {
                            ctx.Channel.SendMessageAsync(
                                    ":x: Der Announcement Channel wurde bisher noch nicht gesetzt !")
                                .GetAwaiter().GetResult();
                        }
                    }
                    else
                    {
                        ctx.Channel.SendMessageAsync(":x: `" + ctx.Arguments[0] + "` ist keine existierender Typ !")
                            .GetAwaiter().GetResult();
                    }
                }
                catch (FormatException)
                {
                    ctx.Channel.SendMessageAsync(":x: `" + ctx.Arguments[0] + "` ist ekien valide Zahl !").GetAwaiter()
                        .GetResult();
                }
                catch (IndexOutOfRangeException)
                {
                    ctx.Channel.SendMessageAsync(":x: Du hast die Titel/Nachricht Trennung nicht richtig gemacht !").GetAwaiter()
                        .GetResult();
                }
            }
            else SendHelpMessage(ctx.Channel);
        }
        
        private void SendHelpMessage(ISocketMessageChannel channel)
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithColor(Color.Blue);
            builder.WithTitle("Announcement Command");
            builder.WithDescription("Dieser Command ist dazu da um eine sch\u00F6n aussehende Embed Nachricht in den gesetzten Announcement Channel zu schicken.");
            List<EmbedFieldBuilder> fieldBuilders = new List<EmbedFieldBuilder>();
            fieldBuilders.Add(new EmbedFieldBuilder().WithName("Nutzung des Command").WithValue("`+announcement [Announcement Typ] [Titel]\u00A7[Nachricht]`").WithIsInline(false));
            StringBuilder stringBuilder = new StringBuilder();
            foreach(AnnouncementType type in AnnouncementType.Values) {
                if(type.Id != AnnouncementType.Values.Length)
                {
                    stringBuilder.AppendLine("`" + type.Id + "` \u279C " + type.Title);
                } else stringBuilder.Append("`" + type.Id + "` \u279C " + type.Title);
            }
            fieldBuilders.Add(new EmbedFieldBuilder().WithName("Announcement Typen").WithValue("Es gibt `" + AnnouncementType.Values.Length + "` verschiedene Arten an Typen.\nHier eine Auflistung:\n\n" + stringBuilder.ToString()).WithIsInline(false));
            builder.WithFields(fieldBuilders);
            channel.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
        }
     
        private class AnnouncementType
        {
            public static AnnouncementType Info = new AnnouncementType(1, "Info", Color.Green);
            public static AnnouncementType InfoForDiscord = new AnnouncementType(2, "Info zum Discord Server", new Color(114, 92, 218));
            public static AnnouncementType InfoForYoutube = new AnnouncementType(3, "Info zum Youtube Kanal", Color.Red);
            public static AnnouncementType Important = new AnnouncementType(4,"Wichtige Info", Color.Red);
            public static AnnouncementType ImportantForDiscord = new AnnouncementType(5, "Wichtige Info zum Discord Server", Color.Red);
            public static AnnouncementType ImportantForYoutube = new AnnouncementType(6, "Wichtige Info zum Youtube Kanal", Color.Red);
            public static AnnouncementType[] Values = {Info, InfoForDiscord, InfoForYoutube, Important, ImportantForDiscord, ImportantForYoutube };

            public int Id { get; }
            public String Title { get; }
            public Color AnnouncementColor { get; }

            private AnnouncementType(int id, string title, Color announcementColor)
            {
                Id = id;
                Title = title;
                AnnouncementColor = announcementColor;
            }
            
            public static AnnouncementType GetAnnouncementById(int id) {
                foreach (var type in Values)
                {
                    if (type.Id == id)
                    {
                        return type;
                    }
                }
                return null;
            }
            
        }
        
    }

}