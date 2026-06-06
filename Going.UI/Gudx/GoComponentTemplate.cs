using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;

namespace Going.UI.Gudx;

/// <summary>역직렬화된 컴포넌트 템플릿(파라미터 정의 + 내부 컨트롤 트리).</summary>
public sealed class GoComponentTemplate
{
    /// <summary>컴포넌트 이름(사용처 태그).</summary>
    public required string Name { get; init; }
    /// <summary>파라미터 정의 목록.</summary>
    public required List<GoComponentParam> Params { get; init; }
    /// <summary>내부 컨트롤 트리 루트(들). 인스턴스마다 ReadElement로 복제.</summary>
    public required List<XElement> Roots { get; init; }

    /// <summary>전역 컴포넌트 레지스트리(이름→템플릿).</summary>
    public static class Registry
    {
        private static readonly Dictionary<string, GoComponentTemplate> _map = new(System.StringComparer.Ordinal);

        /// <summary>템플릿 등록(동명 시 마지막 승, Debug 경고).</summary>
        public static void Register(GoComponentTemplate t)
        {
            if (_map.ContainsKey(t.Name))
                Debug.WriteLine($"[GoComponent] duplicate component '{t.Name}' — replaced.");
            _map[t.Name] = t;
        }

        /// <summary>이름으로 조회.</summary>
        public static bool TryGet(string name, out GoComponentTemplate template)
            => _map.TryGetValue(name, out template!);

        /// <summary>레지스트리 비우기(디자인 재로드/테스트).</summary>
        public static void Clear() => _map.Clear();
    }
}
