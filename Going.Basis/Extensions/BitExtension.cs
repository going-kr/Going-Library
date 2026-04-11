using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Extensions
{
    /// <summary>
    /// 정수 타입(byte, short, ushort, int, uint)에 대한 비트/바이트 조작 확장 메서드 모음.
    /// GetBit/SetBit으로 개별 비트를, GetByte/SetByte로 개별 바이트를 읽고 쓸 수 있다.
    /// </summary>
    public static class Bits
    {
        /// <summary>byte 값에서 지정 인덱스의 비트를 가져온다.</summary>
        /// <param name="value">대상 값</param>
        /// <param name="bitIndex">비트 인덱스 (0~7)</param>
        /// <returns>비트 값 (true=1, false=0)</returns>
        public static bool GetBit(this byte value, int bitIndex) => (value & (1 << bitIndex)) != 0;
        /// <summary>byte 값에서 지정 인덱스의 비트를 설정한다.</summary>
        /// <param name="value">대상 값</param>
        /// <param name="bitIndex">비트 인덱스 (0~7)</param>
        /// <param name="bitValue">설정할 비트 값</param>
        public static void SetBit(this ref byte value, int bitIndex, bool bitValue)
        {
            if (bitValue) value |= (byte)(1 << bitIndex);
            else value &= (byte)~(1 << bitIndex);
        }

        /// <summary>ushort 값에서 지정 인덱스의 비트를 가져온다.</summary>
        /// <param name="value">대상 값</param>
        /// <param name="bitIndex">비트 인덱스 (0~15)</param>
        /// <returns>비트 값</returns>
        public static bool GetBit(this ushort value, int bitIndex) => (value & (1 << bitIndex)) != 0;
        /// <summary>ushort 값에서 지정 인덱스의 비트를 설정한다.</summary>
        /// <param name="value">대상 값</param>
        /// <param name="bitIndex">비트 인덱스 (0~15)</param>
        /// <param name="bitValue">설정할 비트 값</param>
        public static void SetBit(this ref ushort value, int bitIndex, bool bitValue)
        {
            if (bitValue) value |= (ushort)(1 << bitIndex);
            else value &= (ushort)~(1 << bitIndex);
        }

        /// <summary>short 값에서 지정 인덱스의 비트를 가져온다.</summary>
        /// <param name="value">대상 값</param>
        /// <param name="bitIndex">비트 인덱스 (0~15)</param>
        /// <returns>비트 값</returns>
        public static bool GetBit(this short value, int bitIndex) => ((ushort)value).GetBit(bitIndex);
        /// <summary>short 값에서 지정 인덱스의 비트를 설정한다.</summary>
        /// <param name="value">대상 값</param>
        /// <param name="bitIndex">비트 인덱스 (0~15)</param>
        /// <param name="bitValue">설정할 비트 값</param>
        public static void SetBit(this ref short value, int bitIndex, bool bitValue)
        {
            ushort temp = (ushort)value;
            temp.SetBit(bitIndex, bitValue);
            value = (short)temp;
        }

        /// <summary>int 값에서 지정 인덱스의 비트를 가져온다.</summary>
        /// <param name="value">대상 값</param>
        /// <param name="bitIndex">비트 인덱스 (0~31)</param>
        /// <returns>비트 값</returns>
        public static bool GetBit(this int value, int bitIndex) => (value & (1 << bitIndex)) != 0;
        /// <summary>int 값에서 지정 인덱스의 비트를 설정한다.</summary>
        /// <param name="value">대상 값</param>
        /// <param name="bitIndex">비트 인덱스 (0~31)</param>
        /// <param name="bitValue">설정할 비트 값</param>
        public static void SetBit(this ref int value, int bitIndex, bool bitValue)
        {
            if (bitValue) value |= (1 << bitIndex);
            else value &= ~(1 << bitIndex);
        }

        /// <summary>uint 값에서 지정 인덱스의 비트를 가져온다.</summary>
        /// <param name="value">대상 값</param>
        /// <param name="bitIndex">비트 인덱스 (0~31)</param>
        /// <returns>비트 값</returns>
        public static bool GetBit(this uint value, int bitIndex) => (value & (1u << bitIndex)) != 0;
        /// <summary>uint 값에서 지정 인덱스의 비트를 설정한다.</summary>
        /// <param name="value">대상 값</param>
        /// <param name="bitIndex">비트 인덱스 (0~31)</param>
        /// <param name="bitValue">설정할 비트 값</param>
        public static void SetBit(this ref uint value, int bitIndex, bool bitValue)
        {
            if (bitValue) value |= (1u << bitIndex);
            else value &= ~(1u << bitIndex);
        }

        /// <summary>short 값에서 지정 인덱스의 바이트를 가져온다.</summary>
        /// <param name="value">대상 값</param>
        /// <param name="byteIndex">바이트 인덱스 (0=하위, 1=상위)</param>
        /// <returns>해당 바이트 값</returns>
        public static byte GetByte(this short value, int byteIndex) => (byte)((value >> (byteIndex * 8)) & 0xFF);

        /// <summary>short 값에서 지정 인덱스의 바이트를 설정한다.</summary>
        /// <param name="value">대상 값</param>
        /// <param name="byteIndex">바이트 인덱스 (0=하위, 1=상위)</param>
        /// <param name="byteValue">설정할 바이트 값</param>
        public static void SetByte(this ref short value, int byteIndex, byte byteValue)
        {
            int mask = 0xFF << (byteIndex * 8);
            value = (short)((value & ~mask) | (byteValue << (byteIndex * 8)));
        }

        /// <summary>ushort 값에서 지정 인덱스의 바이트를 가져온다.</summary>
        /// <param name="value">대상 값</param>
        /// <param name="byteIndex">바이트 인덱스 (0=하위, 1=상위)</param>
        /// <returns>해당 바이트 값</returns>
        public static byte GetByte(this ushort value, int byteIndex)
        {
            return (byte)((value >> (byteIndex * 8)) & 0xFF);
        }

        /// <summary>ushort 값에서 지정 인덱스의 바이트를 설정한다.</summary>
        /// <param name="value">대상 값</param>
        /// <param name="byteIndex">바이트 인덱스 (0=하위, 1=상위)</param>
        /// <param name="byteValue">설정할 바이트 값</param>
        public static void SetByte(this ref ushort value, int byteIndex, byte byteValue)
        {
            int mask = 0xFF << (byteIndex * 8);
            value = (ushort)((value & ~mask) | (byteValue << (byteIndex * 8)));
        }

        /// <summary>int 값에서 지정 인덱스의 바이트를 가져온다.</summary>
        /// <param name="value">대상 값</param>
        /// <param name="byteIndex">바이트 인덱스 (0=최하위 ~ 3=최상위)</param>
        /// <returns>해당 바이트 값</returns>
        public static byte GetByte(this int value, int byteIndex)
        {
            return (byte)((value >> (byteIndex * 8)) & 0xFF);
        }

        /// <summary>int 값에서 지정 인덱스의 바이트를 설정한다.</summary>
        /// <param name="value">대상 값</param>
        /// <param name="byteIndex">바이트 인덱스 (0=최하위 ~ 3=최상위)</param>
        /// <param name="byteValue">설정할 바이트 값</param>
        public static void SetByte(this ref int value, int byteIndex, byte byteValue)
        {
            int mask = 0xFF << (byteIndex * 8);
            value = (value & ~mask) | (byteValue << (byteIndex * 8));
        }

        /// <summary>uint 값에서 지정 인덱스의 바이트를 가져온다.</summary>
        /// <param name="value">대상 값</param>
        /// <param name="byteIndex">바이트 인덱스 (0=최하위 ~ 3=최상위)</param>
        /// <returns>해당 바이트 값</returns>
        public static byte GetByte(this uint value, int byteIndex)
        {
            return (byte)((value >> (byteIndex * 8)) & 0xFF);
        }

        /// <summary>uint 값에서 지정 인덱스의 바이트를 설정한다.</summary>
        /// <param name="value">대상 값</param>
        /// <param name="byteIndex">바이트 인덱스 (0=최하위 ~ 3=최상위)</param>
        /// <param name="byteValue">설정할 바이트 값</param>
        public static void SetByte(this ref uint value, int byteIndex, byte byteValue)
        {
            uint mask = 0xFFu << (byteIndex * 8);
            value = (value & ~mask) | ((uint)byteValue << (byteIndex * 8));
        }
    }
}
