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
    public static class Main
    {
        private const string invalidDevice = "Error, invalid device number";
        private static MMDeviceCollection? inputDevices = null, outputDevices = null;
        private static string[]? ports = null;
        private static Listen? audioServer = null, serialServer = null;
        private static UART? serialPort = null;
        private static Capture? capture = null;
        private static Playback? playback = null;
        public static void Run()
        {
            Init();
            bool quit = false;
            while (!quit)
            {
                Console.Write("\nEnter Command (? for help) # ");
                var s = Console.ReadLine();
                if (s != null)
                {
                    if (s.Length > 0)
                    {
                        string[] p = s.Trim().Split(' ');
                        string p1s = p.Length > 1 ? p[1] : string.Empty;
                        int p1 = int.TryParse(p1s, out int i) ? i : -1;
                        switch (p[0].ToUpper())
                        {
                            case "?":
                                Help();
                                break;
                            case "Q":
                                quit = true;
                                break;
                            case "I":
                                if (p.Length == 1) { DisplayInputs(); break; }
                                if (inputDevices != null && p1 >= 0 && p1 <= inputDevices.Count)
                                {
                                    Vars.AudioInput = p1 == 0 ? Vars.Disabled : inputDevices[p1 - 1].FriendlyName;
                                    Init();
                                }
                                else
                                    Console.Error.WriteLine(invalidDevice);
                                break;
                            case "O":
                                if (p.Length == 1) { DisplayOutputs(); break; }
                                if (outputDevices != null && p1 >= 0 && p1 <= outputDevices.Count)
                                {
                                    Vars.AudioOutput = p1 == 0 ? Vars.Disabled : outputDevices[p1 - 1].FriendlyName;
                                    Init();
                                }
                                else
                                    Console.Error.WriteLine(invalidDevice);
                                break;
                            case "S":
                                if (p.Length == 1) { DisplayComPorts(); break; }
                                if (ports != null && p1 >= 0 && p1 <= ports.Length)
                                {
                                    Vars.ComPort = p1 == 0 ? Vars.Disabled : ports[p1 - 1];
                                    Init();
                                }
                                else
                                    Console.Error.WriteLine(invalidDevice);
                                break;
                            case "N":
                                if (p.Length == 1) { DisplayNetPorts(); break; }
                                if (p1 > 1023 && p1 < 65500)
                                {
                                    Vars.NetworkPort = p1;
                                    Init();
                                }
                                else
                                    Console.Error.WriteLine("Error, invalid port number");
                                break;
                            case "P":
                                if (p.Length == 1) { DisplayPassword(true); break; }
                                Vars.Password = p1s.ToLower().Equals("none") ? string.Empty : p1s;
                                Init();
                                break;
                            case "L":
                                if (p.Length == 1) { DisplayLatency(); break; }
                                Vars.Latency = p1;
                                Init();
                                break;
                            default:
                                Console.Error.WriteLine("Error, unknown command");
                                break;
                        }
                    }
                }
            }
        }

        private static void Help()
        {
            Console.WriteLine("\n");
            Console.WriteLine("I n\t\tChange input audio device number");
            Console.WriteLine("O n\t\tChange output audio device number");
            Console.WriteLine("S n\t\tChange serial port device number");
            Console.WriteLine("N port\t\tChange network ports");
            Console.WriteLine("P newpassword\tSet login password");
            Console.WriteLine("P none\t\tClear login password");
            Console.WriteLine("L milliseconds\tSet audio latency");
            Console.WriteLine("Q\t\tQuit program\n");
        }

        private static void DisplayInputs()
        {
            inputDevices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
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
            outputDevices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
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
            ports = SerialPort.GetPortNames();
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
            Console.WriteLine($"\nNetwork Ports\n  {Vars.NetworkPort}, {Vars.NetworkPort + 1}");
        }

        private static void DisplayPassword(bool unmask)
        {
            string isSet = Vars.Password.Length == 0 ? "NOT " : string.Empty;
            Console.WriteLine($"\nLogin Password is {isSet}SET");
            if (unmask)
                Console.WriteLine($"  {Vars.Password}");
        }

        private static void DisplayLatency()
        {
            Console.WriteLine($"\nAudio Latency\n  {Vars.Latency}");
        }

        private static void Init()
        {
            Console.WriteLine("(* shows currently selected)\n");
            DisplayInputs();
            DisplayOutputs();
            DisplayComPorts();
            DisplayNetPorts();
            DisplayPassword(false);
            DisplayLatency();
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
