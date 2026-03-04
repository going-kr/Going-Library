---
name: going-ui
description: Going Library(Going.UI + Going.Basis)를 사용하는 모든 작업에 반드시 이 스킬을 참조. HMI 화면 설계, 산업용 UI 개발, PLC 연동 앱, 임베디드 GUI, SCADA 화면, 터치패널 UI, .gud 파일 생성/수정, GoButton/GoLamp/GoInput 등 컨트롤 코드 작성, Going.Basis로 Modbus RTU/TCP 마스터·슬레이브 통신, MQTT 클라이언트 코드 작성, GoDesign/GoPage/GoWindow 구조 작업, SkiaSharp 기반 커스텀 렌더링, 테마 색상 적용, Going Library 패턴으로 C# .NET 8.0 코드 작성 등을 요청할 때 사용. Going 관련 코드 작성 시 항상 이 스킬의 패턴과 네이밍을 따를 것.
---

# Going Library Skill

## 작업별 참조 파일

| 작업 | 참조 파일 | 설명 |
|------|----------|------|
| .gud 파일 생성/수정 | `ui-json.md` | JSON 구조, 컨트롤 속성, 컨테이너, 테마, Enum |
| C# 코드 작성 (Page/Window/Main) | `ui-code.md` | 코드 패턴, Designer.cs 규칙, MakePropCode |
| 통신 코드 작성 (Modbus/MQTT) | `basis.md` | 통신 패턴, DeviceManager, 데이터 모델 |

**작업 시작 전 해당 파일을 반드시 `Read` 도구로 읽은 후 작업할 것.**

---

## API 문서 참조 방법

Going Library의 클래스·속성·메서드·이벤트 확인이 필요할 때는 `Read` 도구로 아래 HTML 파일을 직접 읽어 확인한다.
**코드 작성 전 반드시 해당 파일을 확인하고 정확한 속성명·타입·기본값을 사용할 것.**

> **경로 규칙**: 아래 경로는 **이 스킬 파일(.md) 기준 상대경로**. `Read` 도구 사용 시 이 스킬 파일이 위치한 디렉터리를 기준으로 경로를 해석할 것.

### Going.UI

| 경로 (스킬 파일 기준 상대경로) | 내용 |
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

| 경로 (스킬 파일 기준 상대경로) | 내용 |
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
// GoButton 속성 확인 — Read 도구로 스킬 파일 기준 상대경로 읽기
Read("{이 스킬 파일 디렉터리}/api/ui/controls.html")

// MasterRTU/ModbusTCPMaster 메서드 확인
Read("{이 스킬 파일 디렉터리}/api/basis/communications-modbus.html")
```

---

## 전체 워크플로우

Going Library 프로젝트의 전형적인 작업 흐름:

```
1. project_brief.md 작성 (사용자)
   └─ 프로젝트 개요, 화면 구성, 페이지/윈도우 목록
   └─ 통신 방식 (Modbus RTU/TCP, MQTT 등)
   └─ 장치 데이터 모델, 레지스터 맵 (있는 경우)

2. .gud 파일 생성 (Claude — 채팅)
   └─ 사용자가 project_brief.md 내용을 Claude 채팅에 전달
   └─ Claude가 ui-json.md 참조하여 .gud 파일(JSON) 생성
   └─ 페이지/윈도우/컨테이너/컨트롤 배치, 테마 설정 포함
   └─ 주의: Design 필드는 이중 직렬화, SKRect는 좌표값(Width/Height 아님)
   └─ 주의: Enum은 숫자, 모든 컨트롤에 UUID Id 필수

3. UIEditor에서 수정 및 코드 배포 (사용자)
   └─ 생성된 .gud 파일을 UIEditor에서 열기
   └─ 컨트롤 위치/크기 미세 조정, 디자인 확인
   └─ MakeCode 실행 → 아래 파일들 자동 생성:
       ├─ design.json          (GoDesign 직렬화, 런타임 로드용)
       ├─ Program.cs           (진입점, ExistsCheck=true)
       ├─ MainWindow.cs        (사용자 파일, ExistsCheck=true)
       ├─ MainWindow.Designer.cs (자동생성, 항상 덮어씀)
       ├─ Pages/*.cs           (사용자 파일, ExistsCheck=true)
       ├─ Pages/*.Designer.cs  (자동생성, 항상 덮어씀)
       ├─ Windows/*.cs         (사용자 파일, ExistsCheck=true)
       └─ Windows/*.Designer.cs (자동생성, 항상 덮어씀)

4. Claude Code에서 프로젝트 구현 ★핵심 단계★
   └─ 사용자가 생성된 코드 폴더를 작업 디렉터리로 지정
   └─ project_brief.md를 Claude Code에 전달
   └─ Claude Code가 *.cs(사용자 파일)만 작업 (Designer.cs/design.json 절대 수정 금지)

   4-1. UI 코드 작성 (참조: ui-code.md)
        ├─ Program.cs: Main 싱글턴, DataManager/DeviceManager 초기화, Run/Stop
        ├─ MainWindow.cs: 전역 이벤트 바인딩, OnUpdateFrame에서 상태 갱신
        ├─ Pages/*.cs: 컨트롤 이벤트 바인딩(ButtonClicked 등), OnUpdate에서 데이터→UI 반영
        └─ Windows/*.cs: 팝업 동작, 콜백 패턴 (Show → 결과 → Close)

   4-2. 통신 코드 작성 (참조: basis.md)
        ├─ DeviceManager: Modbus Master/Slave 또는 MQTT 구성
        ├─ DeviceData: 레지스터 맵 → 의미있는 프로퍼티 변환 (CommState 패턴)
        ├─ DataManager: 설정 파일(JSON) 로드/저장
        └─ ⚠ project_brief.md에 장치 모델이 없으면 반드시 사용자에게 문의

5. 빌드 및 실행 (사용자)
   └─ dotnet build → dotnet run
   └─ design.json이 실행 디렉터리에 있어야 함
```

> **역할 분담**: 1=사용자, 2=Claude(채팅), 3=사용자(UIEditor), **4=Claude Code(핵심)**, 5=사용자.
> Claude Code는 ***.Designer.cs와 design.json을 절대 수정하지 않으며**, *.cs(사용자 파일)만 작업한다.

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
