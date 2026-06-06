# Gudx 컴포넌트 (재사용 마크업 모듈)

**작성일**: 2026-06-06
**브랜치**: `hjh-gudx-binding`
**대상**: Going.UI (Gudx 마크업 + 선언적 바인딩 레이어)
**선행**: [2026-06-06-gudx-declarative-binding-design.md](2026-06-06-gudx-declarative-binding-design.md)

---

## 목표

마크업으로 **파라미터를 받는 재사용 컴포넌트**를 정의하고, 새 태그처럼 인스턴스화한다 (WPF UserControl / React 컴포넌트):

```xml
<!-- 정의 -->
<GoComponent name="MotorCard">
  <Param name="Title" type="string" default=""/>
  <Param name="motor" type="MotorVM"/>
  <GoBoxPanel>
    <GoLabel Text="{Title}"/>
    <GoGauge Value="{motor.Rpm}"/>
  </GoBoxPanel>
</GoComponent>

<!-- 사용 -->
<MotorCard Title="펌프 A" motor="{PumpA}"/>
<MotorCard Title="펌프 B" motor="{PumpB}"/>
<MotorCard Title="컨베이어" motor="{Conveyor}"/>
```

## 비목표 (1차 범위 밖)

- 자식/슬롯 투영(`<MotorCard><추가컨트롤/></MotorCard>`로 컨트롤을 끼워넣기)
- 컴포넌트 로컬 상태/이벤트/메서드
- 에디터(Going.UIEditor) 컴포넌트 저작 GUI
- 인스턴스 자동 레이아웃(템플릿을 카드 크기에 맞춰 자동 fill) — 1차는 템플릿 좌표 그대로
- 문자열 보간(평면 바인딩과 동일하게 후속)

---

## 핵심 통찰: 평면 바인딩 위에 root만 바꿔 얹힌다

내부 바인딩 `{motor.Rpm}`은 결국 **root만 다른 `{path}` 바인딩**이다. 글로벌 root 대신 **그 인스턴스의 파라미터 스코프**를 root로 주면 끝.

```
GudxBindingExpression.Wire(gauge, "Value", "{motor.Rpm}", paramScope)
                                                          ^^^^^^^^^^ root만 교체
```

따라서 **바인딩 엔진·펌프는 무수정**. 새로 만드는 건 *컴포넌트 시스템*(정의·인스턴스화·스코프)뿐이다.

---

## 결정 사항

### D1. 컴포넌트 정의 위치 — 디자인 전역, 페이지보다 먼저 파싱

`<GoComponent>`는 master `.gudx`의 `GoDesign` 아래 **`<Components>` 그룹**에 모인다. 역직렬화 시 **페이지/윈도우보다 먼저** 레지스트리(`name → ComponentTemplate`)에 등록되어야, 페이지 안의 `<MotorCard>` 태그가 해석된다.

- 1차: 정의는 디자인 전역 네임스페이스(이름 충돌 시 마지막 승, Debug 경고).
- 마스터 분할(페이지가 별도 파일)과의 상호작용: `DeserializeGoDesignFromFiles`가 master를 읽을 때 `<Components>`를 **가장 먼저** 등록한 뒤 페이지 파일들을 읽는다.

### D2. 인스턴스 = `GoComponentInstance : GoContainer`

사용처 `<MotorCard .../>`는 `_gudxControlTypes`엔 없지만 **컴포넌트 레지스트리에 있으면** `GoComponentInstance`로 역직렬화된다.

- 보관: `ComponentName`(string), `ParamValues`(`Dictionary<string,string>` — 사용처 attribute 원본), `Childrens`(템플릿 트리 복제본).
- **복제 방식**: 템플릿 내부 컨트롤 트리를 XElement로 저장해두고, 인스턴스마다 `ReadElement(templateElem)`로 **새로 역직렬화** → 깊은 복제. 별도 deep-copy 리플렉션 불필요.
- 렌더: 투명 컨테이너 — 자식(템플릿 트리)을 그대로 그림. 기존 `GoContainer` 메커니즘 재사용.
- 레이아웃(1차): 인스턴스 `Bounds`(사용처)가 컨테이너 위치, 템플릿 컨트롤은 자기 `Bounds`(카드 로컬 좌표) 유지. 작성자가 좌표를 맞춤. (자동 fill은 후속)

