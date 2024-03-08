using QDNH.Language;
using QDNH.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace QDNH.Network
{

    public class Listen
    {
        private readonly TcpListener listener;
        private readonly Action<byte[], int> callback;
        private TcpClient? client = null;
        private NetworkStream? stream = null;
        private bool closed = false, authenticated = false;
        private readonly bool allowSkip = false;
        private readonly Task? loop = null;
        private Task skipTask = new(() => { });
        public Listen(int port, Action<byte[], int> callback, bool allowSkip)
        {
            skipTask.Start();
            skipTask.Wait();
            this.allowSkip = allowSkip;
            this.callback = callback;
            try
            {
                listener = new(IPAddress.Any, port);
                listener.Start();
                loop = Loop();
            }
            catch
            {
                closed = true;
                Vars.Err($"{Lang.StartError} {port}");
                throw;
            }
        }

        public void Close()
        {
            using (loop)
            {
                closed = true;
                try { listener.Stop(); } catch { }
                Disconnect();
                loop?.Wait();
            }
        }

        public void Disconnect()
        {
            using (client)
            {
                using (stream)
                {
                    try { stream?.Close(); } catch { }
                    try { client?.Close(); } catch { }
                }
            }
            client = null;
            stream = null;
        }

        public void Send(byte[] data, int offset, int count)
        {
            var st = stream;
            if (st != null && authenticated)
            {
                switch (allowSkip)
                {
                    case true:
                        if (!skipTask.IsCompleted)
                            return;
                        break;
                    case false:
                        skipTask.Wait();
                        break;
                }
                using (skipTask)
                {
                    skipTask = Send2(st, data, offset, count);
                }
            }
        }

        private static async Task Send2(NetworkStream ns, byte[] data, int offset, int count)
        {
            try
            {
                await ns.WriteAsync(data.AsMemory(offset, count));
                await ns.FlushAsync();
            }
            catch { }
        }

        private async Task Loop()
        {
            while (!closed)
            {
                authenticated = false;
                try
                {
                    client = await listener.AcceptTcpClientAsync();
                    stream = client.GetStream();
                }
                catch { continue; }
                Authenticator auth = new(Vars.Password);
                await Send2(stream, auth.Salt, 0, auth.Salt.Length);
                byte[] chal = new byte[auth.Hash.Length];
                for (int i = 0; i < chal.Length; i++)
                {
                    int b;
                    try { b = stream.ReadByte(); } catch { b = -1; }
                    if (b < 0) break;
                    chal[i] = (byte)b;
                }
                if (auth.Challenge(chal))
                {
                    //Debug.WriteLine($"Auth Passed {client.Client.LocalEndPoint}");
                    authenticated = true;
                    var st = stream;
                    while (!closed)
                    {
                        byte[] b = new byte[4096];
                        int br;
                        try { br = await st.ReadAsync(b); } catch { br = -1; }
                        if (br <= 0) break;
                        callback(b, br);
                    }
                }
                else
                {
                    //Debug.WriteLine($"Auth Failed {client.Client.LocalEndPoint}");
                }
                Disconnect();
            }
        }
    }
}
