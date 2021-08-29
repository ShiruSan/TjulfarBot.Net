using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using TjulfarBot.Net.Economy;
using TjulfarBot.Net.Leveling;
using TjulfarBot.Net.Managers;
using TjulfarBot.Net.Tempban;
using TjulfarBot.Net.Utils;
using TjulfarBot.Net.Warn;
using TjulfarBot.Net.Youtube;
using Color = Discord.Color;
using Image = System.Drawing.Image;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

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
            if (kicked && Program.instance.Settings.serverLogChannel != -1)
            {
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
                arg.Guild.GetTextChannel((ulong) Program.instance.Settings.serverLogChannel).SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
            }

            if (Program.instance.Settings.welcomeChannel != -1)
            {
                builder.WithTitle($"{arg} hat den Server verlassen !").WithColor(Color.DarkerGrey)
                    .WithThumbnailUrl(arg.GetAvatarUrl());
                builder.WithDescription($"Wir wünschen {arg} dennoch weiterhin viel Spaß !");
                arg.Guild.GetTextChannel((ulong) Program.instance.Settings.welcomeChannel).SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
            }
            
            if(EcoManager.Get().HasBankAccount(arg.Id) || EcoManager.Get().HasPocketMoney(arg.Id)) EcoManager.Get().RemoveAccounts(arg.Id);
            
            return Task.CompletedTask;
        }

        private static Font calibril;
        private static Task SocketClientOnUserJoined(SocketGuildUser arg)
        {
            foreach (var autorole in AutoroleManager.Get().GetAutoroles())
            {
                arg.AddRoleAsync(autorole).GetAwaiter().GetResult();
            }
            if(Program.instance.Settings.welcomeChannel == -1) return Task.CompletedTask;
            var channel = arg.Guild.GetTextChannel((ulong) Program.instance.Settings.welcomeChannel);
            var webclient = new WebClient();
            webclient.DownloadDataCompleted += (sender, args) =>
            {
                var downloadedBytes = args.Result;
                var stream = new MemoryStream();
                stream.Write(downloadedBytes, 0, downloadedBytes.Length);
                using var pb = Image.FromStream(stream);
                stream.Close();
                using var welcome = Image.FromFile("TjulfarWelcome.png");
                using var graphics = Graphics.FromImage(welcome);
                graphics.DrawString(arg.ToString(), calibril, new SolidBrush(System.Drawing.Color.White), new PointF(280, 70));
                graphics.DrawImage(pb, new Point(280, 110));
                graphics.Save();
                welcome.Save("welcome.png", ImageFormat.Png);
                channel.SendFileAsync("welcome.png", arg.Mention).GetAwaiter().GetResult();
                webclient.Dispose();
                File.Delete("welcome.png");
            };
            webclient.DownloadDataAsync(new Uri(arg.GetAvatarUrl()));
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
                using var privateFontCollection = new PrivateFontCollection();
                var fontBytes = File.ReadAllBytes("calibril.ttf");
                var handle = GCHandle.Alloc(fontBytes, GCHandleType.Pinned);
                var pointer = handle.AddrOfPinnedObject();
                try
                {
                    privateFontCollection.AddMemoryFont(pointer, fontBytes.Length);
                }
                finally
                {
                    handle.Free();
                }
                calibril = new Font(privateFontCollection.Families.First(), 15);
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