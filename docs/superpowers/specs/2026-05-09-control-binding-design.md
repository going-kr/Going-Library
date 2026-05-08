# Going.UI 컨트롤 바인딩 시스템

**작성일**: 2026-05-09
**대상**: Going.UI (플랫폼 독립 UI 코어)

---

## 목표

- 컨트롤 속성을 임의의 식(필드/프로퍼티/메서드 결과/계산식)에 묶어 자동 동기화
- 단방향(표시) / 양방향(입력) 모두 지원
- 기존 컨트롤 40+개를 거의 수정 없이 바인딩 가능 (입력형 일부만 한 줄 override)
- 사용자 코드는 한 줄, 별도 ViewModel 불필요
- 통신 캐시(`rtu.GetWord`), 일반 객체, View 멤버 어디든 묶임

## 비목표 (이번 범위 밖)

- .gud 에디터에서의 바인딩 GUI 지원 (코드 레벨만)
- INotifyPropertyChanged 등 알림 인터페이스 강요
- DataContext / Path 문자열 체계
- 컬렉션 바인딩 (DataGrid 행 자동 생성 등)

---

## 사용자 API

### 기본 형태

인자 개수가 방향을 결정한다. Expression-tree 마법 없음, 람다 본문은 자유.

```csharp
// 단방향 (표시 전용) — setter 없음
gauge.Bind(c => c.Value,   () => dev.Speed);
lamp.Bind (c => c.Visible, () => dev.IsRunning);
text.Bind (c => c.Text,    () => $"{rtu.GetWord("D0") / 10.0:F1} ℃");

// 양방향 — setter 있음
sw.Bind   (c => c.OnOff, () => dev.OnOff,  v => dev.OnOff = v);
input.Bind(c => c.Value, () => dev.Speed,  v => dev.Speed = v);
slider.Bind(c => c.Value,() => cfg.Limit,  v => cfg.Limit = v);
```

### 시그니처

`Bind`는 **확장 메서드**로 정의한다. 호출부에서 `c => c.Value`의 `c`가 호출 대상의 실제 컨트롤 타입(예: `GoGauge`)으로 추론되어야 컴파일 타임 안전성이 살기 때문 — 인스턴스 메서드로는 `this`를 파생 타입으로 정확히 받기 어렵다.

```csharp
// Going.UI.Bindings 네임스페이스
public static class GoControlBindingExtensions
{
    public static void Bind<TC, TV>(
        this TC ctrl,
        Expression<Func<TC, TV>> ctrlProp,
        Func<TV> getter)
        where TC : GoControl;

    public static void Bind<TC, TV>(
        this TC ctrl,
        Expression<Func<TC, TV>> ctrlProp,
        Func<TV> getter,
        Action<TV> setter)
        where TC : GoControl;

    public static void Unbind<TC, TV>(
        this TC ctrl,
        Expression<Func<TC, TV>> ctrlProp)
        where TC : GoControl;

    public static void UnbindAll(this GoControl ctrl);
}
```

`Bind` 호출 시 `Expression<Func<TC, TV>>`에서 `MemberExpression` 검사로 `PropertyInfo`를 추출하고, `LambdaExpression.Compile()`로 get/set 델리게이트를 만들어 캐시한다. 단순 멤버 접근식이 아니면 `ArgumentException`을 던진다.

### 규칙

- **첫 람다** (`c => c.X`): Expression으로 받아 PropertyInfo 추출, 컴파일된 get/set 델리게이트 캐시
- **두 번째 람다**: 소스 getter — 자유로운 식 가능
- **세 번째 람다** (선택): 소스 setter — 있으면 양방향
- **타입 변환**: 엄격. `TV`가 컨트롤 속성 타입과 정확히 같아야 함. 변환은 람다 안에서 처리
- **중복 바인딩**: 같은 속성에 두 번 호출 시 마지막이 이김(이전 binding은 자동 해제). 디버그 빌드에서 경고 로그
- **해제**: 컨트롤이 GoView에서 제거되거나 GoView가 닫히면 자동 해제. 명시적 해제는 `Unbind`/`UnbindAll`

---

## 동작 메커니즘

### 대칭 폴링 (Symmetric Polling)

