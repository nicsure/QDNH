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
    public class PlaybackALSA : IPlayback
    {
        private readonly AlsaStream stream = new(32768, (Vars.LatencyMils * 441) / 10);
        private readonly CancellationTokenSource tokenSource = new();
        private readonly Task? bgTask = null;
        public PlaybackALSA()
        {
            if (ALSA.Device is ISoundDevice device)
            {
                // the Alsa.net library doesn't seem to have any async methods for audio operations
                // but no matter, we can do things the old-fashioned way easily enough.
                bgTask = Task.Run(() =>
                {
                    try
                    {
                        device.Play(stream, tokenSource.Token);
                    }
                    catch
                    {
                        Vars.Err($"[ALSA] {Lang.OpenOutputError}");
                    }
                });
            }
        }
        public void Close()
        {
            using (bgTask) using (tokenSource) using (stream)
            {
                tokenSource.Cancel();
                stream.Close();
                bgTask?.Wait();
            }
        }
        public void Send(byte[] data, int length)
        {
            if (!tokenSource.IsCancellationRequested)
                stream.Append(data, length);
        }
    }

}
