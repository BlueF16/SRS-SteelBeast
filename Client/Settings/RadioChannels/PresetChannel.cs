﻿namespace Ciribob.FS3D.SimpleRadio.Standalone.Client.Settings.RadioChannels
{
    public class PresetChannel
    {
        public string Text { get; set; }
        public object Value { get; set; }
        public int Channel { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}