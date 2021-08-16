using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using TjulfarBot.Net.Leveling;
using TjulfarBot.Net.Managers;
using TjulfarBot.Net.Tempban;
using TjulfarBot.Net.Utils;
using TjulfarBot.Net.Warn;
using TjulfarBot.Net.Youtube;

namespace TjulfarBot.Net
{
    public class Listener
    {
        private static DiscordSocketClient SocketClient;
        public static CommandManager CommandManager = new CommandManager();
        
        public static void Set(DiscordSocketClient socketClient)
        {
            SocketClient = socketClient;
            SocketClient.Connected += SocketClientOnConnected;
            SocketClient.Log += SocketClientOnLog;
            SocketClient.Ready += SocketClientOnReady;
            SocketClient.MessageReceived += SocketClientOnMessageReceived;
            SocketClient.UserJoined += SocketClientOnUserJoined;
            SocketClient.UserLeft += SocketClientOnUserLeft;
            SocketClient.UserBanned += SocketClientOnUserBanned;
            SocketClient.UserUnbanned += SocketClientOnUserUnbanned;
            SocketClient.UserVoiceStateUpdated += SocketClientOnUserVoiceStateUpdated;
        }

        private static Task SocketClientOnUserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            if (before.VoiceChannel == null && after.VoiceChannel != null)
            {
                if (user.IsBot) return Task.CompletedTask;
                if(LevelManager.Get().ExistsBlacklist(user.Id)) return Task.CompletedTask;
                if(after.VoiceChannel.Id == after.VoiceChannel.Guild.AFKChannel.Id) return Task.CompletedTask;
                VoiceCounter.Get().VoiceCounters.Add(user.Id, after);
            }
            else if (before.VoiceChannel != null && after.VoiceChannel != null)
            {
                if(user.IsBot) return Task.CompletedTask;
                if(!VoiceCounter.Get().VoiceCounters.ContainsKey(user.Id)) return Task.CompletedTask;
                if (after.VoiceChannel.Id == after.VoiceChannel.Guild.AFKChannel.Id)
                {
                    VoiceCounter.Get().VoiceCounters.Remove(user.Id);
                    return Task.CompletedTask;
                }
                VoiceCounter.Get().VoiceCounters[user.Id] = after;
            }
            else if (before.VoiceChannel != null && after.VoiceChannel == null)
            {
                if(user.IsBot) return Task.CompletedTask;
                if(!VoiceCounter.Get().VoiceCounters.ContainsKey(user.Id)) return Task.CompletedTask;
                VoiceCounter.Get().VoiceCounters.Remove(user.Id);
            }
            return Task.CompletedTask;
        }

        public static void OnWarned(WarnManager.Warn warn)
        {
            if(Program.instance.Settings.serverLogChannel == -1) return;
            var channel = warn.mod.Guild.GetTextChannel((ulong) Program.instance.Settings.serverLogChannel);
            var builder = new EmbedBuilder();
            builder.WithTitle($"{warn.user} wurde gewarnt").WithColor(new Color(255, 255, 0)).WithThumbnailUrl(warn.user.GetAvatarUrl());
            string reason;
            if (warn.reason == null) reason = "Nicht angegeben";
            else reason = warn.reason;
            builder.WithFields(new[]
            {
                new EmbedFieldBuilder().WithName("Verwarner:").WithValue(warn.mod).WithIsInline(false),
                new EmbedFieldBuilder().WithName("Grund:").WithValue(reason).WithIsInline(false)
            });
            channel.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
        }

        private static Task SocketClientOnUserUnbanned(SocketUser arg1, SocketGuild arg2)
        {
            if(Program.instance.Settings.serverLogChannel == -1) return Task.CompletedTask;
            var channel = arg2.GetTextChannel((ulong) Program.instance.Settings.serverLogChannel);
            Task.Delay(250).GetAwaiter().GetResult();
            var logs = arg2.GetAuditLogsAsync(10).FlattenAsync().GetAwaiter().GetResult();
            IUser mod = null;
            foreach (var log in logs)
            {
                if(log.Action != ActionType.Unban) continue;
                mod = log.User;
                break;
            }
            var builder = new EmbedBuilder();
            builder.WithTitle("Mitglied wurde entbannt !").WithColor(Color.Red).WithThumbnailUrl(arg1.GetAvatarUrl());
            builder.WithFields(new[]
            {
                new EmbedFieldBuilder().WithName("Mitglied:").WithValue(arg1.ToString()).WithIsInline(true),
                new EmbedFieldBuilder().WithName("Mod:").WithValue(mod.Mention).WithIsInline(true)
            });
            builder.WithFooter($"User-ID: {arg1.Id}");
            channel.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
            return Task.CompletedTask;
        }

