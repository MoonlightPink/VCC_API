using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCC_API {
    internal class CAudio {
        public static CAudioSettings Settings;

        public const string SettingsFilename = "VCC_API.json";

        public static void LoadFromFile(string fn) {
            using (var rfs = new System.IO. StreamReader(fn)) {
                var json = rfs.ReadToEnd();
                Console.WriteLine("Load settings from file. [" + fn + "]");
                Console.WriteLine(json);
                Settings = System.Text.Json.JsonSerializer.Deserialize<CAudioSettings>(json);
            }
        }

        public static void SaveToFile(string fn) {
            using (var wfs = new System.IO.StreamWriter(fn)) {
                var json = System.Text.Json.JsonSerializer.Serialize(Settings, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });
                Console.WriteLine("Save settings to file. [" + fn + "]");
                Console.WriteLine(json);
                wfs.WriteLine(json);
            }
        }

        public static CAudioIn AudioIn;
        public static CAudioConvert AudioConvert;
        public static CAudioOut AudioOut;

        public static void Start(bool AutoStart) {
            AudioIn = new CAudioIn();
            AudioConvert = new CAudioConvert();
            AudioOut = new CAudioOut();

            if (AutoStart) {
                AudioIn.Start();
            }
        }

        public static void Stop() {
            // 終了処理は雑
            AudioIn.Stop();
        }

        public class CAudioBlock {
            public bool isSpeakStart;
            public float[] Samples;
            public CAudioBlock(bool _isSpeakStart, float[] _Samples) {
                isSpeakStart = _isSpeakStart;
                Samples = _Samples;
            }
        }

        public class CResetBuffer {
            private bool AudioIn, AudioOut;
            public void Set() {
                lock (this) {
                    Console.WriteLine(System.DateTime.Now.TimeOfDay.ToString() + "\tCResetBuffer: Reset buffer.");
                    AudioIn = true;
                    AudioOut = true;
                }
            }
            public bool GetAudioIn() {
                lock (this) {
                    var res = AudioIn;
                    AudioIn = false;
                    return res;
                }
            }
            public bool GetAudioOut() {
                lock (this) {
                    var res = AudioOut;
                    AudioOut = false;
                    return res;
                }
            }
        }
        public static CResetBuffer ResetBuffer = new CResetBuffer();

        public class CRateStatus {
            private float _PeakLevel;
            public float PeakLevel {
                get { lock (this) { var res = _PeakLevel; _PeakLevel = 0; return res; } }
                set { lock (this) { if (_PeakLevel < value) { _PeakLevel = value; } } }
            }

            private TimeSpan _AudioIn, _AudioConvert, _AudioOut;
            public TimeSpan AudioIn {
                get { lock (this) { var res = _AudioIn; _AudioIn = TimeSpan.Zero; return res; } }
                set { lock (this) { if (_AudioIn < value) { _AudioIn = value; } } }
            }
            public TimeSpan AudioConvert {
                get { lock (this) { var res = _AudioConvert; _AudioConvert = TimeSpan.Zero; return res; } }
                set { lock (this) { if (_AudioConvert < value) { _AudioConvert = value; } } }
            }
            public TimeSpan AudioOut {
                get { lock (this) { var res = _AudioOut; _AudioOut = TimeSpan.Zero; return res; } }
                set { lock (this) { if (_AudioOut < value) { _AudioOut = value; } } }
            }
        }
        public static CRateStatus RateStatus = new CRateStatus();
    }
}
