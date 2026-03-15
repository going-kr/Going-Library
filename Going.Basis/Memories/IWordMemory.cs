namespace Going.Basis.Memories;

public interface IWordMemory
{
    WordRef this[int index] { get; }
    int Count { get; }
    byte[] RawData { get; }
}
