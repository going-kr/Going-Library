# Changelog

모든 주요 변경 사항을 이 파일에 기록합니다.

형식은 [Keep a Changelog](https://keepachangelog.com/ko/1.1.0/)를 따르고, 버전은 [Semantic Versioning](https://semver.org/lang/ko/)을 준수합니다.

---

## [1.2.4] - 2026-05-01

### Fixed

- **`DarkTheme`** (`Going.UI/Themes/GoTheme.cs:371`): `GradientDarkBrightness = 0.2F` 오타 → `GradientLightBrightness = 0.2F`. 두 줄에 동일 속성 할당되어 `GradientLightBrightness`가 GoTheme base default로 fallback되던 문제 정정.

### Tests

- 66/66 unit tests pass.

---

## [1.2.3] - 2026-05-01

### Fixed (Gudx)

- **`GoGudxConverter.GetWrapperDerivedTypes`**: 1-arity generic wrapper에 대해 numeric type alias 11종 자동 close instance 등록.
  - 예: `GoDataGridNumberColumn<T>` → `GoDataGridNumberColumn_byte`, `_sbyte`, `_short`, `_ushort`, `_int`, `_uint`, `_long`, `_ulong`, `_float`, `_double`, `_decimal` 자동 enum.
  - `baseType.IsAssignableFrom(closeType)` 체크로 generic constraint 위반 자동 skip.
  - GoJsonConverter.ControlTypes의 numeric encoding과 일관 (lowercase alias).
- **`GoGudxConverter.TypeTagName`**: close generic wrapper (non-IGoControl) tag 인코딩 fallback.
  - `_gudxTypeToTag` 미등록 + `IsGenericType` close → `{NameBase}_{NumericAlias}` 형식 자동 생성.

### Why

기존: 1-arity generic wrapper (`GoDataGridNumberColumn<T>` 등)는 `Assembly.GetTypes()`가 open def만 반환해 close instance 자동 enum 안 됨 → `<GoDataGridNumberColumn_double>` 같은 .gudx 박제 시 S002 (미등록 wrapper) 에러.

이번: read (`GetWrapperDerivedTypes`) + write (`TypeTagName`) 둘 다 close generic alias 자동 처리. `<GoDataGrid><Columns><GoDataGridNumberColumn_double /></Columns></GoDataGrid>` 같은 .gudx 박제 round-trip 정상.

### 영향

- GoDataGrid `<Columns>`에 generic 컬럼 (`GoDataGridNumberColumn<T>`, `GoDataGridInputNumberColumn<T>`) 박제 가능.
- 다른 1-arity generic wrapper도 자동 적용 (constraint 위반은 자동 skip).
- 기존 non-generic wrapper 동작 변경 없음.

### Tests

- 66/66 unit tests pass.

### Drift sync

- `SenvasHMI/GudxCli/Validation/Tier1Structural.cs` `ResolveWrapperType`도 동일 패턴 적용 필요 (별도 repo).

---

## [1.2.2] - 2026-04-30

### Added
- **`GoStateLamp`** — N-state 램프 컨트롤 (IcState 패턴 미러링: 색상+텍스트+OnOff 톤). `[GoChildWrappers] List<StateLamp>` 로 상태 항목 정의 (각 항목은 `State`/`Color`/`Text`/`OnOff`). 매칭 실패 시 "Not Matched" + Base3 OFF 톤 fallback. 항목별 `OnOff` 플래그가 같은 색상의 활성/비활성 밝기 톤을 결정.
- **17 개 컨트롤에 `BorderWidth` 속성** — `BorderColor` 가 있는 모든 컨트롤(GoPanel, GoWindow, GoDropDownWindow, GoColorSelector, GoGauge, GoInput, GoListBox, GoNumberBox, GoOnOff, GoProgress, GoSlider, GoRangeSlider, GoSwitch, GoToolBox, GoTreeView, GoValue)에 `float BorderWidth` (기본 1F) 추가 + OnDraw wiring. `GoProgress` 는 OnDraw 에 border render 신규.
- **`GoInput.TitleBoxDraw` / `GoValue.TitleBoxDraw`** — Title 영역의 배경 박스(`FillColor`) 그리기 여부. `false` 면 텍스트만 표시되고 외곽 round 가 Value 영역으로 흡수.
- `UseTitle` / `UseButton` 0 처리 — `TitleSize`/`ButtonSize` 가 0 이면 영역 자체 제외.

### Changed
- **`GoBoxPanel.BorderSize` → `BorderWidth`** rename — 17 개 컨트롤 BorderWidth 명명 일관성.
- Border 그리기 시 Title 영역 제외 처리.

### Migration notes
- `GoBoxPanel.BorderSize` 코드 사용처가 있다면 `BorderWidth` 로 변경. 직렬화 형식은 PascalCase 키(.gudx) 라 자동 동기화.
- v5(.gudx) 직렬화 — `GoStateLamp.States` (P4 패턴) 가 PATTERNS.md Coverage matrix 에 등록됨.

---

## [1.2.1] - 2026-04-30

### Migration notes (.gudx file format)
- Gudx 직렬화 형식 갱신: 모든 collection (P2/P3/P4/P5/B2) 이 property 이름 그룹 element 안에 들어감 (XAML / .NET XmlSerializer 스타일).
- **C# API 불변** — `GoGudxConverter.SerializeControl/DeserializeControl/SerializeGoDesignToFiles/...` 시그니처 그대로. 코드 사용처 변경 0.
- v1.2.0 에서 생성된 `.gudx` 파일은 v1.2.1 로 못 읽음. 마이그레이션은 `.gud` 원본에서 재변환.
- semver 측: 외부 사용 인스턴스 사실상 0 + PoC 단계 정황상 patch (1.2.1) 로 박제. semver 엄격 규칙 적용 시 minor (1.3.0) 가 정석이지만 부채 fix 성격 우세.

### Added
- `IGoCellIndexedControlCollection` interface (`Going.UI.Collections`) — P3 cell-indexed dispatch 일반화. `GoTableLayoutControlCollection` / `GoGridLayoutControlCollection` 둘 다 implement (helper 형태: `EnumerateCells`, `AddCell`, `Controls`).
- `GudxP2P4MixTests` — P2+P4 동시 보유 컨테이너 (GoGroupBox/GoPanel) round-trip 회귀 테스트.

### Changed
- 모든 collection 패턴 (P2/P3/P4/P5/B2) emit 시 property 이름 그룹 element 안에 entries — `<Childrens>`/`<Pages>`/`<Buttons>`/`<Series>`/`<Images>` 등.
- `[GoChildResource]` 의 `TagName`/`Folder`/`Ext` 어트리뷰트가 emit 에 사용됨 (이전: 사용처 0). `WriteB2_Resource` 가 어트리뷰트 데이터로 entry tag/File 경로 조립.
- ReadP2 / ReadP3 에 tag-filter (`_gudxControlTypes` 검사) 추가 — IGoControl 이 아닌 element silent skip.
- ReadP5: TAG-FILTERED 워크어라운드 제거 — 그룹 element 가 자연 격리.
- `ChildProperties`: `BindingFlags.NonPublic` 도 walk — `GoDesign.Images`/`Fonts` 같은 private 마킹 발견.
- `SerializeGoDesignToFiles` / `DeserializeGoDesignFromFiles`: hardcoded `<Image>`/`<Font>`/`resources/` 분기 제거 → reflection 기반 helper (`ExtractGoChildResourceFiles` / `LoadGoChildResourceFiles`) 가 어트리뷰트 정보로 외부 파일 I/O.
- 빈 collection (P2/P3/P4/P5/B2) 은 그룹 element 자체 omit — 작은 emit 사이즈.

### Removed
- `[GoChildCells].NestInto` property — XAML 스타일로 평탄화. `GoGridLayoutPanel` 의 Rows (P4 메타) 와 Childrens (P3 cells + attached `Cell`) 가 독립.
- `WriteP3_CellIndexed_NestedIn` / `ReadP3_CellIndexed_NestedIn` 함수.
- ReadP5 의 TAG-FILTERED 로직 (그룹핑이 자연 안전).
- `WriteAny` / `PopulateAny` 의 nestedTargets 검사.

### Fixed
- P2+P4 mix 컨테이너 (GoGroupBox/GoPanel) 에서 Buttons 데이터 있을 때 ReadP2 unfiltered walk 가 wrapper element (GoButtonItem) 까지 흡수해 `Unknown Gudx tag` throw 하던 latent bug. v1.2.0 의 Test129 round-trip 0 diffs 가 통과한 이유는 모든 fixture 에서 Buttons 가 비어 있어 mix 조건이 발생 안 해서.

---

## [1.1.12] - 2026-04-24

### Added
- **`GoButtons.SelectedItems`** — 선택된 버튼 항목 목록을 조회하는 읽기 전용 속성 (Toggle/Radio 모드에서 유효).
- **`GoButtons.SelectItem(index|item, raiseEvent)`** — 지정한 단일 버튼만 선택 상태로 설정하고 나머지는 해제 (Radio/편의용).
- **`GoButtons.SelectItems(indices|items, raiseEvent)`** — 지정한 버튼들만 선택 상태로 설정 (Toggle용).
- **`GoButtons.DeselectAll(raiseEvent)`** — 모든 버튼의 선택을 해제.
- 모든 선택 메서드에 `raiseEvent` 옵션 제공 — `SelectedChanged` 이벤트 발생 여부 제어.

---

## [1.1.11] - 2026-04-23

### Changed
- **`GoMeter.GraduationLarge`, `GraduationSmall`** 자료형 `int` → `double` — 소수점 눈금 간격 지원 (예: 0.5, 2.5).
- **`GoMeter`** 눈금 라벨에 `Format` 속성 적용 — 값 표시와 일관된 포맷.

### Fixed
- **`GoMeter`** 눈금 계산 부동소수점 누적 오차 수정 — `i += Graduation` 방식에서 인덱스 기반 재계산으로 변경하여 마지막 눈금 누락 방지.

---

## [1.1.10] - 2026-04-14

### Fixed
- **그래프 범례 위치** — `GoTrendGraph`, `GoTimeGraph`, `GoLineGraph`, `GoBarGraph`, `GoCircleGraph`에서 범례(Remark)가 우측 열의 세로 중앙에 배치되어 ViewBox와 겹치던 문제 수정. 이제 ViewBox/PauseBox 아래로 10px 간격 두고 배치됨.

---

## [1.1.9] - 2026-04-14

### Changed
- **`GoControl.Margin` 기본값**: `3,3,3,3` → `4,4,4,4` — [8pt Grid System](https://spec.fm/specifics/8-pt-grid) 원칙에 맞춰 컨트롤 간격이 8px로 정렬됨.

---

## [1.1.8] - 2026-04-14

### Added
- **`GoBaseline.ToString()`** override — 컬렉션 에디터/디버거에서 기준선이 `"Name: Value"` 형식으로 표시되어 편집 용이.

---

## [1.1.7] - 2026-04-14

### Added
- **`GoSparkline` 컨트롤** — 축/범례/스크롤 없이 라인(+옵션 영역 채움)과 기준선, 최소/최대/XScale 텍스트만 표시하는 미니 트렌드 그래프.
  - 여러 시리즈를 공통 Min/Max 스케일로 표시
  - `MaximumXScale`, `XScale`, `Interval`, `ValueFormatString` 등 지원
  - 내부 `GoBaseline` 클래스로 기준선 정의
- **SampleOpenTK** PageSwitchPanel에 GoSparkline 데모 배치.

---

## [1.1.6] - 2026-04-14

### Added
- **`MasterRTU.AutoReconnect`, `MasterTCP.AutoReconnect`** 프로퍼티 노출 — 래퍼 클래스에서 내부 Modbus 객체의 자동 재연결 옵션에 직접 접근 가능.

---

## [1.1.5] - 2026-04-14

### Fixed
- **Going.UI XML 문서 주석 누락** — 1072개의 CS1591 경고 전체 해결.
  - `override` 메서드는 `<inheritdoc/>` 적용
  - 모든 public 멤버에 한국어 XML 주석 추가
  - 주요 수정 파일: `GoDataGridItems.cs`(136), `GoInput.cs`(88), `Scroll.cs`(58), `GoDataGrid.cs`(56), `GoControl.cs`(38) 외 다수
  - NuGet 패키지 XML 문서 크기: 476KB → 691KB

---

## [1.1.4] - 2026-04-14

### Added
- **FlowSystem 컴포넌트 (Going.UI)** — 산업용 배관 흐름 시뮬레이터.
  - `FsFlowObject` (추상 기반), `FsFlowSystemPanel` (배관 네트워크 관리자)
  - `FsPump`, `FsValve`, `FsCrossPipe`, `FsTeePipe`, `FsCylinderTank`, `FsSiloTank`
  - 흐름 전파 알고리즘, 버블 애니메이션, 회전 지원(`IRotatable`)
  - `PipeTool` 유틸리티 (경로 스무딩, 정규화, 거리 계산)
  - `PathTool` 확장 (탱크/밸브/펌프/임펠러 형상 경로)
  - `TopPortPosition`, `BottomPortPosition` 열거형
- **FlowSystem 에디터 (Going.UIEditor)** — 배관 연결 시각적 편집.
  - ToolBox에 "FlowSystem" 카테고리 추가
  - ConnectionMode 토글 버튼 — 포트 선택/배관 연결/앵커 드래그
  - `ConnectionAddAction`/`ConnectionDeleteAction` (Undo/Redo)
  - `EditAction` 확장 — 컨트롤 이동 시 배관 자동 재정규화
  - 키보드 단축키: `R` (회전), `V` (Pump/Valve OnOff 토글), `Escape` (연결 취소), `Delete` (배관 삭제)
- **`GoPanel.BorderColor`** 프로퍼티 — 테두리 색상을 `PanelColor`와 독립 지정.
- **`GoPanel.TitleDivider`** 프로퍼티 — 제목 영역 구분선 표시/숨김.
- **SampleOpenTK `PageFlow`** — 사일로2 → 펌프2 → 밸브3 → 실린더탱크 데모.

### Changed
- **`GoStep.StepChagend` → `StepChanged`** — 이벤트명 오타 수정 (Going.UI + Going.UI.Forms).
- **Going.UI.Forms `GoPanel`** 래퍼에 `BorderColor`, `TitleDivider` 추가.

---

## [1.1.3] - 2026-04-14

### Added
- **Going.Basis 전체 XML 문서 주석** — 39개 파일 모든 public API에 한국어 XML 주석 추가.
- **Going.UI 전체 XML 문서 주석** — 107개 파일 모든 public API에 한국어 XML 주석 추가.
- `GenerateDocumentationFile=true` 활성화로 빌드 시 XML 문서 자동 생성.

### Fixed
- **`LauncherTool`** 파라미터 오타 `faild` → `failed`.
- **`MathTool`** `CenterPoint(List<PointF>)`, `GetPointWithAngle(RectangleF, double)`에 누락된 `static` 키워드 추가.
- **`NetworkTool.IsSocketConnected`** 파라미터명 `PollTime` → `PollTimeMicros` (단위 명확화).
- **`Serialize`** nullable 반환 타입 수정 (`object` → `object?`).
- **`ExternalProgram.Start`** 대소문자 무관 프로세스 비교 (`StringComparison.OrdinalIgnoreCase`).
- **`Chattering.ChatteringMode`** 주석 명확화 ("On 상태에 채터링 필터 적용").

### Changed
- **`MathTool.Constrain`**: `min > max`일 때 자동 스왑하여 정상 범위 제한.
- **`HiResTimer`**: Stopwatch 리셋 시 overshoot 반영으로 타이밍 보정.
- **`Serialize.XmlDeserializeFromFile`**: 불필요한 바이트 배열 복사 제거, FileStream 직접 사용.
- 전체 `new byte[0]` → `[]` 모던 컬렉션 표현식 사용 (6곳).
- 미사용 `catch (Exception ex)` 변수 정리 (9곳).

---

## 이전 버전

이전 버전은 [Git 커밋 히스토리](https://github.com/going-kr/Going-Library/commits/master)를 참조하세요.
