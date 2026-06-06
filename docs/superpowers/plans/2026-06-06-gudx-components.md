# Gudx 컴포넌트 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development or superpowers:executing-plans. Steps use checkbox (`- [ ]`) syntax.

**Goal:** 마크업으로 파라미터 재사용 컴포넌트(`<GoComponent>`)를 정의하고 새 태그(`<MotorCard .../>`)로 인스턴스화. 내부 `{param.path}`는 인스턴스별 파라미터 스코프에 바인딩.

**Architecture:** 평면 바인딩 위에 root만 교체해 얹는다. 정의는 레지스트리에 등록, 사용처는 `GoComponentInstance : GoContainer`로 역직렬화(템플릿 트리 `ReadElement` 복제). `WireTree`가 컴포넌트 경계에서 root→`GoComponentScope`로 교체, 내부 경로 tail은 기존 `CompilePath(type)` 재사용. 엔진·펌프 무수정.

**Tech Stack:** C# / .NET, System.Linq.Expressions, System.Xml.Linq, xUnit.

**Spec:** [2026-06-06-gudx-components-design.md](../specs/2026-06-06-gudx-components-design.md)

**결정**: type 해석=로드 어셈블리 simple-name 자동 스캔. 미등록 대문자 태그=기존대로 throw. 중첩 지원(깊이 가드 32).

---

## File Structure

- `Going.UI/Gudx/GoComponentParam.cs` — **신규**. Param 정의(Name/TypeName/Default/ResolvedType), 타입 해석(별칭+어셈블리 스캔).
- `Going.UI/Gudx/GoComponentTemplate.cs` — **신규**. 템플릿(Name/Params/Roots:XElement[]) + 정적 레지스트리.
- `Going.UI/Containers/GoComponentInstance.cs` — **신규**. `GoContainer` 파생, ComponentName/ParamValues/Template/Childrens.
- `Going.UI/Bindings/GoComponentScope.cs` — **신규**. Values/Types 스코프 + Build(ci, outerRoot) + Eval 헬퍼.
- `Going.UI/Bindings/GudxBindingExpression.cs` — `WireScoped` 추가, 공통부 `WireCore` 추출.
- `Going.UI/Bindings/GudxBinder.cs` — `WireTree`에 컴포넌트 분기(root→scope), scope/plain 디스패치.
- `Going.UI/Gudx/GoGudxConverter.cs` — `<Components>` 우선 등록; `ReadElement` 컴포넌트 분기; `WriteAny` 인스턴스 라운드트립.
- 테스트: `Going.UI.Tests/Gudx/GudxComponentTests.cs`, `Going.UI.Tests/Bindings/GudxComponentScopeTests.cs`.

---

## Task 1: `GoComponentParam` — Param 정의 + 타입 해석

**Files:** Create `Going.UI/Gudx/GoComponentParam.cs`; Test `Going.UI.Tests/Gudx/GudxComponentTests.cs`

- [ ] **Step 1: 실패 테스트**

```csharp
using Going.UI.Gudx;
using Xunit;

namespace Going.UI.Tests.Gudx;

public class GudxComponentTests
{
    public sealed class MotorVM { public double Rpm { get; set; } = 100; }

    [Theory]
    [InlineData("string", typeof(string))]
    [InlineData("int", typeof(int))]
    [InlineData("double", typeof(double))]
    [InlineData("bool", typeof(bool))]
    public void Param_ResolvesAliasTypes(string name, System.Type expected)
        => Assert.Equal(expected, GoComponentParam.ResolveType(name));

    [Fact]
    public void Param_ResolvesUserTypeBySimpleName()
        => Assert.Equal(typeof(MotorVM), GoComponentParam.ResolveType("MotorVM"));

    [Fact]
    public void Param_UnknownType_ReturnsNull()
        => Assert.Null(GoComponentParam.ResolveType("NoSuchTypeXyz"));
}
```

- [ ] **Step 2: 실패 확인** — `dotnet test Going.UI.Tests --filter GudxComponentTests` → FAIL(타입 없음).

- [ ] **Step 3: 구현**

```csharp
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
```

- [ ] **Step 4: 통과 확인** — PASS.
- [ ] **Step 5: 커밋** — `git add ...; git commit -m "feat(component): GoComponentParam + type resolution"`

---

## Task 2: `GoComponentTemplate` + 레지스트리

**Files:** Create `Going.UI/Gudx/GoComponentTemplate.cs`; Test 동일 파일.

- [ ] **Step 1: 실패 테스트**

