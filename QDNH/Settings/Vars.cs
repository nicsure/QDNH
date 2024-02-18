using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDNH.Settings
{
    public static class Vars
    {
        private const int defaultPort = 18822;
        private const int defaultLatency = 100;
        private const string configFile = "qdnh.conf";
        public const string Disabled = "Disabled";

        public static string Version { get; } = "0.01.02q";
        public static string AudioInput { get; set; } = Disabled;
        public static string AudioOutput { get; set; } = Disabled;
        public static string ComPort { get; set; } = Disabled;
        public static int NetworkPort { get; set; } = defaultPort;
        public static string Password { get; set; } = string.Empty;
        public static int AudioInputDevice { get; set; } = -1;
        public static int AudioOutputDevice { get; set; } = -1;
        public static int Latency
        {
            get => latency;
            set
            {
                latency = value;
                latencyms = value / 1000.0;
            }
        }
        public static double LatencyMS => latencyms;
        private static int latency = defaultLatency;
        private static double latencyms = defaultLatency / 1000.0;


        public static void Save()
        {
            try
            {
                File.WriteAllLines(configFile, new List<string> {
                    nameof(AudioInput),
                    AudioInput,
                    nameof(AudioOutput),
                    AudioOutput,
                    nameof(ComPort),
                    ComPort,
                    nameof(NetworkPort),
                    NetworkPort.ToString(),
                    nameof(Password),
                    Password,
                    nameof(Latency),
                    Latency.ToString(),
                });
            }
            catch { }
        }

        static Vars()
        {
            try
            {
                var conf = File.ReadAllLines(configFile);
                int len = conf.Length & 0x7ffffffe, p;
                for (int i = 0; i < len; i += 2)
                {
                    string key = conf[i];
                    string value = conf[i + 1];
                    switch (key)
                    {
                        case "":
                            break;
                        case nameof(AudioInput):
                            AudioInput = value;
                            break;
                        case nameof(AudioOutput):
                            AudioOutput = value;
                            break;
                        case nameof(ComPort):
                            ComPort = value;
                            break;
                        case nameof(Password):
                            Password = value;
                            break;
                        case nameof(NetworkPort):
                            NetworkPort = int.TryParse(value, out p) ? p : defaultPort;
                            break;
                        case nameof(Latency):
                            Latency = int.TryParse(value, out p) ? p : defaultLatency;
                            break;
                        default:
                            Console.Error.WriteLine($"Error in configuration file: {key} / {value}");
                            break;
                    }
                }
            }
            catch { }
        }
    }
}
