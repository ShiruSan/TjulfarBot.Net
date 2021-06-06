using MySql.Data.MySqlClient;

namespace TjulfarBot.Net.Managers
{
    public class DatabaseManager
    {

        private readonly MySqlConnectionStringBuilder _sqlConnectionStringBuilder = new MySqlConnectionStringBuilder()
        {
            Server = Program.instance.Settings.mySQLConfig.ip,
            Port = (uint) Program.instance.Settings.mySQLConfig.port,
            UserID = Program.instance.Settings.mySQLConfig.username,
            Password = Program.instance.Settings.mySQLConfig.password,
            Database = Program.instance.Settings.mySQLConfig.database
        };

        public MySqlConnection GetConnection()
        {
            var connection = new MySqlConnection(_sqlConnectionStringBuilder.ToString());
            connection.Open();
            return connection;
        }
        
        public void Init()
        {
            using var connection = GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE IF NOT EXISTS  `TempBans` (" +
                                  "id integer not null," +
                                  "userid bigint not null," +
                                  "passing bigint not null," +
                                  "passed boolean not null," +
                                  "Primary Key(id));";
            command.Prepare();
            command.ExecuteNonQuery();
            command.CommandText = "CREATE TABLE IF NOT EXISTS  `Mutes` (" +
                                  "`id` integer not null," +
                                  "`userid` bigint not null," +
                                  "`passing` bigint null," +
                                  "`passed` boolean not null," +
                                  "`type` integer not null," +
                                  "`reason` varchar(1000) null," +
                                  "Primary Key(id));";
            command.Prepare();
            command.ExecuteNonQuery();
            command.CommandText = "CREATE TABLE IF NOT EXISTS  `Youtube` (" +
                                  "`channelid` varchar(100) not null," +
                                  "`channelname` varchar(100) not null," +
                                  "Primary Key(channelid));";
            command.Prepare();
            command.ExecuteNonQuery();
        }
        
    }
}