```csharp
// GudxComponentTests.cs 에 추가
using System.Xml.Linq;

[Fact]
public void Registry_RegisterAndLookup()
{
    GoComponentTemplate.Registry.Clear();
    var tpl = new GoComponentTemplate
    {
        Name = "MotorCard",
        Params = new() { new GoComponentParam { Name = "Title", TypeName = "string" } },
        Roots = new() { XElement.Parse("<GoLabel Text=\"{Title}\"/>") },
    };
    GoComponentTemplate.Registry.Register(tpl);

    Assert.True(GoComponentTemplate.Registry.TryGet("MotorCard", out var got));
    Assert.Same(tpl, got);
    Assert.False(GoComponentTemplate.Registry.TryGet("Nope", out _));
}
```

- [ ] **Step 2: 실패 확인** — FAIL.

- [ ] **Step 3: 구현**

```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;

namespace Going.UI.Gudx;

/// <summary>역직렬화된 컴포넌트 템플릿(파라미터 정의 + 내부 컨트롤 트리).</summary>
public sealed class GoComponentTemplate
{
    /// <summary>컴포넌트 이름(사용처 태그).</summary>
    public required string Name { get; init; }
    /// <summary>파라미터 정의 목록.</summary>
    public required List<GoComponentParam> Params { get; init; }
    /// <summary>내부 컨트롤 트리 루트(들). 인스턴스마다 ReadElement로 복제.</summary>
    public required List<XElement> Roots { get; init; }

    /// <summary>전역 컴포넌트 레지스트리(이름→템플릿).</summary>
    public static class Registry
    {
        private static readonly Dictionary<string, GoComponentTemplate> _map = new(System.StringComparer.Ordinal);

        /// <summary>템플릿 등록(동명 시 마지막 승, Debug 경고).</summary>
        public static void Register(GoComponentTemplate t)
        {
            if (_map.ContainsKey(t.Name))
                Debug.WriteLine($"[GoComponent] duplicate component '{t.Name}' — replaced.");
            _map[t.Name] = t;
        }

        /// <summary>이름으로 조회.</summary>
        public static bool TryGet(string name, out GoComponentTemplate template)
            => _map.TryGetValue(name, out template!);

        /// <summary>레지스트리 비우기(디자인 재로드/테스트).</summary>
        public static void Clear() => _map.Clear();
    }
}
```

- [ ] **Step 4: 통과 확인** — PASS.
- [ ] **Step 5: 커밋** — `"feat(component): GoComponentTemplate + registry"`

---

## Task 3: `GoComponentInstance : GoContainer`

**Files:** Create `Going.UI/Containers/GoComponentInstance.cs`; Test `GudxComponentTests.cs`.

- [ ] **Step 1: 실패 테스트**

```csharp
// GudxComponentTests.cs 에 추가
using Going.UI.Containers;
using Going.UI.Controls;

[Fact]
public void Instance_HoldsNameParamsChildren()
{
    var ci = new GoComponentInstance { ComponentName = "MotorCard" };
    ci.ParamValues["Title"] = "펌프 A";
    ci.Childrens.Add(new GoLabel());

    Assert.Equal("MotorCard", ci.ComponentName);
    Assert.Equal("펌프 A", ci.ParamValues["Title"]);
    Assert.Single(ci.Childrens);
}
```

- [ ] **Step 2: 실패 확인** — FAIL.

- [ ] **Step 3: 구현**

```csharp
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Going.UI.Controls;
using Going.UI.Gudx;

namespace Going.UI.Containers;

/// <summary>
/// 컴포넌트 사용처(<c>&lt;MotorCard .../&gt;</c>)의 런타임 인스턴스. 템플릿 트리를 복제해 자식으로 보유하고,
/// 사용처 파라미터 값을 보관한다. 직렬화 시 이름+파라미터만 출력하고 자식은 생략한다(템플릿 파생).
/// </summary>
public class GoComponentInstance : GoContainer
{
    /// <summary>컴포넌트 이름(직렬화 태그).</summary>
    [JsonIgnore] public string ComponentName { get; set; } = "";
    /// <summary>사용처 파라미터 원본 값(이름→식/리터럴).</summary>
    [JsonIgnore] public Dictionary<string, string> ParamValues { get; } = new(System.StringComparer.Ordinal);
    /// <summary>이 인스턴스의 템플릿(런타임 참조).</summary>
    [JsonIgnore] public GoComponentTemplate? Template { get; set; }
    /// <summary>복제된 템플릿 트리.</summary>
    [JsonInclude] public override List<IGoControl> Childrens { get; } = [];
}
```

