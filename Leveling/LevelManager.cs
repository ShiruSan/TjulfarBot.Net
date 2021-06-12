using Discord;
using Discord.WebSocket;

namespace TjulfarBot.Net.Leveling
{
    public class LevelManager
    {
        private static LevelManager _levelManager;

        public static LevelManager Get()
        {
            if (_levelManager == null) _levelManager = new LevelManager();
            return _levelManager;
        }

        public void AddBlacklist(ulong userid)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "insert into `LevelConfig` values (@id, @type)";
            command.Parameters.AddWithValue("@id", userid);
            command.Parameters.AddWithValue("@type", "blacklist");
            command.Prepare();
            command.ExecuteNonQuery();
        }
        
        public void AddLevelChannel(ulong channelid)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "insert into `LevelConfig` values (@id, @type)";
            command.Parameters.AddWithValue("@id", channelid);
            command.Parameters.AddWithValue("@type", "channel");
            command.Prepare();
            command.ExecuteNonQuery();
        }
        
        public void RemoveBlacklist(ulong userid)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "delete fron `LevelConfig` where `id` = @id";
            command.Parameters.AddWithValue("@id", userid);
            command.Prepare();
            command.ExecuteNonQuery();
        }
        
        public void RemoveLevelChannel(ulong channelid)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "delete fron `LevelConfig` where `id` = @id";
            command.Parameters.AddWithValue("@id", channelid);
            command.Prepare();
            command.ExecuteNonQuery();
        }
        
        public bool ExistsBlacklist(ulong channelid)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "select * from `LevelConfig` where id = @id && `type` = @type";
            command.Parameters.AddWithValue("@id", channelid);
            command.Parameters.AddWithValue("@type", "blacklist");
            command.Prepare();
            using var reader = command.ExecuteReader();
            return reader.HasRows;
        }
        
        public bool ExistsLevelChannel(ulong channelid)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "select * from `LevelConfig` where `id` = @id && `type` = @type";
            command.Parameters.AddWithValue("@id", channelid);
            command.Parameters.AddWithValue("@type", "channel");
            command.Prepare();
            using var reader = command.ExecuteReader();
            return reader.HasRows;
        }

        public bool ExistsProfile(ulong userid)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "select * from `Levels` where userid = @userid";
            command.Parameters.AddWithValue("@userid", userid);
            command.Prepare();
            using var reader = command.ExecuteReader();
            return reader.HasRows;
        }

        public Profile CreateProfile(ulong userid)
        {
            var profile = new Profile((long) userid, 0, 0);
            profile.CreateDatabaseEntry();
            return profile;
        }

        public Profile GetProfile(ulong userid)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "select * from `Levels` where userid = @userid";
            command.Parameters.AddWithValue("@userid", userid);
            command.Prepare();
            using var reader = command.ExecuteReader();
            reader.Read();
            return new Profile((long) userid, reader.GetInt32(1), reader.GetInt32(2));
        }

        public void OnLevelUp(Profile profile)
        {
            if(Program.instance.Settings.levelupChannel == -1) return;
            SocketTextChannel channel =  Program.instance.SocketClient.GetGuild((ulong) Program.instance.Settings.defaultGuild)
                .GetTextChannel((ulong) Program.instance.Settings.levelupChannel);
            SocketUser user = Program.instance.SocketClient.GetUser((ulong) profile.userid);
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithColor(Color.Green).WithTitle("Levelaufstieg !").WithThumbnailUrl(user.GetAvatarUrl());
            builder.WithDescription(user.Mention + " stieg auf Level " + profile.level + " !");
            builder.WithFields(new[]
            {
                new EmbedFieldBuilder().WithName("Nachrichten bis n\u00e4chsten Aufstieg").WithValue(profile.level * 100).WithIsInline(false)
            });
            channel.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
        }
        
    }
}