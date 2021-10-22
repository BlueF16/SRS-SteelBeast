using System.IO;

namespace NAudio.FileFormats.SoundFont
{
    internal class ModulatorBuilder : StructureBuilder<Modulator>
    {
        public override int Length => 10;

        public Modulator[] Modulators => data.ToArray();

        public override Modulator Read(BinaryReader br)
        {
            var m = new Modulator();
            m.SourceModulationData = new ModulatorType(br.ReadUInt16());
            m.DestinationGenerator = (GeneratorEnum)br.ReadUInt16();
            m.Amount = br.ReadInt16();
            m.SourceModulationAmount = new ModulatorType(br.ReadUInt16());
            m.SourceTransform = (TransformEnum)br.ReadUInt16();
            data.Add(m);
            return m;
        }

        public override void Write(BinaryWriter bw, Modulator o)
        {
            //Zone z = (Zone) o;
            //bw.Write(p.---);
        }
    }
}