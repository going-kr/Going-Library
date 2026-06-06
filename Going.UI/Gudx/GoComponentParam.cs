using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Going.UI.Gudx;

/// <summary>컴포넌트 파라미터 정의(<c>&lt;Param name type default&gt;</c>).</summary>
public sealed class GoComponentParam
{
    /// <summary>파라미터 이름(스코프 키, 내부 경로 첫 구간).</summary>
    public required string Name { get; init; }
    /// <summary>선언 타입 이름(별칭 또는 CLR simple name).</summary>
    public required string TypeName { get; init; }
    /// <summary>사용처 생략 시 기본값(없으면 null).</summary>
    public string? Default { get; init; }
    /// <summary>해석된 CLR 타입(실패 시 null).</summary>
    public Type? ResolvedType => ResolveType(TypeName);

    private static readonly ConcurrentDictionary<string, Type?> _cache = new();

    /// <summary>타입 이름을 CLR 타입으로 해석. 별칭 우선, 그다음 로드된 어셈블리 simple-name 스캔.</summary>
    public static Type? ResolveType(string typeName)
        => _cache.GetOrAdd(typeName, Resolve);

    private static Type? Resolve(string n) => n switch
    {
        "string" => typeof(string),
        "int" => typeof(int),
        "long" => typeof(long),
        "short" => typeof(short),
        "byte" => typeof(byte),
        "float" => typeof(float),
        "double" => typeof(double),
        "bool" => typeof(bool),
        _ => ScanAssemblies(n),
    };

    private static Type? ScanAssemblies(string simpleName)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try { types = asm.GetTypes(); }
            catch { continue; }   // dynamic/reflection-only 어셈블리 방어
            var hit = types.FirstOrDefault(t => t.Name == simpleName);
            if (hit != null) return hit;
        }
        return null;
    }
}
