using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCC_API {
    internal class CVCC_SocketIO {
        private SocketIOClient.SocketIOOptions socketopts;
        private SocketIOClient.SocketIO socket;

        private void OnConnected(object sender, System.EventArgs e) { Debug.WriteLine("Socket.IO: Event: Connected."); }
        private void OnError(object sender, String e) { Console.WriteLine("Socket.IO: Event: Error. " + e); }
        private void OnDisconnected(object sender, string e) { Console.WriteLine("Socket.IO: Event: Disconnected. " + e); }
        private void OnReconnected(object sender, int e) { Console.WriteLine("Socket.IO: Event: Reconnected. " + e); }
        private void OnReconnectAttempt(object sender, int e) { Console.WriteLine("Socket.IO: Event: ReconnectAttempt. 再接続を試行しています。 " + e + " / " + socketopts.ReconnectionAttempts); }
        private void OnReconnectError(object sender, Exception e) { Console.WriteLine("Socket.IO: Event: ReconnectError. 再接続でエラーが発生しました。 " + e); }
        private void OnReconnectFailed(object sender, System.EventArgs e) { Console.WriteLine("Socket.IO: Event: ReconnectFailed. 再接続に失敗しました。"); }
        private void OnPing(object sender, System.EventArgs e) { Debug.WriteLine("Socket.IO: Event: Ping."); }
        private void OnPong(object sender, TimeSpan e) { Debug.WriteLine("Socket.IO: Event: Pong. " + e); }

        private void OnAnyHandler(string eventName, SocketIOClient.SocketIOResponse response) { }

        private AutoResetEvent ResponseSignal = new(false);

        private int t = 0; // DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        const bool n = false; // this.setting.sendingChunkAsBulk; trueにするとGPUが間に合わなかったチャンクは捨てられるみたい

        private byte[] responsebuf = null;

        public CVCC_SocketIO(string Host = @"127.0.0.1", int Port = 18000, string Path = @"/socket.io", int ReconnectionAttempts = 5) {
            socketopts = new SocketIOClient.SocketIOOptions();
            socketopts.Path = Path;
            socketopts.Transport = SocketIOClient.Transport.TransportProtocol.WebSocket;
            socketopts.AutoUpgrade = false;
            socketopts.ReconnectionAttempts = ReconnectionAttempts;

            socket = new SocketIOClient.SocketIO(@"ws://" + Host + @":" + Port + @"/test", socketopts);

            socket.OnConnected += OnConnected;
            socket.OnError += OnError;
            socket.OnDisconnected += OnDisconnected;
            socket.OnReconnected += OnReconnected;
            socket.OnReconnectAttempt += OnReconnectAttempt;
            socket.OnReconnectError += OnReconnectError;
            socket.OnReconnectFailed += OnReconnectFailed;
            socket.OnPing += OnPing;
            socket.OnPong += OnPong;

            socket.OnAny(OnAnyHandler);

            socket.On("response", response => {
                if (response.InComingBytes.Count != 1) { return; }
                var buf = response.InComingBytes[0];
                if (buf == null) { return; }
                Debug.WriteLine("Socket.IO: Receive response " + buf.Length + " bytes.");
                responsebuf = buf;
                ResponseSignal.Set();
            });
        }

        public bool Connect() {
            Console.WriteLine("Socket.IO: Connecting...");
            try {
                socket.ConnectAsync().Wait();
            } catch (Exception ex) {
                Console.WriteLine("Socket.IO: VCClientに接続できませんでした。 " + ex.ToString());
                return false;
            }
            if (!socket.Connected) {
                Console.WriteLine("Socket.IO: VCClientに接続できませんでした。");
                return false;
            }
            Console.WriteLine("Socket.IO: Connected.");

            Debug.WriteLine("Socket.IO: Namespace: " + socket.Namespace);

            return true;
        }

        public float[] Convert(float[] Samples) {
            if (!socket.Connected) { return null; }

            byte[] srcbuf;

            using (var ms = new System.IO.MemoryStream()) {
                using (var bw = new System.IO.BinaryWriter(ms, System.Text.Encoding.UTF8)) {
                    for (var idx = 0; idx < Samples.Length; idx++) {
                        bw.Write(Samples[idx]);
                    }
                }
                srcbuf = ms.ToArray();
            }

            int _t;
            lock (this) { _t = t; }

            responsebuf = null;
            ResponseSignal.Reset();
            socket.EmitAsync("request_message", new object[] { new object[] { _t, n, srcbuf } }).Wait();
            if (!ResponseSignal.WaitOne(3 * 1000)) { Debug.WriteLine("no response."); }

            lock (this) { t++; }

            if ((responsebuf == null) || (responsebuf.Length == 0)) {
                Console.WriteLine("Socket.IO: Response responsebuf==null or zero.");
                return null;
            }

            var dstbuf = new float[responsebuf.Length / 4];
            for (var idx = 0; idx < dstbuf.Length; idx++) {
                dstbuf[idx] = BitConverter.ToSingle(responsebuf, idx * 4);
            }

            return dstbuf;
        }
    }
}
