# Gudx 선언적 바인딩 (마크업 바인딩)

**작성일**: 2026-06-06
**브랜치**: `hjh-gudx-binding`
**대상**: Going.UI (Gudx 마크업 + 기존 바인딩 레이어)
**선행**: [2026-05-09-control-binding-design.md](2026-05-09-control-binding-design.md) (코드 레벨 바인딩 엔진)

---

## 목표

- gudx 마크업 attribute 값에 바인딩 식을 직접 표기:
  `<GoLabel Text="{Data1.Title}" />`
- 기존 런타임 바인딩 엔진(대칭 폴링 펌프) **재사용** — 새 엔진 없음
- 유저가 주입한 단일 root 객체의 멤버에 경로로 접근, 단방향/양방향 자동
- 유저 코드는 `WireBindings(root)` 한 줄

## 비목표 (1차 범위 밖)

- 문자열 보간(`"속도: {x} m/s"`) — 향후 (아래 "열린 결정" 참조)
- 컬렉션 바인딩(`Items="{App.Logs}"`, DataGrid 행 자동 생성)
- 명령 바인딩(`Command="{App.Start}"`)
- 컨텍스트 상속/override(중첩 컨테이너가 자기 컨텍스트를 덮어쓰기 — WPF DataContext 식)
- 에디터(Going.UIEditor) 바인딩 GUI 저작 — 런타임 우선, 저작은 후속
- INotifyPropertyChanged 등 알림 인터페이스(폴링이라 불필요)

---

## 핵심 결정: root = "유저가 1회 주입하는 단일 컨텍스트 객체"

마크업은 closure가 없다. `{Data1.Title}`는 문자열일 뿐이라, 경로를 **어떤 객체부터 걷기 시작할지(root)** 를 외부에서 주입받아야 한다.

**현 아키텍처 사실**: gudx의 페이지/윈도우는 유저 subclass가 아니라 **plain `GoPage`/`GoWindow`로 역직렬화**되어 `GoDesign.Pages`/`Windows`에 담긴다(데이터). 유저 데이터/코드는 뷰(`MainWindow : GoViewWindow`)나 허브(`Main`) 쪽에 살며 **컨트롤 트리 바깥**이다. 따라서 "Parent 체인을 타고 호스트를 찾는다"로는 유저 데이터에 닿을 수 없다.

**규칙 하나**: 유저가 root 객체 하나를 `GoDesign.WireBindings(root)`로 1회 주입한다. 모든 `{path}`가 그 root에서 출발한다.

```csharp
GoDesign.DeserializeGudx("Master.gudx");   // 페이지=plain GoPage (데이터)
var view = new MainWindow();
view.Design.WireBindings(Main.Current);    // ← root 1회 주입
view.Run();
```

```csharp
public class Main           // 유저의 인스턴스 허브 (라이브러리는 이 타입을 몰라도 됨)
{
    public static Main Current { get; } = new();
    public DataManager DataMgr { get; } = new();
    public DataObject  Data1   { get; } = new();
}
```

```xml
<GoLabel Text="{Data1.Title}" />              <!-- Main.Data1.Title -->
<GoLabel Text="{DataMgr.Tank1.Level:F1}" />   <!-- Main.DataMgr.Tank1.Level, 1자리 포맷 -->
```

엔진은 유저 타입(`Main`)을 컴파일 타임에 몰라도 된다 — `WireBindings(object root)`로 받은 인스턴스의 런타임 타입에서 reflection/Expression으로 경로를 해석한다.

### 왜 단일 root 주입인가 (페이지별 코드비하인드를 안 쓰는 이유)