        private static Task SocketClientOnUserBanned(SocketUser arg1, SocketGuild arg2)
        {
            if(Program.instance.Settings.serverLogChannel == -1) return Task.CompletedTask;
            var channel = arg2.GetTextChannel((ulong) Program.instance.Settings.serverLogChannel);
            Task.Delay(250).GetAwaiter().GetResult();
            var logs = arg2.GetAuditLogsAsync(10).FlattenAsync().GetAwaiter().GetResult();
            IUser mod = null;
            string reason = null;
            foreach (var log in logs)
            {
                if(log.Action != ActionType.Ban) continue;
                if((log.Data as BanAuditLogData).Target.Id != arg1.Id) continue;
                mod = log.User;
                reason = log.Reason;
                break;
            }
            var builder = new EmbedBuilder();
            builder.WithTitle("Mitglied wurde gebannt !").WithColor(Color.Red).WithThumbnailUrl(arg1.GetAvatarUrl());
            if (reason == null)
            {
                builder.WithFields(new[]
                {
                    new EmbedFieldBuilder().WithName("Mitglied:").WithValue(arg1.ToString()).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Mod:").WithValue(mod.Mention).WithIsInline(true),
                });
            }
            else
            {
                builder.WithFields(new[]
                {
                    new EmbedFieldBuilder().WithName("Mitglied:").WithValue(arg1.ToString()).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Mod:").WithValue(mod.Mention).WithIsInline(false),
                    new EmbedFieldBuilder().WithName("Grund:").WithValue(reason).WithIsInline(false)
                });
            }
            builder.WithFooter($"User-ID: {arg1.Id}");
            LevelManager.Get().DeleteProfile(arg1.Id);
            channel.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
            return Task.CompletedTask;
        }

        private static Task SocketClientOnUserLeft(SocketGuildUser arg)
        {
            LevelManager.Get().DeleteProfile(arg.Id);
            var channel = arg.Guild.GetTextChannel((ulong) Program.instance.Settings.serverLogChannel);
            Task.Delay(250).GetAwaiter().GetResult();
            var logs = arg.Guild.GetAuditLogsAsync(10).FlattenAsync().GetAwaiter().GetResult();
            var kicked = false;
            IUser mod = null;
            string reason = null;
            foreach(var log in logs)
            {
                if(log.Action != ActionType.Kick) continue;
                if((log.Data as KickAuditLogData).Target.Id != arg.Id) continue;
                kicked = true;
                mod = log.User;
                reason = log.Reason;
                break;
            }

            var builder = new EmbedBuilder();
            if (kicked)
            {
                if(Program.instance.Settings.serverLogChannel == -1) return Task.CompletedTask;
                builder.WithTitle("Mitglied gekickt !").WithColor(new Color(255, 255, 0))
                    .WithThumbnailUrl(arg.GetAvatarUrl());
                EmbedFieldBuilder[] fields;
                if (reason == null)
                {
                    fields = new[]
                    {
                        new EmbedFieldBuilder().WithName("Mitglied: ").WithValue(arg.ToString()).WithIsInline(true),
                        new EmbedFieldBuilder().WithName("Mod: ").WithValue(mod.Mention).WithIsInline(true)
                    };
                }
                else
                {
                    fields = new[]
                    {
                        new EmbedFieldBuilder().WithName("Mitglied: ").WithValue(arg.ToString()).WithIsInline(true),
                        new EmbedFieldBuilder().WithName("Mod: ").WithValue(mod.Mention).WithIsInline(true),
                        new EmbedFieldBuilder().WithName("Grund: ").WithValue(reason).WithIsInline(false)
                    };
                }

                builder.WithFields(fields);
                builder.WithFooter($"User-ID: {arg.Id}");
            }
            else
            {
                if(Program.instance.Settings.welcomeChannel == -1) return Task.CompletedTask;
                builder.WithTitle($"{arg} hat den Server verlassen !").WithColor(Color.DarkerGrey)
                    .WithThumbnailUrl(arg.GetAvatarUrl());
                builder.WithDescription($"Wir wünschen {arg} dennoch weiterhin viel Spaß !");
                channel = arg.Guild.GetTextChannel((ulong) Program.instance.Settings.welcomeChannel);
            }
            channel.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
            return Task.CompletedTask;
        }

