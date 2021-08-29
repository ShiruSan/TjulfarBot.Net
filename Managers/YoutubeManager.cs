using System;
using System.IO;
using System.Net;
using System.Threading;
using Discord;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using TjulfarBot.Net.Utils;

namespace TjulfarBot.Net.Youtube
{
    public class YoutubeManager
    {
        private static YoutubeManager _instance;
        private Thread _thread;
        private readonly YouTubeService _youTubeService = new(new BaseClientService.Initializer()
        {
            ApplicationName = "discord-bot",
            ApiKey = "AIzaSyAYtR_g7L__NKI_L7LRpHE3aRnTQ8UKc2Y"
        });

        public static YoutubeManager Get()
        {
            if (_instance == null) _instance = new YoutubeManager();
            return _instance;
        }

        public void Init()
        {
            _thread = new Thread(Loop);
            _thread.Start();
        }

        private void Loop()
        {
            while (true)
            {
                try
                {
                    OnSchedulerTick();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception in Youtube Threading Listener:\n{e}");
                }
                Thread.Sleep(TimeSpan.FromMinutes(15));
            }
        }
        
        private void OnSchedulerTick()
        {
            if(Program.instance.Settings.videoUploadChannel == -1) return;
            var request = _youTubeService.Search.List("snippet");
            request.ChannelId = "UC5ZYeY8BxFXmcPu8m-hWDug";
            request.Type = "video";
            request.MaxResults = 1;
            request.Order = SearchResource.ListRequest.OrderEnum.Date;
            var response = request.Execute();
            using (var writer = new StreamWriter(File.OpenWrite("latestResults.txt"))) writer.Write($"{JsonTool.Serialize(response)} \n \n \n");
            if(response.Items.Count == 0) return;
            var item = response.Items[0];
            if (!File.Exists("lastUpload.json"))
            {
                using var stream = File.CreateText("lastUpload.json");
                stream.Write(JsonTool.Serialize(item));
                return;
            }
            using var reader = File.OpenText("lastUpload.json");
            var lastUpload = JsonTool.DeserializeFromString(typeof(SearchResult), reader.ReadToEnd()) as SearchResult;
            if(lastUpload.Id.VideoId.Equals(item.Id.VideoId)) return;
            File.Delete("lastUpload.json");
            using var stream2 = File.CreateText("lastUpload.json");
            stream2.Write(JsonTool.Serialize(item));
            var channel = Program.instance.SocketClient.GetGuild((ulong) Program.instance.Settings.defaultGuild)
                .GetTextChannel((ulong) Program.instance.Settings.videoUploadChannel);
            var builder = new EmbedBuilder();
            builder.WithColor(Color.Red);
            if (item.Snippet.LiveBroadcastContent.Equals("live"))
            {
                builder.WithTitle($"{item.Snippet.ChannelTitle} streamt nun !");
                builder.WithDescription(
                    $"{item.Snippet.ChannelTitle} stream jetzt auf Youtube !\nSchaue jetzt zu: https://www.youtube.com/watch?v={item.Id.VideoId}");
            }
            else
            {
                builder.WithTitle($"{item.Snippet.ChannelTitle} hat ein neues Video hochgeladen !");
                builder.WithDescription(
                    $"{item.Snippet.ChannelTitle} hat ein neues Video hochgeladen !\nhttps://www.youtube.com/watch?v={item.Id.VideoId}");
            }

            builder.WithImageUrl(item.Snippet.Thumbnails.High.Url)
                .WithFooter(item.Snippet.PublishedAt.Value.ToLongDateString() + " " + item.Snippet.PublishedAt.Value.ToShortTimeString());
            builder.WithFields(new []
            {
                new EmbedFieldBuilder().WithName(item.Snippet.Title).WithValue(item.Snippet.Description).WithIsInline(false)
            });
            channel.SendMessageAsync(channel.Guild.EveryoneRole.Mention, false, builder.Build()).GetAwaiter().GetResult();
        }

        public void Close()
        {
            _thread.Interrupt();
        }
        
    }
}