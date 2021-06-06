﻿using System.Linq;
using Discord;
using Discord.WebSocket;
using TjulfarBot.Net.Commands;
using TjulfarBot.Net.Managers;

namespace TjulfarBot.Net.Leveling
{
    public class LevelCommand : Command
    {
        
        public LevelCommand() : base("level", null)
        {
        }

        public override void OnCommand(CommandContext ctx)
        {
            if (ctx.Arguments.Length == 0)
            {
                Embed toSend;
                if (LevelManager.Get().ExistsProfile(ctx.Author.Id))
                {
                    toSend = LevelManager.Get().GetProfile(ctx.Author.Id).CreateEmbed();
                }
                else
                {
                    toSend = LevelManager.Get().CreateProfile(ctx.Author.Id).CreateEmbed();
                }
                ctx.Channel.SendMessageAsync(null, false, toSend).GetAwaiter().GetResult();
            }
            else if (ctx.Arguments.Length == 1)
            {
                if (ctx.Arguments[0].Equals("help"))
                {
                    SendHelpMessage(ctx.Channel);
                    return;
                }

                if (ctx.Message.MentionedUsers.Count != 1)
                {
                    SendHelpMessage(ctx.Channel);
                    return; 
                }

                SocketUser socketUser = ctx.Message.MentionedUsers.ToArray()[0];
                
                Embed toSend;
                if (LevelManager.Get().ExistsProfile(socketUser.Id))
                {
                    toSend = LevelManager.Get().GetProfile(socketUser.Id).CreateEmbed();
                }
                else
                {
                    toSend = LevelManager.Get().CreateProfile(socketUser.Id).CreateEmbed();
                }
                ctx.Channel.SendMessageAsync(null, false, toSend).GetAwaiter().GetResult();
                
            }
            else if (ctx.Arguments.Length == 2)
            {
                if (!ctx.Arguments[0].Equals("blacklist"))
                {
                    SendHelpMessage(ctx.Channel);
                    return;
                }

                if (ctx.Author.GuildPermissions.Has(GuildPermission.MuteMembers))
                {
                    if (ctx.Message.MentionedUsers.Count != 1)
                    {
                        SendHelpMessage(ctx.Channel);
                        return; 
                    }
                    
                    SocketUser socketUser = ctx.Message.MentionedUsers.ToArray()[0];
                    if (LevelManager.Get().ExistsBlacklist(socketUser.Id))
                    {
                        LevelManager.Get().RemoveBlacklist(socketUser.Id);
                        ctx.Channel.SendMessageAsync(":white_check_mark: " + socketUser.Mention +
                                                     " wurde von der Blacklist entfernt und kann nun wieder neue Levels erreichen !")
                            .GetAwaiter().GetResult();
                    }
                    else
                    {
                        LevelManager.Get().AddBlacklist(socketUser.Id);
                        ctx.Channel.SendMessageAsync(":white_check_mark: " + socketUser.Mention + " wurde geblacklisted und kriegt kein Levelaufstieg !")
                            .GetAwaiter().GetResult();
                    }
                }
                else
                {
                    ctx.Channel.SendMessageAsync(":x: Dazu hast du nicht die Rechte !").GetAwaiter().GetResult();
                }
            }
            else if (ctx.Arguments.Length == 3)
            {
                if (!(ctx.Arguments[0].Equals("channels")))
                {
                    SendHelpMessage(ctx.Channel);
                    return;
                }

                if (ctx.Message.MentionedChannels.Count != 1)
                {
                    SendHelpMessage(ctx.Channel);
                    return;
                }

                var channel = ctx.Message.MentionedChannels.ToArray()[0];

                if (ctx.Arguments[1].Equals("add"))
                {
                    if (!(LevelManager.Get().ExistsLevelChannel(channel.Id)))
                    {
                        LevelManager.Get().AddLevelChannel(channel.Id);
                        ctx.Channel.SendMessageAsync($":white_check_mark: In <#{channel.Id}> kann man nun im Level aufsteigen !").GetAwaiter().GetResult();
                    }
                    else
                    {
                        ctx.Channel.SendMessageAsync($":x: In <#{channel.Id}> kann man schon im Level aufsteigen !").GetAwaiter().GetResult();
                    }
                }
                else if (ctx.Arguments[1].Equals("remove"))
                {
                    if (LevelManager.Get().ExistsLevelChannel(channel.Id))
                    {
                        LevelManager.Get().RemoveLevelChannel(channel.Id);
                        ctx.Channel.SendMessageAsync($":white_check_mark: In <#{channel.Id}> kann man nun nicht mehr im Level aufsteigen !").GetAwaiter().GetResult();
                    }
                    else
                    {
                        ctx.Channel.SendMessageAsync($":x: In <#{channel.Id}> kann man schon im Level aufsteigen !").GetAwaiter().GetResult();
                    }
                }
                else
                {
                    SendHelpMessage(ctx.Channel);
                }

            }
            else
            {
                SendHelpMessage(ctx.Channel);
            }
        }

        private void SendHelpMessage(ISocketMessageChannel channel)
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle("Level Command").WithColor(Color.Blue);
            builder.WithDescription(
                "Der Level Command ist daf\u0252r da um seinen Level aufzurufen oder mit den n\u00F6tigen Rechten, die Channel in denen man seinen Level steigern kann zu \u00E4ndern oder Mitglieder zu blacklisten");
            builder.WithFields(new[]
            {
                new EmbedFieldBuilder().WithName("Nutzung des Command").WithValue("`+level`\n`+level @Mitglied`\n`+level channels add/remove #channel`\n`+level blacklist @Member`").WithIsInline(false)
            });
            channel.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
        }
        
    }
}