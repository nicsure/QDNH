﻿using NAudio.Wave.SampleProviders;
using QDNH.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDNH.Language
{
    public static class Lang
    {   
        public static string CommandPrompt => table[nameof(CommandPrompt)];
        public static string InvalidDevice => table[nameof(InvalidDevice)];
        public static string InvalidPort => table[nameof(InvalidPort)];
        public static string Input => table[nameof(Input)];
        public static string Output => table[nameof(Output)];
        public static string Serial => table[nameof(Serial)];
        public static string Disabled => table[nameof(Disabled)];
        public static string Selected => table[nameof(Selected)];
        public static string UnknownCommand => table[nameof(UnknownCommand)];
        public static string AvailInput => table[nameof(AvailInput)];
        public static string AvailOutput => table[nameof(AvailOutput)];
        public static string AvailCom => table[nameof(AvailCom)];
        public static string NetPorts => table[nameof(NetPorts)];
        public static string LocalHost => table[nameof(LocalHost)];
        public static string Password => table[nameof(Password)];
        public static string Latency => table[nameof(Latency)];
        public static string Error => table[nameof(Error)];
        public static string OpenOutputError => table[nameof(OpenOutputError)];
        public static string OpenInputError => table[nameof(OpenInputError)];
        public static string OpenComError => table[nameof(OpenComError)];
        public static string StartError => table[nameof(StartError)];
        public static string SaveError => table[nameof(SaveError)];
        public static string ReadError => table[nameof(ReadError)];
        public static string ConfigError => table[nameof(ConfigError)];
        public static string AudioDisabled => table[nameof(AudioDisabled)];
        public static string SerialDisabled => table[nameof(SerialDisabled)];
        public static string Language => table[nameof(Language)];
        public static string HelpCL => table[nameof(HelpCL)].Reformat();
        public static string Help => table[nameof(Help)].Reformat();
        private static readonly Dictionary<string, string> entable = new()
        {
            { nameof(CommandPrompt), "Command (? for help)" },
            { nameof(InvalidDevice), "Invalid device number" },
            { nameof(InvalidPort), "Invalid port number" },
            { nameof(Input), "Input" },
            { nameof(Output), "Output" },
            { nameof(Serial), "Serial" },
            { nameof(Disabled), "Disabled" },
            { nameof(Selected), "'*' shows currently selected" },
            { nameof(UnknownCommand), "Unknown Command" },
            { nameof(AvailInput), "Available input devices" },
            { nameof(AvailOutput), "Available output devices" },
            { nameof(AvailCom), "Available serial ports" },
            { nameof(NetPorts), "Network Ports" },
            { nameof(LocalHost), "Local Hostname" },
            { nameof(Password), "Login Password" },
            { nameof(Latency), "Audio Latency" },
            { nameof(Error), "Error" },
            { nameof(OpenOutputError), "Opening audio output device" },
            { nameof(OpenInputError), "Opening audio input device" },
            { nameof(OpenComError), "Opening serial port" },
            { nameof(StartError), "Starting server on port" },
            { nameof(SaveError), "Saving configuration file" },
            { nameof(ReadError), "Reading configuration file" },
            { nameof(ConfigError), "Processing configuration file" },
            { nameof(AudioDisabled), "Audio is not enabled" },
            { nameof(SerialDisabled), "Serial is not enabled" },
            { nameof(Language), "Language" },
            { nameof(HelpCL),   @" Usage:\n" +
                                @"  QDNH\t\t\t\t\tRun with default config, config is persistent\n" +
                                @"  QDNH -E\t\t\t\tShow list of devices\n" +
                                @"  QDNH -C config\t\t\tRun with specific config, config is persistent\n" +
                                @"  QDNH [switch value ...]\t\tDefault config with override switches, config is not persistent\n" +
                                @"  QDNH -C config [switch value ...]\tSpecific config with override switches, config is not persistent\n\n" +
                                @"   switches\n" +
                                @"    -I device\t\tSet input device\n" +
                                @"    -O device\t\tSet output device\n" +
                                @"    -S device\t\tSet COM device\n" +
                                @"    -N port\t\tSet network ports\n" +
                                @"    -P password\t\tSet network password\n" +
                                @"    -P none\t\tSet no authentication\n" +
                                @"    -L milliseconds\tSet audio latency\n" +
                                @"    -G language\t\tSet language\n" +
                                @"    -M mode\t\tSet operation mode: all, serial or audio\n" },
            { nameof(Help),     @"\n" +
                                @"I n\t\tChange input audio device number\n" +
                                @"O n\t\tChange output audio device number\n" +
                                @"S n\t\tChange serial port device number\n" +
                                @"N port\t\tChange network ports\n" +
                                @"G language\tChange language\n" +
                                @"P newpassword\tSet login password\n" +
                                @"P none\t\tClear login password\n" +
                                @"L milliseconds\tSet audio latency\n" +
                                @"M mode\t\tSet operation mode: all, serial or audio\n" +
                                @"R\t\tRefesh devices\n" +
                                @"Q\t\tQuit program\n\n" }};
        private static readonly Dictionary<string, string> table = entable.Copy();

        public static Dictionary<string, string> Copy(this Dictionary<string, string> source)
        {
            Dictionary<string, string> copy = new();
            foreach (string key in source.Keys) copy[key] = source[key];
            return copy;
        }

        public static void Copy(this Dictionary<string, string> source, Dictionary<string, string> destination)
        {            
            foreach (string key in source.Keys)
                destination[key] = source[key];
        }

        public static void LoadLanguge(string lang)
        {
            entable.Copy(table);
            string templateFile = $"template_{Vars.Version}.lang";
            lang = lang.ToLower();
            string langFile = $"{lang}.lang";
            if (!File.Exists(templateFile))
            {
                string tmp = string.Empty;
                foreach (string k in entable.Keys)
                    tmp += $"{k}={entable[k]}\r\n";
                File.WriteAllText(templateFile, tmp);
            }
            if (!lang.Equals("en"))
            {
                try
                {
                    string[] lines = File.ReadAllLines(langFile);
                    foreach (string line in lines)
                    {
                        string[] s = line.Trim().Split('=');
                        if (s.Length > 1)
                        {
                            string v = string.Join("=", s, 1, s.Length - 1);
                            table[s[0].Trim()] = v;
                        }
                    }
                    Vars.Language = lang;
                }
                catch
                {
                    // yes this is literally in English because if there was an
                    // error loading a new language there's no available translation is there?
                    Vars.Err($"Loading language file {langFile}");
                }
            }
        }

    }
}
