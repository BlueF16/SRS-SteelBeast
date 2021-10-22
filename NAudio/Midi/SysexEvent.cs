using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NAudio.Midi
{
    /// <summary>
    ///     Represents a MIDI sysex message
    /// </summary>
    public class SysexEvent : MidiEvent
    {
        private byte[] data;
        //private int length;

        /// <summary>
        ///     Reads a sysex message from a MIDI stream
        /// </summary>
        /// <param name="br">Stream of MIDI data</param>
        /// <returns>a new sysex message</returns>
        public static SysexEvent ReadSysexEvent(BinaryReader br)
        {
            var se = new SysexEvent();
            //se.length = ReadVarInt(br);
            //se.data = br.ReadBytes(se.length);

            var sysexData = new List<byte>();
            var loop = true;
            while (loop)
            {
                var b = br.ReadByte();
                if (b == 0xF7)
                    loop = false;
                else
                    sysexData.Add(b);
            }

            se.data = sysexData.ToArray();

            return se;
        }

        /// <summary>
        ///     Creates a deep clone of this MIDI event.
        /// </summary>
        public override MidiEvent Clone()
        {
            return new SysexEvent { data = (byte[])data?.Clone() };
        }

        /// <summary>
        ///     Describes this sysex message
        /// </summary>
        /// <returns>A string describing the sysex message</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var b in data) sb.AppendFormat("{0:X2} ", b);

            return string.Format("{0} Sysex: {1} bytes\r\n{2}", AbsoluteTime, data.Length, sb);
        }

        /// <summary>
        ///     Calls base class export first, then exports the data
        ///     specific to this event
        ///     <seealso cref="MidiEvent.Export">MidiEvent.Export</seealso>
        /// </summary>
        public override void Export(ref long absoluteTime, BinaryWriter writer)
        {
            base.Export(ref absoluteTime, writer);
            //WriteVarInt(writer,length);
            //writer.Write(data, 0, data.Length);
            writer.Write(data, 0, data.Length);
            writer.Write((byte)0xF7);
        }
    }
}