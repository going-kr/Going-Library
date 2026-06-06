# Gudx 선언적 바인딩 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** gudx 마크업 attribute에 `{path}` / `{path:fmt}` 바인딩 식을 적어, 유저가 1회 주입한 root 객체에 단방향/양방향 자동 동기화.

**Architecture:** 기존 런타임 바인딩 펌프(대칭 폴링) 위에 얇은 마크업 껍데기를 얹는다. (1) gudx 역직렬화 시 `{...}` attribute는 컨트롤의 `PendingBindings`에 보관, (2) `GoDesign.WireBindings(root)`가 트리를 훑어 경로를 Expression으로 1회 컴파일해 `GoBinding`을 생성·등록, (3) 이후는 기존 `PumpBindings`가 매 프레임 동기화. 새 엔진·새 인스턴스화 장치 없음.

**Tech Stack:** C# / .NET, System.Linq.Expressions, xUnit, Going.UI Gudx 리플렉션 직렬화.

**Spec:** [2026-06-06-gudx-declarative-binding-design.md](../specs/2026-06-06-gudx-declarative-binding-design.md)

---

## File Structure

- `Going.UI/Bindings/GudxBindingExpression.cs` — **신규**. 순수 파싱(`IsBinding`/`Parse`) + 경로→델리게이트 컴파일 + 캐시 + `Wire`(GoBinding 생성·등록). 모듈 핵심.
- `Going.UI/Bindings/GudxBinder.cs` — **신규**. `WireTree` 트리 재귀 순회(통합).
- `Going.UI/Controls/GoControl.cs` — `PendingBindings` 보관소 + `AddPendingBinding`.
- `Going.UI/Gudx/GoGudxConverter.cs` — 읽기(`PopulateAny`)·쓰기(`WriteAny`) `{...}` 분기.
- `Going.UI/Design/GoDesign.cs` — `WireBindings(object root)`.
- `Going.UI.Tests/Bindings/GudxBindingExpressionTests.cs` — 신규 단위 테스트.
- `Going.UI.Tests/Bindings/GudxBindingWireTests.cs` — 신규 wire/통합 테스트.
- `Going.UI.Tests/Gudx/GudxBindingRoundTripTests.cs` — 신규 라운드트립 테스트.

---

## Task 1: `IsBinding` / `Parse` 순수 함수

**Files:**
- Create: `Going.UI/Bindings/GudxBindingExpression.cs`
- Test: `Going.UI.Tests/Bindings/GudxBindingExpressionTests.cs`

- [ ] **Step 1: 실패 테스트**

```csharp
using Going.UI.Bindings;
using Xunit;

namespace Going.UI.Tests.Bindings;

public class GudxBindingExpressionTests
{
    [Theory]
    [InlineData("{Data1.Title}", true)]
    [InlineData("{X}", true)]
    [InlineData("{X:F1}", true)]
    [InlineData("Hello", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("{{literal}}", false)]   // escape — 바인딩 아님
    [InlineData("{unclosed", false)]
    [InlineData("trailing}", false)]
    public void IsBinding_Detects(string? value, bool expected)
        => Assert.Equal(expected, GudxBindingExpression.IsBinding(value));

    [Theory]
    [InlineData("{Data1.Title}", "Data1.Title", null)]
    [InlineData("{DataMgr.Tank1.Level:F1}", "DataMgr.Tank1.Level", "F1")]
    [InlineData("{X : 0.0 }", "X", "0.0")]   // 공백 trim
    public void Parse_SplitsPathAndFormat(string value, string path, string? fmt)
    {
        var (p, f) = GudxBindingExpression.Parse(value);
        Assert.Equal(path, p);
        Assert.Equal(fmt, f);
    }
}
```

- [ ] **Step 2: 테스트 실패 확인**

Run: `dotnet test Going.UI.Tests --filter GudxBindingExpressionTests`
Expected: FAIL — `GudxBindingExpression` 타입 없음(컴파일 에러).

