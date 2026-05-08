using System;
using System.Collections.Generic;
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

    /// <summary>
    /// 컨트롤 속성을 소스 식에 단방향 바인딩한다.
    /// 매 FireUpdate에 소스 → 컨트롤로 값이 흐른다.
    /// </summary>
    public static void Bind<TC, TV>(
        this TC ctrl,
        Expression<Func<TC, TV>> ctrlProp,
        Func<TV> getter)
        where TC : GoControl
    {
        BindInternal(ctrl, ctrlProp, getter, setter: null);
    }

    /// <summary>
    /// 컨트롤 속성을 소스 식에 양방향 바인딩한다.
    /// 소스 → 컨트롤은 매 FireUpdate, 컨트롤 → 소스는 컨트롤 값 변경 감지 시.
    /// </summary>
    public static void Bind<TC, TV>(
        this TC ctrl,
        Expression<Func<TC, TV>> ctrlProp,
        Func<TV> getter,
        Action<TV> setter)
        where TC : GoControl
    {
        BindInternal(ctrl, ctrlProp, getter, setter);
    }

    private static void BindInternal<TC, TV>(
        TC ctrl,
        Expression<Func<TC, TV>> ctrlProp,
        Func<TV> getter,
        Action<TV>? setter)
        where TC : GoControl
    {
        var pi = ExtractProperty(ctrlProp);

        // 컴파일된 get/set 델리게이트 (object 박싱 경로)
        var ctrlParam = Expression.Parameter(typeof(GoControl), "c");
        var valueParam = Expression.Parameter(typeof(object), "v");
        var castCtrl = Expression.Convert(ctrlParam, typeof(TC));
        var propAccess = Expression.Property(castCtrl, pi);

        var getterLambda = Expression.Lambda<Func<GoControl, object?>>(
            Expression.Convert(propAccess, typeof(object)), ctrlParam);
        var setterLambda = Expression.Lambda<Action<GoControl, object?>>(
            Expression.Assign(propAccess, Expression.Convert(valueParam, typeof(TV))),
            ctrlParam, valueParam);

        var compiledGet = getterLambda.Compile();
        var compiledSet = setterLambda.Compile();

        var binding = new GoBinding
        {
            CtrlProperty = pi,
            CtrlGet = compiledGet,
            CtrlSet = compiledSet,
            SourceGet = () => (object?)getter(),
            SourceSet = setter == null ? null : (Action<object?>)(v => setter((TV)v!)),
        };

        // 같은 속성에 기존 binding 있으면 교체 (마지막 승)
        ctrl.AddOrReplaceBinding(binding);
    }

    /// <summary>
    /// 지정한 속성에 대한 binding을 해제한다.
    /// </summary>
    public static void Unbind<TC, TV>(
        this TC ctrl,
        Expression<Func<TC, TV>> ctrlProp)
        where TC : GoControl
    {
        var pi = ExtractProperty(ctrlProp);
        ctrl.RemoveBindingByProperty(pi);
    }

    /// <summary>
    /// 컨트롤의 모든 binding을 해제한다.
    /// </summary>
    public static void UnbindAll(this GoControl ctrl)
    {
        ctrl.ClearBindings();
    }
}
