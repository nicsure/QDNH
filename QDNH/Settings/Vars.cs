using NAudio.Wave;
using QDNH.Language;
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
        private static string config = "default";
        private static string mode = "all";

        public static bool Loaded { get; private set; } = false;
        public static string Config { get => config; set => config = value; }
        public static string ConfigFile => $"{config}{confExt}";
        public static string Version { get; } = "0.02.01q";
        public static string Language { get; set; } = "en";
        public static string Mode
        {
            get => mode;
            set
            {
                Audio = true;
                Serial = true;
                switch(mode = value)
                {
                    case "audio": Serial = false; break;
                    case "serial": Audio = false; break;
                    default: mode = "all"; break;
                }
            }
        }
        public static string AudioInput { get; set; } = Lang.Disabled;
        public static string AudioOutput { get; set; } = Lang.Disabled;
        public static string ComPort { get; set; } = Lang.Disabled;
        public static int NetworkPort { get; set; } = defaultPort;
        public static string Password { get; set; } = string.Empty;
        public static int AudioInputDevice { get; set; } = -1;
        public static int AudioOutputDevice { get; set; } = -1;
        public static bool AllowSaveConfig { get; set; } = true;
        public static WaveFormat WaveFormat { get; } = new(22050, 16, 1);
        public static bool Audio { get; private set; } = true;
        public static bool Serial { get; private set; } = true;
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
                        nameof(Language),
                        Language,
                        nameof(Mode),
                        Mode
                    });
                }
                catch 
                {
                    Err($"{Lang.SaveError}: {ConfigFile}");
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
                        case nameof(Language):
                            Lang.LoadLanguge(value);
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
                        case nameof(Mode):
                            Mode = value;
                            break;
                        default:
                            Err($"{Lang.ConfigError}: {key} / {value}");
                            break;
                    }
                }
            }
            catch(FileNotFoundException) { }
            catch
            {
                Err($"{Lang.ReadError}: {ConfigFile}");
            }
        }

        public static void Out(string s, string suffix = "\n") => Console.Write(s + suffix);

        public static void Err(string s) => Console.Error.WriteLine($"{Lang.Error}: {s}");

        public static string In() => Console.ReadLine() ?? string.Empty;

        public static int Clamp(this int val, int min, int max) => val < min ? min : val > max ? max : val;

        public static string Element(this string[] array, int element) => element < array.Length && element >= 0 ? array[element] : string.Empty;

        public static string Reformat(this string s) => s.Replace(@"\t", "\t").Replace(@"\n", "\n").Replace(@"\r", "\r");
    }
}
