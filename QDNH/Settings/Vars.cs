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
        public const string confExt = ".conf";
        public const string Disabled = "Disabled";
        private static string config = "default";

        public static bool Loaded { get; private set; } = false;
        public static string Config { get => config; set => config = value; }
        public static string ConfigFile => $"{config}{confExt}";
        public static string Version { get; } = "0.01.06q";
        public static string AudioInput { get; set; } = Disabled;
        public static string AudioOutput { get; set; } = Disabled;
        public static string ComPort { get; set; } = Disabled;
        public static int NetworkPort { get; set; } = defaultPort;
        public static string Password { get; set; } = string.Empty;
        public static int AudioInputDevice { get; set; } = -1;
        public static int AudioOutputDevice { get; set; } = -1;
        public static bool AllowSaveConfig { get; set; } = true;
        public static int LatencyMils
        {
            get => latency;
            set
            {
                latency = value.Clamp(1, 10000);
                latencysecs = latency / 1000.0;
            }
        }
        public static double LatencySecs => latencysecs;
        private static int latency = defaultLatency;
        private static double latencysecs = defaultLatency / 1000.0;


        public static void Save()
        {
            if (AllowSaveConfig)
            {
                try
                {
                    File.WriteAllLines(ConfigFile, new List<string> {
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
                        nameof(LatencyMils),
                        LatencyMils.ToString(),
                    });
                }
                catch 
                {
                    Console.Error.WriteLine($"Error saving configuration file: {ConfigFile}");
                }
            }
        }

        public static void Load()
        {
            Loaded = true;
            try
            {
                var conf = File.ReadAllLines(ConfigFile);
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
                        case nameof(LatencyMils):
                            LatencyMils = int.TryParse(value, out p) ? p : defaultLatency;
                            break;
                        default:
                            Console.Error.WriteLine($"Error in configuration file: {key} / {value}");
                            break;
                    }
                }
            }
            catch(FileNotFoundException) { }
            catch
            {
                Console.Error.WriteLine($"Error reading configuration file: {ConfigFile}");
            }
        }

        public static int Clamp(this int val, int min, int max) => val < min ? min : val > max ? max : val;
    }
}
