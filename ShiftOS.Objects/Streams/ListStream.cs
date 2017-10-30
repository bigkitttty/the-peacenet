using System;
using System.IO;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

namespace Plex.Objects.Streams
{
    /// <summary>
    /// A Stream interface to an internal List of bytes. Reading will
    /// pop bytes from the top and writing will push them to the bottom.
    /// This is like a UNIX pipe in a way. One end of the pipe should
    /// see this as read-only and the other as write-only, which you can
    /// accomplish with a ReadOnlyStream and WriteOnlyStream around
    /// this one.
    /// </summary>
    public class ListStream: Stream
    {
        public override bool CanRead { get { return true; } }
        public override bool CanWrite { get { return true; } }
        public override bool CanSeek { get { return false; } }
        private List<byte> myBuffer;
        private bool closed;
        private EventWaitHandle haveWritten;
        
        public override long Length { get { throw new NotSupportedException("The stream does not support seeking."); } }
        public override long Position { get { throw new NotSupportedException("The stream does not support seeking."); } set { throw new NotSupportedException("The stream does not support seeking."); } }
        
        public override void Flush()
        {
        }
        
        public override int Read(byte[] buffer, int offset, int count)
        {
            int curcount, read = 0;
            while (!closed && read < count)
            {
                haveWritten.WaitOne();
                lock (myBuffer)
                {
                    curcount = Math.Min(myBuffer.Count, count);
                    Array.Copy(myBuffer.Take(count).ToArray(), 0, buffer, offset, count);
                    myBuffer.RemoveRange(0, count);
                }
                read += curcount;
            }
            return read;
        }
        
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("The stream does not support seeking.");
        }
        
        public override void SetLength(long value)
        {
            throw new NotSupportedException("The stream does not support seeking.");
        }
        
        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (myBuffer)
            {
                if (closed)
                    throw new ObjectDisposedException("Write was called after the stream was closed.");
                myBuffer.AddRange(buffer.Skip(offset).Take(count));
                haveWritten.Set();
            }
        }
        
        /// <summary>
        /// Disables writing to the stream and makes reading non-blocking.
        /// </summary>
        public void Dispose()
        {
            lock (myBuffer)
            {
                closed = true;
                haveWritten.Set();
            }
        }
        
        public ListStream()
        {
            myBuffer = new List<byte>();
            closed = false;
            haveWritten = new AutoResetEvent(false);
        }
    }
}