        private static Task SocketClientOnUserJoined(SocketGuildUser arg)
        {
            if(Program.instance.Settings.welcomeChannel == -1) return Task.CompletedTask;
            var channel = arg.Guild.GetTextChannel((ulong) Program.instance.Settings.welcomeChannel);
            var builder = new EmbedBuilder();
            builder.WithTitle("Neues Mitglied !").WithThumbnailUrl(arg.GetAvatarUrl()).WithColor(Color.Green);
            var regeln = "Regeln";
            if (Program.instance.Settings.levelupChannel != -1) regeln = arg.Guild.GetTextChannel((ulong) Program.instance.Settings.ruleChannel).Mention;
            builder.WithDescription(
                $"Willkommen zur Tjulfars Bande, {arg} !\nLies dir doch als erstes die Regeln ({regeln}) durch !");
            channel.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
            return Task.CompletedTask;
        }

        private static Task SocketClientOnMessageReceived(SocketMessage arg)
        {
            if(arg.Author.IsBot) return Task.CompletedTask;
            if(!(arg.Channel is SocketGuildChannel)) return Task.CompletedTask;
            string message = arg.Content;
            if (message.StartsWith(Program.instance.Settings.prefix))
            {
                string cmd = message.Substring(1);
                foreach (var command in CommandManager.Commands)
                {
                    if (cmd.Length == command.name.Length)
                    {
                        if (cmd.Equals(command.name))
                        {
                            CommandManager.ExecuteCommand(command, SocketClient, arg, new string[0]);
                        }
                    }
                    else
                    {
                        string[] cmdArray = cmd.Split(" ");
                        if (cmdArray[0].Equals(command.name))
                        {
                            if (cmdArray.Length == 1)
                            {
                                CommandManager.ExecuteCommand(command, SocketClient, arg, new string[0]);
                            }
                            else
                            {
                                string[] args = new string[cmdArray.Length - 1];
                                Array.Copy(cmdArray, 1, args, 0, cmdArray.Length - 1);
                                CommandManager.ExecuteCommand(command, SocketClient, arg, args);
                            }
                        }
                    }
                }
            }
            else
            {
                if (!LevelManager.Get().ExistsLevelChannel(arg.Channel.Id)) return Task.CompletedTask;
                if (LevelManager.Get().ExistsProfile(arg.Author.Id))
                {
                    LevelManager.Get().GetProfile(arg.Author.Id).AddExp(1 * Multiplier.GetMultiplier((SocketGuildUser) arg.Author).MultiplierNumber);
                }
                else LevelManager.Get().CreateProfile(arg.Author.Id).AddExp(1 * Multiplier.GetMultiplier((SocketGuildUser) arg.Author).MultiplierNumber);
            }
            return Task.CompletedTask;
        }

        private static bool _wasReady = false;
        private static Task SocketClientOnReady()
        {
            if (!_wasReady)
            {
                Program.instance.DatabaseManager.Init();
                TempbanManager.Get().Init();
                YoutubeManager.Get().Init();
                WarnManager.Get().Init();
                LevelManager.Get().CheckForValidEntries();
                VoiceCounter.Get().Start();
                ConsoleListener.Get().Start();
                _wasReady = true;
            }
            Console.WriteLine("Ready !");
            return Task.CompletedTask;
        }

        private static Task SocketClientOnLog(LogMessage arg)
        {
            Console.WriteLine(arg.ToString());
            return Task.CompletedTask;
        }

        private static Task SocketClientOnConnected()
        {
            Console.WriteLine("Connected to " + SocketClient.CurrentUser);
            return Task.CompletedTask;
        }
        
        
    }
}