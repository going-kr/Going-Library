---
name: going-ui
description: "Going Library 스킬. 다음 작업 시 반드시 참조 — (1) .gud 파일 생성·수정 (GoDesign/GoPage/GoWindow JSON 직렬화), (2) GoButton/GoLamp/GoInput/GoDataGrid 등 Going.UI 컨트롤 C# 코드 작성·이벤트 바인딩·OnUpdate 데이터 연동, (3) Going.Basis Modbus RTU/TCP 마스터·슬레이브 통신·MQTT·LS CNet·Mitsubishi MC PLC 연동 코드, (4) LauncherTouch MCP 장치 배포·키오스크 설정. 트리거 키워드: Going, GoButton, GoLamp, GoSlider, GoInput, GoValue, GoDesign, .gud, Modbus, MasterRTU, SlaveRTU, MQTT, HMI, SCADA, PLC, LauncherTouch, gtcli."
---

# Going Library Skill

Going Library는 산업용 HMI/SCADA 화면을 만들기 위한 C# .NET 8.0 UI 프레임워크. SkiaSharp 기반 커스텀 렌더링으로 GoButton, GoLamp, GoDataGrid 등 40여 종의 산업용 컨트롤을 제공하며, Going.Basis로 Modbus RTU/TCP, LS Electric CNet, Mitsubishi MC, MQTT 통신을 지원한다. 주 타겟은 라즈베리파이(linux-arm64) 터치패널이며 Windows에서도 동작한다.

| 패키지 | 버전 |
|--------|------|
| Going.Basis | 1.0.4 |
| Going.UI | 1.0.8 |
| Going.UI.OpenTK | 1.0.8 |

> 스킬 최종 갱신: 2026-03-16

---

## 작업별 참조 파일

| 작업 | 참조 파일 | 설명 |
|------|----------|------|
| .gud 빠른 생성 (1파일) | `ui-json-quick.md` | .gud 전체 구조 + 전 컨트롤 속성 압축본. **이 파일만으로 .gud 생성 가능** |
| 이미지 → .gud 변환 절차 | `ui-image-to-gud.md` | HMI 캡처/스케치 이미지를 .gud로 변환하는 6단계 절차 (인벤토리→그루핑→컨테이너→도식→검증→생성) |
| .gud 파일 상세 참조 | `ui-json.md` | .gud 구조, GoDesign, Pages/Windows, 공통 속성, Enum, 테마 |
| ↳ 기본 컨트롤 JSON | `ui-json-controls.md` | GoLabel, GoButton, GoLamp, GoSlider 등 기본 14종 |
| ↳ 확장 컨트롤 JSON | `ui-json-controls-ext.md` | GoDataGrid, GoGauge, GoBarGraph, GoNavigator 등 확장 19종 |
| ↳ GoInput/GoValue JSON | `ui-json-input-value.md` | GoInputString, GoInputNumber 등 입력 7종 + GoValue 표시 3종 |
| ↳ 컨테이너 JSON | `ui-json-containers.md` | GoTableLayoutPanel, GoBoxPanel, GoTabControl 등 9종 |
| ↳ ImageCanvas JSON | `ui-json-imagecanvas.md` | IcButton, IcLabel, IcSlider 등 이미지 기반 7종 |
| C# 코드 작성 (Page/Window/Main) | `ui-code.md` | 코드 패턴, Designer.cs 규칙, MakePropCode |
| ↳ 복잡한 컨트롤 C# 코드 | `ui-control-sample.md` | GoBarGraph, GoLineGraph, GoTimeGraph, GoTrendGraph, GoGauge, GoMeter, GoKnob, GoSwitchPanel, GoDataGrid의 C# 바인딩·시리즈 설정·데이터 업데이트 예제. **로드 조건: 위 컨트롤 중 하나라도 C# 코드에서 다룰 때 읽을 것** |
| 통신 코드 작성 (Modbus/MQTT) | `basis.md` | 통신 패턴, DeviceManager, 데이터 모델 |
| 장치 배포/제어 (LauncherTouch MCP) | `ui-mcp.md` | gtcli CLI 도구, MCP 도구 25개, 빌드-배포 자동화, 네트워크, 키오스크 |
| project_brief.md 작성 가이드 | `ui-project-brief.md` | 템플릿, 이미지 네이밍, 섹션별 작성법 |

