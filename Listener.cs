using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using TjulfarBot.Net.Leveling;
using TjulfarBot.Net.Managers;
using TjulfarBot.Net.Tempban;
using TjulfarBot.Net.Utils;

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
        }

        private static Task SocketClientOnUserUnbanned(SocketUser arg1, SocketGuild arg2)
        {
            if(Program.instance.Settings.serverLogChannel == -1) return Task.CompletedTask;
            var channel = arg2.GetTextChannel((ulong) Program.instance.Settings.serverLogChannel);
            Task.Delay(1000).GetAwaiter().GetResult();
            var logs = arg2.GetAuditLogsAsync(10).FlattenAsync().GetAwaiter().GetResult();
            IUser mod = null;
            foreach (var log in logs)
            {
                if(log.Id != arg1.Id) continue;
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
            channel.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
            return Task.CompletedTask;
        }

        private static Task SocketClientOnUserBanned(SocketUser arg1, SocketGuild arg2)
        {
            if(Program.instance.Settings.serverLogChannel == -1) return Task.CompletedTask;
            var channel = arg2.GetTextChannel((ulong) Program.instance.Settings.serverLogChannel);
            Task.Delay(1000).GetAwaiter().GetResult();
            var logs = arg2.GetAuditLogsAsync(10).FlattenAsync().GetAwaiter().GetResult();
            IUser mod = null;
            string reason = null;
            foreach (var log in logs)
            {
                if(log.Id != arg1.Id) continue;
                if(log.Action != ActionType.Ban) continue;
                mod = log.User;
                reason = log.Reason;
                break;
            }
            var builder = new EmbedBuilder();
            builder.WithTitle("Mitglied wurde gebannt !").WithColor(Color.Red).WithThumbnailUrl(arg1.GetAvatarUrl());
            builder.WithFields(new[]
            {
                new EmbedFieldBuilder().WithName("Mitglied:").WithValue(arg1.ToString()).WithIsInline(true),
                new EmbedFieldBuilder().WithName("Mod:").WithValue(mod.Mention).WithIsInline(true)
            });
            channel.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
            return Task.CompletedTask;
        }

        private static Task SocketClientOnUserLeft(SocketGuildUser arg)
        {
            if(Program.instance.Settings.serverLogChannel == -1) return Task.CompletedTask;
            var channel = arg.Guild.GetTextChannel((ulong) Program.instance.Settings.serverLogChannel);
            Task.Delay(1000).GetAwaiter().GetResult();
            var logs = arg.Guild.GetAuditLogsAsync(10).FlattenAsync().GetAwaiter().GetResult();
            var kicked = false;
            IUser mod = null;
            string reason = null;
            foreach(var log in logs)
            {
                if(log.Id != arg.Id) continue;
                if(log.Action != ActionType.Kick) continue;
                kicked = true;
                mod = log.User;
                reason = log.Reason;
                break;
            }

            var builder = new EmbedBuilder();
            if (kicked)
            {
                builder.WithTitle("Mitglied gekickt !").WithColor(new Color(255, 255, 0)).WithThumbnailUrl(arg.GetAvatarUrl());
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
            }
            else
            {
                builder.WithTitle("Mitglied hat den Server verlassen").WithColor(Color.Blue)
                    .WithThumbnailUrl(arg.GetAvatarUrl());
                builder.WithFields(new[]
                {
                    new EmbedFieldBuilder().WithName("Mitglied:").WithValue(arg.ToString()).WithIsInline(false)
                });
            }
            channel.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
            return Task.CompletedTask;
        }

        private static Task SocketClientOnUserJoined(SocketGuildUser arg)
        {
            if(Program.instance.Settings.welcomeChannel == -1) return Task.CompletedTask;
            var channel = arg.Guild.GetTextChannel((ulong) Program.instance.Settings.welcomeChannel);
            var builder = new EmbedBuilder();
            builder.WithTitle("Neues Mitglied !").WithThumbnailUrl(arg.GetAvatarUrl());
            var regeln = "Regeln";
            if (Program.instance.Settings.levelupChannel != -1) regeln = arg.Guild.GetTextChannel((ulong) Program.instance.Settings.ruleChannel).Mention;
            builder.WithDescription(
                $"Willkommen zur Tjulfars Bande, {arg.Mention} !\nLies dir doch als erstes die Regeln ({regeln}) durch !");
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
                if (!LevelManager.Get().ExistsLevelChannel(arg.Id)) return Task.CompletedTask;
                if (LevelManager.Get().ExistsProfile(arg.Author.Id))
                {
                    LevelManager.Get().GetProfile(arg.Author.Id).AddMessage(1);
                }
                else LevelManager.Get().CreateProfile(arg.Author.Id).AddMessage(1);
            }
            return Task.CompletedTask;
        }

        private static Task SocketClientOnReady()
        {
            Console.WriteLine("Ready !");
            Program.instance.DatabaseManager.Init();
            ConsoleListener.Get().Start();
            TempbanManager.Get().Init();
            return Task.CompletedTask;
        }

        private static Task SocketClientOnLog(LogMessage arg)
        {
            Console.WriteLine(arg.ToString());
            return Task.CompletedTask;
        }

        private static Task SocketClientOnConnected()
        {
            Console.WriteLine("Connected to " + SocketClient.CurrentUser.Username);
            return Task.CompletedTask;
        }
        
        
    }
}