- [ ] **Step 3: 최소 구현**

```csharp
using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Going.UI.Controls;

namespace Going.UI.Bindings;

/// <summary>
/// gudx 마크업 attribute의 바인딩 식(`{path}` / `{path:fmt}`)을 파싱하고,
/// 경로를 root 객체 기준 델리게이트로 컴파일해 GoControl 바인딩에 연결한다.
/// </summary>
public static class GudxBindingExpression
{
    /// <summary>값이 `{...}` 바인딩 식인지 판정. `{{` escape와 미완성 식은 제외.</summary>
    public static bool IsBinding(string? value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        if (value.Length < 2) return false;
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
}
```

- [ ] **Step 4: 테스트 통과 확인**

Run: `dotnet test Going.UI.Tests --filter GudxBindingExpressionTests`
Expected: PASS

- [ ] **Step 5: 커밋**

```bash
git add Going.UI/Bindings/GudxBindingExpression.cs Going.UI.Tests/Bindings/GudxBindingExpressionTests.cs
git commit -m "feat(binding): GudxBindingExpression IsBinding/Parse"
```

---

## Task 2: 경로 → 델리게이트 컴파일 + 캐시

말단까지 reflection-walked Expression으로 getter(`Func<object,object?>`)를 빌드하고, 말단 property가 settable이면 setter(`Action<object,object?>`)도 빌드. `(rootType, path)` 키로 캐시.

**Files:**
- Modify: `Going.UI/Bindings/GudxBindingExpression.cs`
- Test: `Going.UI.Tests/Bindings/GudxBindingExpressionTests.cs`

- [ ] **Step 1: 실패 테스트**

```csharp
// GudxBindingExpressionTests.cs 에 추가
public sealed class Leaf { public string Title { get; set; } = "init"; public int RO { get; } = 7; }
public sealed class Root { public Leaf Data1 { get; } = new(); public double Speed { get; set; } = 1.5; }

[Fact]
public void Compile_Getter_WalksPath()
{
    var root = new Root();
    var cp = GudxBindingExpression.CompilePath(typeof(Root), "Data1.Title");
    Assert.Equal("init", cp.Getter(root));
    root.Data1.Title = "changed";
    Assert.Equal("changed", cp.Getter(root));   // 매번 현재값
    Assert.Equal(typeof(string), cp.LeafType);
}

[Fact]
public void Compile_Setter_WhenLeafWritable()
{
    var root = new Root();
    var cp = GudxBindingExpression.CompilePath(typeof(Root), "Data1.Title");
    Assert.NotNull(cp.Setter);
    cp.Setter!(root, "viaSetter");
    Assert.Equal("viaSetter", root.Data1.Title);
}

[Fact]
public void Compile_NoSetter_WhenLeafReadOnly()
{
    var cp = GudxBindingExpression.CompilePath(typeof(Root), "Data1.RO");
    Assert.Null(cp.Setter);
}

[Fact]
public void Compile_Caches_SameInstance()
{
    var a = GudxBindingExpression.CompilePath(typeof(Root), "Speed");
    var b = GudxBindingExpression.CompilePath(typeof(Root), "Speed");
    Assert.Same(a, b);
}

[Fact]
public void Compile_Throws_OnMissingMember()
    => Assert.Throws<ArgumentException>(() => GudxBindingExpression.CompilePath(typeof(Root), "Nope.Missing"));
```

- [ ] **Step 2: 테스트 실패 확인**

Run: `dotnet test Going.UI.Tests --filter GudxBindingExpressionTests`
Expected: FAIL — `CompilePath` / `CompiledPath` 없음.

- [ ] **Step 3: 구현 추가** (`GudxBindingExpression` 안에)

```csharp
/// <summary>컴파일된 경로 접근자. getter는 항상, setter는 말단이 settable일 때만.</summary>
public sealed class CompiledPath
{
    public required Func<object, object?> Getter { get; init; }
    public Action<object, object?>? Setter { get; init; }
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
```

- [ ] **Step 4: 테스트 통과 확인**

Run: `dotnet test Going.UI.Tests --filter GudxBindingExpressionTests`
Expected: PASS

- [ ] **Step 5: 커밋**

```bash
git add Going.UI/Bindings/GudxBindingExpression.cs Going.UI.Tests/Bindings/GudxBindingExpressionTests.cs
git commit -m "feat(binding): compile gudx path to cached get/set delegates"
```

---

## Task 3: `GoControl.PendingBindings` 보관소

**Files:**
- Modify: `Going.UI/Controls/GoControl.cs` (bindings 필드 근처, line ~207)
- Test: `Going.UI.Tests/Bindings/GudxBindingWireTests.cs`

- [ ] **Step 1: 실패 테스트**

```csharp
using Going.UI.Controls;
using Xunit;

namespace Going.UI.Tests.Bindings;

public class GudxBindingWireTests
{
    [Fact]
    public void AddPendingBinding_StoresExpr()
    {
        var lbl = new GoLabel();
        lbl.AddPendingBinding("Text", "{Data1.Title}");
        Assert.NotNull(lbl.PendingBindings);
        Assert.Equal("{Data1.Title}", lbl.PendingBindings!["Text"]);
    }
}
```

> `AddPendingBinding`/`PendingBindings`는 `internal`. 테스트 어셈블리가 `InternalsVisibleTo`로 접근 가능한지 확인 — 기존 `AddOrReplaceBinding` 등이 internal인데 테스트가 쓰고 있으므로 이미 노출됨.

- [ ] **Step 2: 테스트 실패 확인**

Run: `dotnet test Going.UI.Tests --filter GudxBindingWireTests`
Expected: FAIL — `PendingBindings`/`AddPendingBinding` 없음.

- [ ] **Step 3: 구현** — `GoControl.cs` `private List<GoBinding>? bindings;` 바로 아래 추가

```csharp
        // gudx 선언적 바인딩: 역직렬화 시 보관, WireBindings(root) 때 실제 바인딩으로 전환
        internal Dictionary<string, string>? PendingBindings { get; private set; }
        internal void AddPendingBinding(string propName, string expr)
            => (PendingBindings ??= new Dictionary<string, string>(StringComparer.Ordinal))[propName] = expr;
```

> 파일 상단에 `using System.Collections.Generic;`가 이미 있는지 확인(있음 — `List<GoBinding>` 사용 중).

- [ ] **Step 4: 테스트 통과 확인**

Run: `dotnet test Going.UI.Tests --filter GudxBindingWireTests`
Expected: PASS

- [ ] **Step 5: 커밋**

```bash
git add Going.UI/Controls/GoControl.cs Going.UI.Tests/Bindings/GudxBindingWireTests.cs
git commit -m "feat(binding): GoControl pending-binding store"
```

---

## Task 4: gudx 읽기 훅 — `{...}`를 PendingBinding으로

**Files:**
- Modify: `Going.UI/Gudx/GoGudxConverter.cs` `PopulateAny` (line ~760-766)
- Test: `Going.UI.Tests/Gudx/GudxBindingRoundTripTests.cs`

- [ ] **Step 1: 실패 테스트**

```csharp
using Going.UI.Controls;
using Going.UI.Gudx;
using Xunit;

namespace Going.UI.Tests.Gudx;

public class GudxBindingRoundTripTests
{
    [Fact]
    public void Deserialize_BindingAttr_BecomesPendingBinding_NotLiteral()
    {
        var xml = "<GoLabel Text=\"{Data1.Title}\" />";
        var ctrl = (GoLabel)GoGudxConverter.DeserializeControl(xml);

        Assert.NotNull(ctrl.PendingBindings);
        Assert.Equal("{Data1.Title}", ctrl.PendingBindings!["Text"]);
        // 리터럴로 새지 않음: Text는 기본값 유지(바인딩 식이 텍스트로 안 들어감)
        Assert.NotEqual("{Data1.Title}", ctrl.Text);
    }

    [Fact]
    public void Deserialize_LiteralAttr_StillParsesNormally()
    {
        var ctrl = (GoLabel)GoGudxConverter.DeserializeControl("<GoLabel Text=\"Hello\" />");
        Assert.Equal("Hello", ctrl.Text);
        Assert.Null(ctrl.PendingBindings);
    }
}
```

> `DeserializeControl`(line 200)이 단일 컨트롤 XML을 IGoControl로 역직렬화하며 내부적으로 `PopulateAny`를 거치는지 Step 3 전에 코드로 확인. 거치지 않으면 그 경로의 attribute 루프에도 동일 분기를 넣는다.

- [ ] **Step 2: 테스트 실패 확인**

Run: `dotnet test Going.UI.Tests --filter GudxBindingRoundTripTests`
Expected: FAIL — `PendingBindings` null(현재는 `{Data1.Title}`가 ParseScalar로 처리되어 무시되거나 리터럴화).

- [ ] **Step 3: 구현** — `PopulateAny`의 attribute 루프 수정

```csharp
        // P1: read attributes onto scalar properties.
        foreach (var attr in elem.Attributes())
        {
            var prop = type.GetProperty(attr.Name.LocalName);
            if (prop == null || !prop.CanWrite) continue;

            // gudx 선언적 바인딩: {path} 식은 리터럴 파싱 대신 보류 바인딩으로 보관
            if (instance is GoControl gc && GudxBindingExpression.IsBinding(attr.Value))
            {
                gc.AddPendingBinding(prop.Name, attr.Value);
                continue;
            }

            var parsed = ParseScalar(prop.PropertyType, attr.Value);
            if (parsed != null) prop.SetValue(instance, parsed);
        }
```

> 파일 상단 using에 `using Going.UI.Bindings;` 추가(없으면). `using Going.UI.Controls;`는 이미 있음(IGoControl 등).

- [ ] **Step 4: 테스트 통과 확인**

Run: `dotnet test Going.UI.Tests --filter GudxBindingRoundTripTests`
Expected: PASS

- [ ] **Step 5: 커밋**

```bash
git add Going.UI/Gudx/GoGudxConverter.cs Going.UI.Tests/Gudx/GudxBindingRoundTripTests.cs
git commit -m "feat(binding): gudx read hook stores {expr} as pending binding"
```

---

## Task 5: gudx 쓰기 훅 — 라운드트립 보존

**Files:**
- Modify: `Going.UI/Gudx/GoGudxConverter.cs` `WriteAny` (line ~224-229)
- Test: `Going.UI.Tests/Gudx/GudxBindingRoundTripTests.cs`

- [ ] **Step 1: 실패 테스트**

```csharp
// GudxBindingRoundTripTests.cs 에 추가
[Fact]
public void RoundTrip_BindingExpr_Preserved()
{
    var ctrl = (GoLabel)GoGudxConverter.DeserializeControl("<GoLabel Text=\"{Data1.Title}\" />");
    var elem = GoGudxConverter.WriteAny(ctrl);
    Assert.Equal("{Data1.Title}", elem.Attribute("Text")?.Value);   // 리터럴화 안 됨
}
```

- [ ] **Step 2: 테스트 실패 확인**

Run: `dotnet test Going.UI.Tests --filter GudxBindingRoundTripTests`
Expected: FAIL — Text attribute가 비거나 기본값으로 출력.

- [ ] **Step 3: 구현** — `WriteAny`의 scalar 루프 수정

```csharp
        // P1: scalar properties become attributes.
        var pendBind = (obj as GoControl)?.PendingBindings;
        foreach (var prop in ScalarProperties(type))
        {
            // 보류 바인딩이 있는 속성은 원본 {expr}를 그대로 재출력(라운드트립)
            if (pendBind != null && pendBind.TryGetValue(prop.Name, out var expr))
            {
                elem.SetAttributeValue(prop.Name, expr);
                continue;
            }

            var value = prop.GetValue(obj);
            if (value == null) continue;
            var s = FormatScalar(value);
            if (s != null) elem.SetAttributeValue(prop.Name, s);
        }
```

- [ ] **Step 4: 테스트 통과 확인**

Run: `dotnet test Going.UI.Tests --filter GudxBindingRoundTripTests`
Expected: PASS

- [ ] **Step 5: 커밋**

```bash
git add Going.UI/Gudx/GoGudxConverter.cs Going.UI.Tests/Gudx/GudxBindingRoundTripTests.cs
git commit -m "feat(binding): gudx write hook round-trips {expr}"
```

---

## Task 6: `Wire` — PendingBinding을 실제 GoBinding으로

경로 컴파일 결과로 source getter/setter를 만들고, 컨트롤 속성 접근자를 빌드해 `GoBinding`을 `AddOrReplaceBinding`. 포맷·타입 강제 처리.

**Files:**
- Modify: `Going.UI/Bindings/GudxBindingExpression.cs` (`Wire` 추가)
- Test: `Going.UI.Tests/Bindings/GudxBindingWireTests.cs`

- [ ] **Step 1: 실패 테스트**

```csharp
// GudxBindingWireTests.cs 에 추가
private sealed class Src { public string Title { get; set; } = "hi"; public double Level { get; set; } = 12.345; }
private sealed class Box { public Src Data1 { get; } = new(); }

[Fact]
public void Wire_OneWay_SourceToControl_OnPump()
{
    var lbl = new GoLabel();
    var box = new Box();
    GudxBindingExpression.Wire(lbl, "Text", "{Data1.Title}", box);

    lbl.FireUpdate();
    Assert.Equal("hi", lbl.Text);

    box.Data1.Title = "bye";
    lbl.FireUpdate();
    Assert.Equal("bye", lbl.Text);
}

[Fact]
public void Wire_Format_ProducesFormattedString()
{
    var lbl = new GoLabel();
    var box = new Box();
    GudxBindingExpression.Wire(lbl, "Text", "{Data1.Level:F1}", box);
    lbl.FireUpdate();
    Assert.Equal("12.3", lbl.Text);
}

[Fact]
public void Wire_TwoWay_ControlToSource_WhenSettable()
{
    var inp = new GoInput();   // settable Text, 표시형이라 IsBindingSuppressed=false 기본
    var box = new Box();
    GudxBindingExpression.Wire(inp, "Text", "{Data1.Title}", box);
    inp.FireUpdate();          // 초기 동기화: source→control

    inp.Text = "edited";
    inp.FireUpdate();          // control→source
    Assert.Equal("edited", box.Data1.Title);
}
```

> `GoInput`의 정확한 텍스트 속성명/타입을 Step 3 전에 확인(예: `Text` string). 다르면 테스트의 속성명을 실제에 맞춘다. 양방향 테스트가 편집 상태(IsBindingSuppressed)로 막히면, 표시형 컨트롤(예: settable string 속성을 가진 다른 컨트롤)로 교체한다.

- [ ] **Step 2: 테스트 실패 확인**

Run: `dotnet test Going.UI.Tests --filter GudxBindingWireTests`
Expected: FAIL — `Wire` 없음.

- [ ] **Step 3: 구현** — `GudxBindingExpression`에 추가

```csharp
/// <summary>
/// 컨트롤 속성 propName을 root 기준 식 expr(`{path}` / `{path:fmt}`)에 바인딩한다.
/// 경로 말단이 settable이고 포맷이 없으면 양방향, 아니면 단방향.
/// 해석 실패 시 ArgumentException은 호출자(WireTree)가 잡아 로그 후 스킵.
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
        srcSet = v => cp.Setter(root, v);

    // 컨트롤 속성 접근자(박싱 경로)
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
    if (value is IFormattable f) return f.ToString(format, System.Globalization.CultureInfo.CurrentCulture);
    return value.ToString();
}

private static object? Coerce(object? value, Type targetType)
{
    if (value == null) return null;
    if (targetType.IsInstanceOfType(value)) return value;
    if (targetType == typeof(string)) return value.ToString();
    var under = Nullable.GetUnderlyingType(targetType) ?? targetType;
    try { return Convert.ChangeType(value, under, System.Globalization.CultureInfo.CurrentCulture); }
    catch { return value; }   // 펌프의 set이 실패하면 per-binding catch가 처리
}
```

- [ ] **Step 4: 테스트 통과 확인**

Run: `dotnet test Going.UI.Tests --filter GudxBindingWireTests`
Expected: PASS

- [ ] **Step 5: 커밋**

```bash
git add Going.UI/Bindings/GudxBindingExpression.cs Going.UI.Tests/Bindings/GudxBindingWireTests.cs
git commit -m "feat(binding): Wire pending expr to GoBinding (format, coerce, two-way)"
```

---

## Task 7: `GudxBinder.WireTree` + `GoDesign.WireBindings` (통합)

**Files:**
- Create: `Going.UI/Bindings/GudxBinder.cs`
- Modify: `Going.UI/Design/GoDesign.cs` (`WireBindings` 추가, `Init` 근처)
- Test: `Going.UI.Tests/Bindings/GudxBindingWireTests.cs`

- [ ] **Step 1: 실패 테스트**

```csharp
// GudxBindingWireTests.cs 에 추가
using Going.UI.Containers;

[Fact]
public void WireTree_RecursesContainers_AndBinds()
{
    var box = new Box();
    var panel = new GoBoxPanel();
    var lbl = new GoLabel();
    lbl.AddPendingBinding("Text", "{Data1.Title}");
    panel.Childrens.Add(lbl);

    GudxBinder.WireTree(panel, box);

    lbl.FireUpdate();
    Assert.Equal("hi", lbl.Text);
}

[Fact]
public void WireTree_BadPath_SkipsAndContinues()
{
    var box = new Box();
    var good = new GoLabel(); good.AddPendingBinding("Text", "{Data1.Title}");
    var bad = new GoLabel();  bad.AddPendingBinding("Text", "{Nope.Missing}");
    var panel = new GoBoxPanel();
    panel.Childrens.Add(bad);
    panel.Childrens.Add(good);

    GudxBinder.WireTree(panel, box);   // 예외 없이 끝나야 함

    good.FireUpdate();
    Assert.Equal("hi", good.Text);     // 정상 바인딩은 살아있음
}
```

- [ ] **Step 2: 테스트 실패 확인**

Run: `dotnet test Going.UI.Tests --filter GudxBindingWireTests`
Expected: FAIL — `GudxBinder` 없음.

- [ ] **Step 3: 구현**

`Going.UI/Bindings/GudxBinder.cs` 생성:

```csharp
using System.Diagnostics;
using Going.UI.Containers;
using Going.UI.Controls;

namespace Going.UI.Bindings;

/// <summary>
/// 컨트롤 트리를 재귀 순회하며 각 컨트롤의 보류 바인딩을 root 기준 실제 바인딩으로 wire한다.
/// </summary>
public static class GudxBinder
{
    /// <summary>node와 그 자손(IGoContainer.Childrens)의 보류 바인딩을 root에 연결.</summary>
    public static void WireTree(IGoControl node, object root)
    {
        if (node is GoControl gc && gc.PendingBindings is { } pend)
        {
            foreach (var kv in pend)
            {
                try { GudxBindingExpression.Wire(gc, kv.Key, kv.Value, root); }
                catch (System.Exception ex)
                {
                    Debug.WriteLine($"[GudxBinder] wire failed for {gc.GetType().Name}.{kv.Key} = '{kv.Value}': {ex.Message}");
                }
            }
        }

        if (node is IGoContainer cont)
            foreach (var child in cont.Childrens)
                WireTree(child, root);
    }
}
```

`GoDesign.cs`의 `Init()` 메서드 바로 아래에 추가:

```csharp
        /// <summary>
        /// gudx 마크업 바인딩(`{path}`)을 root 객체에 연결한다. Init() 이후 1회 호출.
        /// </summary>
        public void WireBindings(object root)
        {
            foreach (var page in Pages.Values)   Going.UI.Bindings.GudxBinder.WireTree(page, root);
            foreach (var wnd  in Windows.Values) Going.UI.Bindings.GudxBinder.WireTree(wnd, root);
            Going.UI.Bindings.GudxBinder.WireTree(TitleBar, root);
            Going.UI.Bindings.GudxBinder.WireTree(LeftSideBar, root);
            Going.UI.Bindings.GudxBinder.WireTree(RightSideBar, root);
            Going.UI.Bindings.GudxBinder.WireTree(Footer, root);
        }
```

> `Pages`/`Windows` 값 타입이 `GoPage`/`GoWindow`(IGoControl 구현)인지 확인 — `WireTree(IGoControl, ...)` 시그니처에 맞아야 함. 맞지 않으면 시그니처를 실제 타입에 맞춘다.

- [ ] **Step 4: 테스트 통과 확인**

Run: `dotnet test Going.UI.Tests --filter GudxBindingWireTests`
Expected: PASS

- [ ] **Step 5: 커밋**

```bash
git add Going.UI/Bindings/GudxBinder.cs Going.UI/Design/GoDesign.cs Going.UI.Tests/Bindings/GudxBindingWireTests.cs
git commit -m "feat(binding): GudxBinder.WireTree + GoDesign.WireBindings"
```

---

## Task 8: 전체 회귀 + CHANGELOG

**Files:**
- Modify: `CHANGELOG.md`

- [ ] **Step 1: 전체 테스트**

Run: `dotnet test Going.UI.Tests`
Expected: 전부 PASS(기존 + 신규). 실패 시 해당 Task로 돌아가 수정.

- [ ] **Step 2: CHANGELOG 항목 추가** (최신 섹션 상단)

```markdown
- gudx 선언적 바인딩: 마크업 attribute에 `{path}` / `{path:fmt}` 식을 적어 `GoDesign.WireBindings(root)`로 주입한 객체에 단방향/양방향 자동 동기화. 기존 런타임 바인딩 펌프 재사용.
```

- [ ] **Step 3: 커밋**

```bash
git add CHANGELOG.md
git commit -m "docs(changelog): gudx declarative binding"
```

---

## Self-Review 메모

- **Spec 커버리지**: 문법(T1) / 경로 컴파일·캐시(T2) / 보류 보관(T3) / 읽기 훅(T4) / 쓰기 라운드트립(T5) / Wire·포맷·양방향(T6) / 트리 순회·주입(T7) — spec 통합 지점 표와 1:1.
- **1차 비목표 유지**: 문자열 보간·컬렉션·명령 바인딩·SubPage/TabPage 내부 순회·에디터 GUI 없음.
- **타입 일관성**: `CompiledPath`(Getter/Setter/LeafType), `Wire(GoControl,string,string,object)`, `WireTree(IGoControl,object)`, `WireBindings(object)` — 모든 Task에서 동일 시그니처.
- **검증 가드**: 실제 속성명(`GoInput.Text`, `Pages`/`Windows` 값 타입, `DeserializeControl`의 PopulateAny 경유 여부)은 각 Task Step 3 전에 코드로 확인하라고 명시.