> `Childrens`에 `[GoChildList]`를 **붙이지 않는다** — 자식은 라운드트립에서 생략되므로 generic P2 직렬화 대상이 아님.

- [ ] **Step 4: 통과 확인** — PASS.
- [ ] **Step 5: 커밋** — `"feat(component): GoComponentInstance container"`

---

## Task 4: `<Components>` 등록 + `ReadElement` 컴포넌트 분기

**Files:** Modify `Going.UI/Gudx/GoGudxConverter.cs` (`ReadGoDesign`, `ReadElement`, `DeserializeGoDesignFromFiles`); Test `GudxComponentTests.cs`.

- [ ] **Step 1: 실패 테스트**

```csharp
// GudxComponentTests.cs 에 추가
[Fact]
public void Deserialize_RegistersComponents_AndInstantiatesUsage()
{
    var xml = @"<GoDesign>
      <Components>
        <GoComponent name='MotorCard'>
          <Param name='Title' type='string' default=''/>
          <GoBoxPanel><GoLabel Text='{Title}'/></GoBoxPanel>
        </GoComponent>
      </Components>
      <Pages>
        <GoPage Name='Main'><MotorCard Title='펌프 A'/></GoPage>
      </Pages>
    </GoDesign>";

    var d = GoGudxConverter.ReadGoDesign(XElement.Parse(xml));
    Assert.NotNull(d);
    Assert.True(GoComponentTemplate.Registry.TryGet("MotorCard", out _));

    var page = d!.Pages["Main"];
    var inst = Assert.IsType<GoComponentInstance>(page.Childrens[0]);
    Assert.Equal("MotorCard", inst.ComponentName);
    Assert.Equal("펌프 A", inst.ParamValues["Title"]);
    // 복제된 자식 트리: GoBoxPanel > GoLabel(PendingBindings {Title})
    var box = Assert.IsType<GoBoxPanel>(inst.Childrens[0]);
    var lbl = Assert.IsType<GoLabel>(box.Childrens[0]);
    Assert.Equal("{Title}", lbl.PendingBindings!["Text"]);
}
```

> 실제 GoPage가 `<MotorCard>`를 자식으로 읽으려면 P2 read가 `_gudxControlTypes` 외 컴포넌트 태그도 통과시켜야 함. P2(`ReadP2_HomogeneousList`)는 태그 필터(`_gudxControlTypes.ContainsKey`)로 막혀 있으니(line ~831/851), 컴포넌트 레지스트리 검사도 OR로 추가해야 한다. Step 3에서 처리.

- [ ] **Step 2: 실패 확인** — FAIL.

- [ ] **Step 3: 구현**

(a) `ReadGoDesign`에 컴포넌트 우선 등록:

```csharp
    public static GoDesign? ReadGoDesign(XElement elem)
    {
        if (elem.Name.LocalName != "GoDesign") return null;
        RegisterComponents(elem);          // NEW: <Components> 먼저 레지스트리에
        var d = new GoDesign();
        PopulateAny(elem, d);
        return d;
    }

    /// <summary>디자인 요소의 &lt;Components&gt; 그룹을 파싱해 컴포넌트 레지스트리에 등록.</summary>
    internal static void RegisterComponents(XElement designElem)
    {
        var group = designElem.Element("Components");
        if (group == null) return;
        foreach (var compElem in group.Elements("GoComponent"))
        {
            var name = compElem.Attribute("name")?.Value;
            if (string.IsNullOrEmpty(name)) continue;

            var prms = new List<GoComponentParam>();
            var roots = new List<XElement>();
            foreach (var child in compElem.Elements())
            {
                if (child.Name.LocalName == "Param")
                {
                    var pn = child.Attribute("name")?.Value;
                    var pt = child.Attribute("type")?.Value;
                    if (string.IsNullOrEmpty(pn) || string.IsNullOrEmpty(pt)) continue;
                    prms.Add(new GoComponentParam { Name = pn!, TypeName = pt!, Default = child.Attribute("default")?.Value });
                }
                else roots.Add(child);   // 내부 컨트롤 트리 루트
            }
            GoComponentTemplate.Registry.Register(new GoComponentTemplate { Name = name!, Params = prms, Roots = roots });
        }
    }
```

(b) `ReadElement`에 컴포넌트 분기:

