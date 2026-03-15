---
name: going-ui
description: Going Library(Going.UI + Going.Basis)를 사용하는 모든 작업에 반드시 이 스킬을 참조. HMI 화면 설계, 산업용 UI 개발, PLC 연동 앱, 임베디드 GUI, SCADA 화면, 터치패널 UI, .gud 파일 생성/수정, GoButton/GoLamp/GoInput 등 컨트롤 코드 작성, Going.Basis로 Modbus RTU/TCP 마스터·슬레이브 통신, MQTT 클라이언트 코드 작성, GoDesign/GoPage/GoWindow 구조 작업, SkiaSharp 기반 커스텀 렌더링, 테마 색상 적용, Going Library 패턴으로 C# .NET 8.0 코드 작성, LauncherTouch MCP 장치 제어, 라즈베리파이 키오스크 배포, mDNS 장치 검색 등을 요청할 때 사용.
---

# Going Library Skill

## 작업별 참조 파일

| 작업 | 참조 파일 | 설명 |
|------|----------|------|
| .gud 파일 생성/수정 | `ui-json.md` | .gud 구조, GoDesign, Pages/Windows, 공통 속성, Enum, 테마 |
| ↳ 컨트롤 JSON | `ui-json-controls.md` | GoButton, GoSlider 등 40여 종 + GoInput/GoValue 계열 |
| ↳ 컨테이너 JSON | `ui-json-containers.md` | GoTableLayoutPanel, GoBoxPanel, GoTabControl 등 9종 |
| ↳ ImageCanvas JSON | `ui-json-imagecanvas.md` | IcButton, IcLabel, IcSlider 등 이미지 기반 7종 |
| C# 코드 작성 (Page/Window/Main) | `ui-code.md` | 코드 패턴, Designer.cs 규칙, MakePropCode |
| 통신 코드 작성 (Modbus/MQTT) | `basis.md` | 통신 패턴, DeviceManager, 데이터 모델 |
| 장치 배포/제어 (LauncherTouch MCP) | `ui-mcp.md` | gtcli CLI 도구, MCP 도구 25개, 빌드-배포 자동화, 네트워크, 키오스크 |
| project_brief.md 작성 가이드 | `ui-project-brief.md` | 템플릿, 이미지 네이밍, 섹션별 작성법 |

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
| `api/ui/datas.html` | GoListItem, GoPadding, GoButtonItem, GoMouseEventArgs, ButtonClickEventArgs, GoDrawnEventArgs 등 데이터/이벤트 클래스 |
| `api/ui/enums.html` | GoContentAlignment, GoRoundType, GoDockStyle, GoKeys 등 전체 열거형 |
| `api/ui/imagecanvas.html` | IcButton, IcLabel, IcOnOff, IcProgress, IcSlider, IcState |

### Going.Basis

| 경로 (스킬 파일 기준 상대경로) | 내용 |
|------|------|
| `api/basis/communications-modbus.html` | ModbusRTUMaster, ModbusRTUSlave, ModbusTCPMaster, ModbusTCPSlave |
| `api/basis/communications-mqtt.html` | MQClient, MQSubscribe, MQReceiveArgs |
| `api/basis/communications-ls.html` | CNet (LS Electric PLC), DataReadEventArgs, WriteEventArgs, TimeoutEventArgs 등 |
| `api/basis/communications-mitsubishi.html` | MC (Mitsubishi PLC), WordDataReadEventArgs, BitDataReadEventArgs, WriteEventArgs 등 |
| `api/basis/datas.html` | INI, Serialize, BitMemory, WordMemory, WordRef, BitAccessor |
| `api/basis/extensions.html` | Bits (비트/바이트 확장 메서드) |
| `api/basis/measure.html` | Chattering, Stable |
| `api/basis/tools.html` | CryptoTool, MathTool, NetworkTool, WindowsTool |
| `api/basis/utils.html` | EasyTask, HiResTimer, ExternalProgram, NaturalSortComparer, CompressionUtility |

### HTML 파일 내부 구조

각 API HTML 파일은 동일한 구조로 되어 있다:
- **클래스별 섹션**: `<div class="class-card" id="class-GoButton">` 형태로 클래스 구분, `<hr class="class-separator">`로 클래스 간 시각적 분리
- **속성 테이블**: `<table class="api-table">` — 타입(`col-type`), 이름(`col-name`, get/set 접근자 표시), 설명(`col-desc`) 컬럼
- **메서드 테이블**: 반환타입(`ret`), 이름(`col-method-name`), 파라미터(`params`), 설명(`col-desc`)
- **이벤트 테이블**: 이벤트 핸들러 타입(`event-args`), 이름(`col-method-name`), 설명(`col-desc`)
- **검색 팁**: `data-search` 속성에 키워드가 있으므로, `Grep`으로 컨트롤명 검색하면 해당 섹션을 빠르게 찾을 수 있음

