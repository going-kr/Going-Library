namespace Going.Basis.Memories;

public interface IBitMemory
{
    bool this[int index] { get; set; }
    int Count { get; }
    byte[] RawData { get; }
}