```csharp
    internal static IGoControl ReadElement(XElement elem)
    {
        var tagName = elem.Name.LocalName;
        if (!_gudxControlTypes.TryGetValue(tagName, out var type))
        {
            if (GoComponentTemplate.Registry.TryGet(tagName, out var template))
                return BuildComponentInstance(elem, template);
            throw new InvalidOperationException($"Unknown Gudx tag: {tagName}");
        }
        var instance = (IGoControl)Activator.CreateInstance(type)!;
        PopulateAny(elem, instance);
        // (기존 Id 처리 유지)
        var idAttr = elem.Attribute("Id");
        if (idAttr != null && Guid.TryParse(idAttr.Value, out var xmlId))
        {
            var idProp = type.GetProperty("Id");
            var setter = idProp?.GetSetMethod(nonPublic: true);
            try { setter?.Invoke(instance, new object[] { xmlId }); } catch { }
        }
        EnsureNonEmptyId(instance);
        return instance;
    }

    /// <summary>컴포넌트 사용처 요소를 GoComponentInstance로 빌드(파라미터 분리 + 템플릿 트리 복제).</summary>
    private static IGoControl BuildComponentInstance(XElement usage, GoComponentTemplate template)
    {
        var ci = new Going.UI.Containers.GoComponentInstance { ComponentName = template.Name, Template = template };
        var paramNames = new HashSet<string>(System.StringComparer.Ordinal);
        foreach (var p in template.Params) paramNames.Add(p.Name);

        // 사용처 attribute 분리: 파라미터 vs 인스턴스 본인 스칼라(Bounds 등)
        foreach (var attr in usage.Attributes())
        {
            var an = attr.Name.LocalName;
            if (paramNames.Contains(an)) { ci.ParamValues[an] = attr.Value; continue; }

            var prop = typeof(Going.UI.Containers.GoComponentInstance).GetProperty(an);
            if (prop == null || !prop.CanWrite) continue;
            if (GudxBindingExpression.IsBinding(attr.Value)) { ci.AddPendingBinding(prop.Name, attr.Value); continue; }
            var parsed = ParseScalar(prop.PropertyType, attr.Value);
            if (parsed != null) prop.SetValue(ci, parsed);
        }
        // 생략된 파라미터는 default로 채움
        foreach (var p in template.Params)
            if (!ci.ParamValues.ContainsKey(p.Name) && p.Default != null) ci.ParamValues[p.Name] = p.Default;

        // 템플릿 트리 복제(ReadElement 재귀 — {param} 식은 PendingBindings로 저장됨)
        foreach (var rootElem in template.Roots)
            ci.Childrens.Add(ReadElement(rootElem));

        return ci;
    }
```

(c) P2 read 태그 필터에 컴포넌트 허용 (line ~831, ~851 두 곳 `_gudxControlTypes.ContainsKey(...)` → 컴포넌트 OR):

```csharp
            if (!_gudxControlTypes.ContainsKey(childElem.Name.LocalName)
                && !GoComponentTemplate.Registry.TryGet(childElem.Name.LocalName, out _)) continue;
```

> Step 3 전에 line 831/851 실제 컨텍스트를 읽어 정확히 수정. using에 `Going.UI.Gudx`는 자기 네임스페이스(동일 파일), `GudxBindingExpression`은 `using Going.UI.Bindings;` 확인.

- [ ] **Step 4: 통과 확인** — PASS.
- [ ] **Step 5: 커밋** — `"feat(component): register <Components>, instantiate usage tags"`

---

## Task 5: `WriteAny` 인스턴스 라운드트립

**Files:** Modify `Going.UI/Gudx/GoGudxConverter.cs` (`WriteAny` 진입부); Test `GudxComponentTests.cs`.

- [ ] **Step 1: 실패 테스트**

```csharp
// GudxComponentTests.cs 에 추가
[Fact]
public void RoundTrip_Instance_EmitsNameAndParams_NoChildren()
{
    GoComponentTemplate.Registry.Clear();
    GoComponentTemplate.Registry.Register(new GoComponentTemplate
    {
        Name = "MotorCard",
        Params = new() { new GoComponentParam { Name = "Title", TypeName = "string" } },
        Roots = new() { XElement.Parse("<GoBoxPanel><GoLabel Text='{Title}'/></GoBoxPanel>") },
    });
    var ci = (GoComponentInstance)GoGudxConverter.ReadElement(
        XElement.Parse("<MotorCard Title='펌프 A' Bounds='0,0,100,60'/>"));

    var e = GoGudxConverter.WriteAny(ci);
    Assert.Equal("MotorCard", e.Name.LocalName);
    Assert.Equal("펌프 A", e.Attribute("Title")?.Value);
    Assert.Equal("0,0,100,60", e.Attribute("Bounds")?.Value);
    Assert.Empty(e.Elements());   // 자식 트리 미출력
}
```

