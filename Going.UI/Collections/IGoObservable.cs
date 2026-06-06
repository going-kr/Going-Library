namespace Going.UI.Collections;

/// <summary>컬렉션 변경 dirty-bit를 제네릭 인자 없이 노출.</summary>
public interface IGoObservable
{
    /// <summary>마지막 소비 이후 변경되었는지. 소비자가 false로 리셋.</summary>
    bool Changed { get; set; }
}
