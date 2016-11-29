using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicStudio.Infrastructure.IO.Monitor
{
    public class Monitoring
    {
        private static Monitoring _instance = null;
        public static Monitoring Instance { get { if (_instance == null) return new Monitoring(); else return _instance; } }

        public string LogPath { get; set; }
        public string LogName { get; set; }

        private Monitoring()
        {
            LogName = "Monitoring.txt";
            LogPath = Environment.CurrentDirectory + "\\" + LogName;
        }

        public void Write(string message)
        {
            StreamWriter sw = new StreamWriter(LogPath,true);
            sw.WriteLine(message);
            sw.Close();
        }

    }
}
