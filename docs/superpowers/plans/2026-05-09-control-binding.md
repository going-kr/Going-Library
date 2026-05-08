# Going.UI 컨트롤 바인딩 구현 플랜

> **For agentic workers:** REQUIRED SUB-SKILL: superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** GoControl 확장 메서드(`Bind`)로 컨트롤 속성을 임의의 식에 묶어 매 프레임 자동 동기화한다. 단/양방향 모두 지원, 기존 컨트롤 거의 무수정.

**Architecture:** 대칭 폴링. 매 `FireUpdate()` 직전에 binding 펌프가 양쪽 값(소스/컨트롤)을 읽고 변화 감지 시 한쪽으로 반영. Expression-tree로 컨트롤 속성의 `PropertyInfo`만 추출해 컴파일된 델리게이트로 캐시 — 호출 시점 reflection 비용 0. 입력 조작 중 폭주는 `IsBindingSuppressed` 가상 플래그로 정지하고 종료 시 한 번만 flush.

**Tech Stack:** C# 12 (Expression Trees, `LambdaExpression.Compile`), .NET 8, xUnit 2.5

**Spec:** `docs/superpowers/specs/2026-05-09-control-binding-design.md`

---

## File Structure

**New files:**
- `Going.UI/Bindings/GoBinding.cs` — 내부 binding 인스턴스 (델리게이트 + 상태)
- `Going.UI/Bindings/GoControlBindingExtensions.cs` — `Bind`/`Unbind`/`UnbindAll` 확장 메서드 + Expression 분석
- `Going.UI.Tests/Bindings/BindingTests.cs` — xUnit 테스트

**Modified:**
- `Going.UI/Controls/GoControl.cs` — `internal List<GoBinding>? bindings` 필드, `IsBindingSuppressed` 가상, `FireUpdate()`에 펌프 훅, `Dispose()`에 `UnbindAll()`
- `Going.UI/Controls/GoSlider.cs` — `IsBindingSuppressed => isDragging` override
- `Going.UI/Controls/GoRangeSlider.cs` — `IsBindingSuppressed => isDraggingLower || isDraggingUpper` override
- `Going.UI/Controls/GoKnob.cs` — `IsBindingSuppressed => bDown` override

각 task는 TDD로 진행: 실패 테스트 → 구현 → 통과 → 커밋.

테스트 명령(전체):
```
dotnet test Going.UI.Tests/Going.UI.Tests.csproj
```

테스트 명령(단일):
```
dotnet test Going.UI.Tests/Going.UI.Tests.csproj --filter "FullyQualifiedName~BindingTests.<TestName>"
```

---

## Task 1: GoBinding 내부 타입 + Expression → PropertyInfo 추출

**Files:**
- Create: `Going.UI/Bindings/GoBinding.cs`
- Create: `Going.UI/Bindings/GoControlBindingExtensions.cs` (분석 헬퍼만 우선)
- Create: `Going.UI.Tests/Bindings/BindingTests.cs`

목표: `c => c.Value` 같은 Expression에서 `PropertyInfo`를 꺼내고, 컨트롤 속성의 get/set 컴파일된 델리게이트를 만드는 헬퍼를 검증.

- [ ] **Step 1: 실패 테스트 작성**

`Going.UI.Tests/Bindings/BindingTests.cs`:

```csharp
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
    public void ExtractProperty_FromField_Throws()
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
}
```

- [ ] **Step 2: 테스트 실행 — 컴파일 실패 확인**

Run: `dotnet test Going.UI.Tests/Going.UI.Tests.csproj --filter "FullyQualifiedName~BindingTests"`
Expected: 컴파일 에러 — `GoControlBindingExtensions` 또는 `Going.UI.Bindings` 네임스페이스 없음.

- [ ] **Step 3: GoBinding.cs 작성**

`Going.UI/Bindings/GoBinding.cs`:

