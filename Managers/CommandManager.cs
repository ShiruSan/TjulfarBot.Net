using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using TjulfarBot.Net.Commands;
using TjulfarBot.Net.Leveling;
using TjulfarBot.Net.Tempban;

namespace TjulfarBot.Net.Managers
{
    public class CommandManager
    {
        public List<Command> Commands { get; } = new List<Command>();

        public CommandManager()
        {
            Commands.Add(new About());
            Commands.Add(new Announcement());
            Commands.Add(new Channel());
            Commands.Add(new Clear());
            Commands.Add(new LevelCommand());
            Commands.Add(new TempbanCommand());
        }

        public async void ExecuteCommand(Command command, DiscordSocketClient socketClient, SocketMessage message, string[] arguments)
        {
            await Task.Delay(1);
            SocketGuildUser guildUser = (SocketGuildUser) message.Author;
            if (command.Permission != null)
            {
                if (command.Permission is ChannelPermission channelPermission)
                {
                    if (!guildUser.GetPermissions((SocketGuildChannel)message.Channel).Has(channelPermission))
                    {
                        await message.Channel.SendMessageAsync(":x: Dazu hast du nicht die Rechte !");
                        return;
                    }
                }
                else if (command.Permission is GuildPermission guildPermission)
                {
                    if (!guildUser.GuildPermissions.Has(guildPermission))
                    {
                        await message.Channel.SendMessageAsync(":x: Dazu hast du nicht die Rechte !");
                        return;
                    }
                }
            }
            command.OnCommand(new CommandContext(socketClient, message, arguments));
        }
        
    }

    public class CommandContext
    {
        public DiscordSocketClient SocketClient;
        public SocketGuildUser Author;
        public ISocketMessageChannel Channel;
        public SocketMessage Message;
        public SocketGuild Guild;
        public SocketSelfUser SelfUser;
        public string[] Arguments;

        public CommandContext(DiscordSocketClient socketClient, SocketMessage message, string[] arguments)
        {
            SocketClient = socketClient;
            Author = (SocketGuildUser) message.Author;
            Channel = message.Channel;
            Guild = Author.Guild;
            SelfUser = socketClient.CurrentUser;
            Message = message;
            Arguments = arguments;
        }
    }
    
}