### D3. Param — 값/바인딩 두 종류, `type`이 타입 안전 컴파일의 다리

`<Param name type default?>`:
- `name`: 파라미터 이름(스코프 키, 내부 `{name...}` 경로의 첫 구간).
- `type`: CLR 타입(`string`/`int`/`double`/유저 VM 타입명 등). **내부 경로 `{motor.Rpm}`을 MotorVM 기준 타입 있게 컴파일**하기 위한 정보. 타입 해석은 등록된 어셈블리에서 이름으로 조회.
- `default`(선택): 사용처에서 attribute 생략 시 기본값.

사용처 attribute 값 두 종류:
- **리터럴**: `Title="펌프 A"` → `type` 따라 파싱.
- **바인딩**: `motor="{PumpA}"` → 바깥 root에서 `PumpA` 평가해 스코프에 **참조 스냅샷**(객체 참조는 안정적, 속성만 매 프레임 변동).

### D4. 스코프 + wiring — WireTree가 컴포넌트에서 root를 교체

`GudxBinder.WireTree`가 트리를 내려가다 `GoComponentInstance`를 만나면:
1. `GoComponentScope` 구성: 각 Param을 평가(리터럴 파싱 / `{outer}` 스냅샷)해 `name→value` + `name→type`.
2. 인스턴스의 `Childrens`를 **scope를 root로** 재귀 wire (바깥 root 대신).

```csharp
// WireTree 분기 (의사코드)
if (node is GoComponentInstance ci)
{
    var scope = GoComponentScope.Build(ci, root /*바깥*/);
    foreach (var child in ci.Childrens) WireTree(child, scope);  // root = scope
    return;
}
```

### D5. 스코프 경로 해석 — 첫 구간=스코프, 나머지=`type` 기반 (기존 CompilePath 재사용)

- `{Title}`(점 없음): `scope["Title"]` 직접 → 단방향(파라미터는 입력).
- `{motor.Rpm}`: `scope["motor"]`(=MotorVM 참조) → 나머지 `Rpm`을 **기존 `CompilePath(MotorVM, "Rpm")`로 컴파일**해 그 참조에 적용. 말단 settable이면 양방향.

즉 평면 바인딩의 `CompilePath`를 **tail에 그대로 재사용**. 신규 로직은 "첫 구간 스코프 분리"뿐.

### D6. 중첩 컴포넌트 — 지원(가드 포함)

템플릿이 다른 컴포넌트를 쓰면(`MotorCard` 내부에 `<StatusDot .../>`), 복제·wire가 자연히 재귀된다(레지스트리가 먼저 채워져 있으므로). **자기참조/순환은 깊이 가드로 차단**(초과 시 Debug 경고 + 중단).

---

## 아키텍처: 신규/수정

| 파일 | 변경 |
|---|---|
| `Going.UI/Gudx/GoComponentTemplate.cs` | **신규** — 템플릿(ParamDefs + 내부 트리 XElement) + 레지스트리(`name→template`) |
| `Going.UI/Gudx/GoComponentParam.cs` | **신규** — Param 정의(name/type/default) |
| `Going.UI/Containers/GoComponentInstance.cs` | **신규** — `GoContainer` 파생, ComponentName/ParamValues/복제 Childrens |
| `Going.UI/Bindings/GoComponentScope.cs` | **신규** — 파라미터 스코프(값+타입), Build(인스턴스, 바깥 root) |
| `Going.UI/Bindings/GudxBindingExpression.cs` | `WireScoped(ctrl, prop, expr, scope)` 추가(기존 Wire 코어 공유) |
| `Going.UI/Bindings/GudxBinder.cs` | `WireTree`에 `GoComponentInstance` 분기(root→scope) |
| `Going.UI/Gudx/GoGudxConverter.cs` | `<Components>` 우선 등록; `ReadElement` 컴포넌트 분기; `WriteAny` 인스턴스 라운드트립(name+params, Childrens 생략) |

