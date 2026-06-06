using System;
using System.Collections.Generic;
using Going.UI.Containers;
using Going.UI.Gudx;

namespace Going.UI.Bindings;

/// <summary>컴포넌트 인스턴스의 파라미터 스코프. 내부 바인딩의 root 역할.</summary>
public sealed class GoComponentScope
{
    /// <summary>파라미터 이름→값(리터럴 파싱값 또는 바깥 바인딩 객체 참조).</summary>
    public Dictionary<string, object?> Values { get; } = new(StringComparer.Ordinal);
    /// <summary>파라미터 이름→선언 타입(내부 경로 tail 컴파일용).</summary>
    public Dictionary<string, Type> Types { get; } = new(StringComparer.Ordinal);

    /// <summary>인스턴스의 파라미터를 outerRoot 기준으로 평가해 스코프를 만든다.</summary>
    public static GoComponentScope Build(GoComponentInstance ci, object outerRoot)
    {
        var scope = new GoComponentScope();
        var prms = ci.Template?.Params ?? new List<GoComponentParam>();
        foreach (var p in prms)
        {
            var type = p.ResolvedType ?? typeof(object);
            scope.Types[p.Name] = type;

            ci.ParamValues.TryGetValue(p.Name, out var raw);
            raw ??= p.Default;
            if (raw == null) { scope.Values[p.Name] = null; continue; }

            if (GudxBindingExpression.IsBinding(raw))
                scope.Values[p.Name] = GudxBindingExpression.EvalExpr(raw, outerRoot);   // 바깥 평가(스냅샷)
            else
                scope.Values[p.Name] = GudxBindingExpression.ParseLiteral(raw, type);
        }
        return scope;
    }
}
