using SocketIOClient.Transport.WebSockets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VCC_API;

namespace VCC_API_Setup {
    /// <summary>
    /// WBenchmarkDlg.xaml の相互作用ロジック
    /// </summary>
    public partial class WBenchmarkDlg : Window {
        private MainWindow Parent;

        private bool Started = false;

        public bool ReqExit = false;

        public WBenchmarkDlg(MainWindow _Parent) {
            InitializeComponent();

            Parent = _Parent;
        }

        private Task BenchmarkTask = null;
        private bool ReqStopBenchmark = false;
        private List<string> Logs = new List<string>();

        private void CancelBtn_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void ExecBenchmarkTask() {
            lock (this) {
                Logs.Add(System.DateTime.Now.ToString() + " ベンチマークの前準備をしています。");
            }

            CAudio.Start(false);

            Logs.Add(System.DateTime.Now.ToString() + " VCClient settings.");
            {
                var Item = CVCC_RestAPI_JSON.JSON_hello_Deserialize(CAudio.AudioConvert.VCC_RestAPI.HttpGet_ThrowException(@"/hello"));
                Logs.Add("hello response from " + Item.credit + ". [" + Item.message + "]");
            }

            int gpu_device_id;

            {
                var Item = CAudio.AudioConvert.VCC_RestAPI.SetSampleRate(CAudio.Settings.AudioIn.SampleRate, CAudio.Settings.AudioOut.SampleRate);
                Logs.Add("current_slot_index: " + Item.current_slot_index);
                Logs.Add("extra_frame_sec: " + Item.extra_frame_sec);
                gpu_device_id = Item.gpu_device_id_int;
            }

            {
                var Items = CVCC_RestAPI_JSON.JSON_gpu_device_manager_devices_Deserialize(CAudio.AudioConvert.VCC_RestAPI.HttpGet_ThrowException(@"/gpu-device-manager/devices"));
                Console.WriteLine("VCClient gpu device manager.");
                for (var idx = 0; idx < Items.Count; idx++) {
                    var Item = Items[idx];
                    if (Item.device_id_int == gpu_device_id) {
                        var RamGB = (double)Item.adapter_ram / 1024 / 1024 / 1024;
                        Logs.Add("device_id=" + Item.device_id_int + " " + Item.name + " " + RamGB.ToString("F3") + "GB CUDA" + Item.cuda_compute_version_major + "." + Item.cuda_compute_version_minor);
                    }
                }
            }

            {
                var Item = CVCC_RestAPI_JSON.JSON_voice_changer_manager_information_Deserialize(CAudio.AudioConvert.VCC_RestAPI.HttpGet_ThrowException(@"/voice-changer-manager/information"));
                var SlotInfo = Item.voice_changer_information.pipeline_info.slot_info;
                Logs.Add("voice_changer_type: " + SlotInfo.voice_changer_type);
                Logs.Add("name: " + SlotInfo.name);
                Logs.Add("description: " + SlotInfo.description);
                Logs.Add("credit: " + SlotInfo.credit);
                Logs.Add("terms_of_use_url: " + SlotInfo.terms_of_use_url);
                Logs.Add("is_onnx: " + SlotInfo.is_onnx);
                Logs.Add("inferencer_type: " + SlotInfo.inferencer_type);
                Logs.Add("sample_rate: " + SlotInfo.sample_rate);
                Logs.Add("version: " + SlotInfo.version);
            }

            var SampleRate = CAudio.Settings.AudioIn.SampleRate;

            for (var idx = 0; idx < 10; idx++) {
                CAudio.AudioConvert.Benchmark(System.TimeSpan.FromSeconds(1), SampleRate);
                lock (this) { if (ReqStopBenchmark) { break; } }
            }

            lock (this) {
                Logs.Add(System.DateTime.Now.ToString() + " ベンチマークを開始しました。");
            }

            for (var ChunkSize = 0.05; ChunkSize <= 1.01;) {
                var ChunkTS = TimeSpan.FromSeconds(ChunkSize);

                for (var idx = 0; idx < 5; idx++) {
                    CAudio.AudioConvert.Benchmark(ChunkTS, SampleRate);
                    lock (this) { if (ReqStopBenchmark) { break; } }
                }
                lock (this) { if (ReqStopBenchmark) { break; } }

                var sws = new List<TimeSpan>();
                for (var idx = 0; idx < 5; idx++) {
                    sws.Add(CAudio.AudioConvert.Benchmark(ChunkTS, SampleRate));
                    lock (this) { if (ReqStopBenchmark) { break; } }
                }
                lock (this) { if (ReqStopBenchmark) { break; } }

                { // 最小と最大を捨てる
                    var min = TimeSpan.Zero;
                    var max = TimeSpan.Zero;
                    foreach (var sw in sws) {
                        if (min == TimeSpan.Zero) { min = sw; }
                        if (sw < min) { min = sw; }
                        if (max == TimeSpan.Zero) { max = sw; }
                        if (max < sw) { max = sw; }
                    }
                    sws.Remove(min);
                    sws.Remove(max);
                }

                {
                    var min = TimeSpan.Zero;
                    var max = TimeSpan.Zero;
                    var total = TimeSpan.Zero;
                    foreach (var sw in sws) {
                        if (min == TimeSpan.Zero) { min = sw; }
                        if (sw < min) { min = sw; }
                        if (max == TimeSpan.Zero) { max = sw; }
                        if (max < sw) { max = sw; }
                        total += sw;
                    }
                    var avg = total / sws.Count;

                    lock (this) {
                        Logs.Add("Chunk size: " + ChunkSize.ToString("F2") + "秒, GPU使用率: (最小=" + (min.TotalSeconds / ChunkSize * 100).ToString("F0") + "%) (平均=" + (avg.TotalSeconds / ChunkSize * 100).ToString("F0") + "%) (最大=" + (max.TotalSeconds / ChunkSize * 100).ToString("F0") + "%)");
                    }
                }

                if (ChunkSize < 0.3) {
                    ChunkSize += 0.01;
                } else if (ChunkSize < 0.5) {
                    ChunkSize += 0.02;
                } else if (ChunkSize < 0.7) {
                    ChunkSize += 0.05;
                } else {
                    ChunkSize += 0.1;
                }
            }

            CAudio.Stop();

            CAudio.AudioConvert.VCC_RestAPI.ResetSampleRateTo48k();

            lock (this) {
                Logs.Add(System.DateTime.Now.ToString() + " ベンチマークを" + (ReqStopBenchmark ? "中断" : "終了") + "しました。");
                Logs.Add("[END]");
            }
        }

