using System.Collections.Generic;

namespace Hiale.Win32Forms
{
    public class SimpleLogger
    {
        private static readonly Dictionary<string, SimpleLogger> Loggers;
        private readonly List<string> _messages;

        private static readonly object Locker = new object();

        static SimpleLogger()
        {
            Loggers = new Dictionary<string, SimpleLogger>();
            CreateLogger(string.Empty);
        }

        private SimpleLogger()
        {
            _messages = new List<string>();
        }

        public List<string> Messages
        {
            get
            {
                lock (this)
                {
                    return _messages;
                }
            }
        }

        public static SimpleLogger CreateLogger(string name)
        {
            lock (Locker)
            {
                var simpleLogger = new SimpleLogger();
                Loggers.Add(name, simpleLogger);
                return simpleLogger;
            }
        }

        public static SimpleLogger GetLogger(string name = "")
        {
            lock (Locker)
            {
                SimpleLogger simpleLogger;
                return Loggers.TryGetValue(name, out simpleLogger) ? simpleLogger : Loggers[string.Empty];
            }
        }

        public void WriteLog(string message)
        {
            lock (this)
            {
                _messages.Add(message);
            }
        }
    }
}