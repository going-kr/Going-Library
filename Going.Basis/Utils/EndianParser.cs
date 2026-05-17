using System.Buffers.Binary;

namespace Going.Basis.Utils
{
    /// <summary>
    /// 32비트 값을 Modbus 워드 배열로 변환할 때 사용할 바이트/워드 순서입니다.
    /// A, B, C, D는 원본 값을 big-endian 바이트 순서로 보았을 때의 각 바이트입니다.
    /// </summary>
    public enum EndianOrder
    {
        /// <summary>Big-endian. bytes: A B C D, words: AB CD.</summary>
        ABCD,

        /// <summary>Byte-swap. bytes: B A D C, words: BA DC.</summary>
        BADC,

        /// <summary>Word-swap. bytes: C D A B, words: CD AB.</summary>
        CDAB,

        /// <summary>Little-endian. bytes: D C B A, words: DC BA.</summary>
        DCBA
    }

    /// <summary>
    /// 숫자 값을 Modbus 16비트 워드 배열로 변환하거나 다시 원래 값으로 복원합니다.
    /// </summary>
    public static class EndianParser
    {
        /// <summary>16비트 부호 없는 정수를 1개 워드로 변환합니다.</summary>
        public static int[] ToWords(ushort value, EndianOrder order = EndianOrder.ABCD)
            => new[] { (int)value };

        /// <summary>16비트 부호 있는 정수를 1개 워드로 변환합니다.</summary>
        public static int[] ToWords(short value, EndianOrder order = EndianOrder.ABCD)
            => new[] { (int)unchecked((ushort)value) };

        /// <summary>32비트 부호 있는 정수를 2개 워드로 변환합니다.</summary>
        public static int[] ToWords(int value, EndianOrder order = EndianOrder.ABCD)
            => ToWords(unchecked((uint)value), order);

        /// <summary>32비트 부호 없는 정수를 2개 워드로 변환합니다.</summary>
        public static int[] ToWords(uint value, EndianOrder order = EndianOrder.ABCD)
        {
            Span<byte> bytes = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(bytes, value);
            ApplyOrder(bytes, order);
            return ToWordArray(bytes);
        }

        /// <summary>32비트 부동소수점 값을 2개 워드로 변환합니다.</summary>
        public static int[] ToWords(float value, EndianOrder order = EndianOrder.ABCD)
            => ToWords(BitConverter.SingleToUInt32Bits(value), order);

        /// <summary>지원되는 숫자 값을 워드 배열로 변환합니다. 지원 타입: short, ushort, int, uint, float.</summary>
        public static int[] ToWords<T>(T value, EndianOrder order = EndianOrder.ABCD) where T : struct
            => value switch
            {
                short v => ToWords(v, order),
                ushort v => ToWords(v, order),
                int v => ToWords(v, order),
                uint v => ToWords(v, order),
                float v => ToWords(v, order),
                _ => throw new NotSupportedException($"{typeof(T).Name} is not supported. Supported types: short, ushort, int, uint, float.")
            };

        /// <summary>워드 배열에서 지정 타입 값을 복원합니다. 지원 타입: short, ushort, int, uint, float.</summary>
        public static T FromWords<T>(IReadOnlyList<int> words, EndianOrder order = EndianOrder.ABCD) where T : struct
        {
            if (typeof(T) == typeof(ushort)) return (T)(object)ReadUInt16(words);
            if (typeof(T) == typeof(short)) return (T)(object)unchecked((short)ReadUInt16(words));

            var value = ReadUInt32(words, order);
            if (typeof(T) == typeof(uint)) return (T)(object)value;
            if (typeof(T) == typeof(int)) return (T)(object)unchecked((int)value);
            if (typeof(T) == typeof(float)) return (T)(object)BitConverter.UInt32BitsToSingle(value);

            throw new NotSupportedException($"{typeof(T).Name} is not supported. Supported types: short, ushort, int, uint, float.");
        }

        /// <summary>워드 배열의 지정 인덱스부터 값을 복원합니다. 지원 타입: short, ushort, int, uint, float.</summary>
        public static T FromWords<T>(IReadOnlyList<int> words, int index, EndianOrder order = EndianOrder.ABCD) where T : struct
        {
            ValidateIndex(words, index, GetWordCount<T>());
            return FromWords<T>(new WordSlice(words, index), order);
        }

        /// <summary>워드 배열에서 값을 복원합니다. 실패하면 false를 반환하고 value는 default가 됩니다.</summary>
        public static bool TryFromWords<T>(IReadOnlyList<int> words, EndianOrder order, out T value) where T : struct
            => TryFromWords(words, 0, order, out value);

