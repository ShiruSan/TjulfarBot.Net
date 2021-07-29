using System;
using MySql.Data.MySqlClient;

namespace TjulfarBot.Net.Managers
{
    public class DatabaseManager
    {

        private MySqlConnectionStringBuilder _sqlConnectionStringBuilder;

        public MySqlConnection GetConnection()
        {
            var connection = new MySqlConnection(_sqlConnectionStringBuilder.ToString());
            try
            {
                connection.Open();
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e);
                Console.WriteLine("\n");
                Console.WriteLine(e.InnerException);
                throw;
            }
            return connection;
        }
        
        public void Init()
        {
            _sqlConnectionStringBuilder = new MySqlConnectionStringBuilder()
            {
                Server = Program.instance.Settings.mySQLConfig.ip,
                Port = (uint) Program.instance.Settings.mySQLConfig.port,
                UserID = Program.instance.Settings.mySQLConfig.username,
                Password = Program.instance.Settings.mySQLConfig.password,
                Database = Program.instance.Settings.mySQLConfig.database,
                SslMode = MySqlSslMode.None
            };
            
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
            command.CommandText = "CREATE TABLE IF NOT EXISTS `Eco`" +
                                  "( `id` INT NOT NULL ," +
                                  "`userid` BIGINT NOT NULL ," +
                                  " `money` INT NOT NULL ," +
                                  "`accountType` VARCHAR(12) NOT NULL ," +
                                  "PRIMARY KEY (`id`));";
            command.Prepare();
            command.ExecuteNonQuery();
        }
        
    }
}