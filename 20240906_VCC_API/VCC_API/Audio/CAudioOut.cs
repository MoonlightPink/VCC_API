using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCC_API {
    internal class CAudioOut {
        private System.Threading.Tasks.Task ThreadTask = null;
        public AutoResetEvent Signal = new(false);

        private CVBAN VBAN;

        public CAudioOut() {
            VBAN = new CVBAN(CAudio.Settings.VBAN.Addr, CAudio.Settings.VBAN.Port, CAudio.Settings.AudioOut.SampleRate, CAudio.Settings.VBAN.StreamName);

            ThreadTask = System.Threading.Tasks.Task.Run(ThreadExec);
        }

        private List<float> AudioBuf = new List<float>();

        public void ThreadExec() {
            try {
                while (true) {
                    Signal.WaitOne();

                    Console.WriteLine(System.DateTime.Now.TimeOfDay.ToString() + "\tCAudioOut: Started.");

                    var ResetTimeout = System.DateTime.Now + System.TimeSpan.FromSeconds(3);

                    var EndDT = System.DateTime.Now;

                    var BlankTS = System.TimeSpan.FromSeconds(0);

                    var buf = new float[256];

                    while (true) {
                        if (ResetTimeout != System.DateTime.FromBinary(0)) {
                            if (ResetTimeout <= System.DateTime.Now) {
                                ResetTimeout = System.DateTime.FromBinary(0);
                                CAudio.ResetBuffer.Set();
                            }
                        }

                        if (CAudio.ResetBuffer.GetAudioOut()) { AudioBuf.Clear(); }

                        int bufcnt;
                        bool isBlank;

                        lock (AudioBuf) {
                            if (1 <= AudioBuf.Count) {
                                isBlank = false;
                                bufcnt = (buf.Length < AudioBuf.Count) ? buf.Length : AudioBuf.Count;
                                for (var idx = 0; idx < bufcnt; idx++) {
                                    buf[idx] = AudioBuf[idx];
                                }
                                AudioBuf.RemoveRange(0, bufcnt);
                            } else {
                                Console.Write("!"); // System.DateTime.Now.TimeOfDay.ToString() + "\t!!! CAudioOut: Empty.");
                                isBlank = true;
                                bufcnt = buf.Length;
                                for (var idx = 0; idx < bufcnt; idx++) {
                                    buf[idx] = 0;
                                }
                            }
                        }

                        var BlockTS = System.TimeSpan.FromSeconds((double)bufcnt / CAudio.Settings.AudioOut.SampleRate);

                        if (!isBlank) {
                            BlankTS = System.TimeSpan.FromSeconds(0);
                        } else {
                            BlankTS += BlockTS;
                            if (System.TimeSpan.FromSeconds(1) < BlankTS) { break; } // 約1秒間無音なら停止する
                        }

                        if (bufcnt != 0) { VBAN.SendPacket(buf, 0, bufcnt); }

                        EndDT += BlockTS;
                        while (System.DateTime.Now < EndDT) { }
                    }

                    Signal.Reset();

                    Console.WriteLine(System.DateTime.Now.TimeOfDay.ToString() + "\tCAudioOut: Stopped.");
                }
            } catch (Exception ex) { Console.WriteLine("CAudioOut: Thread exception: " + ex.Message + "\n" + ex.StackTrace); Environment.Exit(1); }
        }

        public void SendSamples(CAudio.CAudioBlock AudioBlock) {
            if (AudioBlock.Samples.Length == 0) { return; }
            //Console.WriteLine(System.DateTime.Now.TimeOfDay.ToString() + "\tCAudioOut: SendSamples: AudioBlock.Samples.Length=" + AudioBlock.Samples.Length);
            lock (AudioBuf) {
                if (AudioBlock.isSpeakStart) {
                    for (var idx = AudioBlock.Samples.Length; idx < (CAudio.Settings.AudioOut.SampleRate * 0.0); idx++) { // 初動遅延
                        AudioBuf.Add(0);
                    }
                }
                CAudio.RateStatus.AudioOut = System.TimeSpan.FromSeconds((double)AudioBuf.Count / CAudio.Settings.AudioOut.SampleRate);
                foreach (var Sample in AudioBlock.Samples) {
                    AudioBuf.Add(Sample);
                }
            }
            Signal.Set();
        }
    }
}
