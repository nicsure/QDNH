using NAudio.CoreAudioApi;
using QDNH.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDNH.Audio
{
    public class Capture
    {
        private readonly WasapiCapture? capture;
        private readonly Action<byte[], int> callback;

        public Capture(MMDevice device, Action<byte[], int> callback)
        {
            this.callback = callback;
            Exception exception;
            try
            {
                capture = new WasapiCapture(device, true, Vars.LatencyMils)
                {                    
                    WaveFormat = new(22050, 16, 1),                    
                };
                capture.DataAvailable += DataAvailable;
                capture.StartRecording();
                return;
            }
            catch(Exception e)
            {
                exception = e;
                Console.Error.WriteLine("Error opening audio input device");
            }
            Close();
            throw exception;
        }

        private void DataAvailable(object? sender, NAudio.Wave.WaveInEventArgs e)
        {
            callback(e.Buffer, e.BytesRecorded);
        }

        public void Close()
        {
            using(capture)
            {
                try { capture?.StopRecording(); } catch { }
                if (capture != null)
                {
                    try { capture.DataAvailable -= DataAvailable; } catch { }
                }
            }
        }
    }
}
