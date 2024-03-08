using NAudio.CoreAudioApi;
using NAudio.Wave;
using QDNH.Language;
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
                provider = new(Vars.WaveFormat);
                playback = new WasapiOut(device, AudioClientShareMode.Shared, true, Vars.LatencyMils);
                playback.Init(provider);
                playback.Play();
                return;
            }
            catch(Exception e)
            {
                Vars.Err(Lang.OpenOutputError);
                exception = e;
            }
            Close();
            throw exception;
        }

        public void Send(byte[] data, int length)
        {
            if (provider != null)
            {
                if (provider.BufferedDuration.TotalSeconds > Vars.LatencySecs)
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