페이지를 유저 subclass(`PageMain : GoPage`)로 만들어 `Data1`을 들게 하는 안도 가능하지만, 그러려면 **"gudx 태그 → 유저 타입 인스턴스화 + 생명주기" 장치**를 GoDesign/gudx 흐름에 새로 박아야 한다. 현 구조(페이지=데이터, 뷰=코드)와 충돌하고 비용이 크다. 단일 root 주입은 **새 인스턴스화 장치 0**, `WireBindings(root)` 한 줄로 끝나며, 페이지 격리가 필요하면 유저가 root 안에서 `{Pages.Main.Data1.Title}`처럼 네임스페이스를 구성하면 된다.

---

## 문법

| 형태 | 의미 | 방향 |
|---|---|---|
| `{Path.To.Member}` | 호스트 기준 경로 바인딩 | 경로 전 구간 settable이면 양방향, 아니면 단방향 |
| `{Path.To.Member:F1}` | 포맷 지정(대상이 string일 때) | 단방향(포맷은 역변환 불가) |
| `{{` / `}}` | 리터럴 중괄호 | — |
| `값이 {로 시작 안 함` | 일반 리터럴(기존과 동일) | — |

- **전체값 바인딩만**: attribute 값 전체가 하나의 `{...}` 식이어야 한다. 부분 보간(`"속도: {x} m/s"`)은 1차 비목표.
- 바인딩 판정: 값이 `{`로 시작하고 `}`로 끝나며 `{{` escape가 아닐 때. 그 외엔 기존 `ParseScalar` 리터럴 경로 그대로.

---

## 아키텍처: 3 조각 (기존 엔진 위 얇은 껍데기)

### 조각 1 — 파싱 시 `{...}` 감지 → 보류 바인딩 저장

[GoGudxConverter.cs:760](../../../Going.UI/Gudx/GoGudxConverter.cs) `PopulateAny`의 P1 스칼라 읽기 루프에서, attribute 값이 바인딩 식이면 `ParseScalar+SetValue` 대신 **보류 바인딩**으로 저장한다.

```csharp
foreach (var attr in elem.Attributes())
{
    var prop = type.GetProperty(attr.Name.LocalName);
    if (prop == null || !prop.CanWrite) continue;

    if (GudxBindingExpression.IsBinding(attr.Value) && instance is GoControl gc)
    {
        gc.AddPendingBinding(prop.Name, attr.Value);   // ← 신규
        continue;
    }
    var parsed = ParseScalar(prop.PropertyType, attr.Value);
    if (parsed != null) prop.SetValue(instance, parsed);
}
```

`GoControl`에 보류 바인딩 보관소를 추가(직렬화 역할만, 펌프와 무관):

```csharp
// GoControl
internal Dictionary<string, string>? PendingBindings { get; private set; }
internal void AddPendingBinding(string prop, string expr)
    => (PendingBindings ??= new())[prop] = expr;
```

> 라운드트립: 보류 바인딩이 있는 속성은 직렬화 시 리터럴 대신 원본 `{...}` 식을 다시 써야 한다(Write 경로에서 `PendingBindings` 우선). 이로써 gudx ↔ 객체 무손실.

### 조각 2 — wire: `GoDesign.WireBindings(root)` 1회 top-down

유저가 root를 주입하면, GoDesign이 모든 페이지/윈도우(및 바) 트리를 훑으며 보류 바인딩이 있는 컨트롤마다 경로를 **컴파일**하여 기존 바인딩에 연결한다.

```csharp
// GoDesign (신규 public 메서드)
public void WireBindings(object root)
{
    foreach (var page in Pages.Values)   GudxBinder.WireTree(page, root);
    foreach (var wnd  in Windows.Values) GudxBinder.WireTree(wnd, root);
    GudxBinder.WireTree(TitleBar, root);
    GudxBinder.WireTree(LeftSideBar, root);
    GudxBinder.WireTree(RightSideBar, root);
    GudxBinder.WireTree(Footer, root);
}

// GudxBinder (신규) — IGoContainer.Childrens 재귀 순회
internal static void WireTree(IGoControl node, object root)
{
    if (node is GoControl gc && gc.PendingBindings is { } pend)
        foreach (var (propName, expr) in pend)
            GudxBindingExpression.Wire(gc, propName, expr, root);

    if (node is IGoContainer cont)
        foreach (var child in cont.Childrens) WireTree(child, root);
}
```

