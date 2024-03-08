using QDNH.Language;
using QDNH.Settings;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDNH.Serial
{
    public class UART
    {
        private readonly SerialPort port;
        private readonly Task? loop = null;
        private bool closed = false;
        private readonly Action<byte[], int> callback;
        public UART(string comPort, Action<byte[], int> callback)
        {
            this.callback = callback;
            try
            {
                port = new SerialPort(comPort, 38400, Parity.None, 8, StopBits.One);
                port.Open();
                loop = Task.Run(Loop);
            }
            catch
            {
                closed = true;
                Vars.Err($"{Lang.OpenComError} {comPort}");
                throw;
            }
        }

        public void Close()
        {
            closed = true;
            using (port)
            {
                port.WriteTimeout = 1;
                try { port.Close(); } catch { }
            }
            using (loop)
                loop?.Wait();
        }

        public void Send(byte[] data, int offset, int length)
        {
            if (!closed)
            {
                try { port.Write(data, offset, length); } catch { }
            }
        }

        private void Loop()
        {
            while(!closed)
            {
                byte[] b = new byte[512];
                int br;
                try { br = port.Read(b, 0, b.Length); } catch { br = -1; }
                if (br <= 0)
                    break;
                callback(b, br);
            }
        }

    }

}
