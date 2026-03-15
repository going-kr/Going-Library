using System.Text;

namespace Going.Basis.Memories;

public class WordRef
{
    private readonly byte[] _rawData;
    private readonly int _byteOffset;

    public WordRef(byte[] rawData, int byteOffset)
    {
        _rawData = rawData;
        _byteOffset = byteOffset;
        Bit = new BitAccessor(_rawData, _byteOffset);
    }

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

    public byte Byte0
    {
        get => _byteOffset < _rawData.Length ? _rawData[_byteOffset] : (byte)0;
        set { if (_byteOffset < _rawData.Length) _rawData[_byteOffset] = value; }
    }

    public byte Byte1
    {
        get => _byteOffset + 1 < _rawData.Length ? _rawData[_byteOffset + 1] : (byte)0;
        set { if (_byteOffset + 1 < _rawData.Length) _rawData[_byteOffset + 1] = value; }
    }

    public BitAccessor Bit { get; }
}

public class BitAccessor
{
    private readonly byte[] _rawData;
    private readonly int _byteOffset;

    public BitAccessor(byte[] rawData, int byteOffset)
    {
        _rawData = rawData;
        _byteOffset = byteOffset;
    }

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
