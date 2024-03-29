using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDNH.Audio
{
    public interface IPlayback
    {
        void Send(byte[] data, int length);
        void Close();
    }
}
