using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using TjulfarBot.Net.Utils;

namespace TjulfarBot.Net
{
    class Program
    {
        public static Program instance { get; private set; }
        public Settings Settings { get; }
        public DiscordSocketClient SocketClient { get; private set; }

        private Program()
        {
            if (File.Exists("settings.json"))
            {
                Settings = (Settings) JsonTool.Deserialize(typeof(Utils.Settings), "settings.json");
            }
            else
            {
                Settings = Settings.Default;
                JsonTool.Serialize(Settings, "settings.json");
            }
        }

        private async Task StartBot()
        {
            SocketClient = new DiscordSocketClient();
            
            Listener.Set(SocketClient);

            StreamReader reader = new StreamReader(File.OpenRead("token.txt"));
            await SocketClient.LoginAsync(TokenType.Bot, await reader.ReadToEndAsync());
            reader.Close();
            await SocketClient.StartAsync();
            await SocketClient.SetGameAsync("Tjulfars Kanal", null, ActivityType.Watching);

            await Task.Delay(-1);
        }
        
        static void Main(string[] args)
        {
            instance = new Program();
            instance.StartBot().GetAwaiter().GetResult();
        }

        public async Task Close()
        {
            await SocketClient.StopAsync();
            await SocketClient.LogoutAsync();
            SocketClient.Dispose();
            
            JsonTool.Serialize(Settings, "settings.json");
        }
        
    }
}