using NAudio.CoreAudioApi;
using NAudio.Wave;
using QDNH.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDNH.Audio
{
    public class Playback
    {
        private readonly WasapiOut? playback;
        private readonly BufferedWaveProvider? provider;
        public Playback(MMDevice device) 
        {
            Exception exception;
            try
            {
                provider = new(new(22050, 16, 1));
                playback = new WasapiOut(device, AudioClientShareMode.Shared, true, Vars.Latency);
                playback.Init(provider);
                playback.Play();
                return;
            }
            catch(Exception e)
            {
                Console.Error.WriteLine("Error opening audio output device");
                exception = e;
            }
            Close();
            throw exception;
        }

        public void Send(byte[] data, int length)
        {
            if (provider != null)
            {
                if (provider.BufferedDuration.TotalSeconds > Vars.LatencyMS)
                    provider.ClearBuffer();
                provider.AddSamples(data, 0, length);
            }
        }

        public void Close()
        {
            using (playback)
            {
                playback?.Stop();
                provider?.ClearBuffer();
            }
        }
    }
}
