namespace Going.Basis.Memories;

/// <summary>
/// 워드(16비트) 단위 메모리 접근 인터페이스.
/// 인덱스로 WordRef를 통해 다양한 타입(ushort, int, float, string 등)으로 데이터에 접근할 수 있다.
/// </summary>
public interface IWordMemory
{
    /// <summary>지정 인덱스의 워드 참조를 가져온다.</summary>
    /// <param name="index">워드 인덱스 (0부터 시작)</param>
    /// <returns>해당 워드 위치의 WordRef 인스턴스</returns>
    WordRef this[int index] { get; }
    /// <summary>관리하는 총 워드 수</summary>
    int Count { get; }
    /// <summary>워드 데이터를 저장하는 원시 바이트 배열</summary>
    byte[] RawData { get; }
}