**작업 시작 전 해당 파일을 반드시 `Read` 도구로 읽은 후 작업할 것.**

### 로드 순서 가이드

작업 유형에 따라 아래 순서로 파일을 읽는다. **SKILL.md는 항상 먼저 로드됨** (진입점).

| 작업 | 1순위 | 2순위 | 3순위 (필요 시) |
|------|-------|-------|----------------|
| .gud 파일 생성 | `ui-json-quick.md` (이것만) | 상세 확인 필요 시: `ui-json.md` / `ui-json-controls.md` 등 개별 파일 | — |
| C# 코드 작성 | `ui-code.md` | `basis.md` (통신 있을 때) | `ui-control-sample.md` (복잡한 컨트롤) |
| 통신만 구현 | `basis.md` | — | — |
| 장치 배포 | `ui-mcp.md` | — | — |
| 프로젝트 시작 | `ui-project-brief.md` | `ui-json.md` | `ui-code.md` |

> 모든 파일을 한꺼번에 로드하지 말 것. 작업에 필요한 파일만 순서대로 읽어 컨텍스트를 절약한다.

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

### API HTML 로드 전략

| 상황 | 도구 | 이유 |
|------|------|------|
| 특정 속성/메서드/이벤트명 확인 | `Grep("속성명", path="api/...")` | 해당 섹션만 빠르게 찾음 — 컨텍스트 절약 |
| 처음 다루는 클래스의 전체 API 파악 | `Read("api/...")` | 속성·메서드·이벤트를 모두 봐야 올바른 코드 작성 가능 |
| 클래스명은 아는데 정확한 시그니처가 불확실 | `Grep("클래스명", path="api/...")` | 클래스 섹션 전체를 반환 |

> **원칙**: 이미 알고 있는 API를 재확인할 때는 `Grep`, 처음 접하는 클래스를 학습할 때는 `Read`.

```
// 예: GoDataGrid의 메서드만 찾기
Grep("GoDataGrid", path="{스킬 디렉터리}/api/ui/controls.html")

// 예: MasterRTU의 GetWord 시그니처 확인
Grep("GetWord", path="{스킬 디렉터리}/api/basis/communications-modbus.html")
```

### 사용 예시

```
// GoButton 속성 확인
Read("./api/ui/controls.html")

// MasterRTU/ModbusTCPMaster 메서드 확인
Read("./api/basis/communications-modbus.html")
```

> **API HTML 조회가 불가능한 환경**(채팅 등)에서는:
>   1. 이 스킬 MD 파일들에 기재된 패턴·속성명·예시 코드만 사용
>   2. MD 파일에 없는 속성/메서드는 **"이 API는 확인이 필요합니다 — Claude Code에서 `api/` HTML을 조회하거나, 공식 문서를 참고하세요"** 라고 안내
>   3. 절대 규칙 #2(속성/메서드 추측 금지)가 더 엄격하게 적용됨 — 확실하지 않으면 코드에 포함하지 말 것

---

## 전체 워크플로우

Going Library 프로젝트의 전형적인 작업 흐름:

### 환경별 작업 범위

| 작업 | Claude 채팅 | Claude Code |
|------|:-----------:|:-----------:|
| 1. project_brief.md 작성 | O | O |
| 2. .gud 파일(JSON) 생성 | O | O |
| 3. UIEditor 조정 + MakeCode | — (사용자) | — (사용자) |
| 4. C# 코드 구현 (*.cs 작성) | **X** | **O** |
| 5. 빌드 및 실행 | — (사용자) | O (`dotnet build`) |
| 6. 장치 배포 (gtcli) | **X** | **O** |

> **채팅 환경**은 파일 시스템 접근(`Read`/`Edit`/`Bash`)이 없으므로 코드 구현·빌드·배포가 불가능하다.
> project_brief.md 작성과 .gud JSON 생성은 대화만으로 완료 가능하므로 양쪽 환경 모두 지원.

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
   └─ 주의: .gud = GoDesign 단일 JSON, SKRect는 좌표값(L,T,R,B 형식, Width/Height 아님)
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

