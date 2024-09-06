using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace VCC_API {
    internal class CAudioSettings {
        public class CAudioIn {
            public double ChunkSecs { get; set; } = 0.15;
            public TimeSpan GetChunkTS() { return TimeSpan.FromSeconds(ChunkSecs); }
            public string DeviceName { get; set; } = "Microphone (High Definition Audio Device)";
            public int SampleRate { get; set; } = 16000;
            public float ThresholdLevel { get; set; } = 0.1f;
        }
        public CAudioIn AudioIn { get; set; } = new CAudioIn();

        public class CVCC {
            public string Addr { get; set; } = "127.0.0.1";
            public int Port { get; set; } = 18000;
            public string Protocol { get; set; } = "sio";
            public enum EProtocol { sio, rest };
            public EProtocol GetProtocol() {
                switch (Protocol) {
                    case "sio": return EProtocol.sio;
                    case "rest": return EProtocol.rest;
                    default: throw new NotImplementedException();
                }
            }
        }
        public CVCC VCC { get; set; } = new CVCC();

        public class CAudioOut {
            public int SampleRate { get; set; } = 48000;
        }
        public CAudioOut AudioOut { get; set; } = new CAudioOut();

        public class CVBAN {
            public string Addr { get; set; } = "127.0.0.1";
            public int Port { get; set; } = 6980;
            public string StreamName { get; set; } = "VCC stream";
        }
        public CVBAN VBAN { get; set; } = new CVBAN();

    }
}
