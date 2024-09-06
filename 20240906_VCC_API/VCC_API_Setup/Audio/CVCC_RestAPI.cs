using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using static VCC_API.CVCC_RestAPI_JSON.JSON_voice_changer_manager_information;

namespace VCC_API {
    internal class CVCC_RestAPI {
        private static HttpClient http = new HttpClient();

        private string BaseURL;

        public string HttpGet_ThrowException(string url) {
            var res = HttpGet(url);
            if (res.Equals("")) { throw new Exception("VCClientのREST API (" + url + ") に対しての返答がありませんでした。"); }
            return res;
        }

        public CVCC_RestAPI_JSON.JSON_configuration_manager_configuration SetSampleRate(int InputSampleRate, int OutputSampleRate) {
            var Item = CVCC_RestAPI_JSON.JSON_configuration_manager_configuration_Deserialize(HttpGet_ThrowException(@"/configuration-manager/configuration"));

            Item.input_sample_rate = InputSampleRate;
            Item.output_sample_rate = OutputSampleRate;
            HttpPostJSON(@"/configuration-manager/configuration", CVCC_RestAPI_JSON.JSON_configuration_manager_configuration_Serialize(Item));

            Item = CVCC_RestAPI_JSON.JSON_configuration_manager_configuration_Deserialize(HttpGet_ThrowException(@"/configuration-manager/configuration"));

            return Item;
        }

        public void ResetSampleRateTo48k() { SetSampleRate(48000, 48000); }

        public CVCC_RestAPI(string Host = @"127.0.0.1", int Port = 18000, int InputSampleRate = 16000, int OutputSampleRate = 48000) {
            BaseURL = "http://" + Host + ":" + Port + "/api";

            {
                var Item = CVCC_RestAPI_JSON.JSON_hello_Deserialize(HttpGet_ThrowException(@"/hello"));
                Console.WriteLine("VCClient hello response from " + Item.credit + ". [" + Item.message + "]");
            }

            {
                var Item = CVCC_RestAPI_JSON.JSON_uploader_info_Deserialize(HttpGet_ThrowException(@"/uploader/info"));
            }

            int gpu_device_id;

            {
                Console.WriteLine("Setup VCClient sample rate.");
                var Item = SetSampleRate(InputSampleRate, OutputSampleRate);

                Console.WriteLine("VCClient configuration.");
                Console.WriteLine("\tcurrent_slot_index:\t" + Item.current_slot_index);
                Console.WriteLine("\tnoise_gate:\t" + Item.noise_gate);
                Console.WriteLine("\textra_frame_sec:\t" + Item.extra_frame_sec);
                Console.WriteLine("\tcrossfade_sec:\t" + Item.crossfade_sec);
                Console.WriteLine("\tsola_search_frame_sec:\t" + Item.sola_search_frame_sec);
                Console.WriteLine("\tgpu_device_id_int:\t" + Item.gpu_device_id_int);
                Console.WriteLine("\tinput_sample_rate:\t" + Item.input_sample_rate);
                Console.WriteLine("\toutput_sample_rate:\t" + Item.output_sample_rate);

                if (Item.input_sample_rate != InputSampleRate) { throw new Exception("VCClient: 入力サンプルレートの設定に失敗しました。"); }
                if (Item.output_sample_rate != OutputSampleRate) { throw new Exception("VCClient: 出力サンプルレートの設定に失敗しました。"); }

                gpu_device_id = Item.gpu_device_id_int;
            }

            {
                var Items = CVCC_RestAPI_JSON.JSON_gpu_device_manager_devices_Deserialize(HttpGet_ThrowException(@"/gpu-device-manager/devices"));
                Console.WriteLine("VCClient gpu device manager.");
                for (var idx = 0; idx < Items.Count; idx++) {
                    var Item = Items[idx];
                    var RamGB = (double)Item.adapter_ram / 1024 / 1024 / 1024;
                    Console.WriteLine("\tdevice_id=" + Item.device_id_int + " " + (Item.device_id_int == gpu_device_id ? "Selected. " : "") + Item.name + "\t" + RamGB.ToString("F3") + "GB CUDA" + Item.cuda_compute_version_major + "." + Item.cuda_compute_version_minor);
                }
            }

            {
                var Item = CVCC_RestAPI_JSON.JSON_voice_changer_manager_information_Deserialize(HttpGet_ThrowException(@"/voice-changer-manager/information"));
                Console.WriteLine("VCClient voice changer information.");
                Console.WriteLine("\tslot_index:\t" + Item.voice_changer_information.slot_index);
                Console.WriteLine("\tpitch_estimator_type:\t" + Item.voice_changer_information.pitch_estimator_type);

                if ((Item.voice_changer_information.input_sample_rate != InputSampleRate) || (Item.voice_changer_information.vc_input_sample_rate != InputSampleRate) || (Item.voice_changer_information.pipeline_info.input_sample_rate != InputSampleRate)) { Console.WriteLine("警告: 入力サンプルレートが一致していません。音程がずれたりノイズが入る可能性があります。"); }
                if ((Item.voice_changer_information.output_sample_rate != OutputSampleRate) || (Item.voice_changer_information.vc_output_sample_rate != OutputSampleRate) || (Item.voice_changer_information.pipeline_info.output_sample_rate != OutputSampleRate)) { Console.WriteLine("警告: 出力サンプルレートが一致していません。音程がずれたりノイズが入る可能性があります。"); }
                if (Item.voice_changer_information.resample_ratio_in != 1) { Console.WriteLine("警告: 入力周波数比率が1ではありません。音程がずれたりノイズが入る可能性があります。"); }
                if (Item.voice_changer_information.resample_ratio_out != 1) { Console.WriteLine("警告: 出力周波数比率が1ではありません。音程がずれたりノイズが入る可能性があります。"); }

                Console.WriteLine("VCClient current slot information.");
                var SlotInfo = Item.voice_changer_information.pipeline_info.slot_info;
                Console.WriteLine("\tslot_index:\t" + SlotInfo.slot_index);
                Console.WriteLine("\tvoice_changer_type:\t" + SlotInfo.voice_changer_type);
                Console.WriteLine("\tname:\t" + SlotInfo.name);
                Console.WriteLine("\tdescription:\t" + SlotInfo.description);
                Console.WriteLine("\tcredit:\t" + SlotInfo.credit);
                Console.WriteLine("\tterms_of_use_url:\t" + SlotInfo.terms_of_use_url);
                Console.WriteLine("\tis_onnx:\t" + SlotInfo.is_onnx);
                Console.WriteLine("\tinferencer_type:\t" + SlotInfo.inferencer_type);
                Console.WriteLine("\tsample_rate:\t" + SlotInfo.sample_rate);
                Console.WriteLine("\tis_f0:\t" + SlotInfo.is_f0);
                Console.WriteLine("\tdeprecated:\t" + SlotInfo.deprecated);
                Console.WriteLine("\tembedder:\t" + SlotInfo.embedder);
                Console.WriteLine("\tpitch_estimator:\t" + SlotInfo.pitch_estimator);
                Console.WriteLine("\tversion:\t" + SlotInfo.version);
                Console.WriteLine("\tchunk_sec:\t" + SlotInfo.chunk_sec);
                Console.WriteLine("\tpitch_shift:\t" + SlotInfo.pitch_shift);
                Console.WriteLine("\tindex_ratio:\t" + SlotInfo.index_ratio);
                Console.WriteLine("\tprotect_ratio:\t" + SlotInfo.protect_ratio);
            }
        }

