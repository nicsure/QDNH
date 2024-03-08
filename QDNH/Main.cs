using NAudio.CoreAudioApi;
using QDNH.Audio;
using QDNH.Language;
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
        private static MMDeviceCollection inputDevices = null!, outputDevices = null!;
        private static string[] ports = null!;
        private static Listen? audioServer = null, serialServer = null;
        private static UART? serialPort = null;
        private static Capture? capture = null;
        private static Playback? playback = null;
        private static bool commandLine = true;

        private static void Out(string s, string suffix = "\n") => Vars.Out(s, suffix);

        private static void Err(string s) => Vars.Err(s);


        public static void Run(string[] args)
        {
            EnumerateDevices();
            for (int i = 0; i < args.Length; i += 2)
            {
                switch (ExecuteCommand($"{args[i]} {args.Element(i + 1)}"))
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
                Out($"\n[{Vars.Config}] {Lang.CommandPrompt} # ", string.Empty);
                switch (ExecuteCommand(Vars.In()))
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
                            Vars.AudioInput = p1 == 0 ? Lang.Disabled : inputDevices[p1 - 1].FriendlyName;
                            postAction = CommandPostAction.Init;
                        }
                        else
                        {
                            Err($"{Lang.InvalidDevice} '{Lang.Input} {p1}'");
                            postAction = CommandPostAction.Error;
                        }
                        break;
                    case "O":
                        if (p.Length == 1) { DisplayOutputs(); break; }
                        if (outputDevices != null && p1 >= 0 && p1 <= outputDevices.Count)
                        {
                            Vars.AudioOutput = p1 == 0 ? Lang.Disabled : outputDevices[p1 - 1].FriendlyName;
                            postAction = CommandPostAction.Init;
                        }
                        else
                        {
                            Err($"{Lang.InvalidDevice} '{Lang.Output} {p1}'");
                            postAction = CommandPostAction.Error;
                        }
                        break;
                    case "S":
                        if (p.Length == 1) { DisplayComPorts(); break; }
                        if (ports != null && p1 >= 0 && p1 <= ports.Length)
                        {
                            Vars.ComPort = p1 == 0 ? Lang.Disabled : ports[p1 - 1];
                            postAction = CommandPostAction.Init;
                        }
                        else
                        {
                            Err($"{Lang.InvalidDevice} '{Lang.Serial} {p1}'");
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
                            Err($"{Lang.InvalidPort} '{p1}'");
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
                    case "G":
                        if (p.Length == 1) { DisplayLanguage(); break; }
                        Lang.LoadLanguge(p1s);
                        Vars.Save();
                        postAction = CommandPostAction.Overview;
                        break;                       
                    default:
                        Err($"{Lang.UnknownCommand} '{command}'");
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
            if (!Vars.Loaded) Vars.Load();
            Out(commandLine ? Lang.HelpCL : Lang.Help);
        }

        private static void EnumerateDevices()
        {
            inputDevices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            outputDevices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            ports = SerialPort.GetPortNames();
        }

        private static void DisplayLanguage()
        {
            Out($"\n{Lang.Language}\t\t{Vars.Language}");
        }

        private static void DisplayInputs()
        {
            Out($"\n{Lang.AvailInput}");
            Vars.AudioInputDevice = -1;
            for (int i = -1; i < inputDevices.Count; i++)
            {
                string fn = i == -1 ? Lang.Disabled : inputDevices[i].FriendlyName;
                string sel;
                if (fn.Equals(Vars.AudioInput))
                {
                    sel = "*";
                    Vars.AudioInputDevice = i;
                }
                else
                    sel = " ";
                Out($" {sel}{i + 1} - {fn}");
            }
        }

        private static void DisplayOutputs()
        {
            Out($"\n{Lang.AvailOutput}");
            Vars.AudioOutputDevice = -1;
            for (int i = -1; i < outputDevices.Count; i++)
            {
                string fn = i == -1 ? Lang.Disabled : outputDevices[i].FriendlyName;
                string sel;
                if (fn.Equals(Vars.AudioOutput))
                {
                    sel = "*";
                    Vars.AudioOutputDevice = i;
                }
                else
                    sel = " ";
                Out($" {sel}{i + 1} - {fn}");
            }
        }

        private static void DisplayComPorts()
        {
            Out($"\n{Lang.AvailCom}");
            for (int i = -1; i < ports.Length; i++)
            {
                string fn = i == -1 ? Lang.Disabled : ports[i];
                string sel = fn.Equals(Vars.ComPort) ? "*" : " ";
                Out($" {sel}{i + 1} - {fn}");
            }
        }

        private static void DisplayNetPorts()
        {
            Out($"\n{Lang.NetPorts}\t\t{Vars.NetworkPort}, {Vars.NetworkPort + 1}");
            Out($"\n{Lang.LocalHost}\t\t{Dns.GetHostName()}");
        }

        private static void DisplayPassword(bool unmask)
        {
            string isSet = Vars.Password.Length == 0 ? "NOT " : string.Empty;
            Out($"\n{Lang.Password}\t\t{isSet}SET");
            if (unmask)
                Out($"  {Vars.Password}");
        }

        private static void DisplayLatency()
        {
            Out($"\n{Lang.Latency}\t\t{Vars.LatencyMils}");
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
            Out($"({Lang.Selected})\n");
            DisplayAll();
            Vars.Save();
            serialPort?.Close();
            audioServer?.Close();
            serialServer?.Close();
            capture?.Close();
            playback?.Close();
            try { audioServer = new(Vars.NetworkPort, NetworkAudioCallback, true); } catch { }
            try { serialServer = new(Vars.NetworkPort + 1, NetworkSerialCallback, false); } catch { }
            if (!Vars.ComPort.Equals(Lang.Disabled))
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