```csharp
using System;
using System.Reflection;
using Going.UI.Controls;

namespace Going.UI.Bindings;

/// <summary>
/// 단일 binding 인스턴스. 한 컨트롤 속성과 한 소스 식 간의 동기화 상태를 보관한다.
/// </summary>
internal sealed class GoBinding
{
    public required PropertyInfo CtrlProperty { get; init; }
    public required Func<GoControl, object?> CtrlGet { get; init; }
    public required Action<GoControl, object?> CtrlSet { get; init; }
    public required Func<object?> SourceGet { get; init; }
    public Action<object?>? SourceSet { get; init; }

    public object? LastSrcValue;
    public object? LastCtrlValue;
    public bool PendingFlush;
    public bool Initialized;
}
```

- [ ] **Step 4: GoControlBindingExtensions.cs (헬퍼만) 작성**

`Going.UI/Bindings/GoControlBindingExtensions.cs`:

```csharp
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
```

- [ ] **Step 5: 테스트 실행 — 통과 확인**

Run: `dotnet test Going.UI.Tests/Going.UI.Tests.csproj --filter "FullyQualifiedName~BindingTests"`
Expected: 3 tests pass.

- [ ] **Step 6: 커밋**

```bash
git add Going.UI/Bindings/GoBinding.cs Going.UI/Bindings/GoControlBindingExtensions.cs Going.UI.Tests/Bindings/BindingTests.cs
git commit -m "Bindings: GoBinding 타입 + Expression PropertyInfo 추출 헬퍼"
```

---

## Task 2: 단방향 Bind — 소스→컨트롤 폴링

**Files:**
- Modify: `Going.UI/Bindings/GoControlBindingExtensions.cs`
- Modify: `Going.UI/Controls/GoControl.cs:317` (`FireUpdate`), `:459` (`Dispose`)
- Modify: `Going.UI.Tests/Bindings/BindingTests.cs`

목표: `Bind(c => c.X, () => src)` 호출 후 `FireUpdate()`마다 소스 값이 컨트롤에 반영되고, 동일값일 땐 재대입 스킵.

- [ ] **Step 1: 실패 테스트 추가**

`Going.UI.Tests/Bindings/BindingTests.cs`에 추가:

```csharp
[Fact]
public void OneWayBind_PropagatesSourceToControl_OnFireUpdate()
{
    var lamp = new GoLamp();
    bool source = false;

    lamp.Bind(c => c.OnOff, () => source);

    // 초기 한 번 동기화: Bind 직후 또는 첫 FireUpdate
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
```

> 주: `TrackingLamp.LampSize`가 `GoLamp.LampSize`를 `new`로 가린다. Expression `c => c.LampSize`에서 `c`는 `TC = TrackingLamp`로 추론되므로 `TrackingLamp.LampSize`(추적 버전)에 바인딩된다.

- [ ] **Step 2: 테스트 실행 — 컴파일 또는 런타임 실패 확인**

Run: `dotnet test Going.UI.Tests/Going.UI.Tests.csproj --filter "FullyQualifiedName~BindingTests"`
Expected: 컴파일 실패 — `Bind` 확장 메서드 미정의.

- [ ] **Step 3: GoControl에 binding 저장소 + 펌프 훅 추가**

`Going.UI/Controls/GoControl.cs`:

3a. `Member Variable` 영역(line ~205) 끝에 추가:
```csharp
        internal List<Going.UI.Bindings.GoBinding>? bindings;
```

3b. `FireUpdate` (line 317) 변경:
```csharp
        public void FireUpdate()
        {
            if (bindings != null) PumpBindings();
            OnUpdate();
        }
```

3c. `Dispose` (line 459) 변경:
```csharp
        public void Dispose()
        {
            this.UnbindAll();
            OnDispose();
        }
```

3d. 같은 클래스 안에 펌프 메서드 추가 (예: `Method/virtual` 영역 위 또는 `#region Method` 시작 직전):
```csharp
        private void PumpBindings()
        {
            // Going.UI.Bindings.GoBinding[] snapshot 형태로 안전 순회
            var list = bindings;
            if (list == null) return;
            for (int i = 0; i < list.Count; i++)
            {
                var b = list[i];

                object? newSrc;
                try { newSrc = b.SourceGet(); }
                catch { continue; }  // 한 binding의 getter 예외는 다음 task에서 다룸

                if (!b.Initialized || !object.Equals(newSrc, b.LastSrcValue))
                {
                    try
                    {
                        b.CtrlSet(this, newSrc);
                        b.LastSrcValue = newSrc;
                        b.LastCtrlValue = newSrc;  // 자기 트리거 방지
                        b.Initialized = true;
                    }
                    catch
                    {
                        // 컨트롤 set 실패는 다음 task에서 로깅
                    }
                }
            }
        }
```

