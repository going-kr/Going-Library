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
                if (Index >= 0 && Index < RawData.Length)
                {
                    switch (BitIndex)
                    {
                        case 0: ret = RawData[Index].Bit0(); break;
                        case 1: ret = RawData[Index].Bit1(); break;
                        case 2: ret = RawData[Index].Bit2(); break;
                        case 3: ret = RawData[Index].Bit3(); break;
                        case 4: ret = RawData[Index].Bit4(); break;
                        case 5: ret = RawData[Index].Bit5(); break;
                        case 6: ret = RawData[Index].Bit6(); break;
                        case 7: ret = RawData[Index].Bit7(); break;
                    }
                }
                else throw new Exception("인덱스 범위 초과");
                return ret;
            }
            set
            {
                var Index = Convert.ToInt32(Math.Floor(index / 8.0));
                var BitIndex = index % 8;
                if (Index >= 0 && Index < RawData.Length)
                {
                    switch (BitIndex)
                    {
                        case 0: RawData[Index].Bit0(value); break;
                        case 1: RawData[Index].Bit1(value); break;
                        case 2: RawData[Index].Bit2(value); break;
                        case 3: RawData[Index].Bit3(value); break;
                        case 4: RawData[Index].Bit4(value); break;
                        case 5: RawData[Index].Bit5(value); break;
                        case 6: RawData[Index].Bit6(value); break;
                        case 7: RawData[Index].Bit7(value); break;
                    }
                }
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

        public ushort this[int index]
        {
            get
            {
                if (index >= 0 && index < W.Length) return W[index].Value;
                else throw new Exception("인덱스 범위 초과");
            }
            set
            {
                if (index >= 0 && index < W.Length) W[index].Value = value;
                else throw new Exception("인덱스 범위 초과");
            }
        }

        public WORD[] W { get; private set; }
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
    #region class : RealMemories
    public class RealMemories : IMemories
    {
        #region Properties
        public string Code { get; private set; }
        public byte[] RawData { get; private set; } = new byte[1024];
        public int Size => RawData.Length / 4;

        public float this[int index]
        {
            get
            {
                if (index >= 0 && index < R.Length) return R[index].Value;
                else throw new Exception("인덱스 범위 초과");
            }
            set
            {
                if (index >= 0 && index < R.Length) R[index].Value = value;
                else throw new Exception("인덱스 범위 초과");
            }
        }

        public REAL[] R { get; private set; }
        #endregion

        #region Constructor
        public RealMemories(string Code, int Size)
        {
            if (Size < 1) throw new Exception("Size는 1이상");
            if (!Regex.IsMatch(Code.ToString(), "[a-zA-Z]", RegexOptions.IgnoreCase)) throw new Exception("Code는 알파벳으로");

            this.RawData = new byte[Size * 4];
            this.Code = Code.ToUpper();

            R = new REAL[Size];
            for (int i = 0; i < R.Length; i++) R[i] = new REAL(this, i * 4);
        }

        public RealMemories(string Code, byte[] RawData)
        {
            if (RawData == null) throw new Exception("RawData는 null일 수 없음");
            if (RawData.Length % 2 != 0) throw new Exception("RawData의 크기는 4의 배수이어야 함");
            if (!Regex.IsMatch(Code.ToString(), "[a-zA-Z]", RegexOptions.IgnoreCase)) throw new Exception("Code는 알파벳으로");

            this.RawData = RawData;
            this.Code = Code.ToUpper();

            R = new REAL[Size];
            for (int i = 0; i < R.Length; i++) R[i] = new REAL(this, i * 4);
        }
        #endregion
    }
    #endregion

    #region class : BYTE
    public class BYTE : IOBJ
    {
        #region Common Properties
        public IMemories Memories { get; set; }
        public int Index { get; set; }
        public MemoryKinds MemoryType => MemoryKinds.BYTE;
        #endregion
        #region Properties
        public byte Value { get => Memories.RawData[Index]; set => Memories.RawData[Index] = value; }
        public sbyte Sign { get => unchecked((sbyte)Value); set => Value = unchecked((byte)value); }

        public bool Bit0 { get => Memories.RawData[Index].Bit0(); set => Memories.RawData[Index].Bit0(value); }
        public bool Bit1 { get => Memories.RawData[Index].Bit1(); set => Memories.RawData[Index].Bit1(value); }
        public bool Bit2 { get => Memories.RawData[Index].Bit2(); set => Memories.RawData[Index].Bit2(value); }
        public bool Bit3 { get => Memories.RawData[Index].Bit3(); set => Memories.RawData[Index].Bit3(value); }
        public bool Bit4 { get => Memories.RawData[Index].Bit4(); set => Memories.RawData[Index].Bit4(value); }
        public bool Bit5 { get => Memories.RawData[Index].Bit5(); set => Memories.RawData[Index].Bit5(value); }
        public bool Bit6 { get => Memories.RawData[Index].Bit6(); set => Memories.RawData[Index].Bit6(value); }
        public bool Bit7 { get => Memories.RawData[Index].Bit7(); set => Memories.RawData[Index].Bit7(value); }
        #endregion

        public BYTE(IMemories mem, int idx) { this.Memories = mem; this.Index = idx; }
    }
    #endregion
    #region class : WORD
    public class WORD
    {
        #region Common Properties
        public IMemories Memories { get; set; }
        public int Index { get; set; }
        public MemoryKinds MemoryType => MemoryKinds.WORD;
        #endregion
        #region Properties
        public ushort Value
        {
            get => unchecked((ushort)(Byte1 << 8 | Byte0));
            set
            {
                Byte1 = (byte)((value & 0xFF00) >> 8);
                Byte0 = (byte)(value & 0x00FF);
            }
        }

        public short Sign { get => unchecked((short)Value); set => Value = unchecked((ushort)value); }

        public byte Byte0 { get => Memories.RawData[Index + 0]; set => Memories.RawData[Index + 0] = value; }
        public byte Byte1 { get => Memories.RawData[Index + 1]; set => Memories.RawData[Index + 1] = value; }

        public bool Bit0 { get => Memories.RawData[Index + 0].Bit0(); set => Memories.RawData[Index + 0].Bit0(value); }
        public bool Bit1 { get => Memories.RawData[Index + 0].Bit1(); set => Memories.RawData[Index + 0].Bit1(value); }
        public bool Bit2 { get => Memories.RawData[Index + 0].Bit2(); set => Memories.RawData[Index + 0].Bit2(value); }
        public bool Bit3 { get => Memories.RawData[Index + 0].Bit3(); set => Memories.RawData[Index + 0].Bit3(value); }
        public bool Bit4 { get => Memories.RawData[Index + 0].Bit4(); set => Memories.RawData[Index + 0].Bit4(value); }
        public bool Bit5 { get => Memories.RawData[Index + 0].Bit5(); set => Memories.RawData[Index + 0].Bit5(value); }
        public bool Bit6 { get => Memories.RawData[Index + 0].Bit6(); set => Memories.RawData[Index + 0].Bit6(value); }
        public bool Bit7 { get => Memories.RawData[Index + 0].Bit7(); set => Memories.RawData[Index + 0].Bit7(value); }
        public bool Bit8 { get => Memories.RawData[Index + 1].Bit0(); set => Memories.RawData[Index + 1].Bit0(value); }
        public bool Bit9 { get => Memories.RawData[Index + 1].Bit1(); set => Memories.RawData[Index + 1].Bit1(value); }
        public bool Bit10 { get => Memories.RawData[Index + 1].Bit2(); set => Memories.RawData[Index + 1].Bit2(value); }
        public bool Bit11 { get => Memories.RawData[Index + 1].Bit3(); set => Memories.RawData[Index + 1].Bit3(value); }
        public bool Bit12 { get => Memories.RawData[Index + 1].Bit4(); set => Memories.RawData[Index + 1].Bit4(value); }
        public bool Bit13 { get => Memories.RawData[Index + 1].Bit5(); set => Memories.RawData[Index + 1].Bit5(value); }
        public bool Bit14 { get => Memories.RawData[Index + 1].Bit6(); set => Memories.RawData[Index + 1].Bit6(value); }
        public bool Bit15 { get => Memories.RawData[Index + 1].Bit7(); set => Memories.RawData[Index + 1].Bit7(value); }
        #endregion

        public WORD(IMemories mem, int idx) { this.Memories = mem; this.Index = idx; }
    }
    #endregion
    #region class : DWORD
    public class DWORD
    {
        #region Common Properties
        public IMemories Memories { get; set; }
        public int Index { get; set; }
        public MemoryKinds MemoryType => MemoryKinds.DWORD;
        #endregion
        #region Properties
        public uint Value
        {
            get => unchecked((uint)(Byte3 << 24 | Byte2 << 16 | Byte1 << 8 | Byte0));
            set
            {
                Byte3 = (byte)((value & 0xFF000000) >> 24);
                Byte2 = (byte)((value & 0x00FF0000) >> 16);
                Byte1 = (byte)((value & 0x0000FF00) >> 8);
                Byte0 = (byte)((value & 0x000000FF));
            }
        }

        public int Sign { get => unchecked((int)Value); set => Value = unchecked((uint)value); }

        public byte Byte0 { get => Memories.RawData[Index + 0]; set => Memories.RawData[Index + 0] = value; }
        public byte Byte1 { get => Memories.RawData[Index + 1]; set => Memories.RawData[Index + 1] = value; }
        public byte Byte2 { get => Memories.RawData[Index + 2]; set => Memories.RawData[Index + 2] = value; }
        public byte Byte3 { get => Memories.RawData[Index + 3]; set => Memories.RawData[Index + 3] = value; }

        public ushort Word0
        {
            get => unchecked((ushort)(Byte0 << 8 | Byte1));
            set
            {
                Byte1 = (byte)((value & 0xFF00) >> 8);
                Byte0 = (byte)(value & 0x00FF);
            }
        }
        public ushort Word1
        {
            get => unchecked((ushort)(Byte2 << 8 | Byte3));
            set
            {
                Byte3 = (byte)((value & 0xFF00) >> 8);
                Byte2 = (byte)(value & 0x00FF);
            }
        }

        public bool Bit0 { get => Memories.RawData[Index + 0].Bit0(); set => Memories.RawData[Index + 0].Bit0(value); }
        public bool Bit1 { get => Memories.RawData[Index + 0].Bit1(); set => Memories.RawData[Index + 0].Bit1(value); }
        public bool Bit2 { get => Memories.RawData[Index + 0].Bit2(); set => Memories.RawData[Index + 0].Bit2(value); }
        public bool Bit3 { get => Memories.RawData[Index + 0].Bit3(); set => Memories.RawData[Index + 0].Bit3(value); }
        public bool Bit4 { get => Memories.RawData[Index + 0].Bit4(); set => Memories.RawData[Index + 0].Bit4(value); }
        public bool Bit5 { get => Memories.RawData[Index + 0].Bit5(); set => Memories.RawData[Index + 0].Bit5(value); }
        public bool Bit6 { get => Memories.RawData[Index + 0].Bit6(); set => Memories.RawData[Index + 0].Bit6(value); }
        public bool Bit7 { get => Memories.RawData[Index + 0].Bit7(); set => Memories.RawData[Index + 0].Bit7(value); }
        public bool Bit8 { get => Memories.RawData[Index + 1].Bit0(); set => Memories.RawData[Index + 1].Bit0(value); }
        public bool Bit9 { get => Memories.RawData[Index + 1].Bit1(); set => Memories.RawData[Index + 1].Bit1(value); }
        public bool Bit10 { get => Memories.RawData[Index + 1].Bit2(); set => Memories.RawData[Index + 1].Bit2(value); }
        public bool Bit11 { get => Memories.RawData[Index + 1].Bit3(); set => Memories.RawData[Index + 1].Bit3(value); }
        public bool Bit12 { get => Memories.RawData[Index + 1].Bit4(); set => Memories.RawData[Index + 1].Bit4(value); }
        public bool Bit13 { get => Memories.RawData[Index + 1].Bit5(); set => Memories.RawData[Index + 1].Bit5(value); }
        public bool Bit14 { get => Memories.RawData[Index + 1].Bit6(); set => Memories.RawData[Index + 1].Bit6(value); }
        public bool Bit15 { get => Memories.RawData[Index + 1].Bit7(); set => Memories.RawData[Index + 1].Bit7(value); }
        public bool Bit16 { get => Memories.RawData[Index + 2].Bit0(); set => Memories.RawData[Index + 2].Bit0(value); }
        public bool Bit17 { get => Memories.RawData[Index + 2].Bit1(); set => Memories.RawData[Index + 2].Bit1(value); }
        public bool Bit18 { get => Memories.RawData[Index + 2].Bit2(); set => Memories.RawData[Index + 2].Bit2(value); }
        public bool Bit19 { get => Memories.RawData[Index + 2].Bit3(); set => Memories.RawData[Index + 2].Bit3(value); }
        public bool Bit20 { get => Memories.RawData[Index + 2].Bit4(); set => Memories.RawData[Index + 2].Bit4(value); }
        public bool Bit21 { get => Memories.RawData[Index + 2].Bit5(); set => Memories.RawData[Index + 2].Bit5(value); }
        public bool Bit22 { get => Memories.RawData[Index + 2].Bit6(); set => Memories.RawData[Index + 2].Bit6(value); }
        public bool Bit23 { get => Memories.RawData[Index + 2].Bit7(); set => Memories.RawData[Index + 2].Bit7(value); }
        public bool Bit24 { get => Memories.RawData[Index + 3].Bit0(); set => Memories.RawData[Index + 3].Bit0(value); }
        public bool Bit25 { get => Memories.RawData[Index + 3].Bit1(); set => Memories.RawData[Index + 3].Bit1(value); }
        public bool Bit26 { get => Memories.RawData[Index + 3].Bit2(); set => Memories.RawData[Index + 3].Bit2(value); }
        public bool Bit27 { get => Memories.RawData[Index + 3].Bit3(); set => Memories.RawData[Index + 3].Bit3(value); }
        public bool Bit28 { get => Memories.RawData[Index + 3].Bit4(); set => Memories.RawData[Index + 3].Bit4(value); }
        public bool Bit29 { get => Memories.RawData[Index + 3].Bit5(); set => Memories.RawData[Index + 3].Bit5(value); }
        public bool Bit30 { get => Memories.RawData[Index + 3].Bit6(); set => Memories.RawData[Index + 3].Bit6(value); }
        public bool Bit31 { get => Memories.RawData[Index + 3].Bit7(); set => Memories.RawData[Index + 3].Bit7(value); }
        #endregion

        public DWORD(IMemories mem, int idx) { this.Memories = mem; this.Index = idx; }
    }
    #endregion
    #region class : REAL
    public class REAL
    {
        #region Common Properties
        public IMemories Memories { get; set; }
        public int Index { get; set; }
        public MemoryKinds MemoryType => MemoryKinds.REAL;
        #endregion
        #region Properties
        public float Value
        {
            get => BitConverter.ToSingle(Memories.RawData, Index);
            set
            {
                var ba = BitConverter.GetBytes(value);
                Array.Copy(ba, 0, Memories.RawData, Index, ba.Length);
            }
        }
        #endregion

        public REAL(IMemories mem, int idx) { this.Memories = mem; this.Index = idx; }
    }
    #endregion

    #region enum : MemoryKinds
    public enum MemoryKinds { BYTE, WORD, DWORD, REAL }
    #endregion
}
