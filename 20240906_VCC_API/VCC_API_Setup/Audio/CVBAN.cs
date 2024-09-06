using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCC_API {
    internal class CVBAN {
        private System.Net.Sockets.UdpClient udp;

        private const int MaxSamplesCount = 256;

        private const int Channels = 1;
        private const int Bits = 32;

        private byte[] buf = new byte[28 + (MaxSamplesCount * Channels * (Bits / 8))];

        public CVBAN(string Addr, int Port, int SampleRate, string StreamName) {
            udp = new System.Net.Sockets.UdpClient(Addr, Port);

            buf[0] = (byte)'V'; buf[1] = (byte)'B'; buf[2] = (byte)'A'; buf[3] = (byte)'N';

            switch (SampleRate) {
                case 6000: buf[4] = 0; break;
                case 48000: buf[4] = 3; break;
                default: throw new Exception();
            }

            buf[5] = (byte)0;

            if ((Channels <= 0) || (256 < Channels)) { throw new Exception(); }
            buf[6] = (byte)(Channels - 1);

            switch (Bits) {
                case 16: buf[7] = 1; break; // 16bits short
                case 32: buf[7] = 4; break; // 32bits float
                default: throw new Exception();
            }

            const byte VBAN_CODEC_PCM = 0x00;
            buf[7] |= VBAN_CODEC_PCM;

            for (var idx = 0; idx < 16; idx++) {
                if (idx < StreamName.Length) {
                    buf[8 + idx] = (byte)StreamName[idx];
                } else {
                    buf[8 + idx] = (byte)0;
                }
            }
        }

        private int FrameCounter = 0;

        public void SendPacket(float[] Samples, int SamplesIndex, int SamplesCount) {
            if (SamplesCount == 0) { return; }

            if ((SamplesCount <= 0) || (256 < SamplesCount)) { throw new Exception(); }
            buf[5] = (byte)(SamplesCount - 1);

            Array.Copy(BitConverter.GetBytes(FrameCounter), 0, buf, 24, 4);
            FrameCounter++;

            var SampleSize = Channels * (Bits / 8);
            var buflen = 28 + (SamplesCount * SampleSize);
            for (var idx = 0; idx < SamplesCount; idx++) {
                Array.Copy(BitConverter.GetBytes(Samples[SamplesIndex + idx]), 0, buf, 28 + (idx * SampleSize), 4);
            }

            udp.Send(buf, buflen);
        }

        public void SendPacket(float[] Samples) {
            var idx = 0;
            var last = Samples.Length;
            while (1 <= last) {
                var size = last;
                if (256 < size) { size = 256; }
                SendPacket(Samples, idx, size);
                idx += size;
                last -= size;
            }
        }
    }
}
