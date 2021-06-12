using Discord;
using Discord.WebSocket;

namespace TjulfarBot.Net.Leveling
{
    public class Profile
    {
        public long userid { get; private set; }
        public int level { get; private set; }
        public int messages { get; private set; }

        public Profile(long userid, int level, int messages)
        {
            this.userid = userid;
            this.level = level;
            this.messages = messages;
        }

        public void CreateDatabaseEntry()
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "insert into `Levels` values (@id, @level, @messages)";
            command.Parameters.AddWithValue("@id", userid);
            command.Parameters.AddWithValue("@level", level);
            command.Parameters.AddWithValue("@messages", messages);
            command.Prepare();
            command.ExecuteNonQuery();
        }

        public void AddMessage(int count)
        {
            messages += count;
            int nextLevel = 10;
            if (level != 0) nextLevel = (level * 100);
            if (messages >= nextLevel)
            {
                level++;
                messages = 0;
                LevelManager.Get().OnLevelUp(this);
            }

            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "update `Levels` set `level` = @level, `messages` = @messages where `userid` = @userid";
            command.Parameters.AddWithValue("@level", level);
            command.Parameters.AddWithValue("@messages", messages);
            command.Parameters.AddWithValue("@userid", userid);
            command.Prepare();
            command.ExecuteNonQuery();
        }

        public Embed CreateEmbed()
        {
            EmbedBuilder builder = new EmbedBuilder();
            SocketUser user = Program.instance.SocketClient.GetUser((ulong) userid);
            builder.WithTitle("Level von " + user).WithColor(Color.Blue);
            builder.WithThumbnailUrl(user.GetAvatarUrl());
            int nextLevel = 10;
            if (level != 0) nextLevel = (100 * level);
            builder.WithFields(new []
            {
                new EmbedFieldBuilder().WithName("Aktuelles Level:").WithValue(level).WithIsInline(false),
                new EmbedFieldBuilder().WithName("Nachrichten bis zum n\u00e4chsten Aufstieg").WithValue(messages + "/" + nextLevel).WithIsInline(false)
            });
            return builder.Build();
        }
        
    }
}