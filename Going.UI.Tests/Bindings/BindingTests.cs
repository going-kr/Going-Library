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
        public int Value = 0;
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

    [Fact]
    public void OneWayBind_PropagatesSourceToControl_OnFireUpdate()
    {
        var lamp = new GoLamp();
        bool source = false;

        lamp.Bind(c => c.OnOff, () => source);

        // 첫 FireUpdate에 초기 동기화
        lamp.FireUpdate();
        Assert.False(lamp.OnOff);

        source = true;
        lamp.FireUpdate();
        Assert.True(lamp.OnOff);

        source = false;
        lamp.FireUpdate();
        Assert.False(lamp.OnOff);
    }

    [Fact]
    public void OneWayBind_SkipsControlSet_WhenSourceUnchanged()
    {
        var lamp = new TrackingLamp();
        int src = 24;

        lamp.Bind(c => c.LampSize, () => src);

        lamp.FireUpdate();
        int afterFirst = lamp.LampSizeSetCount;
        Assert.True(afterFirst >= 1);  // 첫 푸시 발생

        lamp.FireUpdate();
        lamp.FireUpdate();
        Assert.Equal(afterFirst, lamp.LampSizeSetCount);  // 동일값 — 재대입 없음

        src = 30;
        lamp.FireUpdate();
        Assert.Equal(afterFirst + 1, lamp.LampSizeSetCount);  // 변경 시 한 번
    }

    private sealed class TrackingLamp : GoLamp
    {
        public int LampSizeSetCount;
        private int _lampSize = 24;
        public new int LampSize
        {
            get => _lampSize;
            set { _lampSize = value; LampSizeSetCount++; }
        }
    }
}
