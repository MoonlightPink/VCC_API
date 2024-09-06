using NAudio.CoreAudioApi;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VCC_API;

namespace VCC_API_Setup {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private CAudio Audio;

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            this.Title = "VCC_API_Setup.exe 環境設定";

            CAudio.Settings = new CAudioSettings();

            if (System.IO.File.Exists(CAudio.SettingsFilename)) {
                CAudio.LoadFromFile(CAudio.SettingsFilename);
            }

            //CAudio.Start(false);

            { // AudioIn
                AudioIn_ChunkSecs.Items.Clear();
                for (var sec = 0.05; sec <= 1; sec += 0.01) {
                    var thisidx = AudioIn_ChunkSecs.Items.Add(sec.ToString("F2"));
                    if(System.Math.Abs(CAudio.Settings.AudioIn.ChunkSecs - sec)<0.001) { AudioIn_ChunkSecs.SelectedIndex = thisidx; }
                }
                if (AudioIn_ChunkSecs.SelectedIndex == -1) { AudioIn_ChunkSecs.SelectedIndex = 0; }

                AudioIn_DeviceName.Items.Clear();
                foreach (var Device in new NAudio.CoreAudioApi.MMDeviceEnumerator().EnumerateAudioEndPoints(NAudio.CoreAudioApi.DataFlow.Capture, NAudio.CoreAudioApi.DeviceState.Active)) {
                    var thisidx = AudioIn_DeviceName.Items.Add(Device.FriendlyName);
                    if (CAudio.Settings.AudioIn.DeviceName.Equals(Device.FriendlyName)) { AudioIn_DeviceName.SelectedIndex = thisidx; }
                }
                if (AudioIn_DeviceName.SelectedIndex == -1) { AudioIn_DeviceName.SelectedIndex = 0; }

                AudioIn_SampleRate.Items.Clear();
                foreach (var SampleRate in new int[] { 16000, 24000, 48000 }) {
                    var thisidx = AudioIn_SampleRate.Items.Add(SampleRate);
                    if (CAudio.Settings.AudioIn.SampleRate == SampleRate) { AudioIn_SampleRate.SelectedIndex = thisidx; }
                }
                if (AudioIn_SampleRate.SelectedIndex == -1) { AudioIn_SampleRate.SelectedIndex = 0; }

                AudioIn_ThresholdLevel.Items.Clear();
                for (var tv = 0.01; tv < 1; tv += 0.01) {
                    var thisidx = AudioIn_ThresholdLevel.Items.Add(tv.ToString("F2"));
                    if (System.Math.Abs(CAudio.Settings.AudioIn.ThresholdLevel - tv) < 0.001) { AudioIn_ThresholdLevel.SelectedIndex = thisidx; }
                }
                if (AudioIn_ThresholdLevel.SelectedIndex == -1) { AudioIn_ThresholdLevel.SelectedIndex = 0; }
            }

            { // AudioConvert
                AudioConvert_Addr.Text = CAudio.Settings.VCC.Addr;
                AudioConvert_Port.Text = CAudio.Settings.VCC.Port.ToString();

                AudioConvert_Protocol.Items.Clear();
                foreach (var Protocol in new string[] { "sio", "rest" }) {
                    var thisidx = AudioConvert_Protocol.Items.Add(Protocol);
                    if (CAudio.Settings.VCC.Protocol == Protocol) { AudioConvert_Protocol.SelectedIndex = thisidx; }
                }
                if (AudioConvert_Protocol.SelectedIndex == -1) { AudioConvert_Protocol.SelectedIndex = 0; }
            }

            { // AudioOut, VBAN
                AudioOut_SampleRate.Items.Clear();
                foreach (var SampleRate in new int[] { 48000 }) {
                    var thisidx = AudioOut_SampleRate.Items.Add(SampleRate);
                    if (CAudio.Settings.AudioOut.SampleRate == SampleRate) { AudioOut_SampleRate.SelectedIndex = thisidx; }
                }
                if (AudioOut_SampleRate.SelectedIndex == -1) { AudioOut_SampleRate.SelectedIndex = 0; }

                VBAN_Addr.Text = CAudio.Settings.VBAN.Addr;
                VBAN_Port.Text = CAudio.Settings.VBAN.Port.ToString();
                VBAN_StreamName.Text = CAudio.Settings.VBAN.StreamName;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (CAudio.AudioConvert != null) { CAudio.AudioConvert.ResetSampleRateTo48k(); }
        }

        private void VCClientBenchmarkBtn_Click(object sender, RoutedEventArgs e) {
            var window = new WBenchmarkDlg(this);
            window.ShowDialog();
            if (window.ReqExit) { this.Close(); }
        }

        public void SaveSettings() {
            { // AudioIn
                CAudio.Settings.AudioIn.ChunkSecs = double.Parse(AudioIn_ChunkSecs.Text);
                CAudio.Settings.AudioIn.DeviceName = AudioIn_DeviceName.Text;
                CAudio.Settings.AudioIn.SampleRate = int.Parse(AudioIn_SampleRate.Text);
                CAudio.Settings.AudioIn.ThresholdLevel = float.Parse(AudioIn_ThresholdLevel.Text);
            }

            { // AudioConvert
                CAudio.Settings.VCC.Addr = AudioConvert_Addr.Text;
                CAudio.Settings.VCC.Port = int.Parse(AudioConvert_Port.Text);
                CAudio.Settings.VCC.Protocol = AudioConvert_Protocol.Text;
            }

            { // AudioOut, VBAN
                CAudio.Settings.AudioOut.SampleRate = int.Parse(AudioOut_SampleRate.Text);
                CAudio.Settings.VBAN.Addr = VBAN_Addr.Text;
                CAudio.Settings.VBAN.Port = int.Parse(VBAN_Port.Text);
                CAudio.Settings.VBAN.StreamName = VBAN_StreamName.Text;
            }

            CAudio.SaveToFile(CAudio.SettingsFilename);
        }

        private void SaveAndExitBtn_Click(object sender, RoutedEventArgs e) {
            SaveSettings();
            this.Close();
        }

        private void WithoutSaveAndExitBtn_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
