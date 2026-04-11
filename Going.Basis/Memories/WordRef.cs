using System.Text;

namespace Going.Basis.Memories;

/// <summary>
/// 원시 바이트 배열의 특정 오프셋에 대한 다중 타입 접근을 제공하는 워드 참조 클래스.
/// 동일한 메모리 위치를 ushort(W), uint(DW), short(IW), int(IDW), float(R), string(S) 등 다양한 타입으로 읽고 쓸 수 있다.
/// </summary>
public class WordRef
{
    private readonly byte[] _rawData;
    private readonly int _byteOffset;

    /// <summary>워드 참조를 생성한다.</summary>
    /// <param name="rawData">원시 바이트 배열</param>
    /// <param name="byteOffset">바이트 오프셋 위치</param>
    public WordRef(byte[] rawData, int byteOffset)
    {
        _rawData = rawData;
        _byteOffset = byteOffset;
        Bit = new BitAccessor(_rawData, _byteOffset);
    }

    /// <summary>부호 없는 16비트 정수 (ushort) 값</summary>
    public ushort W
    {
        get
        {
            if (_byteOffset + 1 >= _rawData.Length) return 0;
            return BitConverter.ToUInt16(_rawData, _byteOffset);
        }
        set
        {
            if (_byteOffset + 1 >= _rawData.Length) return;
            Buffer.BlockCopy(BitConverter.GetBytes(value), 0, _rawData, _byteOffset, 2);
        }
    }

    /// <summary>부호 없는 32비트 정수 (uint) 값. 현재 워드와 다음 워드(4바이트)를 사용한다.</summary>
    public uint DW
    {
        get
        {
            if (_byteOffset + 3 >= _rawData.Length) return 0;
            return BitConverter.ToUInt32(_rawData, _byteOffset);
        }
        set
        {
            if (_byteOffset + 3 >= _rawData.Length) return;
            Buffer.BlockCopy(BitConverter.GetBytes(value), 0, _rawData, _byteOffset, 4);
        }
    }

    /// <summary>부호 있는 16비트 정수 (short) 값</summary>
    public short IW
    {
        get
        {
            if (_byteOffset + 1 >= _rawData.Length) return 0;
            return BitConverter.ToInt16(_rawData, _byteOffset);
        }
        set
        {
            if (_byteOffset + 1 >= _rawData.Length) return;
            Buffer.BlockCopy(BitConverter.GetBytes(value), 0, _rawData, _byteOffset, 2);
        }
    }

    /// <summary>부호 있는 32비트 정수 (int) 값. 현재 워드와 다음 워드(4바이트)를 사용한다.</summary>
    public int IDW
    {
        get
        {
            if (_byteOffset + 3 >= _rawData.Length) return 0;
            return BitConverter.ToInt32(_rawData, _byteOffset);
        }
        set
        {
            if (_byteOffset + 3 >= _rawData.Length) return;
            Buffer.BlockCopy(BitConverter.GetBytes(value), 0, _rawData, _byteOffset, 4);
        }
    }

    /// <summary>32비트 부동소수점 (float) 값. 현재 워드와 다음 워드(4바이트)를 사용한다.</summary>
    public float R
    {
        get
        {
            if (_byteOffset + 3 >= _rawData.Length) return 0f;
            return BitConverter.ToSingle(_rawData, _byteOffset);
        }
        set
        {
            if (_byteOffset + 3 >= _rawData.Length) return;
            Buffer.BlockCopy(BitConverter.GetBytes(value), 0, _rawData, _byteOffset, 4);
        }
    }

    private const int MaxStringLength = 256;

    /// <summary>UTF-8 문자열 값. 널 종단 문자열로 최대 256바이트까지 저장된다.</summary>
    public string S
    {
        get
        {
            int maxEnd = Math.Min(_rawData.Length, _byteOffset + MaxStringLength);
            int end = _byteOffset;
            while (end < maxEnd && _rawData[end] != 0) end++;
            return Encoding.UTF8.GetString(_rawData, _byteOffset, end - _byteOffset);
        }
        set
        {
            var bytes = Encoding.UTF8.GetBytes(value ?? "");
            int maxLen = Math.Min(_rawData.Length - _byteOffset - 1, MaxStringLength - 1);
            if (maxLen < 0) return;
            int len = Math.Min(bytes.Length, maxLen);
            if (len > 0)
                Buffer.BlockCopy(bytes, 0, _rawData, _byteOffset, len);
            _rawData[_byteOffset + len] = 0;
        }
    }

    /// <summary>하위 바이트 (오프셋 + 0)</summary>
    public byte Byte0
    {
        get => _byteOffset < _rawData.Length ? _rawData[_byteOffset] : (byte)0;
        set { if (_byteOffset < _rawData.Length) _rawData[_byteOffset] = value; }
    }

    /// <summary>상위 바이트 (오프셋 + 1)</summary>
    public byte Byte1
    {
        get => _byteOffset + 1 < _rawData.Length ? _rawData[_byteOffset + 1] : (byte)0;
        set { if (_byteOffset + 1 < _rawData.Length) _rawData[_byteOffset + 1] = value; }
    }

    /// <summary>워드 내 개별 비트에 접근하는 BitAccessor (0~15 인덱스)</summary>
    public BitAccessor Bit { get; }
}

/// <summary>
/// 워드(16비트) 내 개별 비트에 대한 접근을 제공하는 클래스.
/// 인덱스 0~15로 각 비트를 읽고 쓸 수 있다.
/// </summary>
public class BitAccessor
{
    private readonly byte[] _rawData;
    private readonly int _byteOffset;

    /// <summary>BitAccessor를 생성한다.</summary>
    /// <param name="rawData">원시 바이트 배열</param>
    /// <param name="byteOffset">바이트 오프셋 위치</param>
    public BitAccessor(byte[] rawData, int byteOffset)
    {
        _rawData = rawData;
        _byteOffset = byteOffset;
    }

    /// <summary>지정 인덱스(0~15)의 비트 값을 가져오거나 설정한다.</summary>
    /// <param name="index">비트 인덱스 (0~15)</param>
    /// <returns>비트 값 (true=1, false=0)</returns>
    public bool this[int index]
    {
        get
        {
            if (index < 0 || index > 15) return false;
            int byteIdx = _byteOffset + (index / 8);
            int bitIdx = index % 8;
            if (byteIdx >= _rawData.Length) return false;
            return (_rawData[byteIdx] & (1 << bitIdx)) != 0;
        }
        set
        {
            if (index < 0 || index > 15) return;
            int byteIdx = _byteOffset + (index / 8);
            int bitIdx = index % 8;
            if (byteIdx >= _rawData.Length) return;
            if (value) _rawData[byteIdx] |= (byte)(1 << bitIdx);
            else _rawData[byteIdx] &= (byte)~(1 << bitIdx);
        }
    }
}