### 2. 속성/메서드 추측 금지 — API 문서 확인 필수

이 스킬 문서에 기재되지 않은 속성명, 메서드 시그니처, 이벤트명을 추측하여 사용하지 말 것.
확실하지 않으면 **반드시 `api/` 디렉터리의 HTML 파일을 `Read` 또는 `Grep`으로 확인** 후 사용한다.

- 컨트롤 속성/이벤트 → `api/ui/controls.html`
- 컨테이너 속성 → `api/ui/containers.html`
- 통신 클래스 메서드 → `api/basis/communications-*.html`
- 테마/다이얼로그/컬렉션 → `api/ui/themes.html`, `api/ui/dialogs.html`, `api/ui/collections.html`

> Going Library는 WPF/WinForms/Avalonia 등 다른 프레임워크와 API가 다르다. 다른 프레임워크 지식으로 속성명을 유추하면 높은 확률로 틀린다.

### 3. 코드 네이밍 — 영문+숫자만 사용

변수명, 속성명, 메서드명, 클래스명 등 코드에서 사용하는 모든 식별자는 **반드시 영문+숫자**로 작성.
한글 식별자는 절대 사용 금지. 대신 **주석으로 한글 명칭을 기입**하여 가독성을 확보한다.

```csharp
// ✅ 올바른 예
public double CondTank1Inlet => ...;  // T01-유증기응축탱크1단유입
public void SetTargetTemp(double value) ...  // 설정-정상운전 가동 온도

// ❌ 잘못된 예
public double 응축탱크1단유입 => ...;
public void Set정상운전가동온도(double value) ...
```

### 4. 데이터 구조 / 레지스터 맵 임의 작성 금지

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
- DeviceData 사용 시 MasterRTU / MasterTCP 기반을 권장
  (ID(slaveId) 필수, 읽기=GetWord/GetBit 래핑, 쓰기=Set{기능이름}()로 SetWord/SetBit 래핑)

### 5. .gud 파일 Pages/Windows는 반드시 Dictionary 형태

.gud Design 내부의 Pages와 Windows는 **Array `[]`가 아닌 Dictionary `{}` 형태**로 작성해야 한다.
각 항목은 `"Type"` + `"Value"` 래핑 객체로 감싸야 하며, Key는 페이지/윈도우의 Name이다.
컨트롤 배열 프로퍼티명은 `"Childrens"` (Controls 아님).

```
✅ "Pages": { "PageMain": { "Type": "GoPage", "Value": { "Childrens": [...], ... } } }
❌ "Pages": [{ "Type": "GoPage", "Name": "PageMain", "Controls": [...] }]

✅ "Windows": { "Keypad": { "Type": "GoWindow", "Value": { "Childrens": [...], ... } } }
❌ "Windows": [{ "Type": "GoWindow", "Name": "Keypad" }]
```

Array로 작성하면 GoPagesConverter/GoWindowsConverter에서 `Expected StartObject` 예외 발생.

### 6. 이미지 참조 처리

project_brief.md + 이미지가 함께 제공된 경우:
- 이미지의 레이아웃을 우선 참조
- 텍스트 설명과 이미지가 충돌하면 이미지 우선
- 이미지에서 읽기 어려운 컨트롤은 텍스트로 보완 후 작업
- 이미지만으로 판단 불가한 레지스터 맵은 반드시 문의

### 7. .gud 파일 생성 시 Python/스크립트 사용 금지

.gud 파일은 순수 JSON이다. `Write` 도구로 직접 JSON 텍스트를 생성하라.
Python, Node.js 등 스크립트를 작성하여 JSON을 생성하지 말 것.
UUID 생성도 스크립트를 사용하지 말고 직접 작성하라.

- .gud는 단일 GoDesign JSON — 이스케이프 처리 불필요
- AI가 JSON을 직접 작성하는 것이 스크립트 작성+실행보다 빠름
- 디버깅 불필요 — JSON 구조만 정확하면 됨
- UUID는 컨트롤마다 **고유하게 순차 생성** — 중복 금지 (예: `00000001-0000-0000-0000-000000000001`, `...-002`, `...-003` 순서도 허용)

