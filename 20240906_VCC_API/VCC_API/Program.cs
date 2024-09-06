using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace VCC_API {
    internal class Program {
        static void Main(string[] args) {
            try {
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

                Console.WriteLine();
                Console.WriteLine("VCC_API.exe 2024/09/05");
                Console.WriteLine();

                Console.WriteLine("これは、ハードウェアインターフェースを使わずに、VCClient と Voice Meeter を使って、できるだけ低遅延でRVC変換するためのサポートアプリです。");
                Console.WriteLine();

                if (!System.IO.File.Exists(CAudio.SettingsFilename)) {
                    Console.WriteLine("設定ファイル [" + CAudio.SettingsFilename + "]が見つかりません。");
                    Console.WriteLine("VCC_API_Setup.exe で初期設定してください。");
                    return;
                }

                CAudio.Settings = new CAudioSettings();
                CAudio.LoadFromFile(CAudio.SettingsFilename);

                CAudio.Start(true);

                Console.WriteLine();
                Console.WriteLine("変換を開始しました。何かキーを押すと終了します。");
                Console.WriteLine("モデル関連の操作（使用中スロットの変更や編集など）は、当アプリを終了した状態で行ってください。");
                Console.WriteLine("最初の第一声の変換は非常に遅いので、何か音を出して5秒ほど待って安定した後に使ってください。");
                Console.WriteLine();

                while (true) {
                    if (Console.KeyAvailable) { Console.ReadKey(); break; }
                    System.Threading.Thread.Sleep(1000);
                    var RateIn = CAudio.RateStatus.AudioIn;
                    var RateConvert = CAudio.RateStatus.AudioConvert;
                    var RateOut = CAudio.RateStatus.AudioOut;
                    var RateTotal = RateIn + RateConvert + RateOut;
                    var PeakLevel = CAudio.RateStatus.PeakLevel;
                    var ChunkTS = CAudio.Settings.AudioIn.GetChunkTS();
                    Console.WriteLine(System.DateTime.Now.TimeOfDay.ToString() + "\tRate: Total=" + RateTotal.TotalMilliseconds.ToString("F0") + "ms, In=" + RateIn.TotalMilliseconds.ToString("F0") + "ms,  Convert=" + RateConvert.TotalMilliseconds.ToString("F0") + "ms,  Out=" + RateOut.TotalMilliseconds.ToString("F0") + "ms. (Chunk:" + ChunkTS.TotalMilliseconds.ToString("F0") + "ms) (Peak:" + PeakLevel.ToString("F3") + ")");
                    Console.Title = "VCC_API rate " + RateTotal.TotalMilliseconds.ToString("F0") + "ms";
                }

                Console.WriteLine("Terminate.");

                CAudio.Stop();

                CAudio.AudioConvert.ResetSampleRateTo48k();

            } catch (Exception ex) { Console.WriteLine("Main exception: " + ex.ToString()); Environment.Exit(1); }
        }
    }
}
