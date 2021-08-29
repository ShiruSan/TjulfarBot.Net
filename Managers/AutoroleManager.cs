using System.Collections.Generic;
using Discord.WebSocket;

namespace TjulfarBot.Net.Managers
{
    public class AutoroleManager
    {
        private static AutoroleManager _instance;

        public static AutoroleManager Get()
        {
            if (_instance == null) _instance = new AutoroleManager();
            return _instance;
        }
        
        public bool IsAutorole(ulong roleid)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "select * from `Autorole` where `roleid` = @roleid";
            command.Parameters.AddWithValue("@roleid", roleid);
            command.Prepare();
            using var reader = command.ExecuteReader();
            return reader.HasRows;
        }

        public void AddAutorole(ulong roleid)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "insert into `Autorole` values (@roleid)";
            command.Parameters.AddWithValue("@roleid", roleid);
            command.Prepare();
            command.ExecuteNonQuery();
        }
        
        public void RemoveAutorole(ulong roleid)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "delete from `Autorole` where `roleid` = @roleid)";
            command.Parameters.AddWithValue("@roleid", roleid);
            command.Prepare();
            command.ExecuteNonQuery();
        }

        public IReadOnlyList<SocketRole> GetAutoroles()
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "select * from `Autorole`";
            command.Prepare();
            var list = new List<SocketRole>();
            using (var reader = command.ExecuteReader())
            {
                var defaultGuild = Program.instance.SocketClient.GetGuild(Program.instance.Settings.defaultGuild);
                while (reader.Read())
                {
                    var role = defaultGuild.GetRole(reader.GetUInt64(0));
                    if(role != null) list.Add(role);
                }
            }
            return list.AsReadOnly();
        }

    }
}