- [ ] **Step 2: 실패 확인** — FAIL(태그가 GoComponentInstance + 자식 출력).

- [ ] **Step 3: 구현** — `WriteAny` 최상단에 분기 추가

```csharp
    public static XElement WriteAny(object obj, string? explicitTagName = null, ISet<string>? skipChildren = null)
    {
        // 컴포넌트 인스턴스: 이름+파라미터+본인 스칼라만, 자식(템플릿 파생) 생략
        if (obj is Going.UI.Containers.GoComponentInstance ci)
        {
            var ce = new XElement(ci.ComponentName);
            foreach (var prop in ScalarProperties(typeof(Going.UI.Containers.GoComponentInstance)))
            {
                var pv = prop.GetValue(ci);
                if (pv == null) continue;
                var ps = FormatScalar(pv);
                if (ps != null) ce.SetAttributeValue(prop.Name, ps);
            }
            foreach (var kv in ci.ParamValues) ce.SetAttributeValue(kv.Key, kv.Value);
            return ce;
        }

        var type = obj.GetType();
        // ... 기존 로직 유지 ...
```

> 기존 `var type = obj.GetType();` 줄 바로 위에 분기 삽입. 컴포넌트 인스턴스의 PendingBindings(Bounds 등)도 원하면 우선 출력할 수 있으나 1차는 스칼라+파라미터로 충분.

- [ ] **Step 4: 통과 확인** — PASS.
- [ ] **Step 5: 커밋** — `"feat(component): round-trip GoComponentInstance as <Name params/>"`

---

## Task 6: `GoComponentScope` + `WireScoped`

**Files:** Create `Going.UI/Bindings/GoComponentScope.cs`; Modify `GudxBindingExpression.cs`; Test `Going.UI.Tests/Bindings/GudxComponentScopeTests.cs`.

- [ ] **Step 1: 실패 테스트**

```csharp
using Going.UI.Bindings;
using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Gudx;
using System.Xml.Linq;
using Xunit;

namespace Going.UI.Tests.Bindings;

public class GudxComponentScopeTests
{
    public sealed class MotorVM { public double Rpm { get; set; } = 100; }
    public sealed class Outer { public MotorVM PumpA { get; } = new(); }

    private static GoComponentInstance MakeInstance()
    {
        GoComponentTemplate.Registry.Clear();
        GoComponentTemplate.Registry.Register(new GoComponentTemplate
        {
            Name = "MotorCard",
            Params = new()
            {
                new GoComponentParam { Name = "Title", TypeName = "string", Default = "" },
                new GoComponentParam { Name = "motor", TypeName = "MotorVM" },
            },
            Roots = new() { XElement.Parse("<GoBoxPanel><GoLabel Text='{Title}'/><GoLabel Text='{motor.Rpm:F0}'/></GoBoxPanel>") },
        });
        return (GoComponentInstance)Going.UI.Gudx.GoGudxConverter.ReadElement(
            XElement.Parse("<MotorCard Title='펌프 A' motor='{PumpA}'/>"));
    }

    [Fact]
    public void Scope_Build_ResolvesLiteralAndBindingParams()
    {
        var ci = MakeInstance();
        var outer = new Outer();
        var scope = GoComponentScope.Build(ci, outer);

        Assert.Equal("펌프 A", scope.Values["Title"]);
        Assert.Same(outer.PumpA, scope.Values["motor"]);   // 객체 참조 스냅샷
        Assert.Equal(typeof(MotorVM), scope.Types["motor"]);
    }

    [Fact]
    public void WireScoped_ValueParam_OneWay()
    {
        var ci = MakeInstance();
        var outer = new Outer();
        var scope = GoComponentScope.Build(ci, outer);
        var lbl = new GoLabel();
        GudxBindingExpression.WireScoped(lbl, "Text", "{Title}", scope);
        lbl.FireUpdate();
        Assert.Equal("펌프 A", lbl.Text);
    }

    [Fact]
    public void WireScoped_ObjectParamPath_TracksLiveValue()
    {
        var ci = MakeInstance();
        var outer = new Outer();
        var scope = GoComponentScope.Build(ci, outer);
        var lbl = new GoLabel();
        GudxBindingExpression.WireScoped(lbl, "Text", "{motor.Rpm:F0}", scope);

        lbl.FireUpdate();
        Assert.Equal("100", lbl.Text);
        outer.PumpA.Rpm = 250;
        lbl.FireUpdate();
        Assert.Equal("250", lbl.Text);   // 참조 통해 live
    }
}
```

