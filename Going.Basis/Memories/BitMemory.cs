namespace Going.Basis.Memories;

public class BitMemory : IBitMemory
{
    public byte[] RawData { get; }
    public int Count { get; }

    public BitMemory(int bitCount)
    {
        Count = bitCount;
        RawData = new byte[(int)Math.Ceiling(bitCount / 8.0 / 2.0) * 2];
    }

    public BitMemory(byte[] sharedRawData)
    {
        RawData = sharedRawData;
        Count = sharedRawData.Length * 8;
    }

    public bool this[int index]
    {
        get
        {
            int byteIdx = index / 8;
            int bitIdx = index % 8;
            if (byteIdx < 0 || byteIdx >= RawData.Length) return false;
            return (RawData[byteIdx] & (1 << bitIdx)) != 0;
        }
        set
        {
            int byteIdx = index / 8;
            int bitIdx = index % 8;
            if (byteIdx < 0 || byteIdx >= RawData.Length) return;
            if (value) RawData[byteIdx] |= (byte)(1 << bitIdx);
            else RawData[byteIdx] &= (byte)~(1 << bitIdx);
        }
    }
}
