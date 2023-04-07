namespace OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo8.Uncoupled.Chapter1;

public class Page3_0 : IStaticCodeBook
{
    public int Dimensions { get; } = 4;

    public byte[] LengthList { get; } = {
         1, 5, 5, 7, 7, 6, 7, 7, 9, 9, 6, 7, 7, 9, 9, 8,
        10, 9,11,11, 9, 9, 9,11,11, 6, 8, 8,10,10, 8,10,
        10,11,11, 8, 9,10,11,11,10,11,11,12,12,10,11,11,
        12,13, 6, 8, 8,10,10, 8,10, 9,11,11, 8,10, 9,11,
        11,10,11,11,12,12,10,11,11,12,12, 9,11,11,14,13,
        10,12,11,14,14,10,12,11,14,13,12,13,13,15,14,12,
        13,13,15,14, 8,11,11,13,14,10,11,12,13,15,10,11,
        12,14,14,12,13,13,14,15,12,13,13,14,15, 5, 8, 8,
        11,11, 8,10,10,12,12, 8,10,10,12,12,11,12,12,14,
        13,11,12,12,13,14, 8,10,10,12,12, 9,11,12,13,14,
        10,12,12,13,13,12,12,13,14,14,11,13,13,15,15, 7,
        10,10,12,12, 9,12,11,14,12,10,11,12,13,14,12,13,
        12,14,14,12,13,13,15,16,10,12,12,15,14,11,12,13,
        15,15,11,13,13,15,16,14,14,15,15,16,13,14,15,17,
        15, 9,12,12,14,15,11,13,12,15,15,11,13,13,15,15,
        13,14,13,15,14,13,14,14,17, 0, 5, 8, 8,11,11, 8,
        10,10,12,12, 8,10,10,12,12,11,12,12,14,14,11,12,
        12,14,14, 7,10,10,12,12,10,12,12,13,13, 9,11,12,
        12,13,11,12,13,15,15,11,12,13,14,15, 8,10,10,12,
        12,10,12,11,13,13,10,12,11,13,13,11,13,13,15,14,
        12,13,12,15,13, 9,12,12,14,14,11,13,13,16,15,11,
        12,13,16,15,13,14,15,16,16,13,13,15,15,16,10,12,
        12,15,14,11,13,13,14,16,11,13,13,15,16,13,15,15,
        16,17,13,15,14,16,15, 8,11,11,14,15,10,12,12,15,
        15,10,12,12,15,16,14,15,15,16,17,13,14,14,16,16,
         9,12,12,15,15,11,13,14,15,17,11,13,13,15,16,14,
        15,16,19,17,13,15,15, 0,17, 9,12,12,15,15,11,14,
        13,16,15,11,13,13,15,16,15,15,15,18,17,13,15,15,
        17,17,11,15,14,18,16,12,14,15,17,17,12,15,15,18,
        18,15,15,16,15,19,14,16,16, 0, 0,11,14,14,16,17,
        12,15,14,18,17,12,15,15,18,18,15,17,15,18,16,14,
        16,16,18,18, 7,11,11,14,14,10,12,12,15,15,10,12,
        13,15,15,13,14,15,16,16,14,15,15,18,18, 9,12,12,
        15,15,11,13,13,16,15,11,12,13,16,16,14,15,15,17,
        16,15,16,16,17,17, 9,12,12,15,15,11,13,13,15,17,
        11,14,13,16,15,13,15,15,17,17,15,15,15,18,17,11,
        14,14,17,15,12,14,15,17,18,13,13,15,17,17,14,16,
        16,19,18,16,15,17,17, 0,11,14,14,17,17,12,15,15,
        18, 0,12,15,14,18,16,14,17,17,19, 0,16,18,15, 0,
        16,
    };

    public CodeBookMapType MapType { get; } = (CodeBookMapType)1;
    public int QuantMin { get; } = -533725184;
    public int QuantDelta { get; } = 1611661312;
    public int Quant { get; } = 3;
    public int QuantSequenceP { get; } = 0;

    public int[] QuantList { get; } = {
        2,
        1,
        3,
        0,
        4,
    };
}
