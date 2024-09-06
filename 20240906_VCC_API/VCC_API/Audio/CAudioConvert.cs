using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCC_API {
    internal class CAudioConvert {
        private System.Threading.Tasks.Task ThreadTask = null;
        public AutoResetEvent Signal = new(false);

        public CVCC_RestAPI VCC_RestAPI;
        private CVCC_SocketIO VCC_SocketIO = null;

        public CAudioConvert() {
            VCC_RestAPI = new CVCC_RestAPI(CAudio.Settings.VCC.Addr, CAudio.Settings.VCC.Port, CAudio.Settings.AudioIn.SampleRate, CAudio.Settings.AudioOut.SampleRate);

            switch (CAudio.Settings.VCC.GetProtocol()) {
                case CAudioSettings.CVCC.EProtocol.sio:
                    VCC_SocketIO = new CVCC_SocketIO(CAudio.Settings.VCC.Addr, CAudio.Settings.VCC.Port);
                    VCC_SocketIO.Connect();
                    break;
                case CAudioSettings.CVCC.EProtocol.rest: break;
                default: throw new Exception("CAudioConvert: Unknown VCC protocol. " + CAudio.Settings.VCC.Protocol.ToString());
            }


            ThreadTask = System.Threading.Tasks.Task.Run(ThreadExec);
        }

        public void ResetSampleRateTo48k() { VCC_RestAPI.ResetSampleRateTo48k(); }

        private void ThreadExec() {
            try {
                while (true) {
                    var buf = CAudio.AudioIn.GetCapturedSamples();
                    if (buf == null) {
                        Signal.WaitOne();
                        continue;
                    }

                    var IgnoreSamplesBlock = CAudio.Settings.AudioIn.SampleRate * 1; // バッファサイズが1秒以上なら溜まっちゃってるので捨てる。
                    if (IgnoreSamplesBlock <= buf.Samples.Length) {
                        Console.WriteLine(System.DateTime.Now.TimeOfDay.ToString() + "\tCAudioConvert: Overflow. " + IgnoreSamplesBlock + "<=" + buf.Samples.Length);
                        continue;
                    }

                    {
                        var srclen = buf.Samples.Length;
                        var sw = Stopwatch.StartNew();
                        switch (CAudio.Settings.VCC.GetProtocol()) {
                            case CAudioSettings.CVCC.EProtocol.sio: buf.Samples = VCC_SocketIO.Convert(buf.Samples); break;
                            case CAudioSettings.CVCC.EProtocol.rest: buf.Samples = VCC_RestAPI.Convert(buf.Samples); break;
                            default: throw new Exception();
                        }

                        if (buf.Samples == null) { continue; }
                        var elp = sw.Elapsed;
                        CAudio.RateStatus.AudioConvert = elp;
                        if (buf.Samples.Length < srclen) {
                            Console.WriteLine(System.DateTime.Now.TimeOfDay.ToString() + "\tCAudioConvert: src!=dst missmatch. VCC Converted. srcbuf.Length: " + srclen + ", converted.Length: " + buf.Samples.Length);
                        } else {
                            var chunkts = System.TimeSpan.FromSeconds((double)buf.Samples.Length / CAudio.Settings.AudioOut.SampleRate);
                            var per = elp / chunkts;
                            if (1 <= per) {
                                Console.WriteLine(System.DateTime.Now.TimeOfDay.ToString() + "\tCAudioConvert: GPU使用率が100%を超えた。 (" + (per * 100).ToString("F0") + "%) ChunkTS=" + chunkts.TotalMilliseconds.ToString("F0") + "ms");
                            }
                        }
                        CAudio.AudioOut.SendSamples(buf);
                    }

                }
            } catch (Exception ex) { Console.WriteLine("CAudioConvert: Thread exception: " + ex.ToString()); Environment.Exit(1); }
        }

        public TimeSpan Benchmark(TimeSpan ChunkTS, int SampleRate) {
            var buf = new float[(int)(ChunkTS.TotalSeconds * SampleRate)];
            for(var idx=0;idx<buf.Length; idx++) {
                buf[idx] = 0.5f;
            }

            var sw = Stopwatch.StartNew();
            switch (CAudio.Settings.VCC.GetProtocol()) {
                case CAudioSettings.CVCC.EProtocol.sio: VCC_SocketIO.Convert(buf); break;
                case CAudioSettings.CVCC.EProtocol.rest: VCC_RestAPI.Convert(buf); break;
                default: throw new Exception();
            }
            return sw.Elapsed;
        }
    }
}
