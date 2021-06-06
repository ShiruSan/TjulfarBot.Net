namespace TjulfarBot.Net.Youtube
{
    public class YoutubeManager
    {
        private static YoutubeManager _instance;

        public static YoutubeManager Get()
        {
            if (_instance == null) _instance = new YoutubeManager();
            return _instance;
        }
        
        
        
    }
}