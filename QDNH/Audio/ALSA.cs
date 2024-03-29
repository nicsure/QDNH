using Alsa.Net;
using QDNH.Language;
using QDNH.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDNH.Audio
{
    public static class ALSA
    {
        public static SoundDeviceSettings Settings { get; } = new SoundDeviceSettings();
        public static ISoundDevice? Device { get; set; } = null;
        public static void Configure()
        {
            try
            {
                Settings.PlaybackDeviceName = $"sysdefault:CARD={Vars.AudioOutput}";
                Settings.RecordingDeviceName = $"sysdefault:CARD={Vars.AudioInput}";
                Settings.RecordingBitsPerSample = 16;
                Settings.RecordingChannels = 1;
                Settings.RecordingSampleRate = 22050;
                using (Device)
                {
                    Device = AlsaDeviceBuilder.Create(Settings);
                }
            }
            catch
            {
                Vars.Err($"[ALSA] {Lang.AudioDisabled}");
            }
            
        }
        private static int AddString(this byte[] b, string s, int index)
        {
            foreach (var v in s)
                b[index++] = (byte)v;
            return index;
        }
        private static int AddNumber(this byte[] b, long number, int byteCount, int index)
        {
            while (byteCount-- > 0)
            {
                b[index++] = (byte)number;
                number >>= 8;
            }
            return index;
        }
        private static int AddInt(this byte[] b, long i, int index) => AddNumber(b, i, 4, index);
        private static int AddShort(this byte[] b, long s, int index) => AddNumber(b, s, 2, index);
        public static int WavHeader(byte[] b, int c)
        {
            long bytesPerSample = (Settings.RecordingBitsPerSample * Settings.RecordingChannels) / 8;
            long max = ((uint.MaxValue - 44) / bytesPerSample) * bytesPerSample;
            c = b.AddString("RIFF", c);
            c = b.AddInt(max + 36, c);
            c = b.AddString("WAVEfmt ", c);
            c = b.AddInt(16, c);
            c = b.AddShort(1, c);
            c = b.AddShort(Settings.RecordingChannels, c);
            c = b.AddInt(Settings.RecordingSampleRate, c);
            c = b.AddInt(Settings.RecordingSampleRate * bytesPerSample, c);
            c = b.AddShort(bytesPerSample, c);
            c = b.AddShort(Settings.RecordingBitsPerSample, c);
            c = b.AddString("data", c);
            return b.AddInt(max, c);
        }
    }
}