        public string HttpGet(string Request, bool Debug = false) {
            if (Debug) { Console.WriteLine("Request: " + Request); }

            var res = http.GetAsync(BaseURL + Request).Result;
            if (!res.IsSuccessStatusCode) {
                Console.WriteLine("error: " + res.ToString());
                Console.WriteLine(res.Content.ReadAsStringAsync().Result);
                return "";
            }

            var body = res.Content.ReadAsStringAsync().Result;

            if (Debug) { Console.WriteLine(body); }

            return body.Equals("") ? "nores" : body;
        }

        public string HttpPostJSON(string Request, string Content, bool Debug = false) {
            if (Debug) { Console.WriteLine("Request: " + Request); }

            var content = new StringContent(Content, Encoding.UTF8, "application/json");

            var res = http.PutAsync(BaseURL + Request, content).Result;
            if (!res.IsSuccessStatusCode) {
                Console.WriteLine("error: " + res.ToString());
                Console.WriteLine(res.Content.ReadAsStringAsync().Result);
                return "";
            }

            var body = res.Content.ReadAsStringAsync().Result;

            if (Debug) { Console.WriteLine(body); }

            return body.Equals("") ? "nores" : body;
        }

        private float[] HttpPostWaveForm(string Request, float[] buf32, bool Debug = false) {
            // if (Debug) { Console.WriteLine("Request: " + Request); }

            var buf8 = new byte[buf32.Length * sizeof(float)];
            Buffer.BlockCopy(buf32, 0, buf8, 0, buf8.Length); // _src全体を_dstにcopy

            var MultiContent = new MultipartFormDataContent();

            MultiContent.Add(new ByteArrayContent(buf8), "waveform", "waveform.bin");

            var res = http.PostAsync(BaseURL + Request, MultiContent).Result;
            if (!res.IsSuccessStatusCode) {
                Console.WriteLine("HttpPostWaveForm: buf32.Length=" + buf32.Length);
                Console.WriteLine("error: " + res.ToString());
                Console.WriteLine(res.Content.ReadAsStringAsync().Result);
                return null;
            }

            var stream = res.Content.ReadAsStream();

            if (stream.Length < 4) { return new float[0]; }

            using (var br = new System.IO.BinaryReader(stream, Encoding.UTF8, true)) {
                stream.Position = 0;
                if ((br.ReadByte() == 0x00) && (br.ReadByte() == 0x00) && (br.ReadByte() == 0x00) && (br.ReadByte() == 0x00)) { return new float[0]; }
            }

            var body = new float[stream.Length / 4];

            using (var br = new System.IO.BinaryReader(stream, Encoding.UTF8, true)) {
                stream.Position = 0;
                for (var idx = 0; idx < body.Length; idx++) {
                    body[idx] = br.ReadSingle();
                }
            }

            return body;
        }

        public float[] Convert(float[] srcsmps) {
            return HttpPostWaveForm(@"/voice-changer/convert_chunk", srcsmps, true);
        }

        public float[] Convert_Bulk(float[] srcsmps) {
            return HttpPostWaveForm(@"/voice-changer/convert_chunk_bulk", srcsmps, true);
        }
    }
}
