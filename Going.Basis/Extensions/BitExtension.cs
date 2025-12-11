using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Extensions
{
    public static class Bits
    {
        public static bool GetBit(this byte value, int bitIndex) => (value & (1 << bitIndex)) != 0;
        public static void SetBit(this ref byte value, int bitIndex, bool bitValue)
        {
            if (bitValue) value |= (byte)(1 << bitIndex);
            else value &= (byte)~(1 << bitIndex);
        }

        public static bool GetBit(this ushort value, int bitIndex) => (value & (1 << bitIndex)) != 0;
        public static void SetBit(this ref ushort value, int bitIndex, bool bitValue)
        {
            if (bitValue) value |= (ushort)(1 << bitIndex);
            else value &= (ushort)~(1 << bitIndex);
        }

        public static bool GetBit(this short value, int bitIndex) => ((ushort)value).GetBit(bitIndex);
        public static void SetBit(this ref short value, int bitIndex, bool bitValue)
        {
            ushort temp = (ushort)value;
            temp.SetBit(bitIndex, bitValue);
            value = (short)temp;
        }

        public static bool GetBit(this int value, int bitIndex) => (value & (1 << bitIndex)) != 0;
        public static void SetBit(this ref int value, int bitIndex, bool bitValue)
        {
            if (bitValue) value |= (1 << bitIndex);
            else value &= ~(1 << bitIndex);
        }

        public static bool GetBit(this uint value, int bitIndex) => (value & (1u << bitIndex)) != 0;
        public static void SetBit(this ref uint value, int bitIndex, bool bitValue)
        {
            if (bitValue) value |= (1u << bitIndex);
            else value &= ~(1u << bitIndex);
        }

        public static byte GetByte(this short value, int byteIndex) => (byte)((value >> (byteIndex * 8)) & 0xFF);

        public static void SetByte(this ref short value, int byteIndex, byte byteValue)
        {
            int mask = 0xFF << (byteIndex * 8);
            value = (short)((value & ~mask) | (byteValue << (byteIndex * 8)));
        }

        public static byte GetByte(this ushort value, int byteIndex)
        {
            return (byte)((value >> (byteIndex * 8)) & 0xFF);
        }

        public static void SetByte(this ref ushort value, int byteIndex, byte byteValue)
        {
            int mask = 0xFF << (byteIndex * 8);
            value = (ushort)((value & ~mask) | (byteValue << (byteIndex * 8)));
        }

        public static byte GetByte(this int value, int byteIndex)
        {
            return (byte)((value >> (byteIndex * 8)) & 0xFF);
        }

        public static void SetByte(this ref int value, int byteIndex, byte byteValue)
        {
            int mask = 0xFF << (byteIndex * 8);
            value = (value & ~mask) | (byteValue << (byteIndex * 8));
        }

        public static byte GetByte(this uint value, int byteIndex)
        {
            return (byte)((value >> (byteIndex * 8)) & 0xFF);
        }

        public static void SetByte(this ref uint value, int byteIndex, byte byteValue)
        {
            uint mask = 0xFFu << (byteIndex * 8);
            value = (value & ~mask) | ((uint)byteValue << (byteIndex * 8));
        }
    }
}
