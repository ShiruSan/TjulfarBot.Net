using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using TjulfarBot.Net.Commands;
using TjulfarBot.Net.Managers;

namespace TjulfarBot.Net.Warn
{
    public class WarnCommand : Command
    {
        public WarnCommand() : base("warn", GuildPermission.BanMembers)
        {
        }

        public override void OnCommand(CommandContext ctx)
        {
            if (ctx.Message.MentionedUsers.Count == 1)
            {
                var user = GetUserBySocket(ctx.Message.MentionedUsers.First(), ctx.Guild);
                if (ctx.Arguments.Length == 1)
                {
                    var warn = WarnManager.Get().CreateWarn(user, ctx.Author, null);
                    if (WarnManager.Get().HasUserWarned(user.Id))
                    {
                        var builder = new EmbedBuilder();
                        builder.WithTitle("Du wurdest gewarnt !").WithColor(new Color(255, 255, 0)).WithThumbnailUrl(user.GetAvatarUrl());
                        builder.WithFields(new[]
                        {
                            new EmbedFieldBuilder().WithName("Verwarner:").WithValue(ctx.Author).WithIsInline(false),
                            new EmbedFieldBuilder().WithName("Grund:").WithValue("Nicht angegeben").WithIsInline(false)
                        });
                        try
                        {
                            user.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
                            builder.WithTitle($"{user} wurde gewarnt !");
                            ctx.Channel.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
                        }
                        catch (HttpException)
                        {
                            ctx.Channel.SendMessageAsync(user.Mention, false, builder.Build()).GetAwaiter().GetResult();
                        }
                        builder.WithTitle($"{user} wurde gewarnt !");
                        ctx.Channel.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
                        Listener.OnWarned(warn);
                        var warnings = WarnManager.Get().GetWarns(user);
                        var reason = "[Warning System: Warnungsüberschreitung]\n";
                        reason += "1. Grund: ";
                        if (warnings[0].reason == null) reason += "Nicht angegeben";
                        else reason += warnings[0].reason;
                        reason += " von " + warnings[0].mod;
                        reason += "2. Grund: ";
                        if (warnings[1].reason == null) reason += "Nicht angegeben";
                        else reason += warnings[1].reason;
                        reason += " von " + warnings[1].mod;
                        user.BanAsync(7, reason).GetAwaiter().GetResult();
                    }
                    else
                    {
                        var builder = new EmbedBuilder();
                        builder.WithTitle("Du wurdest gewarnt !").WithColor(new Color(255, 255, 0)).WithThumbnailUrl(user.GetAvatarUrl());
                        builder.WithFields(new[]
                        {
                            new EmbedFieldBuilder().WithName("Verwarner:").WithValue(ctx.Author).WithIsInline(false),
                            new EmbedFieldBuilder().WithName("Grund:").WithValue("Nicht angegeben").WithIsInline(false)
                        });
                        try
                        {
                            user.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
                        }
                        catch (HttpException)
                        {
                            ctx.Channel.SendMessageAsync(user.Mention, false, builder.Build()).GetAwaiter().GetResult();
                        }
                        builder.WithTitle($"{user} wurde gewarnt !");
                        ctx.Channel.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
                        Listener.OnWarned(warn);
                    }
                }
                else if (ctx.Arguments.Length == 2)
                {
                    if (ctx.Arguments[0].Equals("list"))
                    {
                        var warns = WarnManager.Get().GetWarns(user);
                        var builder = new EmbedBuilder();
                        builder.WithTitle($"Warnungen von {user}").WithColor(new Color(255, 255, 0)).WithThumbnailUrl(user.GetAvatarUrl());
                        var list = new List<EmbedFieldBuilder>();
                        foreach (var listwarn in warns)
                        {
                            string reason;
                            if (listwarn.reason == null) reason = "Nicht angegeben";
                            else reason = listwarn.reason;
                            var fieldBuilder = new EmbedFieldBuilder();
                            fieldBuilder.WithName($"Warnung {(warns.IndexOf(listwarn) + 1)}")
                                .WithValue($"***Grund***: {reason}\n***Mod***: {listwarn.mod}\n***Verwarnt*** am: {listwarn.time.ToLongDateString()}  {listwarn.time.ToLongTimeString()}").WithIsInline(false);
                            list.Add(fieldBuilder);
                        }
                        if (warns.Count == 0)
                        {
                            list.Add(new EmbedFieldBuilder().WithName("Keine Warnungen vorhanden !").WithValue("Dieser Benutzer hat keine Verwarnungen.").WithIsInline(false));
                        }
                        builder.WithFields(list);
                        ctx.Channel.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
                        return;
                    }

                    var warn = WarnManager.Get().CreateWarn(user, ctx.Author, ctx.Arguments[1]);
                    if (WarnManager.Get().HasUserWarned(user.Id))
                    {
                        var builder = new EmbedBuilder();
                        builder.WithTitle("Du wurdest gewarnt !").WithColor(new Color(255, 255, 0)).WithThumbnailUrl(user.GetAvatarUrl());
                        builder.WithFields(new []
                        {
                            new EmbedFieldBuilder().WithName("Verwarner:").WithValue(ctx.Author).WithIsInline(false),
                            new EmbedFieldBuilder().WithName("Grund:").WithValue(ctx.Arguments[1]).WithIsInline(false)
                        });
                        try
                        {
                            user.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
                            builder.WithTitle($"{user} wurde gewarnt !");
                            ctx.Channel.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
                        }
                        catch (HttpException)
                        {
                            ctx.Channel.SendMessageAsync(user.Mention, false, builder.Build()).GetAwaiter().GetResult();
                        }
                        Listener.OnWarned(warn);
                        var reason = "[Warning System: Warnungsüberschreitung]\n";
                        var warnings = WarnManager.Get().GetWarns(user);
                        for (int i = 0; i < warnings.Count; i++)
                        {
                            reason += $"{i + 1}. Grund: ";
                            if (warnings[i].reason == null) reason += "Nicht angegeben";
                            else reason += warnings[i].reason;
                            reason += "\n";
                        }
                        user.BanAsync(7, reason).GetAwaiter().GetResult();
                    }
                    else
                    {
                        var builder = new EmbedBuilder();
                        builder.WithTitle("Du wurdest gewarnt !").WithColor(new Color(255, 255, 0)).WithThumbnailUrl(user.GetAvatarUrl());
                        builder.WithFields(new[]
                        {
                            new EmbedFieldBuilder().WithName("Verwarner:").WithValue(ctx.Author).WithIsInline(false),
                            new EmbedFieldBuilder().WithName("Grund:").WithValue(ctx.Arguments[1]).WithIsInline(false)
                        });
                        user.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
                        builder.WithTitle($"{user} wurde gewarnt !");
                        ctx.Channel.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
                        Listener.OnWarned(warn);
                    }
                    
                }
                else if (ctx.Arguments.Length > 2)
                {

                    var reasonArray = new string[ctx.Arguments.Length - 1];
                    Array.Copy(ctx.Arguments, 1, reasonArray, 0, reasonArray.Length);
                    var warn = WarnManager.Get().CreateWarn(user, ctx.Author, String.Join(" ", reasonArray));
                    if (WarnManager.Get().HasUserWarned(user.Id))
                    {
                        var builder = new EmbedBuilder();
                        builder.WithTitle("Du wurdest gewarnt !").WithColor(new Color(255, 255, 0)).WithThumbnailUrl(user.GetAvatarUrl());
                        builder.WithFields(new[]
                        {
                            new EmbedFieldBuilder().WithName("Verwarner:").WithValue(ctx.Author).WithIsInline(false),
                            new EmbedFieldBuilder().WithName("Grund:").WithValue(String.Join(" ", reasonArray)).WithIsInline(false)
                        });
                        try
                        {
                            user.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
                            builder.WithTitle($"{user} wurde gewarnt !");
                            ctx.Channel.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
                        }
                        catch (HttpException)
                        {
                            ctx.Channel.SendMessageAsync(user.Mention, false, builder.Build()).GetAwaiter().GetResult();
                        }
                        Listener.OnWarned(warn);
                        var reason = "[Warning System: Warnungsüberschreitung]\n";
                        var warnings = WarnManager.Get().GetWarns(user);
                        for (int i = 0; i < warnings.Count; i++)
                        {
                            reason += $"{i + 1}. Grund: ";
                            if (warnings[i].reason == null) reason += "Nicht angegeben";
                            else reason += warnings[i].reason;
                            reason += "\n";
                        }
                        user.BanAsync(7, reason).GetAwaiter().GetResult();
                    }
                    else
                    {
                        var builder = new EmbedBuilder();
                        builder.WithTitle("Du wurdest gewarnt !").WithColor(new Color(255, 255, 0)).WithThumbnailUrl(user.GetAvatarUrl());
                        builder.WithFields(new[]
                        {
                            new EmbedFieldBuilder().WithName("Verwarner:").WithValue(ctx.Author).WithIsInline(false),
                            new EmbedFieldBuilder().WithName("Grund:").WithValue(String.Join(" ", reasonArray)).WithIsInline(false)
                        });
                        try
                        {
                            user.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
                            builder.WithTitle($"{user} wurde gewarnt !");
                            ctx.Channel.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
                        }
                        catch (HttpException)
                        {
                            ctx.Channel.SendMessageAsync(user.Mention, false, builder.Build()).GetAwaiter().GetResult();
                        }
                        Listener.OnWarned(warn);
                    }
                }
            }
            else SendHelpMessage(ctx.Channel);
        }

        private SocketGuildUser GetUserBySocket(SocketUser user, SocketGuild guild)
        {
            if ((user as SocketGuildUser) != null) return (user as SocketGuildUser);
            return guild.GetUser(user.Id);
        }
        
        private void SendHelpMessage(ISocketMessageChannel channel)
        {
            var builder = new EmbedBuilder();
            builder.WithColor(Color.Blue);
            builder.WithTitle("Warn Command");
            builder.WithDescription("Der Warn Command ist dafür da um Nutzer auf ein Fehlverhalten zu verwarnen oder ihre Verwarnungen anzeigen zu lassen.");
            var fieldBuilders = new List<EmbedFieldBuilder>();
            fieldBuilders.Add(new EmbedFieldBuilder().WithName("Nutzung des Commands").WithValue("`+warn [@Member] (Grund)`\n`+warn list [@Member]`").WithIsInline(false));
            builder.WithFields(fieldBuilders);
            channel.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
        }
        
    }
}