---
name: going-ui
description: Going Library(Going.UI + Going.Basis)를 사용하는 모든 작업에 반드시 이 스킬을 참조. 사용자가 Going UI로 화면 설계, .gud 파일 생성, GoButton/GoLamp/GoInput 등 컨트롤 코드 작성, Going.Basis로 Modbus/MQTT/통신 코드 작성, GoDesign/GoPage/GoWindow 구조 작업, 테마 색상 적용, Going Library 패턴으로 C# 코드 작성 등을 요청할 때 사용. Going 관련 코드 작성 시 항상 이 스킬의 패턴과 네이밍을 따를 것.
---

# Going Library Skill

## 작업별 참조 파일

| 작업 | 참조 파일 | 설명 |
|------|----------|------|
| .gud 파일 생성/수정 | `ui-json.md` | JSON 구조, 컨트롤 속성, 컨테이너, 테마, Enum |
| C# 코드 작성 (Page/Window/Main) | `ui-code.md` | 코드 패턴, Designer.cs 규칙, MakePropCode |
| 통신 코드 작성 (Modbus/MQTT) | `basis.md` | 통신 패턴, DeviceManager, 데이터 모델 |

**작업 시작 전 해당 파일을 반드시 view로 읽은 후 작업할 것.**

---

## API 문서 참조 방법

Going Library의 클래스·속성·메서드·이벤트 확인이 필요할 때는 `view` 도구로 아래 HTML 파일을 직접 읽어 확인한다.
**코드 작성 전 반드시 해당 파일을 확인하고 정확한 속성명·타입·기본값을 사용할 것.**

### Going.UI

| 파일 | 내용 |
|------|------|
| `api/ui/design.html` | GoDesign, GoPage, GoWindow, GoTitleBar, GoSideBar, GoFooter |
| `api/ui/controls.html` | 전체 컨트롤 (GoButton, GoLabel, GoInput*, GoValue*, GoDataGrid, GoSlider, GoKnob, GoStep, GoProgress 등 40여 종) |
| `api/ui/containers.html` | GoBoxPanel, GoPanel, GoGroupBox, GoScrollablePanel, GoScalePanel, GoTabControl, GoTableLayoutPanel, GoGridLayoutPanel, GoSwitchPanel |
| `api/ui/dialogs.html` | GoDialogs, GoMessageBox, GoInputBox, GoSelectorBox |
| `api/ui/themes.html` | GoTheme, 색상 팔레트, ToColor() |
| `api/ui/collections.html` | ObservableList\<T\> |
| `api/ui/datas.html` | GoListItem, GoPadding, GoButtonItem, GoMouseEventArgs 등 데이터 클래스 |
| `api/ui/enums.html` | GoContentAlignment, GoRoundType, GoDockStyle, GoKeys 등 전체 열거형 |
| `api/ui/imagecanvas.html` | IcButton, IcLabel, IcOnOff, IcProgress, IcSlider, IcState |

### Going.Basis

| 파일 | 내용 |
|------|------|
| `api/basis/communications-modbus.html` | ModbusRTUMaster, ModbusRTUSlave, ModbusTCPMaster, ModbusTCPSlave |
| `api/basis/communications-mqtt.html` | MQClient, MQSubscribe, MQReceiveArgs |
| `api/basis/communications-ls.html` | CNet (LS Electric PLC) |
| `api/basis/communications-mitsubishi.html` | MC (Mitsubishi PLC) |
| `api/basis/datas.html` | INI, BitMemories, WordMemories, BYTE, WORD, Serialize |
| `api/basis/extensions.html` | Bits (비트/바이트 확장 메서드) |
| `api/basis/measure.html` | Chattering, Stable |
| `api/basis/tools.html` | CryptoTool, MathTool, NetworkTool, WindowsTool |
| `api/basis/utils.html` | EasyTask, HiResTimer, ExternalProgram, NaturalSortComparer, CompressionUtility |

### 사용 예시

```
// GoButton 속성 확인이 필요할 때 — Read 도구로 HTML 파일 읽기
Read("api/ui/controls.html")

// MasterRTU/ModbusTCPMaster 메서드 확인이 필요할 때
Read("api/basis/communications-modbus.html")
```

---

## 절대 규칙

### 1. Designer.cs / design.json 수정 금지

`*.Designer.cs`와 `design.json`은 UIEditor가 자동 생성하는 파일.
**절대 수정하지 말 것.** 작업 대상은 `*.cs` (사용자 파일)만.

### 2. 데이터 구조 / 레지스터 맵 임의 작성 금지