- [ ] **Step 4: Bind 확장 메서드(단방향) 추가**

`Going.UI/Bindings/GoControlBindingExtensions.cs`에 추가:

```csharp
using System.Collections.Generic;

// 클래스 안에 추가:

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
        BindInternal(ctrl, ctrlProp, getter, sourceSet: null);
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

        ctrl.bindings ??= new List<GoBinding>();

        // 같은 속성에 기존 binding 있으면 교체 (마지막 승)
        for (int i = 0; i < ctrl.bindings.Count; i++)
        {
            if (ctrl.bindings[i].CtrlProperty == pi)
            {
                ctrl.bindings[i] = binding;
                return;
            }
        }
        ctrl.bindings.Add(binding);
    }

    /// <summary>
    /// 컨트롤의 모든 binding을 해제한다.
    /// </summary>
    public static void UnbindAll(this GoControl ctrl)
    {
        ctrl.bindings?.Clear();
    }
```

- [ ] **Step 5: 테스트 실행 — 통과 확인**

Run: `dotnet test Going.UI.Tests/Going.UI.Tests.csproj --filter "FullyQualifiedName~BindingTests"`
Expected: 5 tests pass (Task 1의 3개 + Task 2의 2개).

- [ ] **Step 6: 커밋**

```bash
git add Going.UI/Bindings/GoControlBindingExtensions.cs Going.UI/Controls/GoControl.cs Going.UI.Tests/Bindings/BindingTests.cs
git commit -m "Bindings: 단방향 Bind + GoControl 펌프 훅 (FireUpdate/Dispose)"
```

---

## Task 3: 양방향 Bind — 컨트롤→소스 역방향 폴링

**Files:**
- Modify: `Going.UI/Bindings/GoControlBindingExtensions.cs`
- Modify: `Going.UI/Controls/GoControl.cs` (PumpBindings 확장)
- Modify: `Going.UI.Tests/Bindings/BindingTests.cs`

목표: 3-arg `Bind(...)` 오버로드 추가. 컨트롤 속성이 변경되면 setter 호출.

- [ ] **Step 1: 실패 테스트 추가**

```csharp
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
```

- [ ] **Step 2: 테스트 실행 — 컴파일 실패 확인**

Run: `dotnet test ... --filter "FullyQualifiedName~BindingTests"`
Expected: 3-arg Bind 오버로드 미정의로 컴파일 실패.

- [ ] **Step 3: 양방향 Bind 오버로드 추가**

`Going.UI/Bindings/GoControlBindingExtensions.cs`에 추가:

```csharp
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
```

- [ ] **Step 4: PumpBindings 메서드 교체 (역방향 분기 포함)**

`Going.UI/Controls/GoControl.cs`의 `PumpBindings` 메서드 전체를 다음 코드로 교체:

```csharp
        private void PumpBindings()
        {
            var list = bindings;
            if (list == null) return;
            for (int i = 0; i < list.Count; i++)
            {
                var b = list[i];

                // 소스 → 컨트롤
                object? newSrc;
                try { newSrc = b.SourceGet(); }
                catch { continue; }

                if (!b.Initialized || !object.Equals(newSrc, b.LastSrcValue))
                {
                    try
                    {
                        b.CtrlSet(this, newSrc);
                        b.LastSrcValue = newSrc;
                        b.LastCtrlValue = newSrc;
                        b.Initialized = true;
                    }
                    catch { }
                }

                // 컨트롤 → 소스
                if (b.SourceSet != null)
                {
                    object? cur;
                    try { cur = b.CtrlGet(this); }
                    catch { continue; }

                    if (!object.Equals(cur, b.LastCtrlValue))
                    {
                        try
                        {
                            b.SourceSet(cur);
                            b.LastCtrlValue = cur;
                            b.LastSrcValue = cur;  // setter가 소스 변경 → 다음 프레임 자기 트리거 방지
                        }
                        catch { }
                    }
                }
            }
        }
```

- [ ] **Step 5: 테스트 실행 — 통과 확인**