기존 펌프·`GoBinding`·평면 `Wire`/`CompilePath`는 **무수정/재사용**.

---

## 데이터 흐름

```
역직렬화:
  master 읽기 → <Components> 먼저 레지스트리 등록
  페이지 읽기 → <MotorCard> 만남 → 레지스트리 hit
            → GoComponentInstance 생성(ParamValues 저장, 템플릿 트리 복제 → Childrens)

WireBindings(root):
  WireTree(page, root) 재귀
    → GoComponentInstance ci 만남
       → scope = Build(ci, root): Title="펌프 A"(리터럴), motor=root.PumpA(스냅샷)
       → WireTree(ci.Childrens, scope)
          → GoLabel.Text "{Title}"  → WireScoped → scope["Title"]
          → GoGauge.Value "{motor.Rpm}" → WireScoped → CompilePath(MotorVM,"Rpm")(scope["motor"])

런타임:
  기존 펌프가 매 프레임 동기화 (scope 경유든 글로벌이든 동일)
```

---

## 에러 처리

- 미등록 컴포넌트 타입(레지스트리에 없는 대문자 태그): 기존대로 `Unknown Gudx tag` — 또는 1차에선 Debug 경고 후 빈 컨테이너로 스킵(정책 택1, 구현 시 확정).
- Param `type` 해석 실패: 그 인스턴스의 내부 바인딩 wire 실패 → per-binding 스킵(기존 격리).
- 순환 중첩: 깊이 가드(예: 32) 초과 시 중단 + 경고.

---

## 테스트 전략

- **레지스트리**: `<GoComponent>` 파싱 → ParamDefs/템플릿 보관, 이름 조회.
- **인스턴스화**: `<MotorCard>` → GoComponentInstance, ParamValues 저장, Childrens가 템플릿 복제(인스턴스 간 독립 — 한쪽 수정이 다른쪽 영향 없음).
- **스코프 빌드**: 리터럴/바인딩 파라미터 평가, default 적용.
- **내부 wire**: `{Title}` 단방향, `{motor.Rpm}` 양방향(settable), 인스턴스별 다른 motor.
- **통합**: 정의+사용 디자인 역직렬화 → WireBindings(root) → 펌프 1틱에 각 카드가 자기 motor 값 표시.
- **라운드트립**: GoComponentInstance가 `<MotorCard Title=... motor="{PumpA}"/>`로 재출력(Childrens 미출력).
- **중첩**: 컴포넌트 안 컴포넌트, 순환 가드.

---

## 열린 결정 (유저 리뷰)

1. **인스턴스 레이아웃**: 1차는 "템플릿 좌표 그대로 + 사용처 Bounds가 위치"로 두는데, 카드를 사용처 크기에 맞춰 자동 fill하고 싶은지(후속 vs 1차).
2. **미등록 대문자 태그 정책**: throw vs 경고-후-스킵.
3. **Param `type` 표기**: `string`/`int` 같은 별칭만 vs 유저 VM 타입은 FQN/등록 필요 — 타입 해석 범위.

---

## 결정 요약

| 항목 | 결정 |
|---|---|
| 정의 위치 | master `<Components>`, 페이지보다 먼저 레지스트리 등록 |
| 인스턴스 | `GoComponentInstance : GoContainer`, 템플릿 트리 복제 보관 |
| 복제 | 템플릿 XElement를 `ReadElement`로 재역직렬화(깊은 복제) |
| 파라미터 | 리터럴 / `{outer}` 바인딩, `type`이 내부 경로 타입 컴파일의 다리 |
| 스코프 wiring | WireTree가 컴포넌트에서 root→scope 교체, 내부는 WireScoped |
| 경로 해석 | 첫 구간=scope, tail=기존 CompilePath(type) 재사용 |
| 엔진 | 평면 바인딩·펌프 전부 재사용, 무수정 |
| 라운드트립 | 인스턴스는 name+params만 출력, Childrens 생략 |
| 중첩 | 지원(깊이 가드) |
