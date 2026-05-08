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

    [Fact]
    public void TwoWayBind_PropagatesControlToSource_WhenControlChanges()
    {
        var lamp = new GoLamp();
        bool src = false;

        lamp.Bind(c => c.OnOff, () => src, v => src = v);

        // 첫 FireUpdate: 소스(false)가 컨트롤에 반영
        lamp.FireUpdate();
        Assert.False(lamp.OnOff);

        // 코드에서 컨트롤 속성 직접 변경
        lamp.OnOff = true;
        lamp.FireUpdate();
        Assert.True(src);

        lamp.OnOff = false;
        lamp.FireUpdate();
        Assert.False(src);
    }

    [Fact]
    public void TwoWayBind_DoesNotLoopOnSourceUpdate()
    {
        var lamp = new GoLamp();
        bool src = false;
        int setterCalls = 0;

        lamp.Bind(c => c.OnOff, () => src, v => { setterCalls++; src = v; });

        src = true;
        lamp.FireUpdate();   // 소스 → 컨트롤 (setter 호출되면 안 됨)
        Assert.True(lamp.OnOff);
        Assert.Equal(0, setterCalls);

        lamp.FireUpdate();   // 변화 없음
        Assert.Equal(0, setterCalls);
    }

    private sealed class SuppressibleLamp : GoLamp
    {
        public bool Suppress;
        protected internal override bool IsBindingSuppressed => Suppress;
    }

    [Fact]
    public void Suppressed_BlocksSourceToControl()
    {
        var lamp = new SuppressibleLamp();
        bool src = false;

        lamp.Bind(c => c.OnOff, () => src);
        lamp.FireUpdate();              // 초기 동기화 (false)
        Assert.False(lamp.OnOff);

        lamp.Suppress = true;
        src = true;
        lamp.FireUpdate();
        Assert.False(lamp.OnOff);       // 정지 — 반영 안 됨

        lamp.Suppress = false;
        lamp.FireUpdate();
        Assert.True(lamp.OnOff);        // 해제 후 반영
    }

    [Fact]
    public void Suppressed_BlocksControlToSource_AndFlushesOnRelease()
    {
        var lamp = new SuppressibleLamp();
        bool src = false;
        int setterCalls = 0;

        lamp.Bind(c => c.OnOff, () => src, v => { setterCalls++; src = v; });
        lamp.FireUpdate();              // 초기

        lamp.Suppress = true;
        lamp.OnOff = true;              // 조작 중 컨트롤 변경
        lamp.FireUpdate();
        Assert.False(src);              // setter 호출 안 됨
        Assert.Equal(0, setterCalls);

        lamp.FireUpdate();              // 조작 중 한 번 더 — 여전히 호출 없음
        Assert.Equal(0, setterCalls);

        lamp.Suppress = false;
        lamp.FireUpdate();              // 해제 — flush 1회
        Assert.True(src);
        Assert.Equal(1, setterCalls);

        lamp.FireUpdate();              // 추가 호출 없음
        Assert.Equal(1, setterCalls);
    }

    [Fact]
    public void Suppressed_BeforeFirstInit_DoesNotClobberSourceOnRelease()
    {
        var lamp = new SuppressibleLamp();
        bool src = true;                        // 소스는 true
        int setterCalls = 0;

        lamp.Suppress = true;                   // 첫 FireUpdate 전에 이미 suppressed
        lamp.Bind(c => c.OnOff, () => src, v => { setterCalls++; src = v; });

        lamp.FireUpdate();                      // suppressed → 아직 초기화 안 됐으니 PendingFlush 표시도 안 됨
        Assert.True(src);                       // 소스 변하지 않음
        Assert.Equal(0, setterCalls);

        lamp.Suppress = false;
        lamp.FireUpdate();                      // 정상 흐름 — 소스(true) → 컨트롤(true)
        Assert.True(lamp.OnOff);
        Assert.True(src);                       // 소스 그대로
        Assert.Equal(0, setterCalls);           // setter 절대 호출되면 안 됨
    }

    [Fact]
    public void Unbind_RemovesBindingForGivenProperty()
    {
        var lamp = new GoLamp();
        bool src = true;

        lamp.Bind(c => c.OnOff, () => src);
        lamp.FireUpdate();
        Assert.True(lamp.OnOff);

        lamp.Unbind(c => c.OnOff);
        src = false;
        lamp.FireUpdate();
        Assert.True(lamp.OnOff);  // 해제됐으므로 변화 없음
    }

    [Fact]
    public void UnbindAll_ClearsAllBindings()
    {
        var lamp = new GoLamp();
        bool a = false; bool b = false;

        lamp.Bind(c => c.OnOff,   () => a);
        lamp.Bind(c => c.Visible, () => b);

        lamp.UnbindAll();
        a = true; b = false;
        var beforeOnOff = lamp.OnOff;
        var beforeVisible = lamp.Visible;
        lamp.FireUpdate();
        Assert.Equal(beforeOnOff, lamp.OnOff);
        Assert.Equal(beforeVisible, lamp.Visible);
    }

    [Fact]
    public void RebindSameProperty_ReplacesPreviousBinding()
    {
        var lamp = new GoLamp();
        bool a = true;
        bool b = false;

        lamp.Bind(c => c.OnOff, () => a);
        lamp.Bind(c => c.OnOff, () => b);   // 교체

        lamp.FireUpdate();
        Assert.False(lamp.OnOff);   // b가 이김

        a = false; b = true;
        lamp.FireUpdate();
        Assert.True(lamp.OnOff);
    }

    [Fact]
    public void Dispose_ClearsBindings()
    {
        var lamp = new GoLamp();
        bool src = false;

        lamp.Bind(c => c.OnOff, () => src);
        lamp.Dispose();

        // 직접 list 접근 대신 동작으로 검증
        src = true;
        lamp.FireUpdate();
        Assert.False(lamp.OnOff);
    }

    [Fact]
    public void GetterException_DoesNotCrashPump_AndDoesNotMutateControl()
    {
        var lamp = new GoLamp();
        bool reachedSecond = false;

        lamp.Bind(c => c.OnOff,   () => throw new InvalidOperationException("boom"));
        lamp.Bind(c => c.Visible, () => { reachedSecond = true; return false; });

        var before = lamp.OnOff;
        lamp.FireUpdate();   // 첫 binding 예외 → 두 번째 binding은 정상 처리되어야 함

        Assert.Equal(before, lamp.OnOff);
        Assert.True(reachedSecond);
    }

    [Fact]
    public void SetterException_DoesNotCrashPump()
    {
        var lamp = new GoLamp();
        bool src = false;

        lamp.Bind(c => c.OnOff, () => src,
                                v => throw new InvalidOperationException("setter boom"));
        lamp.FireUpdate();   // 초기 push만, setter는 호출 안 됨 (loop 방지)

        lamp.OnOff = true;   // 코드 변경 → 다음 frame에 setter 호출 시도 → 예외 → 무시
        var ex = Record.Exception(() => lamp.FireUpdate());
        Assert.Null(ex);
    }

    [Fact]
    public void GoSlider_IsBindingSuppressed_FalseWhenIdle()
    {
        var s = new Going.UI.Controls.GoSlider();
        var p = typeof(GoControl).GetProperty("IsBindingSuppressed",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(p);
        Assert.False((bool)p!.GetValue(s)!);
    }

    [Fact]
    public void GoSlider_HasIsBindingSuppressedOverride()
    {
        var t = typeof(Going.UI.Controls.GoSlider);
        var p = t.GetProperty("IsBindingSuppressed",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(p);
        Assert.NotEqual(typeof(GoControl), p!.DeclaringType);
    }

    [Fact]
    public void GoRangeSlider_HasIsBindingSuppressedOverride()
    {
        var t = typeof(Going.UI.Controls.GoRangeSlider);
        var p = t.GetProperty("IsBindingSuppressed",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(p);
        Assert.NotEqual(typeof(GoControl), p!.DeclaringType);
    }

    [Fact]
    public void GoKnob_HasIsBindingSuppressedOverride()
    {
        var t = typeof(Going.UI.Controls.GoKnob);
        var p = t.GetProperty("IsBindingSuppressed",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(p);
        Assert.NotEqual(typeof(GoControl), p!.DeclaringType);
    }

    [Fact]
    public void Bind_OnDisposedControl_DoesNotAffectControl()
    {
        var lamp = new GoLamp();
        lamp.Dispose();

        bool src = true;
        lamp.Bind(c => c.OnOff, () => src);
        lamp.FireUpdate();

        Assert.False(lamp.OnOff);  // disposed → bind 무효, OnOff 변경 안 됨
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        var lamp = new GoLamp();
        bool src = true;
        lamp.Bind(c => c.OnOff, () => src);

        lamp.Dispose();
        var ex = Record.Exception(() => lamp.Dispose());
        Assert.Null(ex);  // 두 번째 호출도 예외 없이
    }
}
