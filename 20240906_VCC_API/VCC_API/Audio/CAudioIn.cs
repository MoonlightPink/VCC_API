using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace VCC_API {
    internal class CAudioIn {
        private NAudio.CoreAudioApi.WasapiCapture WaveIn;

        private NAudio.CoreAudioApi.MMDevice MMDevice_FindCapture(string FriendlyName) {
            foreach (var Device in new NAudio.CoreAudioApi.MMDeviceEnumerator().EnumerateAudioEndPoints(NAudio.CoreAudioApi.DataFlow.Capture, NAudio.CoreAudioApi.DeviceState.Active)) {
                if (Device.FriendlyName.Equals(FriendlyName)) {
                    Console.WriteLine("CAudioIn: Capture device selected. [" + Device.FriendlyName + "]");
                    return Device;
                }
            }

            foreach (var Device in new NAudio.CoreAudioApi.MMDeviceEnumerator().EnumerateAudioEndPoints(NAudio.CoreAudioApi.DataFlow.Capture, NAudio.CoreAudioApi.DeviceState.Active)) {
                Console.WriteLine(Device.ID + "\t" + Device.FriendlyName + "\t" + Device.DeviceFriendlyName + "\t" + Device.DataFlow.ToString());
            }

            throw new Exception("CAudioIn: 指定されたCaptureデバイスがWASAPIで見つかりませんでした。 [" + FriendlyName + "]");
        }

        public CAudioIn() {
            WaveIn = new NAudio.CoreAudioApi.WasapiCapture(MMDevice_FindCapture(CAudio.Settings.AudioIn.DeviceName));

            WaveIn.DataAvailable += WaveInOnDataAvailable;
            WaveIn.RecordingStopped += WaveInOnRecordingStopped;

            var wf = WaveIn.WaveFormat;
            Console.WriteLine("CAudioIn: WaveIn format. SampleRate:" + wf.SampleRate + ", bps:" + wf.BitsPerSample + ", chs:" + wf.Channels + ",");

            if ((wf.BitsPerSample != 32)) { throw new Exception("CAudioIn: 32bits以外は録音しない。 bps:" + WaveIn.WaveFormat.BitsPerSample); }

            var Ratio = (double)wf.SampleRate / CAudio.Settings.AudioIn.SampleRate;
            if (Ratio != System.Math.Ceiling(Ratio)) { throw new Exception("CAudioIn: マイク入力周波数(" + wf.SampleRate + "Hz)は、VCC入力周波数(" + CAudio.Settings.AudioIn.SampleRate + "Hz)の、整数倍である必要があります。 Ratio=" + Ratio); }
        }

        public void Start() { lock (this) { WaveIn.StartRecording(); } }
        public void Stop() { lock (this) { WaveIn.StopRecording(); } }

        private void WaveInOnRecordingStopped(object sender, NAudio.Wave.StoppedEventArgs e) {
            return;
            lock (this) {
                throw new Exception("CAudioIn: 録音ストリームが停止しました。");
            }
        }

        private bool isSpeakStart = false;
        private List<Single> WriteBuf = new List<Single>();

        private System.TimeSpan EnableTS = System.TimeSpan.Zero;

        private System.DateTime ResetTimeout = System.DateTime.FromBinary(0);


        private void WaveInOnDataAvailable(object sender, NAudio.Wave.WaveInEventArgs e) {
            try {
                if (CAudio.ResetBuffer.GetAudioIn()) { WriteBuf.Clear(); }

                var SampleRate = WaveIn.WaveFormat.SampleRate;
                var SampleLen = WaveIn.WaveFormat.BitsPerSample / 8;
                var Channels = WaveIn.WaveFormat.Channels;
                var SamplesCount = e.BytesRecorded / SampleLen / Channels;

                var Ratio = SampleRate / CAudio.Settings.AudioIn.SampleRate;
                var smps = new float[SamplesCount / Ratio];
                var BlockSize = Channels * Ratio;

                var _isSpeakStart = false;
                var startidx = 0;

                for (var idx = 0; idx < smps.Length; idx++) {
                    float smp = 0;
                    for (var blkidx = 0; blkidx < BlockSize; blkidx++) {
                        smp += BitConverter.ToSingle(e.Buffer, ((idx * BlockSize) + blkidx) * SampleLen);
                    }
                    smp /= BlockSize;
                    var Level = System.Math.Abs(smp);
                    CAudio.RateStatus.PeakLevel = Level;
                    if (CAudio.Settings.AudioIn.ThresholdLevel <= Level) {
                        if (EnableTS < System.TimeSpan.Zero) {
                            _isSpeakStart = true;
                            startidx = idx - (int)(0.05 * SampleRate); // 50ms未満（16kHzで800サンプル未満）だとVCCエラーが返るので、先頭無音領域を追加する。（サンプリングレートに依存するか不明。チャンクサイズ設定には影響がなかった）
                            if (startidx < 0) { startidx = 0; }
                        }
                        EnableTS = System.TimeSpan.FromMinutes(1); // 無音検出から指定時間中は録音する
                        ResetTimeout = System.DateTime.Now + System.TimeSpan.FromSeconds(5); // 無音検出から指定時間後にバッファをリセットする
                    }
                    smps[idx] = smp;
                }

                if (ResetTimeout != System.DateTime.FromBinary(0)) {
                    if (ResetTimeout < System.DateTime.Now) {
                        ResetTimeout = System.DateTime.FromBinary(0);
                        CAudio.ResetBuffer.Set();
                    }
                }

                EnableTS -= System.TimeSpan.FromSeconds((double)(smps.Length - startidx) / SampleRate);
                if (EnableTS < System.TimeSpan.Zero) { return; }

                // Console.WriteLine(System.DateTime.Now.TimeOfDay.ToString() + "\tCAudioIn: startidx=" + startidx.ToString() + ", SamplesCount=" + SamplesCount.ToString() + ", Stored=" + (SamplesCount - startidx).ToString());

                lock (this) {
                    if (_isSpeakStart) {
                        isSpeakStart = true;
                        WriteBuf.Clear();
                    }
                    for (var idx = startidx; idx < smps.Length; idx++) {
                        WriteBuf.Add(smps[idx]);
                    }
                }

                if (CAudio.AudioConvert.Signal != null) { CAudio.AudioConvert.Signal.Set(); }
            } catch (Exception ex) { Console.WriteLine("CAudioIn: WaveInOnDataAvailable exception: " + ex.ToString()); Environment.Exit(1); }
        }

        public CAudio.CAudioBlock GetCapturedSamples() {
            lock (this) {
                var ChunkSize = (int)(CAudio.Settings.AudioIn.GetChunkTS().TotalSeconds * CAudio.Settings.AudioIn.SampleRate);

                if (WriteBuf.Count < ChunkSize) { return null; }
                CAudio.RateStatus.AudioIn = System.TimeSpan.FromSeconds((double)WriteBuf.Count / CAudio.Settings.AudioIn.SampleRate);


                var tmp = new float[ChunkSize];
                for (var idx = 0; idx < tmp.Length; idx++) {
                    tmp[idx] = WriteBuf[idx];
                }
                WriteBuf.RemoveRange(0, tmp.Length);

                var res = new CAudio.CAudioBlock(isSpeakStart, tmp);
                isSpeakStart = false;
                return res;
            }
        }
    }
}
