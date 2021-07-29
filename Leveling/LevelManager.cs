using System;
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

        public void CheckForValidEntries()
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "select * from `Levels`";
            command.Prepare();
            using var reader = command.ExecuteReader();
            int unvalidEntries = 0;
            while (reader.Read())
            {
                var user = Program.instance.SocketClient.GetUser((ulong) reader.GetInt64(0));
                if (user == null)
                {
                    DeleteProfile((ulong) reader.GetInt64(0));
                    unvalidEntries++;
                }
            }
            Console.WriteLine($"Level validation complete!\n{unvalidEntries} unvalid Entires deleted !");
        }

        public bool IsOnLeaderboard(ulong userid)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "select * from `Levels` ORDER BY `level` DESC,`exp` DESC LIMIT 10";
            command.Prepare();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (reader.GetUInt64(0) == userid) return true;
            }
            return false;
        }

        public int GetLeaderboardRank(ulong userid)
        {
            var i = 1;
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "select * from `Levels` ORDER BY `level` DESC,`exp` DESC";
            command.Prepare();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (reader.GetUInt64(0) != userid) i++;
                else break;
            }
            return i;
        }

        public string[] GetLeaderboard(ulong requester)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "select * from `Levels` ORDER BY `level` DESC,`exp` DESC LIMIT 10";
            command.Prepare();
            using var reader = command.ExecuteReader();
            var lines = new string[10];
            var i = 0;
            while (reader.Read())
            {
                var user = Program.instance.SocketClient.GetUser((ulong) reader.GetInt64(0));
                if (user.Id == requester)
                {
                    lines[i] = $"   **{i + 1}.** {user} [Level {reader.GetInt32(1)}] (Du)";
                }
                else
                {
                    lines[i] = $"   **{i + 1}.** {user} [Level {reader.GetInt32(1)}]";
                }
                i++;
            }
            return lines;
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
            command.CommandText = "delete from `LevelConfig` where `id` = @id";
            command.Parameters.AddWithValue("@id", userid);
            command.Prepare();
            command.ExecuteNonQuery();
        }
        
        public void RemoveLevelChannel(ulong channelid)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "delete from `LevelConfig` where `id` = @id";
            command.Parameters.AddWithValue("@id", channelid);
            command.Prepare();
            command.ExecuteNonQuery();
        }
        
        public bool ExistsBlacklist(ulong userid)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "select * from `LevelConfig` where id = @id && `type` = @type";
            command.Parameters.AddWithValue("@id", userid);
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

        public void DeleteProfile(ulong id)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "delete from `Levels` where `userid` = @userid";
            command.Parameters.AddWithValue("@userid", id);
            command.Prepare();
            command.ExecuteNonQuery();
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
            return new Profile((long) userid, reader.GetInt32(1), reader.GetFloat(2));
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
                new EmbedFieldBuilder().WithName("Exp. bis n\u00e4chsten Aufstieg").WithValue(profile.level * 100).WithIsInline(false)
            });
            channel.SendMessageAsync(null, false, builder.Build()).GetAwaiter().GetResult();
        }
        
    }
}