- 트리 순회는 `IGoContainer.Childrens` 재귀 — Page/Window + 중첩 P2 패널 컨테이너 전부 커버.
- **1차 한계(문서화)**: `GoSwitchPanel.Pages`(SubPage)·`GoTabControl.TabPages`(TabPage) **내부** 컨트롤은 `Childrens`에 직접 없어 1차 wire 대상 아님. 후속에서 wrapper 순회 추가. (아키텍처는 그대로 확장 가능)
- 호출 시점: 유저가 `Init()` 이후 `WireBindings(root)` 호출. (자동화는 후속 — 1차는 명시 호출)

### 조각 3 — 펌프는 기존 것 그대로

`GudxBindingExpression.Wire`는 경로를 컴파일해 `AddOrReplaceBinding`을 호출할 뿐. 이후 [GoControl.cs:326](../../../Going.UI/Controls/GoControl.cs) `FireUpdate → PumpBindings`가 매 프레임 동기화. **펌프·대칭 폴링·IsBindingSuppressed 전부 재사용.**

---

## path → 델리게이트 컴파일 (`GudxBindingExpression`)

신규 정적 클래스 `Going.UI.Bindings.GudxBindingExpression`.

- **getter**: `"Data1.Title"` → Expression tree로 `(object root) => ((PageMain)root).Data1.Title` 빌드 후 `Compile()`. 매 프레임 리플렉션 없음.
- **setter(양방향)**: 경로 **전 구간이 settable**(각 단계가 `{ get; set; }` 또는 settable)이면 setter도 빌드. 마지막 구간만 set 가능해도 중간 객체 참조가 안정적이면 동작. 하나라도 set 불가면 단방향.
- **타입 일치**: 컨트롤 속성 타입 == 경로 말단 타입이면 그대로. 대상이 `string`이고 소스가 비-string이면 `ToString()`(단방향). 그 외 불일치는 wire 에러 로그 후 해당 바인딩 스킵.
- **포맷**: `:F1` 등 지정 시 getter가 `string.Format`/`IFormattable.ToString(fmt)` 결과를 반환(단방향, 대상 string 필수).
- **캐싱**: `(rootType, path, format)` 키로 컴파일된 델리게이트 캐시 → 같은 페이지 재오픈 시 컴파일 0.

### wire 결과를 기존 Bind에 연결

`GudxBindingExpression.Wire`는 컨트롤 속성 PropertyInfo + 컴파일된 src getter/setter로 `GoBinding`을 만들어 `AddOrReplaceBinding`. (기존 `BindInternal`과 동일 구조 — 소스가 람다 대신 컴파일된 경로 델리게이트일 뿐.)

---

## 성능

| 단계 | 언제 | 비용 |
|---|---|---|
| 파싱 시 `{...}` 감지 | gudx 역직렬화 1회 | 문자열 검사(무시 가능) |
| wire(search+compile) | 호스트 활성화 1회 | Parent 탐색 없음(top-down) + `Expression.Compile()` |
| 펌프 | 매 프레임 | **컴파일된 델리게이트 호출 1번** — search·리플렉션 0 |

- `Expression.Compile()`은 바인딩당 수십~백 μs. 페이지당 수십 개면 열 때 1~3ms(비가시). 캐싱으로 재오픈 시 0.
- 매 프레임 비용은 기존 코드 레벨 바인딩과 동일.

---

## 에러 처리

