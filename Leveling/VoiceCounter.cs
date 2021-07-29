using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using Discord.WebSocket;

namespace TjulfarBot.Net.Leveling
{
    public class VoiceCounter
    {
        public Dictionary<ulong, SocketVoiceState> VoiceCounters = new();
        private Thread _thread;
        private static VoiceCounter _instance;

        public static VoiceCounter Get()
        {
            if (_instance == null) _instance = new();
            return _instance;
        }

        public void Start()
        {
            _thread = new(Counter);
            _thread.Start();
        }

        public void Stop()
        {
            _thread.Interrupt();
        }
        
        private void Counter()
        {
            while (true)
            {
                Thread.Sleep(TimeSpan.FromMinutes(10));
                try
                {
                    
                    foreach (var id in VoiceCounters.Keys)
                    {
                        var VoiceState = VoiceCounters[id];
                        if (!(VoiceState.IsSelfMuted || VoiceState.IsSelfDeafened || VoiceState.IsMuted ||
                              VoiceState.IsDeafened))
                        {
                            var multiplier = Multiplier.GetMultiplier(VoiceState.VoiceChannel.Guild.GetUser(id));
                            if(!LevelManager.Get().ExistsProfile(id)) LevelManager.Get().CreateProfile(id).AddExp(5 * multiplier.MultiplierNumber);
                            else LevelManager.Get().GetProfile(id).AddExp(5 * multiplier.MultiplierNumber);
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("Async modification!\nCouldn't give all voice member XP !");
                }
            }
        }
        
    }
}