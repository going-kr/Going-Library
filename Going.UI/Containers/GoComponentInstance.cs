using System.Collections.Generic;
using System.Text.Json.Serialization;
using Going.UI.Controls;
using Going.UI.Gudx;

namespace Going.UI.Containers;

/// <summary>
/// 컴포넌트 사용처(<c>&lt;MotorCard .../&gt;</c>)의 런타임 인스턴스. 템플릿 트리를 복제해 자식으로 보유하고,
/// 사용처 파라미터 값을 보관한다. 직렬화 시 이름+파라미터만 출력하고 자식은 생략한다(템플릿 파생).
/// </summary>
public class GoComponentInstance : GoContainer
{
    /// <summary>컴포넌트 이름(직렬화 태그).</summary>
    [JsonIgnore] public string ComponentName { get; set; } = "";
    /// <summary>사용처 파라미터 원본 값(이름→식/리터럴).</summary>
    [JsonIgnore] public Dictionary<string, string> ParamValues { get; } = new(System.StringComparer.Ordinal);
    /// <summary>이 인스턴스의 템플릿(런타임 참조).</summary>
    [JsonIgnore] public GoComponentTemplate? Template { get; set; }
    /// <summary>복제된 템플릿 트리.</summary>
    [JsonInclude] public override List<IGoControl> Childrens { get; } = [];
}