이벤트 의존성 없이 **양쪽 모두 매 프레임 폴링**한다. 컨트롤이 `Changed` 이벤트를 노출하든 말든 무관.

```
binding 별 상태:
  lastSrcValue   // 마지막으로 컨트롤에 푸시한 소스 값
  lastCtrlValue  // 마지막으로 setter로 푸시한 컨트롤 값
  pendingFlush   // 조작이 끝난 후 한 번 push 필요 여부

매 프레임 (FireUpdate 시):
  if control.IsBindingSuppressed:
      // 조작 중 — 양방향 모두 정지, 종료 시점 flush 위해 표시
      pendingFlush = (setter != null)
      continue

  if pendingFlush:
      cur = ctrlProp.Get(control)
      setter(cur)
      lastCtrlValue = cur
      pendingFlush = false

  // 소스 → 컨트롤
  newSrc = getter()
  if !Equals(newSrc, lastSrcValue):
      ctrlProp.Set(control, newSrc)
      lastSrcValue = newSrc
      lastCtrlValue = newSrc   // setter 자기 자신 트리거 방지

  // 컨트롤 → 소스 (코드에서 컨트롤 값 변경한 경우 등)
  if setter != null:
      cur = ctrlProp.Get(control)
      if !Equals(cur, lastCtrlValue):
          setter(cur)
          lastCtrlValue = cur
```

### 핵심 동작

- **이벤트 의존성 0**: Going의 기존 컨트롤은 binding을 위해 추가 이벤트를 노출할 필요 없음
- **드래그/편집 폭주 방지**: `IsBindingSuppressed`가 `true`인 동안 양방향 모두 정지
- **조작 종료 flush**: 드래그 끝 / 포커스 빠짐 시 다음 프레임에 한 번만 setter 호출 (Modbus 쓰기 폭탄 방지)
- **루프 방지**: 소스→컨트롤 푸시 직후 `lastCtrlValue`도 같은 값으로 업데이트하여 같은 프레임에 setter 재트리거 방지

---

## "조작 중" 플래그

`GoControl`에 가상 프로퍼티 추가:

```csharp
protected internal virtual bool IsBindingSuppressed => false;
```

- `protected` — 사용자 정의 컨트롤(외부 어셈블리)에서도 override 가능
- `internal` — 같은 어셈블리의 binding 펌프가 직접 읽을 수 있음

컨트롤별 override:

| 컨트롤 | 조건 |
|---|---|
| GoSlider, GoRangeSlider | 드래그 중 |
| GoKnob | 드래그/회전 중 |
| GoInput, GoNumberBox, GoValue | 편집 중(포커스+키 입력) |
| GoSwitch, GoCheckBox, GoToggleButton, GoOnOff, GoLampButton | (default) — 클릭이 즉시 커밋 |
| GoLamp, GoGauge, GoMeter, GoLabel 외 표시 전용 | (default) |

디폴트가 `false`라 표시 전용 컨트롤은 손댈 필요 없음. 입력형도 이미 내부에 드래그/편집 상태 플래그가 있으면 그대로 노출하면 끝.

---

## 통합 지점 (Going.UI 내부)

### GoControl

- 내부 `List<GoBinding>` 필드로 binding 저장 (확장 메서드가 접근할 수 있도록 `internal`)
- `IsBindingSuppressed` 가상 프로퍼티 추가 (`protected internal virtual`, default `false`)
- `FireUpdate()` 안에서 binding 펌프 호출:
  ```csharp
  public void FireUpdate()
  {
      PumpBindings();   // 추가
      OnUpdate();
  }
  ```
- `Dispose()` 시 `UnbindAll()`로 정리

### GoBinding (신규 내부 타입)

```csharp
internal sealed class GoBinding
{
    public PropertyInfo CtrlProperty { get; init; }
    public Func<GoControl, object?> CtrlGet { get; init; }       // 컴파일된 델리게이트
    public Action<GoControl, object?> CtrlSet { get; init; }     // 컴파일된 델리게이트
    public Func<object?> SourceGet { get; init; }
    public Action<object?>? SourceSet { get; init; }             // null이면 단방향
    public object? LastSrcValue;
    public object? LastCtrlValue;
    public bool PendingFlush;
}
```

### 입력형 컨트롤