```
// 예: GoDataGrid의 메서드만 찾기
Grep("GoDataGrid", path="{스킬 디렉터리}/api/ui/controls.html")

// 예: MasterRTU의 GetWord 시그니처 확인
Grep("GetWord", path="{스킬 디렉터리}/api/basis/communications-modbus.html")
```

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
1. project_brief.md 작성

   경로 A (권장) — Claude 채팅에 프로젝트 개요 설명
     → Claude가 아래 순서로 인터뷰 질문:
        ① 프로젝트명, 앱 이름, 목적, 해상도, 테마
        ② 페이지 목록 (각 페이지 역할, 주요 컨트롤, 페이지 간 이동)
        ③ 윈도우(팝업) 목록 (트리거, 역할, 반환값)
        ④ 통신 방식 선택 (Modbus RTU/TCP, MQTT, 없음)
           → Modbus 선택 시: 포트, 보드레이트, 슬레이브 구성, 레지스터 맵
           → MQTT 선택 시: 브로커 주소, 토픽/페이로드 구조
        ⑤ 설정 파일 항목 (DataManager에 저장할 값)
        ⑥ 레이아웃 이미지 요청 (스케치 또는 레퍼런스 HMI 캡처)
     → Claude가 완성된 project_brief.md 생성
     → 사용자 검토 및 이미지 첨부

   경로 B — 템플릿 직접 작성 (ui-project-brief.md 참조)

   ★ Claude Code 진입 전 완비 확인:
     □ 페이지/윈도우 목록 + 레이아웃 이미지 (스케치 또는 레퍼런스)
     □ 통신 방식 및 파라미터
     □ 레지스터 맵 (Modbus 사용 시 필수 — 누락 시 Claude Code 작업 불가)
     □ 설정 파일 항목

   .gud 생성 시: project_brief.md + 이미지를 채팅에 함께 첨부
   플랫폼: Going.UI.OpenTK 고정 (linux-arm64)

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
   └─ 전제: 1~3단계는 이미 완료된 상태. 작업 디렉터리에 Designer.cs와 design.json이 존재함.
   └─ 사용자가 생성된 코드 폴더를 작업 디렉터리로 지정
   └─ project_brief.md를 Claude Code에 전달
   └─ Claude Code가 *.cs(사용자 파일)만 작업 (Designer.cs/design.json 절대 수정 금지)

   4-1. UI 코드 작성 (참조: ui-code.md)
        ├─ Program.cs: Main 싱글턴, DataManager/DeviceManager 초기화, Run/Stop
        ├─ MainWindow.cs: 전역 이벤트 바인딩, OnUpdateFrame에서 상태 갱신
        ├─ Pages/*.cs: 컨트롤 이벤트 바인딩(ButtonClicked 등), OnUpdate에서 데이터→UI 반영
        └─ Windows/*.cs: 팝업 동작, 콜백 패턴 (Show → 결과 → Close)

   4-2. 통신 코드 작성 (참조: basis.md)
        ├─ DeviceManager: MasterRTU/MasterTCP 기본 (GetWord/GetBit + IsOpen)
        │   └─ ModbusRTUMaster / ModbusTCPMaster는 수신 이벤트 직접 제어 시에만 사용
        ├─ DataManager: 설정 파일(JSON) 로드/저장
        └─ ⚠ project_brief.md에 장치 모델이 없으면 반드시 사용자에게 문의

5. 빌드 및 실행 (사용자)
   └─ dotnet build → dotnet run
   └─ design.json이 실행 디렉터리에 있어야 함

6. 장치 배포 (GoingTouchCLI — 참조: ui-mcp.md)
   └─ 6-1. gtcli scan으로 네트워크의 터치 장치 검색
   └─ 6-2. 사용자가 장치 웹 UI에서 MCP 토큰 확인 → Claude Code에 전달
   └─ 6-3. Claude Code가 gtcli로 빌드-배포 자동화:
       ├─ dotnet publish -r linux-arm64 → zip 압축
       ├─ gtcli deploy {host} App.zip "앱이름" AppName true --token {token}
       └─ gtcli hide-ui {host} --token {token}
```

> **역할 분담**: 1=사용자, 2=Claude(채팅), 3=사용자(UIEditor), **4=Claude Code(핵심)**, 5=사용자, 6=사용자(토큰 확인)+**Claude Code(gtcli로 검색→빌드→배포 자동화)**.
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
- DataManager (단, 설정 파일 구조는 사용자 확인 후 작성)
- MainWindow 초기화, 페이지 전환 로직

**확인 없이 작성 불가:**
- DeviceManager (폴링 주소/영역, 수신 데이터 처리)
- 데이터 모델 클래스 (Board, Channel, DeviceData 등 레지스터 맵 의존)
- 쓰기 메서드 (WriteXxx — 주소가 확정되어야 함)

DeviceManager 작성 시 클래스 선택 규칙:
- MasterRTU / MasterTCP를 기본으로 사용할 것
- ModbusRTUMaster / ModbusTCPMaster는 수신 이벤트 직접 제어가
  명시적으로 필요한 경우에만 허용
- MasterRTU / MasterTCP 사용 시 DeviceData 패턴 사용 금지
  (GetWord/GetBit + IsOpen으로 대체)

### 3. 이미지 참조 처리

project_brief.md + 이미지가 함께 제공된 경우:
- 이미지의 레이아웃을 우선 참조
- 텍스트 설명과 이미지가 충돌하면 이미지 우선
- 이미지에서 읽기 어려운 컨트롤은 텍스트로 보완 후 작업
- 이미지만으로 판단 불가한 레지스터 맵은 반드시 문의

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
