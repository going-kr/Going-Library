namespace Going.Basis.Memories;

/// <summary>
/// 바이트 배열 기반 비트 메모리 구현체.
/// 내부적으로 8비트를 1바이트에 저장하며, 2바이트 단위로 정렬된다.
/// </summary>
public class BitMemory : IBitMemory
{
    /// <summary>비트 데이터를 저장하는 원시 바이트 배열</summary>
    public byte[] RawData { get; }
    /// <summary>관리하는 총 비트 수</summary>
    public int Count { get; }

    /// <summary>지정된 비트 수만큼의 메모리를 할당하여 생성한다.</summary>
    /// <param name="bitCount">관리할 비트 수</param>
    public BitMemory(int bitCount)
    {
        Count = bitCount;
        RawData = new byte[(int)Math.Ceiling(bitCount / 8.0 / 2.0) * 2];
    }

    /// <summary>기존 바이트 배열을 공유하여 비트 메모리를 생성한다.</summary>
    /// <param name="sharedRawData">공유할 바이트 배열</param>
    public BitMemory(byte[] sharedRawData)
    {
        RawData = sharedRawData;
        Count = sharedRawData.Length * 8;
    }

    /// <summary>지정 인덱스의 비트 값을 가져오거나 설정한다.</summary>
    /// <param name="index">비트 인덱스 (0부터 시작)</param>
    /// <returns>비트 값 (true=1, false=0)</returns>
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