- 본인의 드래그/편집 상태 플래그를 `IsBindingSuppressed` override로 노출 (한 줄)

### 별도 수정 불필요

- GoView, GoDesign, GoForm 등 컨테이너/프레임워크 코어
- 표시 전용 컨트롤 30+개

---

## 부가 사항

### 에러 처리

- `getter` 또는 `setter` 예외 발생 시: 해당 프레임만 스킵, GoControl 디버그 로그(또는 GoDesign의 onerror 훅)에 기록
- 한 binding의 예외가 다른 binding 또는 그리기를 막지 않음

### 스레드 안전성

- 펌프는 UI 스레드(렌더 루프)에서만 동작
- `getter` 안에서 다른 스레드 데이터(예: 통신 캐시) 접근은 호출자 책임
- Going.Basis 통신 캐시는 이미 스레드 안전(기존 가정 유지)

### 성능

- 컨트롤당 binding 평균 1-3개, 화면당 컨트롤 수십 개 → 프레임당 수백 회 람다 호출. HMI 30/60 fps 환경에서 무시 가능
- Expression → 컴파일된 델리게이트는 `Bind` 호출 시 한 번만 (이후 reflection 없음)
- `Equals` 비교로 변경 없을 시 컨트롤 속성 set 자체를 스킵 → InvalidateVisual 트리거 최소화

### 동등성 비교

- `object.Equals` 사용. `double` 등 부동소수에서 미세한 변동을 거를 수 있게 향후 epsilon 옵션 검토(이번 범위 아님)

---

## 사용 예시 (시나리오)

**Modbus 캐시 표시 + 입력**
```csharp
public class MachineView : GoViewForm
{
    readonly MasterRTU rtu = ...;

    public MachineView()
    {
        InitializeComponent();

        // 표시
        gauge.Bind(c => c.Value,   () => rtu.GetWord("D0") / 10.0);
        lamp.Bind (c => c.OnOff,   () => rtu.GetBit("M0"));
        text.Bind (c => c.Text,    () => $"속도: {rtu.GetWord("D0")/10.0:F1} m/s");

        // 입력
        spdInput.Bind(c => c.Value, () => rtu.GetWord("D100"),
                                    v => rtu.SetWord(1, "D100", (short)v));
        runSwitch.Bind(c => c.OnOff, () => rtu.GetBit("M10"),
                                     v => rtu.SetBit(1, "M10", v));

        // 가시성/활성도 조건
        emergencyLamp.Bind(c => c.Visible, () => rtu.GetBit("M99"));
        startBtn.Bind     (c => c.Enabled, () => !rtu.GetBit("M50"));
    }
}
```

**View 멤버에 묶기 (별도 ViewModel 없이)**
```csharp
public class SettingsView : GoViewForm
{
    public double Threshold { get; set; } = 50;
    public bool   AutoMode  { get; set; }

    public SettingsView()
    {
        InitializeComponent();
        slider.Bind(c => c.Value, () => Threshold,  v => Threshold = v);
        autoSwitch.Bind(c => c.OnOff, () => AutoMode, v => AutoMode = v);
    }
}
```

---

## 결정 사항 요약

| 항목 | 결정 |
|---|---|
| API 형태 | `Bind(c => c.X, getter, [setter])` — 인자 개수가 방향 결정 |
| 소스 모양 | 람다(Func) — View 멤버 / 임의 객체 / 통신 캐시 / 계산식 모두 통합 |
| 변경 감지 | 양쪽 모두 매 프레임 폴링 (이벤트 의존성 없음) |
| 양방향 충돌 | 컨트롤의 `IsBindingSuppressed`로 조작 중 정지, 종료 시 1회 flush |
| 타입 변환 | 엄격 — 변환은 람다 안에서 |
| 생명주기 | 자동 해제 (컨트롤 dispose / view close) |
| 스코프 | 코드 레벨만 (.gud 에디터 통합은 향후 과제) |

---

## 향후 확장 (이번 범위 밖)

- .gud 에디터에서 바인딩 GUI 지원 (Expression 직렬화 또는 별도 Path 표기)
- epsilon 비교(부동소수)
- 컬렉션 바인딩(DataGrid 등)
- 비동기 getter (`Task<T>` 결과)
