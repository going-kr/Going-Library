using Going.UI.Controls;

namespace Going.UI.ViewObjects
{
    /// <summary>
    /// ViewObject(VoObj)의 속성을 나타내는 어트리뷰트입니다.
    /// <para><see cref="GoPropertyAttribute"/>를 상속하므로 기존 <c>GoGudxConverter</c>가
    /// <c>inherit:true</c>로 P1(스칼라 속성)으로 자동 인식합니다. 즉 vset 직렬화와
    /// 에디터 속성 그리드 노출이 별도 작업 없이 따라옵니다.</para>
    /// <para>동시에 에디터/도구는 <see cref="VoPropertyAttribute"/> 여부를 따로 검사하여
    /// Vo 전용 UX를 적용할 수 있습니다.</para>
    /// </summary>
    /// <param name="category">속성 카테고리 (에디터 그룹핑 기준)</param>
    /// <param name="order">카테고리 내 정렬 순서</param>
    public class VoPropertyAttribute(string category, int order) : GoPropertyAttribute(category, order) { }

    /// <summary>여러 줄 텍스트 Vo 속성. 에디터 멀티라인 편집기를 사용합니다.</summary>
    public class VoMultiLinePropertyAttribute(string category, int order) : GoMultiLinePropertyAttribute(category, order) { }

    /// <summary>글꼴 이름 Vo 속성. 에디터 폰트 선택기를 사용합니다.</summary>
    public class VoFontNamePropertyAttribute(string category, int order) : GoFontNamePropertyAttribute(category, order) { }
}
