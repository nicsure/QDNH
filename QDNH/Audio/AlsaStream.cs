using QDNH.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDNH.Audio
{
    public class AlsaStream : Stream
    {
        private long pos = 0;
        private long wpos;
        private long rpos = 0;
        private readonly long latency;
        private readonly long low;
        private readonly long high;
        private readonly int bSize;
        private readonly byte[] buf;
        public AlsaStream(int bufferSize, int latency)
        {
            this.latency = latency;
            low = latency / 2;
            high = latency + low;
            bSize = bufferSize;
            buf = new byte[bSize];
            wpos = ALSA.WavHeader(buf, 0);
        }
        public void Append(byte[] data, int length)
        {
            lock (buf)
            {
                // data needs to go into the buffer "just ahead" of the read position
                // so we need to see if the write position is reasonable based on the read position
                // first let's see the difference between read and write
                long dif = wpos - rpos;
                // ideally the dif will be whatever the latency is set to
                // so we're going to set the range of a reasonable dif to between latency/2 and latency*1.5
                if (dif < low || dif > high)
                {
                    // if the diff is out of range we set the write position to whatever the read position is currently + latency
                    // this will resync audio that drifts out of sync, moving it forward or backward to try to maintain the latency
                    wpos = rpos + latency;
                }
                // now calculate the start and end positions we need to fill in the buffer
                int start = (int)(wpos % bSize);
                wpos += length;
                int end = (int)(wpos % bSize);
                if (end > start)
                {
                    // if the end position is larger than the start then we don't need to wrap around, so just a single copy is fine
                    Array.Copy(data, 0, buf, start, end - start);
                }
                else
                {
                    // otherwise we need to split the incoming data and copy the start of this data to the end of the buffer
                    // and the end of the data to the start of the buffer (wrap around)
                    int len = bSize - start;
                    Array.Copy(data, 0, buf, start, len);
                    Array.Copy(data, len, buf, 0, end);
                }
            }
        }
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => long.MaxValue;
        public override long Position { get => pos; set => throw new NotImplementedException(); }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (buf)
            {
                int start = (int)(rpos % bSize);
                rpos += count;
                int end = (int)(rpos % bSize);
                if (end > start)
                {
                    Array.Copy(buf, start, buffer, 0, end - start);
                }
                else
                {
                    // similar situation to the wrapping done in the Append method, except we're unwrapping it here
                    int len = bSize - start;
                    Array.Copy(buf, start, buffer, 0, len);
                    Array.Copy(buf, 0, buffer, len, end);
                }
            }
            pos += count;
            return count;
        }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();
        public override void SetLength(long value) => throw new NotImplementedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();
    }
}
