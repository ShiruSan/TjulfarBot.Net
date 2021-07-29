using System;
using System.Timers;

namespace TjulfarBot.Net.Tempban
{
    public class TempbanListener
    {
        private readonly Timer _timer = new Timer();

        public void Start()
        {
            _timer.Elapsed += TimerOnElapsed;
            _timer.Interval = 60000;
            _timer.Start();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "select * from TempBans where `passed` = @passed && `passing` < @passing";
            command.Parameters.AddWithValue("@passed", false);
            command.Parameters.AddWithValue("@passing", DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
            command.Prepare();
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                new TempbanEntry(reader.GetInt32(0), reader.GetInt64(1), reader.GetInt64(2)).Purge();
            }
            reader.Close();
        }

        public void Stop()
        {
            _timer.Stop();
            _timer.Dispose();
        }
        
    }
}