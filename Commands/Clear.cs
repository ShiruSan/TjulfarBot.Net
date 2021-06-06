using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using TjulfarBot.Net.Managers;

namespace TjulfarBot.Net.Commands
{
    public class Clear : Command
    {
        public Clear() : base("clear", ChannelPermission.ManageMessages)
        {
        }
        
        public override async void OnCommand(CommandContext ctx)
        {
            if (ctx.Arguments.Length == 1)
            {
                try
                {
                    int count = int.Parse(ctx.Arguments[0]);
                    if (count > 100)
                    {
                        await ctx.Channel.SendMessageAsync(":x: `" + count + "` ist eine zu hohe Zahl\nDer Limit liegt bei 100 !");
                    }
                    else if (count <= 0)
                    {
                        await ctx.Channel.SendMessageAsync(":x: `" + count + "` ist eine zu geringe Zahl\nDas Minmum leigt bei 1 !");
                    }
                    else
                    {
                        var messages = await ctx.Channel.GetMessagesAsync(count).FlattenAsync();
                        await (ctx.Channel as SocketTextChannel).DeleteMessagesAsync(messages);
                        var message2 = await ctx.Channel.SendMessageAsync(":white_check_mark: `" + count + "` Nachrichten wurden gelöscht !");
                        await Task.Delay(3000);
                        await message2.DeleteAsync();
                    }
                }
                catch (FormatException)
                {
                    await ctx.Channel.SendMessageAsync(":x: `" + ctx.Arguments[0] + "` ist keine valide Zahl !");
                }
            }
            else SendHelpMessage(ctx.Channel);
        }
        
        private void SendHelpMessage(ISocketMessageChannel channel)
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithColor(Color.Blue);
            builder.WithTitle("Clear Command");
            builder.WithDescription("Der Clear Command ist dafür da um eine bestimmte Anzahl letzter Nachrichten zu l\u00F6schen.");
            List<EmbedFieldBuilder> fieldBuilders = new List<EmbedFieldBuilder>();
            fieldBuilders.Add(new EmbedFieldBuilder().WithName("Nutzung des Commands").WithValue("`+clear [Anzahl letzter Nachrichten]`").WithIsInline(false));
            fieldBuilders.Add(new EmbedFieldBuilder().WithName("Kleiner Hinweis").WithValue("Das Limit an Nachrichten liegt bei 100!\nDie Nutzung diesen Commandes im selben Channel, bevor das vorherige Clearen zu Ende ist, kann Bot Fehler ausl\u00F6sen !").WithIsInline(false));
            builder.WithFields(fieldBuilders);
            channel.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
        }
        
    }
}