        /// <summary>워드 배열의 지정 인덱스부터 값을 복원합니다. 실패하면 false를 반환하고 value는 default가 됩니다.</summary>
        public static bool TryFromWords<T>(IReadOnlyList<int> words, int index, EndianOrder order, out T value) where T : struct
        {
            try
            {
                value = FromWords<T>(words, index, order);
                return true;
            }
            catch (ArgumentException)
            {
                value = default;
                return false;
            }
            catch (NotSupportedException)
            {
                value = default;
                return false;
            }
        }

        /// <summary>지정 타입을 표현하는 데 필요한 Modbus 워드 수를 반환합니다.</summary>
        public static int GetWordCount<T>() where T : struct
        {
            var type = typeof(T);
            if (type == typeof(short) || type == typeof(ushort)) return 1;
            if (type == typeof(int) || type == typeof(uint) || type == typeof(float)) return 2;
            throw new NotSupportedException($"{type.Name} is not supported. Supported types: short, ushort, int, uint, float.");
        }

        private static ushort ReadUInt16(IReadOnlyList<int> words)
        {
            ValidateWordCount(words, 1);
            return ToWord(words[0]);
        }

        private static uint ReadUInt32(IReadOnlyList<int> words, EndianOrder order)
        {
            ValidateWordCount(words, 2);

            Span<byte> bytes = stackalloc byte[4];
            WriteWord(bytes, 0, words[0]);
            WriteWord(bytes, 2, words[1]);
            RestoreOrder(bytes, order);

            return BinaryPrimitives.ReadUInt32BigEndian(bytes);
        }

        private static int[] ToWordArray(ReadOnlySpan<byte> bytes)
        {
            var words = new int[bytes.Length / 2];
            for (var i = 0; i < words.Length; i++)
                words[i] = (bytes[i * 2] << 8) | bytes[(i * 2) + 1];
            return words;
        }

        private static void WriteWord(Span<byte> bytes, int offset, int word)
        {
            var value = ToWord(word);
            bytes[offset] = (byte)(value >> 8);
            bytes[offset + 1] = (byte)value;
        }

        private static ushort ToWord(int word)
        {
            if (word < 0 || word > 0xFFFF)
                throw new ArgumentOutOfRangeException(nameof(word), word, "Word value must be between 0 and 65535.");
            return (ushort)word;
        }

        private static void ValidateWordCount(IReadOnlyList<int> words, int required)
        {
            if (words.Count < required)
                throw new ArgumentException($"At least {required} word(s) are required.", nameof(words));
        }

        private static void ValidateIndex(IReadOnlyList<int> words, int index, int required)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be zero or greater.");
            if (words.Count - index < required)
                throw new ArgumentException($"At least {required} word(s) are required from index {index}.", nameof(words));
        }

        private static void ApplyOrder(Span<byte> bytes, EndianOrder order)
        {
            Span<byte> source = stackalloc byte[4];
            bytes.CopyTo(source);

            switch (order)
            {
                case EndianOrder.ABCD:
                    break;
                case EndianOrder.BADC:
                    bytes[0] = source[1]; bytes[1] = source[0]; bytes[2] = source[3]; bytes[3] = source[2];
                    break;
                case EndianOrder.CDAB:
                    bytes[0] = source[2]; bytes[1] = source[3]; bytes[2] = source[0]; bytes[3] = source[1];
                    break;
                case EndianOrder.DCBA:
                    bytes[0] = source[3]; bytes[1] = source[2]; bytes[2] = source[1]; bytes[3] = source[0];
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(order), order, null);
            }
        }

        private static void RestoreOrder(Span<byte> bytes, EndianOrder order)
        {
            Span<byte> source = stackalloc byte[4];
            bytes.CopyTo(source);

            switch (order)
            {
                case EndianOrder.ABCD:
                    break;
                case EndianOrder.BADC:
                    bytes[0] = source[1]; bytes[1] = source[0]; bytes[2] = source[3]; bytes[3] = source[2];
                    break;
                case EndianOrder.CDAB:
                    bytes[0] = source[2]; bytes[1] = source[3]; bytes[2] = source[0]; bytes[3] = source[1];
                    break;
                case EndianOrder.DCBA:
                    bytes[0] = source[3]; bytes[1] = source[2]; bytes[2] = source[1]; bytes[3] = source[0];
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(order), order, null);
            }
        }

        private readonly struct WordSlice : IReadOnlyList<int>
        {
            private readonly IReadOnlyList<int> words;
            private readonly int start;

            public WordSlice(IReadOnlyList<int> words, int start)
            {
                this.words = words;
                this.start = start;
            }

            public int Count => words.Count - start;

            public int this[int index] => words[start + index];

            public IEnumerator<int> GetEnumerator()
            {
                for (var i = start; i < words.Count; i++)
                    yield return words[i];
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
