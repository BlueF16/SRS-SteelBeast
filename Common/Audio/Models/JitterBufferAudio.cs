﻿using Ciribob.SRS.Common.Network.Models;

namespace Ciribob.FS3D.SimpleRadio.Standalone.Common.Audio.Models
{
    public class JitterBufferAudio
    {
        public float[] Audio { get; set; }

        public ulong PacketNumber { get; set; }

        public int ReceivedRadio { get; set; }

        public Modulation Modulation { get; internal set; }

        public bool Decryptable { get; internal set; }

        public float Volume { get; internal set; }
        public bool IsSecondary { get; set; }

        public double Frequency { get; set; }
        public bool NoAudioEffects { get; set; }

        public string Guid { get; set; }
        public string OriginalClientGuid { get; set; }
        public short Encryption { get; set; }
    }
}