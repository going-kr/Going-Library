using System;
using System.Linq.Expressions;
using System.Reflection;
using Going.UI.Controls;

namespace Going.UI.Bindings;

/// <summary>
/// GoControl 바인딩 확장 메서드.
/// </summary>
public static class GoControlBindingExtensions
{
    /// <summary>
    /// Expression이 단순 멤버 접근(c => c.Prop)인 경우 PropertyInfo를 추출한다.
    /// 그렇지 않으면 ArgumentException.
    /// </summary>
    internal static PropertyInfo ExtractProperty<TC, TV>(Expression<Func<TC, TV>> expr)
    {
        if (expr.Body is not MemberExpression me)
            throw new ArgumentException(
                "Bind expression must be a simple property access (e.g., c => c.Value).",
                nameof(expr));

        if (me.Member is not PropertyInfo pi)
            throw new ArgumentException(
                "Bind expression must target a property, not a field or method.",
                nameof(expr));

        return pi;
    }
}
