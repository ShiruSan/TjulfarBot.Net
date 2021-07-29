using System;
using System.Collections.Generic;
using Discord.WebSocket;

namespace TjulfarBot.Net.Warn
{
    public class WarnManager
    {
        private int _nextID = 1;
        private static WarnManager instance;

        public static WarnManager Get()
        {
            if (instance == null) instance = new WarnManager();
            return instance;
        }

        public void Init()
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE IF NOT EXISTS  `Warns` (" +
                                  "id integer not null," +
                                  "userid bigint not null," +
                                  "warnedAt DATETIME not null," +
                                  "reason varchar(1000) null," +
                                  "Primary Key(id));";
            command.Prepare();
            command.ExecuteNonQuery();
            command.CommandText = "select * from `Warns`";
            command.Prepare();
            using var reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while(reader.Read())
                {
                    _nextID = reader.GetInt32(0) + 1;
                }
            }
        }
        
        public bool HasUserWarned(ulong id)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "select * from `Warns` where `id` = @id";
            command.Parameters.AddWithValue("@id", id);
            command.Prepare();
            using var reader = command.ExecuteReader();
            var i = 0;
            while (reader.Read()) i++;
            return i >= 2;
        }

        public Warn CreateWarn(SocketGuildUser user, SocketGuildUser mod, string reason)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "insert into `Warns` values(@id, @userid, @warnedAt, @reason)";
            command.Parameters.AddWithValue("@id", _nextID);
            _nextID++;
            command.Parameters.AddWithValue("@userid", user.Id);
            var warnedAt = DateTime.Now;
            command.Parameters.AddWithValue("@warnedAt", warnedAt);
            command.Parameters.AddWithValue("@reason", reason);
            command.Prepare();
            command.ExecuteNonQuery();
            return new Warn()
            {
                user = user,
                mod = mod,
                reason = reason,
                time = warnedAt
            };
        }

        public List<Warn> GetWarns(SocketGuildUser user)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "select * from `Warns` where `userid` = @userid";
            command.Parameters.AddWithValue("@userid", user.Id);
            command.Prepare();
            using var reader = command.ExecuteReader();
            var list = new List<Warn>();
            while (reader.Read())
            {
                var warn = new Warn()
                {
                    user = user,
                    mod = user.Guild.GetUser(reader.GetUInt64(1)),
                    reason = reader.GetString(3),
                    time = reader.GetDateTime(2)
                };
                list.Add(warn);
            }
            return list;
        }

        public struct Warn
        {
            public SocketGuildUser user;
            public SocketGuildUser mod;
            public DateTime time;
            public string reason;
        }
        
    }
}