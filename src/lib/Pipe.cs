using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace eagle.tunnel.dotnet.core {
    public class Pipe {
        public string userFrom;
        private int speedSignal;
        private const int speedCheckThreshold = 1048576;
        private Socket socketFrom;
        private Socket socketTo;
        public Socket SocketFrom {
            set {
                if (socketFrom != null) {
                    try {
                        socketFrom.Shutdown (SocketShutdown.Both);
                        Thread.Sleep (10);
                        socketFrom.Close ();
                    } catch { }
                }
                socketFrom = value;
                if (socketFrom != null) {
                    BufferSize = socketFrom.ReceiveBufferSize;
                } else {
                    BufferSize = 0;
                }
            }
            get {
                return socketFrom;
            }
        }
        public Socket SocketTo {
            set {
                if (socketTo != null) {
                    try {
                        socketTo.Shutdown (SocketShutdown.Both);
                        Thread.Sleep (10);
                        socketTo.Close ();
                    } catch { }
                }
                socketTo = value;
            }
            get {
                return socketTo;
            }
        }
        public bool EncryptFrom { get; set; }
        public bool EncryptTo { get; set; }
        private Thread flowThread;
        private static byte EncryptionKey = 0x22;
        private byte[] bufferRead;
        private int BufferSize {
            get {
                if (bufferRead == null) {
                    return 0;
                } else {
                    return bufferRead.Length;
                }
            }
            set {
                if (value > 0) {
                    bufferRead = new byte[value];
                } else {
                    bufferRead = null;
                }
            }
        }
        private bool IsRunning;

        public Pipe (Socket from, Socket to, string user = null) {
            SocketFrom = from;
            SocketTo = to;
            EncryptFrom = false;
            EncryptTo = false;

            flowThread = new Thread (_Flow);
            flowThread.IsBackground = true;

            userFrom = user;
            speedSignal = 0;
            IsRunning = false;
        }

        public void Flow () {
            IsRunning = true;
            flowThread.Start ();
        }

        public bool Write (byte[] buffer, int offset, int count) {
            if (SocketTo != null) {
                byte[] tmpBuffer = new byte[count];
                Array.Copy (buffer, tmpBuffer, count);
                if (EncryptTo) {
                    tmpBuffer = Encryption (tmpBuffer);
                }
                try {
                    SocketTo.Send (tmpBuffer);
                    return true;
                } catch {
                    return false;
                }
            }
            return false;
        }

        public bool Write (byte[] buffer) {
            return Write (buffer, 0, buffer.Length);
        }

        public bool Write (string msg) {
            byte[] buffer = Encoding.UTF8.GetBytes (msg);
            return Write (buffer);
        }

        public string ReadString () {
            byte[] tmpBuffer = ReadByte ();
            if (tmpBuffer != null) {
                try {
                    return Encoding.UTF8.GetString (tmpBuffer);
                } catch {
                    return null;
                }
            } else {
                return null;
            }
        }

        public byte[] ReadByte () {
            if (SocketFrom != null) {
                int count = 0;
                try {
                    count = SocketFrom.Receive (bufferRead);
                } catch {
                    count = 0;
                }
                if (count != 0) {
                    if (userFrom != null) {
                        // check speed limit
                        speedSignal += count;
                        // reduce use of lock (Conf.Users.CheckSpeed().lock)
                        if (speedSignal > speedCheckThreshold) {
                            Conf.Users[userFrom].CheckSpeed (speedSignal);
                            speedSignal = 0;
                        }
                    }

                    byte[] tmpBuffer = new byte[count];
                    Array.Copy (bufferRead, tmpBuffer, count);
                    if (EncryptFrom) {
                        tmpBuffer = Decrypt (tmpBuffer);
                    }
                    return tmpBuffer;
                }
            }
            return null;
        }

        private void _Flow () {
            byte[] buffer = ReadByte ();
            while (IsRunning && buffer != null) {
                if (!Write (buffer)) {
                    break;
                } else {
                    buffer = ReadByte ();
                }
            }
            Close ();
        }

        public static byte[] Encryption (byte[] src) {
            byte[] des = new byte[src.Length];
            for (int i = 0; i < src.Length; ++i) {
                des[i] = (byte) (src[i] ^ EncryptionKey);
            }
            return des;
        }

        public static byte[] Decrypt (byte[] src) {
            byte[] des = new byte[src.Length];
            for (int i = 0; i < src.Length; ++i) {
                des[i] = (byte) (src[i] ^ EncryptionKey);
            }
            return des;
        }

        public void Close () {
            IsRunning = false;
            SocketFrom = null;
            SocketTo = null;
        }
    }
}