- [ ] **Step 2: 실패 확인** — FAIL.

- [ ] **Step 3: 구현**

`GoComponentScope.cs`:

```csharp
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
```

`GudxBindingExpression.cs`에 추가(공개 헬퍼 + WireScoped + 공통 WireCore 추출):

```csharp
/// <summary>식(`{path}`)을 root에서 평가해 객체 값을 반환(스코프 파라미터 스냅샷용).</summary>
public static object? EvalExpr(string expr, object root)
{
    var (path, _) = Parse(expr);
    if (root is GoComponentScope sc) return EvalScopePath(sc, path);
    return CompilePath(root.GetType(), path).Getter(root);
}

/// <summary>리터럴 문자열을 대상 타입으로 변환(스코프 값 파싱).</summary>
public static object? ParseLiteral(string raw, Type targetType)
{
    if (targetType == typeof(string)) return raw;
    try { return Convert.ChangeType(raw, Nullable.GetUnderlyingType(targetType) ?? targetType, CultureInfo.CurrentCulture); }
    catch { return raw; }
}

private static (object? value, Type type) EvalScopeFirst(GoComponentScope scope, string first)
{
    scope.Values.TryGetValue(first, out var v);
    scope.Types.TryGetValue(first, out var t);
    return (v, t ?? typeof(object));
}

private static object? EvalScopePath(GoComponentScope scope, string path)
{
    int dot = path.IndexOf('.');
    if (dot < 0) { scope.Values.TryGetValue(path, out var v0); return v0; }
    var first = path.Substring(0, dot);
    var rest = path.Substring(dot + 1);
    var (obj, t) = EvalScopeFirst(scope, first);
    if (obj == null) return null;
    return CompilePath(t, rest).Getter(obj);
}

/// <summary>컴포넌트 스코프 기준 바인딩. 첫 구간=스코프, tail=선언 타입 CompilePath 재사용.</summary>
public static void WireScoped(GoControl ctrl, string propName, string expr, GoComponentScope scope)
{
    var pi = ctrl.GetType().GetProperty(propName)
        ?? throw new ArgumentException($"Binding target '{propName}' not found on {ctrl.GetType().Name}.");
    if (!pi.CanWrite) throw new ArgumentException($"Binding target '{propName}' is read-only.");

    var (path, format) = Parse(expr);
    int dot = path.IndexOf('.');
    var targetType = pi.PropertyType;

    Func<object?> srcGet;
    Action<object?>? srcSet = null;

    if (dot < 0)
    {
        // 파라미터 직접 — 단방향
        var key = path;
        if (format != null) srcGet = () => FormatValue(scope.Values.TryGetValue(key, out var v) ? v : null, format);
        else srcGet = () => Coerce(scope.Values.TryGetValue(key, out var v) ? v : null, targetType);
    }
    else
    {
        var first = path.Substring(0, dot);
        var rest = path.Substring(dot + 1);
        var (_, t) = EvalScopeFirst(scope, first);
        var cp = CompilePath(t, rest);
        if (format != null) srcGet = () => FormatValue(cp.Getter(ScopeObj(scope, first)), format);
        else srcGet = () => Coerce(cp.Getter(ScopeObj(scope, first)), targetType);
        if (format == null && cp.Setter != null)
            srcSet = v => cp.Setter!(ScopeObj(scope, first)!, v);
    }

    WireCore(ctrl, pi, srcGet, srcSet);
}

private static object? ScopeObj(GoComponentScope scope, string key)
    => scope.Values.TryGetValue(key, out var v) ? v : null;
```

그리고 기존 `Wire`의 GoBinding 생성부를 `WireCore`로 추출해 양쪽이 공유:

```csharp
private static void WireCore(GoControl ctrl, PropertyInfo pi, Func<object?> srcGet, Action<object?>? srcSet)
{
    var (ctrlGet, ctrlSet) = BuildCtrlAccessors(ctrl.GetType(), pi);
    ctrl.AddOrReplaceBinding(new GoBinding
    {
        CtrlProperty = pi, CtrlGet = ctrlGet, CtrlSet = ctrlSet,
        SourceGet = srcGet, SourceSet = srcSet,
    });
}
```

