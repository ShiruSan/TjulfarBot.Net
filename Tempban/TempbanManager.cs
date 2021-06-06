using System;
using System.Dynamic;
using Discord.WebSocket;

namespace TjulfarBot.Net.Tempban
{
    public class TempbanManager
    {
        private static TempbanManager _manager;
        private TempbanListener _listener = new TempbanListener();
        private int _currentID;

        public static TempbanManager Get()
        {
            if (_manager == null) _manager = new TempbanManager();
            return _manager;
        }

        public void Tempban(SocketUser user, int time, TimeUnit unit)
        {
            long expires = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            if (unit == TimeUnit.HOURS)
            {
                expires += time * 60 * 60 * 1000;
            }
            else if (unit == TimeUnit.DAYS)
            {
                expires += time * 24 * 60 * 60 * 1000;
            }

            _currentID++;
            var entry = new TempbanEntry(_currentID, (long) user.Id, expires);
            entry.CreateDatabaseEntry();

        }

        public void Init()
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "select * from `TempBans` order by `id` DESC limit 1";
            command.Prepare();
            _currentID = (int) command.ExecuteScalar();
            _listener.Start();
        }

        public void Close()
        {
            _listener.Stop();
        }

        public enum TimeUnit
        {
            HOURS,
            DAYS
        }
        
    }
}