Run: `dotnet test ... --filter "FullyQualifiedName~BindingTests"`
Expected: 7 tests pass.

- [ ] **Step 6: 커밋**

```bash
git add Going.UI/Bindings/GoControlBindingExtensions.cs Going.UI/Controls/GoControl.cs Going.UI.Tests/Bindings/BindingTests.cs
git commit -m "Bindings: 양방향 Bind 오버로드 + 역방향 폴링"
```

---

## Task 4: IsBindingSuppressed 가상 플래그 + 정지/flush 동작

**Files:**
- Modify: `Going.UI/Controls/GoControl.cs` (가상 프로퍼티 추가, PumpBindings 확장)
- Modify: `Going.UI.Tests/Bindings/BindingTests.cs`

목표: 입력 컨트롤이 조작 중일 때 양방향 둘 다 정지, 조작 종료 후 한 번만 flush.

- [ ] **Step 1: 실패 테스트 추가**

`BindingTests.cs`에:

```csharp
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
```

- [ ] **Step 2: 테스트 실행 — 실패 확인**

Run: `dotnet test ... --filter "FullyQualifiedName~BindingTests.Suppressed"`
Expected: 컴파일 실패 — `IsBindingSuppressed` 미정의 (또는 통과 후 두 번째 테스트 실패).

- [ ] **Step 3: GoControl에 가상 프로퍼티 추가**

`Going.UI/Controls/GoControl.cs`의 `#region virtual` 영역(line ~208 근처, `OnInit` 위 또는 아래)에 추가:

```csharp
        /// <summary>
        /// 사용자가 컨트롤을 조작 중일 때 true를 반환하여 binding 펌프를 정지시킵니다.
        /// 슬라이더 드래그, 입력창 편집 등의 일시 정지 신호로 사용됩니다.
        /// </summary>
        protected internal virtual bool IsBindingSuppressed => false;
```

- [ ] **Step 4: PumpBindings 메서드 교체 (정지/flush 분기 포함)**

`Going.UI/Controls/GoControl.cs`의 `PumpBindings` 메서드 전체를 다음 코드로 교체:

```csharp
        private void PumpBindings()
        {
            var list = bindings;
            if (list == null) return;

            bool suppressed = IsBindingSuppressed;

            for (int i = 0; i < list.Count; i++)
            {
                var b = list[i];

                if (suppressed)
                {
                    // 조작 중 — 양방향 모두 정지, 종료 시 flush 위해 표시
                    if (b.SourceSet != null) b.PendingFlush = true;
                    continue;
                }

                // 조작 종료 직후 한 번 flush (컨트롤 → 소스)
                if (b.PendingFlush && b.SourceSet != null)
                {
                    object? cur;
                    try { cur = b.CtrlGet(this); }
                    catch { b.PendingFlush = false; continue; }

                    try
                    {
                        b.SourceSet(cur);
                        b.LastCtrlValue = cur;
                        b.LastSrcValue = cur;
                    }
                    catch { }
                    b.PendingFlush = false;
                    // flush 후엔 이번 프레임은 더 이상 처리 안 함 (다음 프레임에 일반 흐름)
                    continue;
                }

                // 일반 흐름 — 소스 → 컨트롤
                object? newSrc;
                try { newSrc = b.SourceGet(); }
                catch { continue; }

                if (!b.Initialized || !object.Equals(newSrc, b.LastSrcValue))
                {
                    try
                    {
                        b.CtrlSet(this, newSrc);
                        b.LastSrcValue = newSrc;
                        b.LastCtrlValue = newSrc;
                        b.Initialized = true;
                    }
                    catch { }
                }

                // 컨트롤 → 소스
                if (b.SourceSet != null)
                {
                    object? cur;
                    try { cur = b.CtrlGet(this); }
                    catch { continue; }

                    if (!object.Equals(cur, b.LastCtrlValue))
                    {
                        try
                        {
                            b.SourceSet(cur);
                            b.LastCtrlValue = cur;
                            b.LastSrcValue = cur;
                        }
                        catch { }
                    }
                }
            }
        }
```

- [ ] **Step 5: 테스트 실행 — 통과 확인**