> 기존 `Wire`도 끝부분(GoBinding 생성)을 `WireCore(ctrl, pi, srcGet, srcSet)` 호출로 교체. 동작 동일(회귀 테스트로 확인).

- [ ] **Step 4: 통과 확인** — PASS (+ 기존 `GudxBindingWireTests` 회귀 PASS).
- [ ] **Step 5: 커밋** — `"feat(component): GoComponentScope + WireScoped (reuses CompilePath)"`

---

## Task 7: `GudxBinder.WireTree` 컴포넌트 분기 + 통합

**Files:** Modify `Going.UI/Bindings/GudxBinder.cs`; Test `GudxComponentScopeTests.cs`.

- [ ] **Step 1: 실패 테스트**

```csharp
// GudxComponentScopeTests.cs 에 추가
[Fact]
public void WireTree_Component_BindsAgainstScope_PerInstance()
{
    GoComponentTemplate.Registry.Clear();
    GoComponentTemplate.Registry.Register(new GoComponentTemplate
    {
        Name = "MotorCard",
        Params = new() { new GoComponentParam { Name = "motor", TypeName = "MotorVM" } },
        Roots = new() { XElement.Parse("<GoBoxPanel><GoLabel Text='{motor.Rpm:F0}'/></GoBoxPanel>") },
    });

    var outer = new TwoMotors();
    var a = (GoComponentInstance)Going.UI.Gudx.GoGudxConverter.ReadElement(XElement.Parse("<MotorCard motor='{PumpA}'/>"));
    var b = (GoComponentInstance)Going.UI.Gudx.GoGudxConverter.ReadElement(XElement.Parse("<MotorCard motor='{PumpB}'/>"));

    GudxBinder.WireTree(a, outer);
    GudxBinder.WireTree(b, outer);

    outer.PumpA.Rpm = 11; outer.PumpB.Rpm = 22;
    a.Childrens[0].FireUpdate(); ((GoBoxPanel)a.Childrens[0]).Childrens[0].FireUpdate();
    b.Childrens[0].FireUpdate(); ((GoBoxPanel)b.Childrens[0]).Childrens[0].FireUpdate();

    var lblA = (GoLabel)((GoBoxPanel)a.Childrens[0]).Childrens[0];
    var lblB = (GoLabel)((GoBoxPanel)b.Childrens[0]).Childrens[0];
    Assert.Equal("11", lblA.Text);
    Assert.Equal("22", lblB.Text);
}

public sealed class TwoMotors { public MotorVM PumpA { get; } = new(); public MotorVM PumpB { get; } = new(); }
```

- [ ] **Step 2: 실패 확인** — FAIL(컴포넌트 분기 없음 → scope 미적용).

- [ ] **Step 3: 구현** — `GudxBinder.WireTree` 수정

```csharp
public static void WireTree(IGoControl node, object root)
{
    // 컴포넌트 경계: root를 인스턴스 파라미터 스코프로 교체 후 자식 wire
    if (node is GoComponentInstance ci)
    {
        var scope = GoComponentScope.Build(ci, root);
        foreach (var child in ci.Childrens) WireTree(child, scope);
        return;
    }

    if (node is GoControl gc && gc.PendingBindings is { } pend)
    {
        foreach (var kv in pend)
        {
            try
            {
                if (root is GoComponentScope scope)
                    GudxBindingExpression.WireScoped(gc, kv.Key, kv.Value, scope);
                else
                    GudxBindingExpression.Wire(gc, kv.Key, kv.Value, root);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GudxBinder] wire failed for {gc.GetType().Name}.{kv.Key} = '{kv.Value}': {ex.Message}");
            }
        }
    }

    if (node is IGoContainer cont)
        foreach (var child in cont.Childrens) WireTree(child, root);
}
```

> 중첩: 자식 컨테이너 재귀 시 `root`(plain 또는 scope) 그대로 전달. 자식이 또 GoComponentInstance면 위 분기가 `Build(ci, root)`로 새 스코프 생성(중첩 해결). 순환은 클론이 레지스트리 기반이라 무한 정의가 없으면 발생 안 함 — 깊이 가드는 Task 8에서.

- [ ] **Step 4: 통과 확인** — PASS.
- [ ] **Step 5: 커밋** — `"feat(component): WireTree swaps root->scope at component boundary"`

---

## Task 8: 중첩 깊이 가드 + 전체 회귀 + CHANGELOG

**Files:** Modify `GudxBinder.cs`(가드), `CHANGELOG.md`.

- [ ] **Step 1: 깊이 가드 테스트**

