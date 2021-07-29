using System;
using System.Threading;

namespace TjulfarBot.Net.Utils
{
    public class ConsoleListener
    {
        private static ConsoleListener consoleListener;
        private Thread listener;
        
        public void Start()
        {
            consoleListener.listener.Start();
        }

        public static ConsoleListener Get()
        {
            if (consoleListener == null)
            {
                consoleListener = new ConsoleListener();
                consoleListener.listener = new Thread(consoleListener.Run);
            }
            return consoleListener;
        }

        private void Run()
        {
            while (true)
            {
                string line;
                if ((line = Console.ReadLine()) != null)
                {
                    switch (line)
                    {
                        case "shutdown":
                            Program.instance.Close().GetAwaiter().GetResult();
                            Environment.Exit(0);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}