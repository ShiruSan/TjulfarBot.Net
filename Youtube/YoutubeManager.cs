using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Timers;
using Discord;
using Discord.WebSocket;
using TjulfarBot.Net.Utils;

namespace TjulfarBot.Net.Youtube
{
    public class YoutubeManager
    {
        private static YoutubeManager _instance;
        private List<string> _channelIds = new List<string>();
        private readonly string _apiKey = "";
        private Timer _schedulers = new Timer();

        public static YoutubeManager Get()
        {
            if (_instance == null) _instance = new YoutubeManager();
            return _instance;
        }

        public void Init()
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "select * from `Youtube`";
            command.Prepare();
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                _channelIds.Add(reader.GetString(0));
            }
            reader.Close();
            _schedulers.Interval = (15 * 60 * 1000);
            _schedulers.Elapsed += OnSchedulerTick;
            _schedulers.AutoReset = true;
            _schedulers.Start();
        }

        public Item GetItemForID(string channelID)
        {
            try
            {
                var request = WebRequest.CreateHttp($"https://www.googleapis.com/youtube/v3/search?key={_apiKey}&channelId={channelID}&part=snippet,id&order=date&maxResults=1");
                var response = request.GetResponse();
                var reader = new StreamReader(response.GetResponseStream());
                var json = reader.ReadToEnd();
                response.Close();
                SearchResponse response2 = JsonTool.DeserializeFromString(typeof(SearchResponse), json) as SearchResponse;
                if (response2.items[0].id.kind.Equals("youtube#video")) return response2.items[0];
            }
            catch (WebException) {}
            catch(NullReferenceException) {}

            return null;
        }

        private Dictionary<string, Item> _lastUploads = new Dictionary<string, Item>();
        private void OnSchedulerTick(object sender, ElapsedEventArgs e)
        {
            if(Program.instance.Settings.videoUploadChannel == -1) return;
            var channel = Program.instance.SocketClient.GetGuild((ulong) Program.instance.Settings.defaultGuild)
                .GetTextChannel((ulong) Program.instance.Settings.videoUploadChannel);
            foreach (var id in _channelIds)
            {
                var item = GetItemForID(id);
                if(item == null) continue;
                if (!_lastUploads.ContainsKey(id))
                {
                    _lastUploads.Add(id, item);
                    continue;
                }
                if(_lastUploads.ContainsValue(item)) continue;
                var builder = new EmbedBuilder();
                builder.WithColor(Color.Red);
                if (item.snippet.liveBroadcastContent.Equals("live"))
                {
                    builder.WithTitle($"{item.snippet.channelTitle} streamt nun !");
                    builder.WithDescription(
                        $"{item.snippet.channelTitle} stream jetzt auf Youtube !\nSchaue jetzt zu: {item.GetAsVideoUrl()}");
                }
                else
                {
                    builder.WithTitle($"{item.snippet.channelTitle} hat ein neues Video hochgeladen !");
                    builder.WithDescription(
                        $"{item.snippet.channelTitle} hat ein neues Video hochgeladen !\n{item.GetAsVideoUrl()}");
                }

                builder.WithImageUrl(item.snippet.thumbnails.high.ToString())
                    .WithFooter(item.snippet.publishedAt.ToShortDateString());
                builder.WithFields(new []
                {
                    new EmbedFieldBuilder().WithName(item.snippet.title).WithValue(item.snippet.description).WithIsInline(false)
                });
                channel.SendMessageAsync(channel.Guild.EveryoneRole.Mention, false, builder.Build()).GetAwaiter().GetResult();
                _lastUploads.Remove(id);
                _lastUploads.Add(id, item);
            }
        }

        public void Close()
        {
            _schedulers.Stop();
        }
        
    }
}