```csharp
// GudxComponentScopeTests.cs 에 추가 — 순환 정의가 무한 재귀로 죽지 않음
[Fact]
public void WireTree_DepthGuard_DoesNotStackOverflow()
{
    // Self 컴포넌트가 자신을 자식으로 가지면 클론 단계에서 이미 무한 → 클론에 깊이 가드 필요.
    // 여기서는 wire 깊이 가드만 검증: 깊은 정상 중첩이 예외 없이 끝남.
    GoComponentTemplate.Registry.Clear();
    GoComponentTemplate.Registry.Register(new GoComponentTemplate
    {
        Name = "Inner",
        Params = new() { new GoComponentParam { Name = "motor", TypeName = "MotorVM" } },
        Roots = new() { XElement.Parse("<GoLabel Text='{motor.Rpm:F0}'/>") },
    });
    var outer = new TwoMotors();
    var inst = (GoComponentInstance)Going.UI.Gudx.GoGudxConverter.ReadElement(XElement.Parse("<Inner motor='{PumpA}'/>"));
    GudxBinder.WireTree(inst, outer);   // 예외 없이 종료
}
```

- [ ] **Step 2: 실패 확인** — 통과할 수도 있으나, 가드 추가로 안전망 확보(아래 구현).

- [ ] **Step 3: 구현** — `WireTree`에 깊이 매개변수 추가(오버로드 유지)

```csharp
public static void WireTree(IGoControl node, object root) => WireTree(node, root, 0);

private static void WireTree(IGoControl node, object root, int depth)
{
    if (depth > 32)
    {
        System.Diagnostics.Debug.WriteLine("[GudxBinder] nesting depth > 32 — aborting (possible cyclic component).");
        return;
    }
    if (node is GoComponentInstance ci)
    {
        var scope = GoComponentScope.Build(ci, root);
        foreach (var child in ci.Childrens) WireTree(child, scope, depth + 1);
        return;
    }
    if (node is GoControl gc && gc.PendingBindings is { } pend) { /* ... 기존 wire 루프 ... */ }
    if (node is IGoContainer cont)
        foreach (var child in cont.Childrens) WireTree(child, root, depth + 1);
}
```

> 클론(`BuildComponentInstance`) 단계의 순환 가드도 필요하면 동일 깊이 매개변수로 추가(자기참조 정의 방어). 1차는 wire 가드로 안전망 확보, 클론 순환은 "정의가 자신을 직접 포함하지 않는다" 전제(문서화).

- [ ] **Step 4: 전체 테스트** — `dotnet test Going.UI.Tests` → 전부 PASS.

- [ ] **Step 5: CHANGELOG + 커밋**

```markdown
- **Gudx 컴포넌트** — `<GoComponent name><Param/>...`로 재사용 컴포넌트 정의, `<MotorCard .../>`로 인스턴스화. 내부 `{param.path}`는 인스턴스별 파라미터 스코프에 바인딩(평면 바인딩 재사용). 파라미터는 리터럴/`{outer}` 바인딩, `type`으로 내부 경로 타입 컴파일. 라운드트립은 이름+파라미터만 보존(자식은 템플릿 파생). 중첩 지원.
```

```bash
git add -A && git commit -m "feat(component): gudx reusable components + CHANGELOG"
```

---

## Self-Review 메모

- **Spec 커버리지**: D1 등록(T4) / D2 인스턴스(T3,T5) / D3 Param·type(T1) / D4 스코프 wiring(T6,T7) / D5 경로 해석(T6) / D6 중첩(T7,T8) — 1:1.
- **타입 일관성**: `GoComponentParam.ResolveType`, `GoComponentTemplate.Registry.{Register,TryGet,Clear}`, `GoComponentInstance.{ComponentName,ParamValues,Template,Childrens}`, `GoComponentScope.{Values,Types,Build}`, `GudxBindingExpression.{WireScoped,EvalExpr,ParseLiteral,WireCore}`, `GudxBinder.WireTree` — 전 Task 동일.
- **재사용**: 평면 `CompilePath`/`Wire`/펌프/`GoBinding` 무수정(Wire는 WireCore 추출만, 동작 동일).
- **검증 가드**: P2 read 태그 필터 위치(line 831/851), `WriteAny` 진입부, `ScalarProperties` 시그니처는 Step 3 전 코드 확인.
- **1차 비목표 유지**: 슬롯 투영·컴포넌트 이벤트·에디터 GUI·자동 fill·문자열 보간 없음.