        private void BenchmarkStartBtn_Click(object sender, RoutedEventArgs e) {
            BenchmarkStartBtn.IsEnabled = false;
            CancelBtn.Content = "キャンセルして閉じる";

            Parent.SaveSettings();

            Started = true;

            BenchmarkTask = System.Threading.Tasks.Task.Run(ExecBenchmarkTask);

            IntervalTimer.Tick += new EventHandler(IntervalTimerTick);
            IntervalTimer.Interval = TimeSpan.FromSeconds(0.1);
            IntervalTimer.Start();
        }

        private System.Windows.Threading.DispatcherTimer IntervalTimer = new System.Windows.Threading.DispatcherTimer();
        private void IntervalTimerTick(object sender, EventArgs e) {
            lock (this) {
                foreach (var Log in Logs) {
                    if (Log.Equals("[END]")) {
                        CancelBtn.Content = "ウィンドウを閉じる";
                    } else {
                        LogList.Items.Add(Log);
                        LogList.ScrollIntoView(Log);
                    }
                }
                Logs.Clear();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (IntervalTimer != null) { IntervalTimer.Stop(); }

            if (BenchmarkTask != null) {
                lock (this) { ReqStopBenchmark = true; }
                BenchmarkTask.Wait();
                BenchmarkTask = null;
            }

            IntervalTimerTick(null, null);

            using (var wfs = new System.IO.StreamWriter("VCC_API_Benchmark_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt")) {
                foreach (var Log in LogList.Items) {
                    wfs.WriteLine(Log);
                }
            }

            if (Started) { ReqExit = true; }
        }
    }
}