Run: `dotnet test ... --filter "FullyQualifiedName~BindingTests"`
Expected: 9 tests pass.

- [ ] **Step 6: 커밋**

```bash
git add Going.UI/Controls/GoControl.cs Going.UI.Tests/Bindings/BindingTests.cs
git commit -m "Bindings: IsBindingSuppressed 플래그 + 조작 종료 시 1회 flush"
```

---

## Task 5: Unbind / 중복 Bind 교체 / Dispose 정리

**Files:**
- Modify: `Going.UI/Bindings/GoControlBindingExtensions.cs` (Unbind 메서드)
- Modify: `Going.UI.Tests/Bindings/BindingTests.cs`

목표: 명시적 해제 API + 같은 속성 재바인딩 시 이전 교체 + Dispose에서 자동 해제 검증.

- [ ] **Step 1: 실패 테스트 추가**

```csharp
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
```

- [ ] **Step 2: 테스트 실행 — 실패 확인**

Run: `dotnet test ... --filter "FullyQualifiedName~BindingTests"`
Expected: `Unbind` 미정의로 컴파일 실패. (`UnbindAll`/`Rebind`/`Dispose` 테스트는 이미 동작할 수도 — 그래도 OK)

- [ ] **Step 3: Unbind 추가**

`Going.UI/Bindings/GoControlBindingExtensions.cs`에 추가:

```csharp
    /// <summary>
    /// 지정한 속성에 대한 binding을 해제한다.
    /// </summary>
    public static void Unbind<TC, TV>(
        this TC ctrl,
        Expression<Func<TC, TV>> ctrlProp)
        where TC : GoControl
    {
        if (ctrl.bindings == null) return;
        var pi = ExtractProperty(ctrlProp);
        for (int i = 0; i < ctrl.bindings.Count; i++)
        {
            if (ctrl.bindings[i].CtrlProperty == pi)
            {
                ctrl.bindings.RemoveAt(i);
                return;
            }
        }
    }
```

(중복 Bind 교체와 Dispose 정리는 Task 2에서 이미 처리됨 — Bind는 같은 PropertyInfo 발견 시 교체, Dispose는 UnbindAll 호출.)

- [ ] **Step 4: 테스트 실행 — 통과 확인**

Run: `dotnet test ... --filter "FullyQualifiedName~BindingTests"`
Expected: 13 tests pass.

- [ ] **Step 5: 커밋**

```bash
git add Going.UI/Bindings/GoControlBindingExtensions.cs Going.UI.Tests/Bindings/BindingTests.cs
git commit -m "Bindings: Unbind 메서드 + 중복 교체/Dispose 회귀 테스트"
```

---

## Task 6: 에러 격리 검증

**Files:**
- Modify: `Going.UI.Tests/Bindings/BindingTests.cs`

목표: getter/setter 예외가 발생해도 다른 binding과 그리기 흐름이 막히지 않음을 명시 검증. (구현은 Task 2-4에서 이미 try/catch로 처리되어 있음 — 회귀 방지용 테스트.)

- [ ] **Step 1: 테스트 추가**

```csharp
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
```

- [ ] **Step 2: 테스트 실행 — 통과 확인**

Run: `dotnet test ... --filter "FullyQualifiedName~BindingTests"`
Expected: 15 tests pass.

- [ ] **Step 3: 커밋**

```bash
git add Going.UI.Tests/Bindings/BindingTests.cs
git commit -m "Bindings: getter/setter 예외 격리 회귀 테스트"
```

---

## Task 7: 입력 컨트롤에 IsBindingSuppressed override

**Files:**
- Modify: `Going.UI/Controls/GoSlider.cs`
- Modify: `Going.UI/Controls/GoRangeSlider.cs`
- Modify: `Going.UI/Controls/GoKnob.cs`
- Modify: `Going.UI.Tests/Bindings/BindingTests.cs`

목표: 드래그 중인 입력 컨트롤은 자동으로 IsBindingSuppressed=true를 반환.

> 주: GoInput / GoNumberBox / GoValue는 모달 편집 방식(클릭 → 입력창 → 커밋)이라 폴링이 컨트롤 값을 휘젓지 않음. 드래그 컨트롤만 override하면 충분. 향후 인라인 편집을 추가할 때 해당 컨트롤도 override.

