namespace Going.Basis.Memories;

/// <summary>
/// 비트 단위 메모리 접근 인터페이스.
/// 인덱스로 개별 비트를 읽고 쓸 수 있다.
/// </summary>
public interface IBitMemory
{
    /// <summary>지정 인덱스의 비트 값을 가져오거나 설정한다.</summary>
    /// <param name="index">비트 인덱스 (0부터 시작)</param>
    /// <returns>비트 값 (true=1, false=0)</returns>
    bool this[int index] { get; set; }
    /// <summary>관리하는 총 비트 수</summary>
    int Count { get; }
    /// <summary>비트 데이터를 저장하는 원시 바이트 배열</summary>
    byte[] RawData { get; }
}