---

## .gud 파일 직렬화 체인

```
.gud 파일 = GoDesign JSON (단일 직렬화)
  └─ GoDesign.JsonSerialize() → GoJsonConverter.Options
       ├─ GoControlConverter: { "Type": "GoButton", "Value": { ... } }
       ├─ GoPagesConverter: 페이지별 Type/Value 래핑
       ├─ GoWindowsConverter: 윈도우별 Type/Value 래핑
       ├─ SKColorConverter: uint (ARGB)
       └─ SKRectConverter: "Left,Top,Right,Bottom" 문자열
```

> .gud 파일은 GoDesign 객체를 **직접 직렬화**한 단일 JSON.
> Project 래퍼 클래스와 이중 직렬화는 제거됨.

---

## 직렬화 핵심 규칙

> 컨트롤별 상세 규칙은 `ui-json.md`의 "자주 하는 실수" 섹션 참조.

| 규칙 | 올바른 예 | 흔한 실수 |
|------|----------|----------|
| 컨트롤 래퍼 | `{ "Type": "GoButton", "Value": { ... } }` | Value 없이 속성 직접 나열 |
| Bounds (SKRect) | `"10,10,210,60"` (L,T,**R,B** 좌표) | `"10,10,200,50"` (W,H로 착각) |
| Enum | `0`, `1`, `5` (숫자) | `"Normal"`, `"Fill"` (문자열 ❌) |
| Margin/TextPadding | `{"Left":0,"Top":0,"Right":0,"Bottom":0}` | `"0,0,0,0"` (문자열 ❌) |
| Dock=Fill | `"Dock": 5` | `"Fill": true` (Fill 속성 없음 ❌) |
| Id | `"Id": "550e8400-..."` (UUID) | Id 필드 누락 |

---

## 프로젝트 구조

```
Going.UI          - 플랫폼 독립 UI 코어 (컨트롤, 컨테이너, 테마, 디자인)
Going.UI.Forms    - WinForms 어댑터
Going.UI.OpenTK   - OpenTK 어댑터 (임베디드/라즈베리파이)
Going.Basis       - 통신 및 유틸리티 (Modbus, MQTT, 메모리, 직렬화)
Going.UIEditor    - 비주얼 에디터 (.gud 파일 생성 도구)
```

---

## 트러블슈팅

### 빌드 에러

| 증상 (에러 메시지/로그) | 원인 | 해결 |
|------|------|------|
| `FileNotFoundException: design.json` 또는 `Could not find file 'design.json'` | UIEditor에서 MakeCode 미실행, 또는 design.json이 `bin/` 출력 디렉터리에 없음 | UIEditor에서 MakeCode 실행. csproj에 `<Content Include="design.json"><CopyToOutputDirectory>Always</CopyToOutputDirectory></Content>` 추가 |
| `NullReferenceException` in `*.Designer.cs` (예: `tbl.Controls["btn1"]` → null) | .gud에서 컨트롤 Name을 변경했으나 Designer.cs를 재생성하지 않음 | **Designer.cs 직접 수정 금지** — UIEditor에서 .gud 열고 MakeCode 재실행 |
| `JsonException` in `GoDesign.JsonDeserialize` — `The JSON value could not be converted` | design.json이 수동 편집으로 손상되었거나 라이브러리 버전과 불일치 | design.json 직접 수정 금지. UIEditor에서 .gud 다시 열고 MakeCode 재실행 |
| `NU1605` / `Version conflict detected for Going.UI` | Going.UI(1.0.8)와 Going.UI.OpenTK 버전이 다름 | csproj에서 두 패키지 버전을 동일하게 통일 |
| `CS0246: The type or namespace 'GoInputNumber<>' could not be found` | `using Going.UI.Controls;` 누락 또는 제네릭 타입명이 C# alias가 아닌 .NET 타입명 사용 | Designer.cs의 타입 선언 확인: `GoInputNumber<Int32>` (int 아님), `GoValueNumber<Double>` (double 아님) |

### 통신 에러

