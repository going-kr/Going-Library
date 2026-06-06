using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Going.UI.Controls;

namespace Going.UI.Bindings;

/// <summary>
/// gudx 마크업 attribute의 바인딩 식(`{path}` / `{path:fmt}`)을 파싱하고,
/// 경로를 root 객체 기준 델리게이트로 컴파일해 GoControl 바인딩에 연결한다.
/// 런타임 동기화는 기존 펌프(<see cref="GoControl.FireUpdate"/>)가 담당한다.
/// </summary>
public static class GudxBindingExpression
{
    #region Parse
    /// <summary>값이 `{...}` 바인딩 식인지 판정. `{{` escape와 미완성 식은 제외.</summary>
    public static bool IsBinding(string? value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        if (value!.Length < 2) return false;
        if (value[0] != '{' || value[^1] != '}') return false;
        if (value.StartsWith("{{", StringComparison.Ordinal)) return false;  // escape
        return true;
    }

    /// <summary>`{path:fmt}` → (path, fmt). fmt 없으면 (path, null). 양끝 공백 trim.</summary>
    public static (string Path, string? Format) Parse(string value)
    {
        var inner = value.Substring(1, value.Length - 2);   // strip { }
        int colon = inner.IndexOf(':');
        if (colon < 0) return (inner.Trim(), null);
        var path = inner.Substring(0, colon).Trim();
        var fmt = inner.Substring(colon + 1).Trim();
        return (path, fmt.Length == 0 ? null : fmt);
    }
    #endregion

    #region Compile path
    /// <summary>컴파일된 경로 접근자. getter는 항상, setter는 말단이 settable일 때만.</summary>
    public sealed class CompiledPath
    {
        /// <summary>root 객체에서 경로 말단 값을 읽는 컴파일된 getter.</summary>
        public required Func<object, object?> Getter { get; init; }
        /// <summary>말단이 settable일 때만 존재하는 컴파일된 setter. 아니면 null(단방향).</summary>
        public Action<object, object?>? Setter { get; init; }
        /// <summary>경로 말단 property의 타입.</summary>
        public required Type LeafType { get; init; }
    }

    private static readonly ConcurrentDictionary<(Type, string), CompiledPath> _cache = new();

    /// <summary>root 타입 기준 "A.B.C" 경로를 컴파일. 멤버 없거나 property 아니면 ArgumentException.</summary>
    public static CompiledPath CompilePath(Type rootType, string path)
        => _cache.GetOrAdd((rootType, path), key => BuildPath(key.Item1, key.Item2));

    private static CompiledPath BuildPath(Type rootType, string path)
    {
        var segments = path.Split('.');
        var rootParam = Expression.Parameter(typeof(object), "r");
        Expression expr = Expression.Convert(rootParam, rootType);

        PropertyInfo? leaf = null;
        Expression? parentExpr = null;
        foreach (var seg in segments)
        {
            var pi = expr.Type.GetProperty(seg)
                ?? throw new ArgumentException($"Binding path '{path}': member '{seg}' not found on {expr.Type.Name}.");
            parentExpr = expr;
            leaf = pi;
            expr = Expression.Property(expr, pi);
        }
        if (leaf == null || parentExpr == null)
            throw new ArgumentException($"Binding path '{path}' is empty.");

        var getter = Expression.Lambda<Func<object, object?>>(
            Expression.Convert(expr, typeof(object)), rootParam).Compile();

        Action<object, object?>? setter = null;
        if (leaf.CanWrite && leaf.SetMethod != null)
        {
            var valueParam = Expression.Parameter(typeof(object), "v");
            var assign = Expression.Assign(
                Expression.Property(parentExpr, leaf),
                Expression.Convert(valueParam, leaf.PropertyType));
            setter = Expression.Lambda<Action<object, object?>>(assign, rootParam, valueParam).Compile();
        }

        return new CompiledPath { Getter = getter, Setter = setter, LeafType = leaf.PropertyType };
    }
    #endregion

    #region Wire
    /// <summary>
    /// 컨트롤 속성 propName을 root 기준 식 expr(`{path}` / `{path:fmt}`)에 바인딩한다.
    /// 경로 말단이 settable이고 포맷이 없으면 양방향, 아니면 단방향.
    /// 해석 실패 시 ArgumentException — 호출자(<see cref="GudxBinder"/>)가 잡아 로그 후 스킵.
    /// </summary>
    public static void Wire(GoControl ctrl, string propName, string expr, object root)
    {
        var pi = ctrl.GetType().GetProperty(propName)
            ?? throw new ArgumentException($"Binding target '{propName}' not found on {ctrl.GetType().Name}.");
        if (!pi.CanWrite)
            throw new ArgumentException($"Binding target '{propName}' on {ctrl.GetType().Name} is read-only.");

        var (path, format) = Parse(expr);
        var cp = CompilePath(root.GetType(), path);
        var targetType = pi.PropertyType;

        // source getter: 포맷 있으면 문자열, 없으면 대상 타입으로 강제
        Func<object?> srcGet;
        if (format != null)
            srcGet = () => FormatValue(cp.Getter(root), format);
        else
            srcGet = () => Coerce(cp.Getter(root), targetType);

        // source setter: 포맷 없고 말단 settable일 때만 양방향
        Action<object?>? srcSet = null;
        if (format == null && cp.Setter != null)
            srcSet = v => cp.Setter!(root, v);

        var (ctrlGet, ctrlSet) = BuildCtrlAccessors(ctrl.GetType(), pi);

        var binding = new GoBinding
        {
            CtrlProperty = pi,
            CtrlGet = ctrlGet,
            CtrlSet = ctrlSet,
            SourceGet = srcGet,
            SourceSet = srcSet,
        };
        ctrl.AddOrReplaceBinding(binding);
    }

    private static (Func<GoControl, object?>, Action<GoControl, object?>) BuildCtrlAccessors(Type ctrlType, PropertyInfo pi)
    {
        var c = Expression.Parameter(typeof(GoControl), "c");
        var v = Expression.Parameter(typeof(object), "v");
        var prop = Expression.Property(Expression.Convert(c, ctrlType), pi);
        var get = Expression.Lambda<Func<GoControl, object?>>(Expression.Convert(prop, typeof(object)), c).Compile();
        var set = Expression.Lambda<Action<GoControl, object?>>(
            Expression.Assign(prop, Expression.Convert(v, pi.PropertyType)), c, v).Compile();
        return (get, set);
    }

    private static object? FormatValue(object? value, string format)
    {
        if (value == null) return "";
        if (value is IFormattable f) return f.ToString(format, CultureInfo.CurrentCulture);
        return value.ToString();
    }

    private static object? Coerce(object? value, Type targetType)
    {
        if (value == null) return null;
        if (targetType.IsInstanceOfType(value)) return value;
        if (targetType == typeof(string)) return value.ToString();
        var under = Nullable.GetUnderlyingType(targetType) ?? targetType;
        try { return Convert.ChangeType(value, under, CultureInfo.CurrentCulture); }
        catch { return value; }   // 펌프의 set이 실패하면 per-binding catch가 처리
    }
    #endregion
}
