namespace Going.Basis.Memories;

/// <summary>
/// 바이트 배열 기반 워드 메모리 구현체.
/// 내부적으로 워드(2바이트) 단위로 WordRef 배열을 관리하며, 동일한 RawData를 공유한다.
/// </summary>
public class WordMemory : IWordMemory
{
    private readonly byte[] _rawData;
    private readonly WordRef[] _refs;

    /// <summary>지정된 워드 수만큼의 메모리를 할당하여 생성한다.</summary>
    /// <param name="wordCount">관리할 워드 수</param>
    public WordMemory(int wordCount)
    {
        _rawData = new byte[wordCount * 2];
        _refs = new WordRef[wordCount];
        for (int i = 0; i < wordCount; i++)
            _refs[i] = new WordRef(_rawData, i * 2);
    }

    /// <summary>기존 바이트 배열을 공유하여 워드 메모리를 생성한다.</summary>
    /// <param name="sharedRawData">공유할 바이트 배열 (길이는 2의 배수여야 한다)</param>
    public WordMemory(byte[] sharedRawData)
    {
        _rawData = sharedRawData;
        var wordCount = sharedRawData.Length / 2;
        _refs = new WordRef[wordCount];
        for (int i = 0; i < wordCount; i++)
            _refs[i] = new WordRef(_rawData, i * 2);
    }

    /// <summary>지정 인덱스의 워드 참조를 가져온다.</summary>
    /// <param name="index">워드 인덱스 (0부터 시작)</param>
    /// <returns>해당 워드 위치의 WordRef 인스턴스</returns>
    public WordRef this[int index] => _refs[index];
    /// <summary>관리하는 총 워드 수</summary>
    public int Count => _refs.Length;
    /// <summary>워드 데이터를 저장하는 원시 바이트 배열</summary>
    public byte[] RawData => _rawData;
}