| 증상 (코드 상태/로그) | 원인 | 해결 |
|------|------|------|
| `RTU.IsOpen == false`가 `Start()` 후에도 유지 | 시리얼 포트명 오류(`"COM1"` vs `"/dev/ttyUSB0"`), 포트 권한 부족, USB 케이블 미연결 | Linux: `ls /dev/ttyUSB*`로 포트 확인, `sudo chmod 666 /dev/ttyUSB0`. Windows: 장치 관리자에서 COM 포트 확인 |
| `TimeoutReceived` 이벤트만 반복 발생, `DataReceived` 없음 | Baudrate/Parity/StopBits 불일치, 슬레이브 번호 오류, 주소 범위 초과 | PLC 측 설정과 대조: `RTU.Baudrate`, `RTU.Parity`, `RTU.StopBits` 일치 확인. `MonitorWord_F3(slave, addr, len)`의 slave/addr이 PLC 설정과 동일한지 확인 |
| MQTT `Disconnected` 이벤트 반복 | 브로커 IP 오류, 방화벽, ClientID 중복 (동일 ID로 2개 연결 시 킥) | `mqtt.BrokerHostName` 확인. `ping {broker_ip}` 테스트. ClientID를 `$"client_{Guid.NewGuid():N}"` 등 고유값 사용 |
| CNet/MC `NAKReceived` 또는 `NakErrorReceived` 반복 | PLC 디바이스 주소 문자열 형식 오류 (예: `"D100"` vs `"%MW100"`) | CNet은 LS 주소체계(`"%MW100"`), MC는 Mitsubishi 주소체계(`"D0"`) 사용. API 문서에서 디바이스 주소 형식 확인 |

### 런타임 에러

| 증상 (화면/동작) | 원인 | 해결 |
|------|------|------|
| 앱 실행 시 빈 화면 (검은색/흰색만 표시) | `design.Init(this)` 미호출 또는 design.json 파싱 실패 | MainWindow 생성자에서 `design = GoDesign.JsonDeserialize(File.ReadAllText("design.json"))` → `design.Init(this)` 호출 순서 확인 |
| 버튼 클릭해도 이벤트 핸들러 미실행 | Designer.cs의 컨트롤 Name(`tbl.Controls["btnStart"]`)과 코드의 이벤트 바인딩 대상이 불일치 | Designer.cs에서 `var btnStart = ...` 변수명 확인 후, 코드에서 동일한 변수에 `+= (o, s) => { }` 바인딩 |
| GoInputText 탭 시 키패드 미출현 | `Main.Window.Keypad`가 null이거나, GoInputText의 `ReadOnly = true` | MainWindow에서 `Keypad = new GoKeypad()` 초기화 확인. 해당 컨트롤의 `ReadOnly` 속성 확인 |
| `InvalidOperationException: Collection was modified` in OnUpdate | OnUpdate에서 ObservableList에 직접 Add/Remove 중 렌더링과 충돌 | UI 스레드에서만 컬렉션 수정. 통신 스레드에서 값 변경 시 `design.Invoke(() => list.Add(item))` 사용 |

### 배포 에러 (gtcli)

| 증상 (gtcli 출력/동작) | 원인 | 해결 |
|------|------|------|
| `401 Unauthorized` 또는 `Authentication failed` | MCP 토큰이 틀리거나 만료됨 | 장치 웹 UI(http://{host}:5000)에서 토큰 재확인. `--token` 옵션에 정확한 값 전달 |
| `Connection refused` 또는 타임아웃 | 장치 IP 오류, MCP 서버(포트 5001) 미실행, 방화벽 차단 | `gtcli scan`으로 장치 IP 확인. `ping {host}` 테스트. 장치 웹 UI(포트 5000) 접속 가능한지 확인 |
| deploy 성공하지만 앱 미시작 | `executableFileName` 파라미터가 실제 실행 파일명과 불일치 | `dotnet publish` 출력에서 실행 파일명 확인. csproj의 `<AssemblyName>` 과 일치해야 함 |
| `File not found` 또는 zip 관련 에러 | zip 파일 경로 오류, 또는 zip 내부에 실행 파일이 루트에 없음 | zip 내부 구조 확인: 실행 파일이 zip 루트에 있어야 함 (`publish/` 하위가 아닌 `publish` 내용물 직접 압축) |
