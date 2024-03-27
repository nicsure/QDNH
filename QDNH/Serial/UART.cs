using QDNH.Language;
using QDNH.Network;
using QDNH.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QDNH.Serial
{
    public class UART
    {
        private readonly SerialPort? port = null;
        private readonly FileStream? fport = null;
        private readonly Task? loop = null;
        private bool closed = false;
        private readonly Action<byte[], int> callback;

        public UART(string comPort, Action<byte[], int> callback)
        {
            this.callback = callback;
            try
            {
                if (comPort.StartsWith("tty"))
                {
                    var targetPort = new FileInfo($"/dev/{comPort}");
                    fport = targetPort.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);                    
                }
                else
                {
                    port = new SerialPort(comPort, 38400, Parity.None, 8, StopBits.One);
                    port.Open();
                }
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
            using (port) using (fport)
            {
                if (port != null)
                {
                    port.WriteTimeout = 1;
                    try { port.Close(); } catch { }
                }
                if(fport != null)
                {
                    try { fport.Close(); } catch { }
                }
            }
            using (loop)
                loop?.Wait();
        }

        public void Send(byte[] data, int offset, int length)
        {
            if (!closed)
            {
                if (port != null)
                {
                    try { port.Write(data, offset, length); } catch { }
                }
                else if(fport != null)
                {
                    try { fport.Write(data, offset, length); } catch { }
                    try { fport.Flush(); } catch { }
                }
            }
        }

        private void Loop()
        {
            while (!closed)
            {
                byte[] b = new byte[512];                               
                int br = 0;
                if (port != null)
                {
                    try { br = port.Read(b, 0, b.Length); } catch { br = -1; }
                }
                else if (fport != null)
                {
                    try { br = fport.Read(b, 0, b.Length); } catch { br = -1; }
                }
                if (br <= 0)
                    break;
                callback(b, br);
            }
        }

    }

}