Modbus 레지스터 주소, 데이터 의미, 단위, 슬레이브 번호는 프로젝트마다 완전히 다르다.
사용자 확인 없이 DeviceManager, 데이터 모델을 임의로 설계하면 실제 장비와 맞지 않는 코드가 된다.

**반드시 확인 후 작성:**
1. **장치 데이터 모델** — 사용자의 `project_brief.md`에 장치 데이터 모델(레지스터 맵, 데이터 구조)이 없으면 **반드시 사용자에게 문의**. 모델과 모드버스 영역(어떤 주소에 어떤 데이터가 있는지)을 확인해야 함
2. **레지스터 맵** — 시작 주소, 각 주소의 데이터 의미, 단위, 읽기/쓰기 구분
3. **슬레이브 구성** — 슬레이브 번호, 보드/채널 수량
4. **FC 코드** — FC3/FC1/FC6/FC16 등
5. **데이터 클래스 구조** — 사용자가 제공하거나 명시적으로 승인한 구조만 사용

> **필수 문의 규칙**: `project_brief.md`가 제공되었으나 장치 데이터 모델이 누락된 경우, 코드를 작성하기 전에 반드시 사용자에게 "장치의 데이터 모델과 모드버스 영역 정보를 알려주세요"라고 문의해야 한다. 모델 없이 DeviceData/DeviceManager를 임의 작성하면 실제 장비와 맞지 않는 코드가 생성된다.

**확인 없이 작성 가능:**
- UIEditor 코드 생성 후 컨트롤 이벤트 바인딩
- Page/Window UI 로직
- DataManager (설정 파일 구조는 사용자 확인 후)
- MainWindow 초기화, 페이지 전환 로직

**확인 없이 작성 불가:**
- DeviceManager (AutoWordRead 주소, WordReadReceived 처리)
- 데이터 모델 클래스 (Board, Channel, DeviceData 등 레지스터 맵 의존)
- 쓰기 메서드 (WriteXxx — 주소가 확정되어야 함)

---

## .gud 파일 직렬화 체인

```
.gud 파일 (Project JSON)
  └─ JsonOpt.Options + ProjectConverter
       └─ Design 필드: GoDesign을 JSON 문자열로 이중 직렬화
            └─ GoDesign.JsonSerialize() → GoJsonConverter.Options
                 ├─ GoControlConverter: { "Type": "GoButton", "Value": { ... } }
                 ├─ GoPagesConverter: 페이지별 Type/Value 래핑
                 ├─ GoWindowsConverter: 윈도우별 Type/Value 래핑
                 ├─ SKColorConverter: uint (ARGB)
                 └─ SKRectConverter: "Left,Top,Right,Bottom" 문자열
```

> .gud 파일의 `Design` 필드는 **이스케이프된 JSON 문자열** (이중 직렬화).
> UIEditor의 `JsonOpt.ProjectConverter`가 `GoDesign.JsonSerialize()`/`JsonDeserialize()`를 호출하여 처리.

---

## 직렬화 핵심 규칙

| 규칙 | 설명 |
|------|------|
| 컨트롤 래퍼 | `{ "Type": "GoButton", "Value": { ...속성... } }` |
| SKRect (Bounds) | `"Left,Top,Right,Bottom"` 문자열. **Right/Bottom은 좌표값 (Width/Height 아님!)** 예: 좌=10, 상=10, 폭=200, 높=50 → `"10,10,210,60"` |
| Enum | **숫자**로 직렬화 (문자열 ❌) |
| Margin/TextPadding | `{ "Left": 0, "Top": 0, "Right": 0, "Bottom": 0 }` JSON 객체 |
| Dock | 모든 컨트롤에 `"Dock": 0` (GoDockStyle enum, 기본 None=0, Fill=5) 필드 존재. **"Fill" 프로퍼티는 없음** |
| Id | 모든 컨트롤에 UUID 형식의 `"Id"` 필드 필수 |
| 기본 폰트 | `"나눔고딕"`, FontSize 기본: `12` (대부분 컨트롤) |

---

## 프로젝트 구조

```
Going.UI          - 플랫폼 독립 UI 코어 (컨트롤, 컨테이너, 테마, 디자인)
Going.UI.Forms    - WinForms 어댑터
Going.UI.OpenTK   - OpenTK 어댑터 (임베디드/라즈베리파이)
Going.Basis       - 통신 및 유틸리티 (Modbus, MQTT, 메모리, 직렬화)
Going.UIEditor    - 비주얼 에디터 (.gud 파일 생성 도구)
```
