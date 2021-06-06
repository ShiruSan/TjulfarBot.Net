using System;

namespace TjulfarBot.Net.Youtube
{
    public class Item
    {
        public string kind;
        public string etag;
        public ID id;
        public Snippet snippet;
        
        public class ID
        {
            public string kind;
            public string videoId;
        }
        
        public class Snippet
        {
            public DateTime publishedAt;
            public string channelId;
            public string title;
            public string description;
            public Thumbnail thumbnails;
            public string channelTitle;
            public string liveBroadcastContent;
            public DateTime publishTime;
        }

        public class Thumbnail
        {
            public UrlPicture @default;
            public UrlPicture medium;
            public UrlPicture high;
        }
        
        public class UrlPicture
        {
            public Uri url;
            public int height;
            public int width;
        }
        
    }

}