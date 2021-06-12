using System;
using System.Text.Json.Serialization;

namespace TjulfarBot.Net.Utils
{
    public class Settings
    {
        public String prefix;
        public long welcomeChannel;
        public long serverLogChannel;
        public long announcementChannel;
        public long ruleChannel;
        public long levelupChannel;
        public long videoUploadChannel;
        public long defaultGuild;
        public MySQLConfig mySQLConfig;

        public static Settings Default = CreateDefault();

        private static Settings CreateDefault()
        {
            Settings settings = new Settings();
            settings.prefix = "+";
            settings.welcomeChannel = -1;
            settings.serverLogChannel = -1;
            settings.announcementChannel = -1;
            settings.ruleChannel = -1;
            settings.levelupChannel = -1;
            settings.videoUploadChannel = -1;
            settings.mySQLConfig = new MySQLConfig();
            settings.mySQLConfig.use = false;
            settings.mySQLConfig.ip = "";
            settings.mySQLConfig.port = 1111;
            settings.mySQLConfig.database = "";
            settings.mySQLConfig.username = "";
            settings.mySQLConfig.password = "";
            return settings;
        }
        
    }
    
    public class MySQLConfig {
        public bool use;
        public String ip;
        public int port;
        public String database;
        public String username;
        public String password;
    }
    
}