using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Discord.WebSocket;

namespace TjulfarBot.Net.Leveling
{
    public class Multiplier
    {
        private static Multiplier @default = new();
        public float MultiplierNumber = 1;

        public static Multiplier GetMultiplier(SocketGuildUser user)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "select * from `LevelConfig` where `id` = @id && `type` = @type";
            foreach (var role in user.Roles)
            {
                if (!role.IsEveryone)
                {
                    if(command.Parameters.Count > 0) command.Parameters.Clear();
                    command.Parameters.AddWithValue("@id", role.Id);
                    command.Parameters.AddWithValue("@type", "multirole");
                    command.Prepare();
                    using (var reader = command.ExecuteReader())
                    {
                        if(!reader.Read()) continue;
                        return new Multiplier()
                        {
                            MultiplierNumber = float.Parse(reader.GetString(2))
                        };
                    }
                }
            }
            return @default;
        }

        public static IReadOnlyList<SocketRole> GetMultiplierRoles(SocketGuildUser user)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "select * from `LevelConfig` where `id` = @id && `type` = @type";
            var readOnlyBuilder = new ReadOnlyCollectionBuilder<SocketRole>();
            foreach (var role in user.Roles)
            {
                if(role.IsEveryone) continue;
                if(command.Parameters.Count > 0) command.Parameters.Clear();
                command.Parameters.AddWithValue("@id", role.Id);
                command.Parameters.AddWithValue("@type", "multirole");
                command.Prepare();
                using (var reader = command.ExecuteReader())
                {
                    if(!reader.Read()) continue;
                    readOnlyBuilder.Add(role);
                }
            }
            return readOnlyBuilder.ToReadOnlyCollection();
        }

    }
}