- **wire 시점**: 경로 해석 실패(멤버 없음/타입 불일치) → Debug 로그 + GoDesign onerror 훅, 해당 바인딩만 스킵(다른 바인딩·렌더 영향 없음).
- **런타임 getter/setter 예외**: 기존 펌프의 per-binding try/catch가 이미 처리(해당 프레임만 스킵).
- 잘못된 식 문법(`{` 닫힘 없음 등) → wire 에러로 처리, 리터럴로 강등하지 않음(조용한 오작동 방지).

---

## 통합 지점 요약

| 파일 | 변경 |
|---|---|
| `Going.UI/Bindings/GudxBindingExpression.cs` | **신규** — `IsBinding`, `Parse`, `Wire`, 경로 Expression 컴파일, 캐시 |
| `Going.UI/Bindings/GudxBinder.cs` | **신규** — `WireTree` 트리 순회 wire |
| `Going.UI/Controls/GoControl.cs` | `PendingBindings` 보관소 + `AddPendingBinding` (직렬화 역할) |
| `Going.UI/Gudx/GoGudxConverter.cs` | `PopulateAny`(764) 읽기 루프에 `{...}` 분기; `WriteAny`(228) 보류식 우선 |
| `Going.UI/Design/GoDesign.cs` | `WireBindings(object root)` public 메서드 |

기존 펌프(`PumpBindings`), `GoBinding`, `AddOrReplaceBinding`, `IsBindingSuppressed`는 **무수정**.

---

## 테스트 전략

- **파싱**: `IsBinding` 판정(`{x}`/`{{`/리터럴/포맷 접미사), 보류 바인딩 저장.
- **경로 컴파일**: 단일/다단(`A.B.C`) getter, 전 구간 settable → setter 생성, 일부 불가 → 단방향, 타입 불일치 처리.
- **wire**: 가짜 호스트(테스트용 root 객체) + 컨트롤 트리 → wire 후 펌프 1틱에 소스→컨트롤 반영.
- **양방향**: 컨트롤 값 변경 → 소스 반영, `IsBindingSuppressed` 중 정지/flush.
- **포맷**: `{X:F1}` 출력 문자열, 단방향 강등.
- **라운드트립**: `{...}` 식이 gudx 재직렬화에서 보존(리터럴화 안 됨).
- **캐시**: 동일 `(rootType, path)` 재컴파일 안 함.

---

## 열린 결정 (유저 리뷰에서 확정)

1. **문자열 보간 1차 포함 여부**: `"{Tank1.Level:F1} ℃"`처럼 리터럴+바인딩 혼합. HMI Text 표시에 매우 흔하지만(단위 접미사), 본질적으로 단방향이고 composite formatter가 필요해 복잡도↑. → 1차는 **전체값 단일 식**만, 보간은 첫 후속으로 빼는 안을 권장. (이견 있으면 조정)
2. **wire 자동 호출**: 1차는 유저가 `WireBindings(root)`를 명시 호출. `Init()`에 흡수하거나 뷰에서 자동 호출하는 건 후속(root 주입 시점 때문에 1차는 명시가 안전).

---

## 결정 요약

| 항목 | 결정 |
|---|---|
| root | 유저가 `GoDesign.WireBindings(root)`로 1회 주입하는 단일 컨텍스트 객체 |
| 페이지 코드비하인드 | 도입 안 함 — 페이지는 plain GoPage(데이터) 유지, 새 인스턴스화 장치 0 |
| 문법 | 전체값 `{path}` / `{path:fmt}`, `{{`escape. 보간은 후속 |
| 방향 | 경로 말단 settable → 양방향, 아니면/포맷 → 단방향 |
| 평가 | wire 1회 Expression 컴파일, 매 프레임 델리게이트 호출 |
| 엔진 | 기존 펌프·GoBinding·IsBindingSuppressed 재사용(무수정) |
| 신규 코드 | `GudxBindingExpression` + `GudxBinder` + GoControl 보류 보관소 + 파서/직렬화 분기 |
| wiring | `WireBindings(root)` → 트리 top-down 1회. 1차는 명시 호출 |