- [ ] **Step 1: 실패 테스트 추가**

드래그 상태를 외부에서 시뮬레이션하기 까다로우므로, override가 실제로 존재하는지(즉 `DeclaringType`이 GoControl이 아닌지)와 idle 상태에서 false를 반환하는지를 reflection으로 검증한다.

```csharp
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
```

- [ ] **Step 2: 테스트 실행 — 실패 확인**

Run: `dotnet test ... --filter "FullyQualifiedName~BindingTests"`
Expected: 3개 override 테스트가 실패 (DeclaringType이 GoControl).

- [ ] **Step 3: GoSlider override 추가**

`Going.UI/Controls/GoSlider.cs`의 `Member Variable` 영역(line 153 근처 `private bool isDragging;` 밑) 또는 `Override` 영역에 추가:

```csharp
        /// <inheritdoc/>
        protected internal override bool IsBindingSuppressed => isDragging;
```

- [ ] **Step 4: GoRangeSlider override 추가**

`Going.UI/Controls/GoRangeSlider.cs` (`isDraggingLower`/`isDraggingUpper` 정의 근처, line 183):

```csharp
        /// <inheritdoc/>
        protected internal override bool IsBindingSuppressed => isDraggingLower || isDraggingUpper;
```

- [ ] **Step 5: GoKnob override 추가**

`Going.UI/Controls/GoKnob.cs` (`bDown` 정의 근처, line 116):

```csharp
        /// <inheritdoc/>
        protected internal override bool IsBindingSuppressed => bDown;
```

- [ ] **Step 6: 테스트 실행 — 통과 확인**

Run: `dotnet test ... --filter "FullyQualifiedName~BindingTests"`
Expected: 19 tests pass (15 + 4).

- [ ] **Step 7: 커밋**

```bash
git add Going.UI/Controls/GoSlider.cs Going.UI/Controls/GoRangeSlider.cs Going.UI/Controls/GoKnob.cs Going.UI.Tests/Bindings/BindingTests.cs
git commit -m "Bindings: GoSlider/GoRangeSlider/GoKnob에 IsBindingSuppressed override"
```

---

## Task 8: 빌드 검증 + CHANGELOG

**Files:**
- Modify: `CHANGELOG.md`

목표: 전체 빌드/테스트 통과 + 사용자 대상 변경사항 기록.

- [ ] **Step 1: 전체 빌드**

Run: `dotnet build Going.sln -c Release`
Expected: 0 errors, 0 warnings.

- [ ] **Step 2: 전체 테스트 실행**

Run: `dotnet test Going.UI.Tests/Going.UI.Tests.csproj`
Expected: 모든 테스트 통과 (기존 + 신규 binding 19개).

- [ ] **Step 3: CHANGELOG.md 업데이트**

`CHANGELOG.md` 최상단(최근 버전 위)에 추가:

```markdown
## [Unreleased]

### Added
- **Going.UI 컨트롤 바인딩 시스템** — `GoControl.Bind(c => c.X, () => src, [v => src = v])` 확장 메서드로 컨트롤 속성을 임의의 식에 단/양방향 바인딩.
  - 매 `FireUpdate`에 양쪽 폴링하여 동기화. 이벤트 의존성 없음.
  - `IsBindingSuppressed` 가상 플래그 — 입력 컨트롤이 조작 중일 때 펌프 일시 정지(GoSlider/GoRangeSlider/GoKnob).
  - 컨트롤 Dispose 시 자동 해제. `Unbind`/`UnbindAll` 명시 해제 가능.
```

- [ ] **Step 4: 커밋**

```bash
git add CHANGELOG.md
git commit -m "CHANGELOG: 컨트롤 바인딩 시스템 추가 기록"
```

---

## 완료 기준

- [ ] 모든 task의 step이 통과 표시
- [ ] `dotnet test` 19개 binding 테스트 + 기존 모든 테스트 통과
- [ ] `dotnet build -c Release` 무경고 빌드
- [ ] CHANGELOG에 기능 추가 기록
- [ ] 새 파일 2개(GoBinding, GoControlBindingExtensions) + 수정 4개(GoControl, GoSlider, GoRangeSlider, GoKnob) + 테스트 1개
