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
    internal class CaptureALSA : ICapture
    {
        private readonly Action<byte[], int> callback;
        private readonly CancellationTokenSource tokenSource = new();
        private readonly Task? bgTask = null;

        public CaptureALSA(Action<byte[], int> callback)
        {
            this.callback = callback;
            if(ALSA.Device is ISoundDevice device)
            {
                bgTask = Task.Run(() =>
                {
                    try
                    {
                        // the recording system will output a WAV header at the start,
                        // there's no need to handle this as it has an even length and will
                        // just manifest as a "click" at the very start of the audio.
                        // However, this will never normally get heard anway.
                        device.Record(DataAvailable, tokenSource.Token);
                    }
                    catch
                    {
                        Vars.Err($"[ALSA] {Lang.OpenInputError}");
                    }
                });
            }
        }

        private void DataAvailable(byte[] data)
        {
            if (!tokenSource.IsCancellationRequested)
                callback(data, data.Length);
        }

        public void Close()
        {
            using (bgTask) using(tokenSource)
            {
                tokenSource.Cancel();
                bgTask?.Wait();
            }
        }
    }
}
