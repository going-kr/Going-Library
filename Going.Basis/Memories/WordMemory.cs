namespace Going.Basis.Memories;

public class WordMemory : IWordMemory
{
    private readonly byte[] _rawData;
    private readonly WordRef[] _refs;

    public WordMemory(int wordCount)
    {
        _rawData = new byte[wordCount * 2];
        _refs = new WordRef[wordCount];
        for (int i = 0; i < wordCount; i++)
            _refs[i] = new WordRef(_rawData, i * 2);
    }

    public WordMemory(byte[] sharedRawData)
    {
        _rawData = sharedRawData;
        var wordCount = sharedRawData.Length / 2;
        _refs = new WordRef[wordCount];
        for (int i = 0; i < wordCount; i++)
            _refs[i] = new WordRef(_rawData, i * 2);
    }

    public WordRef this[int index] => _refs[index];
    public int Count => _refs.Length;
    public byte[] RawData => _rawData;
}
