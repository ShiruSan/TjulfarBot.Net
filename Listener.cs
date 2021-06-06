using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
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
            return Task.CompletedTask;
        }

        private static Task SocketClientOnReady()
        {
            Console.WriteLine("Ready !");
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