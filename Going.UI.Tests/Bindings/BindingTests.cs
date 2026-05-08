using System;
using System.Linq.Expressions;
using Going.UI.Bindings;
using Going.UI.Controls;
using Xunit;

namespace Going.UI.Tests.Bindings;

public class BindingTests
{
    [Fact]
    public void ExtractProperty_FromMemberAccess_ReturnsPropertyInfo()
    {
        Expression<Func<GoLamp, bool>> e = c => c.OnOff;
        var pi = GoControlBindingExtensions.ExtractProperty(e);
        Assert.Equal(nameof(GoLamp.OnOff), pi.Name);
        Assert.Equal(typeof(bool), pi.PropertyType);
    }

    [Fact]
    public void ExtractProperty_FromNonMember_Throws()
    {
        Expression<Func<GoLamp, bool>> e = c => !c.OnOff;
        Assert.Throws<ArgumentException>(() =>
            GoControlBindingExtensions.ExtractProperty(e));
    }

    [Fact]
    public void ExtractProperty_FromHandBuiltPropertyExpression_ReturnsPropertyInfo()
    {
        // GoControl에 public 필드 없음 — non-property MemberExpression 검증을 위해
        // Expression을 직접 빌드
        var p = Expression.Parameter(typeof(string), "s");
        var lengthField = Expression.Property(p, nameof(string.Length));
        var lambda = Expression.Lambda<Func<string, int>>(lengthField, p);
        // string.Length는 property라 통과해야 함 (음성 케이스 아님)
        var pi = GoControlBindingExtensions.ExtractProperty(lambda);
        Assert.Equal(nameof(string.Length), pi.Name);
    }

    private sealed class FieldHolder
    {
        public int Value;
    }

    [Fact]
    public void ExtractProperty_FromFieldExpression_Throws()
    {
        var p = Expression.Parameter(typeof(FieldHolder), "h");
        var fieldAccess = Expression.Field(p, nameof(FieldHolder.Value));
        var lambda = Expression.Lambda<Func<FieldHolder, int>>(fieldAccess, p);
        Assert.Throws<ArgumentException>(() =>
            GoControlBindingExtensions.ExtractProperty(lambda));
    }
}
