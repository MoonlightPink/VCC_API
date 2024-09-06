using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCC_API {
    internal class CVCC_RestAPI_JSON {
        private static System.Text.Json.JsonSerializerOptions DeserializeOptions = new System.Text.Json.JsonSerializerOptions() { WriteIndented = true };

        public class JSON_hello {
            // {"message":"Hello World! VCClient gives a cute voice to you!","credit":"w-okada","repository":"https://github.com/w-okada/voice-changer"}
            public string message { get; set; }
            public string credit { get; set; }
            public string repository { get; set; }
        }
        public static string JSON_hello_Serialize(JSON_hello json) { return System.Text.Json.JsonSerializer.Serialize(json); }
        public static JSON_hello JSON_hello_Deserialize(string json) { return System.Text.Json.JsonSerializer.Deserialize<JSON_hello>(json, DeserializeOptions); }

        public class JSON_uploader_info {
            // {"uploadable_files":[{"title":"model file","filename":"model_file"},{"title":"index file","filename":"index_file"}]}
            public class Uploadable_Files {
                public string title { get; set; }
                public string filename { get; set; }
            }
            public Uploadable_Files[] uploadable_files { get; set; }
        }
        public static string JSON_uploader_info_Serialize(JSON_uploader_info json) { return System.Text.Json.JsonSerializer.Serialize(json); }
        public static JSON_uploader_info JSON_uploader_info_Deserialize(string json) { return System.Text.Json.JsonSerializer.Deserialize<JSON_uploader_info>(json, DeserializeOptions); }

        public class JSON_configuration_manager_configuration {
            // {"current_slot_index":0,"voice_changer_input_mode":"client","sio_broadcast":false,"pass_through":false,"recording_started":false,"enable_performance_monitor":true,"enable_high_pass_filter":false,"high_pass_filter_cutoff":100.0,"enable_low_pass_filter":false,"low_pass_filter_cutoff":10000.0,"volume_tuning_type":"sqrt","audio_input_device_index":-1,"audio_output_device_index":-1,"audio_monitor_device_index":-1,"wasapi_exclude_emabled":true,"audio_input_device_sample_rate":-1,"audio_output_device_sample_rate":-1,"audio_monitor_device_sample_rate":-1,"audio_input_device_gain":1.0,"audio_output_device_gain":1.0,"audio_monitor_device_gain":1.0,"noise_gate":-100.0,"extra_frame_sec":10.0,"crossfade_sec":0.05,"sola_search_frame_sec":0.012,"gpu_device_id_int":0,"input_sample_rate":16000,"output_sample_rate":48000,"monitor_sample_rate":48000}
            public int current_slot_index { get; set; }
            public string voice_changer_input_mode { get; set; }
            public bool sio_broadcast { get; set; }
            public bool pass_through { get; set; }
            public bool recording_started { get; set; }
            public bool enable_performance_monitor { get; set; }
            public bool enable_high_pass_filter { get; set; }
            public float high_pass_filter_cutoff { get; set; }
            public bool enable_low_pass_filter { get; set; }
            public float low_pass_filter_cutoff { get; set; }
            public string volume_tuning_type { get; set; }
            public int audio_input_device_index { get; set; }
            public int audio_output_device_index { get; set; }
            public int audio_monitor_device_index { get; set; }
            public bool wasapi_exclude_emabled { get; set; }
            public int audio_input_device_sample_rate { get; set; }
            public int audio_output_device_sample_rate { get; set; }
            public int audio_monitor_device_sample_rate { get; set; }
            public float audio_input_device_gain { get; set; }
            public float audio_output_device_gain { get; set; }
            public float audio_monitor_device_gain { get; set; }
            public float noise_gate { get; set; }
            public float extra_frame_sec { get; set; }
            public float crossfade_sec { get; set; }
            public float sola_search_frame_sec { get; set; }
            public int gpu_device_id_int { get; set; }
            public int input_sample_rate { get; set; }
            public int output_sample_rate { get; set; }
            public int monitor_sample_rate { get; set; }
        }
        public static string JSON_configuration_manager_configuration_Serialize(JSON_configuration_manager_configuration json) { return System.Text.Json.JsonSerializer.Serialize(json); }
        public static JSON_configuration_manager_configuration JSON_configuration_manager_configuration_Deserialize(string json) { return System.Text.Json.JsonSerializer.Deserialize<JSON_configuration_manager_configuration>(json, DeserializeOptions); }

        public class JSON_gpu_device_manager_devices {
            // [{"name":"cpu","device_id":"-1","adapter_ram":0,"device_id_int":-1,"cuda_compute_version_major":-1,"cuda_compute_version_minor":-1},{"name":"NVIDIA GeForce RTX 4070 SUPER","device_id":"0","adapter_ram":12878086144,"device_id_int":0,"cuda_compute_version_major":8,"cuda_compute_version_minor":9}]
            public string name { get; set; }
            public string device_id { get; set; }
            public long adapter_ram { get; set; }
            public int device_id_int { get; set; }
            public int cuda_compute_version_major { get; set; }
            public int cuda_compute_version_minor { get; set; }
        }
        public static string JSON_gpu_device_manager_devices_Serialize(List<JSON_gpu_device_manager_devices> json) { return System.Text.Json.JsonSerializer.Serialize(json); }
        public static List<JSON_gpu_device_manager_devices> JSON_gpu_device_manager_devices_Deserialize(string json) { return System.Text.Json.JsonSerializer.Deserialize<List<JSON_gpu_device_manager_devices>>(json, DeserializeOptions); }

        public class JSON_voice_changer_manager_information {
            // {"local_voice_changer_interface_active":false,"voice_changer_information":{"slot_index":0,"pitch_estimator_type":"rmvpe","gpu_device_index":0,"input_sample_rate":16000,"output_sample_rate":48000,"monitor_sample_rate":48000,"vc_input_sample_rate":16000,"vc_output_sample_rate":48000,"resample_ratio_in":1.0,"resample_ratio_out":1.0,"resample_ratio_monitor":1.0,"resample_ratio_pass_through_in_out":3.0,"resample_ratio_pass_through_in_monitor":3.0,"enable_high_pass_filter":false,"high_pass_filter_cutoff":100.0,"enable_low_pass_filter":false,"low_pass_filter_cutoff":10000.0,"chunk_sec":0.05,"pipeline_info":{"slot_index":0,"input_sample_rate":16000,"output_sample_rate":48000,"chunk_sec":0.05,"slot_info":{"slot_index":0,"voice_changer_type":"RVC","name":"凛音エル 3_e700","description":"RinneElu3_e700_s63000","credit":"","terms_of_use_url":"https://dennotenshi.booth.pm/items/5475738","icon_file":"rinneeru_600_800.png","speakers":{},"model_file":"RinneElu3_e700_s63000.pth","index_file":"added_IVF3435_Flat_nprobe_1_RinneEluIndex_v2.index","is_onnx":false,"inferencer_type":"pyTorchRVCv2","sample_rate":48000,"is_f0":true,"deprecated":false,"embedder":"hubert_base_l12","pitch_estimator":"rmvpe","sample_id":null,"version":"v2","chunk_sec":0.05,"pitch_shift":12,"index_ratio":0.0,"protect_ratio":0.5},"embedder_info":{"embedder_type":"contentvec","model_file":"modules\\contentvec\\contentvec-f.onnx","device_id":0,"candidate_onnx_providers":["CUDAExecutionProvider"],"candidate_onnx_provider_options":"[{'device_id': 0}]","onnx_providers":["CUDAExecutionProvider","CPUExecutionProvider"],"onnx_provider_options":"{'CUDAExecutionProvider': {'cudnn_conv_algo_search': 'EXHAUSTIVE', 'device_id': '0', 'has_user_compute_stream': '0', 'cudnn_conv1d_pad_to_nc1d': '0', 'gpu_external_alloc': '0', 'gpu_mem_limit': '18446744073709551615', 'enable_cuda_graph': '0', 'gpu_external_free': '0', 'gpu_external_empty_cache': '0', 'arena_extend_strategy': 'kNextPowerOfTwo', 'do_copy_in_default_stream': '1', 'cudnn_conv_use_max_workspace': '1', 'tunable_op_enable': '0', 'tunable_op_tuning_enable': '0', 'tunable_op_max_tuning_duration_ms': '0', 'enable_skip_layer_norm_strict_mode': '0', 'prefer_nhwc': '0', 'use_ep_level_unified_stream': '0'}, 'CPUExecutionProvider': {}}"},"pitch_estimator_info":{"pitch_estimator_type":"rmvpe","model_file":"modules\\rmvpe\\rmvpe_20231006.pt","device_id":0,"candidate_onnx_providers":null,"candidate_onnx_provider_options":null,"onnx_providers":null,"onnx_provider_options":null},"inferencer_info":{"inferencer_type":"pyTorchRVCv2","model_file":"model_dir\\0\\RinneElu3_e700_s63000.pth","device_id":0,"candidate_onnx_providers":null,"candidate_onnx_provider_options":null,"onnx_providers":null,"onnx_provider_options":null}},"voice_changer_type":"RVC","bulk_process_start_flag":true,"recording_start_flag":false,"monitor_enabled":false}}
            public bool local_voice_changer_interface_active { get; set; }

            public class Voice_Changer_Information {
                public int slot_index { get; set; }
                public string pitch_estimator_type { get; set; }
                public int gpu_device_index { get; set; }
                public int input_sample_rate { get; set; }
                public int output_sample_rate { get; set; }
                public int monitor_sample_rate { get; set; }
                public int vc_input_sample_rate { get; set; }
                public int vc_output_sample_rate { get; set; }
                public float resample_ratio_in { get; set; }
                public float resample_ratio_out { get; set; }
                public float resample_ratio_monitor { get; set; }
                public float resample_ratio_pass_through_in_out { get; set; }
                public float resample_ratio_pass_through_in_monitor { get; set; }
                public bool enable_high_pass_filter { get; set; }
                public float high_pass_filter_cutoff { get; set; }
                public bool enable_low_pass_filter { get; set; }
                public float low_pass_filter_cutoff { get; set; }
                public float chunk_sec { get; set; }

                public class Pipeline_Info {
                    public int slot_index { get; set; }
                    public int input_sample_rate { get; set; }
                    public int output_sample_rate { get; set; }
                    public float chunk_sec { get; set; }

                    public class Slot_Info {
                        public int slot_index { get; set; }
                        public string voice_changer_type { get; set; }
                        public string name { get; set; }
                        public string description { get; set; }
                        public string credit { get; set; }
                        public string terms_of_use_url { get; set; }
                        public string icon_file { get; set; }

                        public class Speakers {
                        }
                        public Speakers speakers { get; set; }

                        public string model_file { get; set; }
                        public string index_file { get; set; }
                        public bool is_onnx { get; set; }
                        public string inferencer_type { get; set; }
                        public int sample_rate { get; set; }
                        public bool is_f0 { get; set; }
                        public bool deprecated { get; set; }
                        public string embedder { get; set; }
                        public string pitch_estimator { get; set; }
                        public object sample_id { get; set; }
                        public string version { get; set; }
                        public float chunk_sec { get; set; }
                        public int pitch_shift { get; set; }
                        public float index_ratio { get; set; }
                        public float protect_ratio { get; set; }
                    }
                    public Slot_Info slot_info { get; set; }

                    public class Embedder_Info {
                        public string embedder_type { get; set; }
                        public string model_file { get; set; }
                        public int device_id { get; set; }
                        public string[] candidate_onnx_providers { get; set; }
                        public string candidate_onnx_provider_options { get; set; }
                        public string[] onnx_providers { get; set; }
                        public string onnx_provider_options { get; set; }
                    }
                    public Embedder_Info embedder_info { get; set; }

                    public class Pitch_Estimator_Info {
                        public string pitch_estimator_type { get; set; }
                        public string model_file { get; set; }
                        public int device_id { get; set; }
                        public object candidate_onnx_providers { get; set; }
                        public object candidate_onnx_provider_options { get; set; }
                        public object onnx_providers { get; set; }
                        public object onnx_provider_options { get; set; }
                    }
                    public Pitch_Estimator_Info pitch_estimator_info { get; set; }

                    public class Inferencer_Info {
                        public string inferencer_type { get; set; }
                        public string model_file { get; set; }
                        public int device_id { get; set; }
                        public object candidate_onnx_providers { get; set; }
                        public object candidate_onnx_provider_options { get; set; }
                        public object onnx_providers { get; set; }
                        public object onnx_provider_options { get; set; }
                    }
                    public Inferencer_Info inferencer_info { get; set; }
                }
                public Pipeline_Info pipeline_info { get; set; }

                public string voice_changer_type { get; set; }
                public bool bulk_process_start_flag { get; set; }
                public bool recording_start_flag { get; set; }
                public bool monitor_enabled { get; set; }
            }
            public Voice_Changer_Information voice_changer_information { get; set; }
        }
        public static string JSON_voice_changer_manager_information_Serialize(JSON_voice_changer_manager_information json) { return System.Text.Json.JsonSerializer.Serialize(json); }
        public static JSON_voice_changer_manager_information JSON_voice_changer_manager_information_Deserialize(string json) { return System.Text.Json.JsonSerializer.Deserialize<JSON_voice_changer_manager_information>(json, DeserializeOptions); }

    }
}
