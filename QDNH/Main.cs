using NAudio.CoreAudioApi;
using QDNH.Audio;
using QDNH.Network;
using QDNH.Serial;
using QDNH.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace QDNH
{
    public enum CommandPostAction
    {
        None, Quit, Init, Overview, Error, LoadConfig, Info
    }

    public static class Main
    {
        private const string invalidDevice = "Error, invalid device number";
        private static MMDeviceCollection inputDevices = null!, outputDevices = null!;
        private static string[] ports = null!;
        private static Listen? audioServer = null, serialServer = null;
        private static UART? serialPort = null;
        private static Capture? capture = null;
        private static Playback? playback = null;
        private static bool commandLine = true;
        public static void Run(string[] args)
        {
            EnumerateDevices();
            for (int i = 0; i < args.Length; i += 2)
            {
                switch (ExecuteCommand($"{args[i]} {(i + 1 >= args.Length ? string.Empty : args[i + 1])}"))
                {
                    case CommandPostAction.Info:
                    case CommandPostAction.Error:
                        return;
                    case CommandPostAction.LoadConfig:
                        Vars.AllowSaveConfig = true;
                        Vars.Load();
                        break;
                    default:
                        Vars.AllowSaveConfig = false;
                        break;
                }
            }
            if (!Vars.Loaded)
                Vars.Load();
            Init();
            bool quit = false;
            while (!quit)
            {
                Console.Write($"\n[{Vars.Config}] Command (? for help) # ");
                switch (ExecuteCommand(Console.ReadLine() ?? string.Empty))
                {
                    case CommandPostAction.Quit:
                        quit = true;
                        break;
                    case CommandPostAction.Init:
                        Init();
                        break;
                    case CommandPostAction.Overview:
                        DisplayAll();
                        break;
                    default:
                        break;
                }
            }
        }

        private static CommandPostAction ExecuteCommand(string command)
        {
            CommandPostAction postAction = CommandPostAction.None;
            command = command.Replace('\t', ' ');
            while (command.IndexOf("  ") != -1)
                command = command.Replace("  ", " ");
            if (command.Length > 0)
            {
                string[] p = command.Trim().Split(' ');
                string p1s = p.Length > 1 ? p[1] : string.Empty;
                int p1 = int.TryParse(p1s, out int i) ? i : -1;
                switch (p[0].ToUpper().Replace("-",""))
                {
                    case "E":
                        DisplayInputs();
                        DisplayOutputs();
                        DisplayComPorts();
                        postAction = CommandPostAction.Info;
                        break;
                    case "C":
                        string confFile = p1s.ToLower();
                        if (confFile.EndsWith(Vars.confExt))
                            confFile = confFile.Replace(Vars.confExt, string.Empty);
                        Vars.Config = confFile;
                        postAction = CommandPostAction.LoadConfig;
                        break;
                    case "?":
                    case "/?":
                    case "HELP":
                        Help();
                        postAction = CommandPostAction.Info;
                        break;
                    case "Q":
                        postAction = CommandPostAction.Quit;
                        break;
                    case "I":
                        if (p.Length == 1) { DisplayInputs(); break; }
                        if (inputDevices != null && p1 >= 0 && p1 <= inputDevices.Count)
                        {
                            Vars.AudioInput = p1 == 0 ? Vars.Disabled : inputDevices[p1 - 1].FriendlyName;
                            postAction = CommandPostAction.Init;
                        }
                        else
                        {
                            Console.Error.WriteLine($"{invalidDevice} 'Input {p1}'");
                            postAction = CommandPostAction.Error;
                        }
                        break;
                    case "O":
                        if (p.Length == 1) { DisplayOutputs(); break; }
                        if (outputDevices != null && p1 >= 0 && p1 <= outputDevices.Count)
                        {
                            Vars.AudioOutput = p1 == 0 ? Vars.Disabled : outputDevices[p1 - 1].FriendlyName;
                            postAction = CommandPostAction.Init;
                        }
                        else
                        {
                            Console.Error.WriteLine($"{invalidDevice} 'Output {p1}'");
                            postAction = CommandPostAction.Error;
                        }
                        break;
                    case "S":
                        if (p.Length == 1) { DisplayComPorts(); break; }
                        if (ports != null && p1 >= 0 && p1 <= ports.Length)
                        {
                            Vars.ComPort = p1 == 0 ? Vars.Disabled : ports[p1 - 1];
                            postAction = CommandPostAction.Init;
                        }
                        else
                        {
                            Console.Error.WriteLine($"{invalidDevice} 'Serial {p1}'");
                            postAction = CommandPostAction.Error;
                        }
                        break;
                    case "N":
                        if (p.Length == 1) { DisplayNetPorts(); break; }
                        if (p1 > 1023 && p1 < 65500)
                        {
                            Vars.NetworkPort = p1;
                            postAction = CommandPostAction.Init;
                        }
                        else
                        {
                            Console.Error.WriteLine($"Error, invalid port number '{p1}'");
                            postAction = CommandPostAction.Error;
                        }
                        break;
                    case "R":
                        EnumerateDevices();
                        postAction = CommandPostAction.Overview;
                        break;
                    case "P":
                        if (p.Length == 1) { DisplayPassword(true); break; }
                        Vars.Password = p1s.ToLower().Equals("none") ? string.Empty : p1s;
                        postAction = CommandPostAction.Init;
                        break;
                    case "L":
                        if (p.Length == 1) { DisplayLatency(); break; }
                        Vars.LatencyMils = p1;
                        postAction = CommandPostAction.Init;
                        break;
                    default:
                        Console.Error.WriteLine($"Error, unknown command '{command}'");
                        postAction = CommandPostAction.Error;
                        break;
                }
            }
            else
                postAction = CommandPostAction.Overview;
            return postAction;
        }

        private static void Help()
        {
            if (!commandLine)
            {
                Console.WriteLine("\n");
                Console.WriteLine("I n\t\tChange input audio device number");
                Console.WriteLine("O n\t\tChange output audio device number");
                Console.WriteLine("S n\t\tChange serial port device number");
                Console.WriteLine("N port\t\tChange network ports");
                Console.WriteLine("P newpassword\tSet login password");
                Console.WriteLine("P none\t\tClear login password");
                Console.WriteLine("L milliseconds\tSet audio latency");
                Console.WriteLine("R\t\tRefesh devices");
                Console.WriteLine("Q\t\tQuit program\n");
            }
            else
            {
                Console.WriteLine(" Usage:");
                Console.WriteLine("  QDNH\t\t\t\t\tRun with default config, config is persistent");
                Console.WriteLine("  QDNH -E\t\t\t\tShow list of devices");
                Console.WriteLine("  QDNH -C config\t\t\tRun with specific config, config is persistent");
                Console.WriteLine("  QDNH [switch value ...]\t\tDefault config with override switches, config is not persistent");
                Console.WriteLine("  QDNH -C config [switch value ...]\tSpecific config with override switches, config is not persistent\n");
                Console.WriteLine("   switches");
                Console.WriteLine("    -I device\t\tSet input device");
                Console.WriteLine("    -O device\t\tSet output device");
                Console.WriteLine("    -S device\t\tSet COM device");
                Console.WriteLine("    -N port\t\tSet network ports");
                Console.WriteLine("    -P password\t\tSet network password");
                Console.WriteLine("    -P none\t\tSet no authentication");
                Console.WriteLine("    -L milliseconds\tSet audio latency");
            }
        }

        private static void EnumerateDevices()
        {
            inputDevices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            outputDevices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            ports = SerialPort.GetPortNames();
        }

        private static void DisplayInputs()
        {
            Console.WriteLine("\nAvailable input devices");
            Vars.AudioInputDevice = -1;
            for (int i = -1; i < inputDevices.Count; i++)
            {
                string fn = i == -1 ? Vars.Disabled : inputDevices[i].FriendlyName;
                string sel;
                if (fn.Equals(Vars.AudioInput))
                {
                    sel = "*";
                    Vars.AudioInputDevice = i;
                }
                else
                    sel = " ";
                Console.WriteLine($" {sel}{i + 1} - {fn}");
            }
        }

        private static void DisplayOutputs()
        {
            Console.WriteLine("\nAvailable output devices");
            Vars.AudioOutputDevice = -1;
            for (int i = -1; i < outputDevices.Count; i++)
            {
                string fn = i == -1 ? Vars.Disabled : outputDevices[i].FriendlyName;
                string sel;
                if (fn.Equals(Vars.AudioOutput))
                {
                    sel = "*";
                    Vars.AudioOutputDevice = i;
                }
                else
                    sel = " ";
                Console.WriteLine($" {sel}{i + 1} - {fn}");
            }
        }

        private static void DisplayComPorts()
        {
            Console.WriteLine("\nAvailable serial ports");
            for (int i = -1; i < ports.Length; i++)
            {
                string fn = i == -1 ? Vars.Disabled : ports[i];
                string sel = fn.Equals(Vars.ComPort) ? "*" : " ";
                Console.WriteLine($" {sel}{i + 1} - {fn}");
            }
        }

        private static void DisplayNetPorts()
        {
            Console.WriteLine($"\nNetwork Ports\t\t{Vars.NetworkPort}, {Vars.NetworkPort + 1}");
            Console.WriteLine($"\nLocal Hostname\t\t{Dns.GetHostName()}");
        }

        private static void DisplayPassword(bool unmask)
        {
            string isSet = Vars.Password.Length == 0 ? "NOT " : string.Empty;
            Console.WriteLine($"\nLogin Password\t\t{isSet}SET");
            if (unmask)
                Console.WriteLine($"  {Vars.Password}");
        }

        private static void DisplayLatency()
        {
            Console.WriteLine($"\nAudio Latency\t\t{Vars.LatencyMils}");
        }

        private static void DisplayAll()
        {
            DisplayInputs();
            DisplayOutputs();
            DisplayComPorts();
            DisplayNetPorts();
            DisplayPassword(false);
            DisplayLatency();
        }

        private static void Init()
        {
            commandLine = false;
            Console.WriteLine("(* shows currently selected)\n");
            DisplayAll();
            Vars.Save();
            serialPort?.Close();
            audioServer?.Close();
            serialServer?.Close();
            capture?.Close();
            playback?.Close();
            try { audioServer = new(Vars.NetworkPort, NetworkAudioCallback, true); } catch { }
            try { serialServer = new(Vars.NetworkPort + 1, NetworkSerialCallback, false); } catch { }
            if (!Vars.ComPort.Equals(Vars.Disabled))
            {
                try { serialPort = new(Vars.ComPort, SerialDataCallback); } catch { }
            }
            if (Vars.AudioInputDevice > -1)
            {
                try { capture = new(inputDevices![Vars.AudioInputDevice], CaptureCallback); } catch { }
            }
            if (Vars.AudioOutputDevice > -1)
            {
                try { playback = new(outputDevices![Vars.AudioOutputDevice]); } catch { }
            }
        }


        private static void CaptureCallback(byte[] data, int length)
        {
            audioServer?.Send(data, 0, length);
        }

        private static void SerialDataCallback(byte[] data, int length)
        {
            serialServer?.Send(data, 0, length);
        }

        private static void NetworkAudioCallback(byte[] data, int length)
        {
            playback?.Send(data, length);
        }

        private static void NetworkSerialCallback(byte[] data, int length)
        {
            serialPort?.Send(data, 0, length);
        }

    }


}
