using Going.Basis.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Going.Basis.Datas
{
    #region interface : IMemories
    public interface IMemories
    {
        string Code { get; }
        byte[] RawData { get; }
        int Size { get; }
    }
    #endregion
    #region interface : IOBJ
    public interface IOBJ
    {
        MemoryKinds MemoryType { get; }
    }
    #endregion

    #region class : BitMemories
    public class BitMemories : IMemories
    {
        #region Properties
        public string Code { get; private set; }
        public byte[] RawData { get; private set; } = new byte[1024];
        public int Size => RawData.Length * 8;

        public bool this[int index]
        {
            get
            {
                bool ret = false;
                var Index = Convert.ToInt32(Math.Floor(index / 8.0));
                var BitIndex = index % 8;
                if (Index >= 0 && Index < RawData.Length) ret = RawData[Index].GetBit(BitIndex);
                else throw new Exception("인덱스 범위 초과");
                return ret;
            }
            set
            {
                var Index = Convert.ToInt32(Math.Floor(index / 8.0));
                var BitIndex = index % 8;
                if (Index >= 0 && Index < RawData.Length) RawData[Index].SetBit(BitIndex, value);
                else throw new Exception("인덱스 범위 초과");
            }
        }
        #endregion

        #region Constructor
        public BitMemories(string Code, int Size)
        {
            if (Size < 1) throw new Exception("Size는 1이상");
            if (!Regex.IsMatch(Code.ToString(), "[a-zA-Z]", RegexOptions.IgnoreCase)) throw new Exception("Code는 알파벳으로");

            this.RawData = new byte[Convert.ToInt32(Math.Ceiling(Size / 8.0 / 2.0)) * 2];
            this.Code = Code.ToUpper();
        }

        public BitMemories(string Code, byte[] RawData)
        {
            if (RawData == null) throw new Exception("RawData는 null일 수 없음");
            if (!Regex.IsMatch(Code.ToString(), "[a-zA-Z]", RegexOptions.IgnoreCase)) throw new Exception("Code는 알파벳으로");

            this.RawData = RawData;
            this.Code = Code.ToUpper();
        }
        #endregion
    }
    #endregion
    #region class : WordMemories
    public class WordMemories : IMemories
    {
        #region Properties
        public string Code { get; private set; }
        public byte[] RawData { get; private set; } = new byte[1024];
        public int Size => RawData.Length / 2;

        public WORD this[int index]
        {
            get
            {
                if (index >= 0 && index < W.Length) return W[index];
                else throw new Exception("인덱스 범위 초과");
            }
        }

        private WORD[] W { get; set; }
        #endregion

        #region Constructor
        public WordMemories(string Code, int Size)
        {
            if (Size < 1) throw new Exception("Size는 1이상");
            if (!Regex.IsMatch(Code.ToString(), "[a-zA-Z]", RegexOptions.IgnoreCase)) throw new Exception("Code는 알파벳으로");

            this.RawData = new byte[Size * 2];
            this.Code = Code.ToUpper();

            W = new WORD[Size];
            for (int i = 0; i < W.Length; i++) W[i] = new WORD(this, i * 2);
        }

        public WordMemories(string Code, byte[] RawData)
        {
            if (RawData == null) throw new Exception("RawData는 null일 수 없음");
            if (RawData.Length % 2 != 0) throw new Exception("RawData의 크기는 2의 배수이어야 함");
            if (!Regex.IsMatch(Code.ToString(), "[a-zA-Z]", RegexOptions.IgnoreCase)) throw new Exception("Code는 알파벳으로");

            this.RawData = RawData;
            this.Code = Code.ToUpper();

            W = new WORD[Size];
            for (int i = 0; i < W.Length; i++) W[i] = new WORD(this, i * 2);
        }
        #endregion
    }
    #endregion

    #region class : BYTE
    public class BYTE(IMemories mem, int idx) : IOBJ
    {
        public IMemories Memories { get; set; } = mem;
        public int Index { get; set; } = idx;
        public MemoryKinds MemoryType => MemoryKinds.BYTE;

        public byte Value { get => Memories.RawData[Index]; set => Memories.RawData[Index] = value; }
        public sbyte Sign { get => unchecked((sbyte)Value); set => Value = unchecked((byte)value); }

        public bool Bit0 { get => Memories.RawData[Index].GetBit(0); set => Memories.RawData[Index].SetBit(0, value); }
        public bool Bit1 { get => Memories.RawData[Index].GetBit(1); set => Memories.RawData[Index].SetBit(1, value); }
        public bool Bit2 { get => Memories.RawData[Index].GetBit(2); set => Memories.RawData[Index].SetBit(2, value); }
        public bool Bit3 { get => Memories.RawData[Index].GetBit(3); set => Memories.RawData[Index].SetBit(3, value); }
        public bool Bit4 { get => Memories.RawData[Index].GetBit(4); set => Memories.RawData[Index].SetBit(4, value); }
        public bool Bit5 { get => Memories.RawData[Index].GetBit(5); set => Memories.RawData[Index].SetBit(5, value); }
        public bool Bit6 { get => Memories.RawData[Index].GetBit(6); set => Memories.RawData[Index].SetBit(6, value); }
        public bool Bit7 { get => Memories.RawData[Index].GetBit(7); set => Memories.RawData[Index].SetBit(7, value); }
    }
    #endregion
    #region class : WORD
    public class WORD(IMemories mem, int idx)
    {
        public IMemories Memories { get; set; } = mem;
        public int Index { get; set; } = idx;
        public MemoryKinds MemoryType => MemoryKinds.WORD;

        #region bit
        public bool Bit0 { get => Memories.RawData[Index + 0].GetBit(0); set => Memories.RawData[Index + 0].SetBit(0, value); }
        public bool Bit1 { get => Memories.RawData[Index + 0].GetBit(1); set => Memories.RawData[Index + 0].SetBit(1, value); }
        public bool Bit2 { get => Memories.RawData[Index + 0].GetBit(2); set => Memories.RawData[Index + 0].SetBit(2, value); }
        public bool Bit3 { get => Memories.RawData[Index + 0].GetBit(3); set => Memories.RawData[Index + 0].SetBit(3, value); }
        public bool Bit4 { get => Memories.RawData[Index + 0].GetBit(4); set => Memories.RawData[Index + 0].SetBit(4, value); }
        public bool Bit5 { get => Memories.RawData[Index + 0].GetBit(5); set => Memories.RawData[Index + 0].SetBit(5, value); }
        public bool Bit6 { get => Memories.RawData[Index + 0].GetBit(6); set => Memories.RawData[Index + 0].SetBit(6, value); }
        public bool Bit7 { get => Memories.RawData[Index + 0].GetBit(7); set => Memories.RawData[Index + 0].SetBit(7, value); }

        public bool Bit8 { get => Memories.RawData[Index + 1].GetBit(0); set => Memories.RawData[Index + 1].SetBit(0, value); }
        public bool Bit9 { get => Memories.RawData[Index + 1].GetBit(1); set => Memories.RawData[Index + 1].SetBit(1, value); }
        public bool Bit10 { get => Memories.RawData[Index + 1].GetBit(2); set => Memories.RawData[Index + 1].SetBit(2, value); }
        public bool Bit11 { get => Memories.RawData[Index + 1].GetBit(3); set => Memories.RawData[Index + 1].SetBit(3, value); }
        public bool Bit12 { get => Memories.RawData[Index + 1].GetBit(4); set => Memories.RawData[Index + 1].SetBit(4, value); }
        public bool Bit13 { get => Memories.RawData[Index + 1].GetBit(5); set => Memories.RawData[Index + 1].SetBit(5, value); }
        public bool Bit14 { get => Memories.RawData[Index + 1].GetBit(6); set => Memories.RawData[Index + 1].SetBit(6, value); }
        public bool Bit15 { get => Memories.RawData[Index + 1].GetBit(7); set => Memories.RawData[Index + 1].SetBit(7, value); }

        public bool Bit(int bitIndex)
        {
            bool ret = false;
            if (bitIndex >= 0 && bitIndex < 8) ret = Memories.RawData[Index + 0].GetBit(bitIndex);
            else if (bitIndex >= 8 && bitIndex < 16) ret = Memories.RawData[Index + 1].GetBit(bitIndex % 8);
            return ret;
        }

        public void Bit(int bitIndex, bool value)
        {
            if (bitIndex >= 0 && bitIndex < 8) Memories.RawData[Index + 0].SetBit(bitIndex, value);
            else if (bitIndex >= 8 && bitIndex < 16) Memories.RawData[Index + 1].SetBit(bitIndex % 8, value);
        }
        #endregion

        #region byte
        public byte Byte0 { get => Memories.RawData[Index + 0]; set => Memories.RawData[Index + 0] = value; }
        public byte Byte1 { get => Memories.RawData[Index + 1]; set => Memories.RawData[Index + 1] = value; }
        #endregion

        #region word
        public ushort Value
        {
            get
            {
                if (Index + 1 >= Memories.RawData.Length) return 0;
                return BitConverter.ToUInt16(Memories.RawData, Index);
            }
            set
            {
                if (Index + 1 >= Memories.RawData.Length) return;
                var bytes = BitConverter.GetBytes(value);
                Buffer.BlockCopy(bytes, 0, Memories.RawData, Index, 2);
            }
        }

        public short SignValue
        {
            get
            {
                if (Index + 1 >= Memories.RawData.Length) return 0;
                return BitConverter.ToInt16(Memories.RawData, Index);
            }
            set
            {
                if (Index + 1 >= Memories.RawData.Length) return;
                var bytes = BitConverter.GetBytes(value);
                Buffer.BlockCopy(bytes, 0, Memories.RawData, Index, 2);
            }
        }
        #endregion

        #region dword
        public uint DW
        {
            get
            {
                if (Index + 3 >= Memories.RawData.Length) return 0;
                return BitConverter.ToUInt32(Memories.RawData, Index);
            }
            set
            {
                if (Index + 3 >= Memories.RawData.Length) return;
                var bytes = BitConverter.GetBytes(value);
                Buffer.BlockCopy(bytes, 0, Memories.RawData, Index, 4);
            }
        }

        public int SignDW
        {
            get
            {
                if (Index + 3 >= Memories.RawData.Length) return 0;
                return BitConverter.ToInt32(Memories.RawData, Index);
            }
            set
            {
                if (Index + 3 >= Memories.RawData.Length) return;
                var bytes = BitConverter.GetBytes(value);
                Buffer.BlockCopy(bytes, 0, Memories.RawData, Index, 4);
            }
        }
        #endregion

        #region float
        public float R
        {
            get
            {
                if (Index + 3 >= Memories.RawData.Length) return 0F;
                return BitConverter.ToSingle(Memories.RawData, Index);
            }
            set
            {
                if (Index + 3 >= Memories.RawData.Length) return;
                var bytes = BitConverter.GetBytes(value);
                Buffer.BlockCopy(bytes, 0, Memories.RawData, Index, 4);
            }
        }
        #endregion

        #region string
        public string String(int len)
        {
            if (Index + len >= Memories.RawData.Length) return string.Empty;

            int vlen = len;
            for (int i = 0; i < len; i++)
                if (Memories.RawData[Index + i] == 0)
                {
                    vlen = i;
                    break;
                }

            return Encoding.UTF8.GetString(Memories.RawData, Index, vlen);
        }

        public void String(int len, string? value)
        {
            if (Index + len >= Memories.RawData.Length) return;

            var bytes = Encoding.UTF8.GetBytes(value ?? "");
            int vlen = Math.Min(len, bytes.Length);
            Buffer.BlockCopy(bytes, 0, Memories.RawData, Index, len);
            for (int i = vlen; i < len; i++) Memories.RawData[Index + i] = 0;
        }
        #endregion
    }
    #endregion

    #region enum : MemoryKinds
    public enum MemoryKinds { BYTE, WORD }
    #endregion
}
