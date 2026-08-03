# 구체적 기술원리 02 — GUDX 속성 표지형 객체 그래프 직렬화·분할 배포

## 0. 문서 목적과 구현 경계

이 문서는 `invention-disclosure-02-gudx.md`를 특허 명세서의 구체적 실시내용으로 발전시키기 위한 기술 부속서다. 단순한 파일 형식 설명이 아니라, 어떤 입력 객체가 어떤 판단을 거쳐 어떤 XML 구조와 외부 파일로 변환되고 다시 같은 의미의 객체 그래프로 복원되는지를 단계별로 설명한다.

| 구분 | 의미 |
|---|---|
| 현재 구현 | 2026-08-03 저장소에서 실행 가능한 동작 |
| 명세서 일반화 | 현재 구현을 특정 class 이름에 한정하지 않고 기술적 원리로 표현한 것 |
| 확장 실시형태 | 구현 가능하도록 구체화했으나 현재 코드에는 없거나 일부만 있는 것 |

현재 구현과 출원용 확장을 혼동해서는 안 된다. 특히 현재 split save는 기존 하위 directory를 먼저 삭제한 뒤 새 파일을 쓰므로 원자적 배포가 아니며, manifest hash·schema version·서명 검증도 없다. 이 기능들은 별도 발명 또는 종속항의 확장 실시형태로만 다룬다.

## 1. 해결하려는 기술적 문제

### 1.1 하나의 HMI 객체 그래프에 서로 다른 관계가 공존하는 문제

산업용 화면 객체는 다음 관계를 한 parent 아래 동시에 가질 수 있다.

- 화면 control의 순서 있는 list
- column, row, span이 결합된 cell-indexed control collection
- control이 아닌 설정 wrapper의 다형 list
- 문자열 key를 갖는 page/window dictionary
- 동일 type이지만 역할이 다른 left/right single child
- image/font 등 binary resource dictionary
- runtime에 실행되는 binding expression
- component template와 component parameter

관계의 의미를 버리고 모든 child를 parent 바로 아래에 평탄화하면, reader가 자신이 담당하지 않는 sibling을 읽어 다른 collection에 넣는 silent corruption이 생길 수 있다. 실제 구버전 형식의 논리적 문제는 다음과 같다.

```xml
<!-- 관계 정보가 없는 평탄 구조 -->
<GoSparkline>
  <GoBaseline />
  <GoLineGraphSeries />
</GoSparkline>
```

P2 reader가 모든 직접 child를 control 후보로 순회하거나, P4 reader가 모든 wrapper 후보를 순회하면 서로의 element를 소비할 수 있다. tag filter를 계속 추가하는 방식은 새 type마다 converter 분기를 수정하게 한다.

### 1.2 직렬화된 값과 runtime 값이 다른 문제

binding property의 runtime 값이 `72`라 하더라도 설계 원문은 `{Tank.Level:0.0}`일 수 있다. runtime 값만 저장하면 다음 load에서 binding 관계가 사라진다. 반대로 expression을 즉시 평가하지 않고 문자열 property로 넣으면 control property type과 맞지 않는다. 따라서 저장 단계에서는 expression 원문을 보존하고, 복원 단계에서는 객체 구조를 먼저 만든 뒤 runtime root에 연결해야 한다.

### 1.3 대형 단일 파일의 배포 결합 문제

page, window, image, font를 하나의 XML에 base64로 넣으면 일부 page만 바뀌어도 전체 파일을 교체해야 한다. binary가 XML diff를 오염시키고, page 단위 검토·배포·복구도 어렵다. 따라서 논리적 참조는 master에 남기고 payload는 별도 파일로 분리하되, 재로딩 시 동일한 객체 관계를 복원해야 한다.

## 2. 발명의 핵심 원리

핵심은 객체 property에 부여된 **관계 표지(attribute marker)**를 입력으로 하여 직렬화 pattern을 선택하고, collection pattern마다 **property-name group**을 경계로 생성하는 것이다.

```text
Property metadata
      │
      ├─ scalar marker ───────────────> P1: owner XML attribute
      └─ child marker subtype
            ├─ list ─────────────────> P2: property group + controls
            ├─ cells ────────────────> P3: property group + Cell
            ├─ wrappers ─────────────> P4: property group + runtime tag
            ├─ map ──────────────────> P5: property group + Name
            ├─ single ───────────────> B1: role tag + child body
            └─ resource ─────────────> B2: group + File reference
```

property-name group은 단순 가독성용 wrapper가 아니라 reader의 탐색 범위를 제한하는 구조적 namespace다. 예를 들어 `Baselines` reader는 `<Baselines>` 내부만, `Series` reader는 `<Series>` 내부만 읽는다.

```xml
<GoSparkline>
  <Baselines>
    <GoBaseline Value="50" />
  </Baselines>
  <Series>
    <GoLineGraphSeries Name="PV" />
  </Series>
</GoSparkline>
```

## 3. 메타모델과 registry

### 3.1 scalar 표지 P1

`[GoProperty]` 및 파생 표지가 붙고 읽기·쓰기가 가능하며 scalar로 분류된 property만 owner element의 XML attribute가 된다. `[JsonIgnore]`가 붙은 runtime alias는 제외한다. 즉 기본 정책은 전부 저장 후 제외가 아니라 **표지된 property만 포함**하는 방식이다.

현재 scalar 범위는 다음을 포함한다.

- `bool`, `string`, enum, `Guid`, `TimeSpan`
- signed/unsigned integer 및 floating numeric type
- nullable scalar
- `SKColor`, `SKRect`, `GoPadding`
- `List<string>`

특수 type은 culture에 종속되지 않는 formatter/parser 한 쌍으로 왕복한다. 명세서에서는 각 scalar type에 대해 `Parse(Format(x)) = x` 또는 허용 오차 내 동등이라는 불변조건을 둔다.

### 3.2 child 표지 family

모든 child marker는 공통 base attribute에서 파생된다. converter는 property의 정적 선언 type만으로 처리하지 않고 marker의 구체 type을 먼저 판정한다.

| 표지 | 관계 의미 | 추가 메타데이터 |
|---|---|---|
| `GoChildList` | control list | runtime control tag |
| `GoChildCells` | cell-indexed controls | `Cell=column,row[,columnSpan,rowSpan]` |
| `GoChildWrappers` | non-control wrapper list | runtime wrapper tag |
| `GoChildMap` | keyed dictionary | `Name=key` |
| `GoChildSingle` | 단일 역할 child | property name 또는 override tag |
| `GoChildResource` | 외부 resource | tag, folder, extension, `File` |

child property 탐색은 public/non-public instance property를 포함한다. 따라서 public API에 노출하지 않은 image/font dictionary도 명시적 marker가 있을 때 처리할 수 있다.

### 3.3 control type registry

시작 시 알려진 control type을 조사해 XML-safe tag에서 concrete `Type`으로 가는 map과 역방향 map을 만든다.

```text
ControlByTag[tag] = concreteControlType
TagByControlType[concreteControlType] = tag
```

reader는 임의 tag를 곧바로 instantiate하지 않고 registry에 있는 control tag만 P2/P3 대상으로 받아들인다. 알 수 없는 tag는 해당 collection에서 건너뛴다.

### 3.4 다형 wrapper registry

P4의 property가 `List<TBase>`이면 실제 item은 `TDerived1`, `TDerived2`일 수 있다. 현재 구현은 base wrapper type의 assembly에서 assignable concrete type을 조사해 다음 cache를 만든다.

```text
DerivedByBase[TBase][xmlTag] = TConcrete
```

첫 조사 이후에는 base type과 tag로 concrete type을 상수시간에 찾는다. 알려지지 않은 tag는 건너뛰며, 현재 collection 생성은 `List<>`와 `ObservableList<>` 계열을 지원한다. 다른 collection 구현은 현재 예외 대상이다.

### 3.5 numeric alias

CLR generic type 이름 등에 XML element name으로 부적합한 문자가 생기는 문제를 피하기 위해 numeric type에는 XML-safe alias를 둔다. type-to-tag와 tag-to-type의 양방향 registry를 유지하여 write/read가 같은 type을 선택한다.

## 4. 공통 직렬화 알고리즘

### 4.1 입력과 출력

```text
입력:
  obj                 직렬화할 객체
  explicitTagName?    B1 등에서 역할명을 강제할 선택값
  skipChildren?       master split 때 일부 property를 제외할 집합

출력:
  XElement            객체의 scalar와 child 관계를 표현한 element
```

### 4.2 `WriteAny` 상당 알고리즘

```text
function WRITE_ANY(obj, explicitTagName, skipChildren):
    if obj is ComponentInstance:
        tag := obj.ComponentName
    else:
        tag := explicitTagName ?? TAG_FOR_RUNTIME_TYPE(obj.Type)

    e := new Element(tag)

    for each property p of obj where p has scalar marker:
        if p is JsonIgnored or not readable/writable or not scalar: continue
        if obj has PendingBinding[p.Name]:
            e.Attribute[p.Name] := original binding expression
        else:
            e.Attribute[p.Name] := FORMAT_SCALAR(p.GetValue(obj))

    for each pending binding b not already emitted as scalar:
        e.Attribute[b.PropertyName] := b.OriginalExpression

    if obj is ComponentInstance:
        emit component parameter values
        do not emit instantiated template-derived children
        return e

    for each child property p carrying a GoChild marker:
        if p.Name is in skipChildren: continue
        DISPATCH_CHILD_WRITE(e, obj, p, markerType)

    if obj is ItemList and has ItemTemplateXml:
        emit the original item template XML rather than generated row instances

    return e
```

component instance에서 template-derived child를 다시 저장하지 않는 이유는 template 정의와 instance 결과를 중복 저장하지 않기 위해서다. instance에는 component 이름과 parameter override만 남기고, load 시 template에서 child tree를 다시 만든다.

### 4.3 child write dispatch 순서

현재 구현의 판정 순서는 P2 → P3 → P4 → P5 → B1 → B2이며, 일치한 handler 하나를 실행한 뒤 반환한다. 하나의 property에는 하나의 관계 의미만 부여한다는 계약이다.

```text
function DISPATCH_CHILD_WRITE(ownerElement, ownerObject, p, marker):
    if marker is ChildList:      WRITE_P2(...); return
    if marker is ChildCells:     WRITE_P3(...); return
    if marker is ChildWrappers:  WRITE_P4(...); return
    if marker is ChildMap:       WRITE_P5(...); return
    if marker is ChildSingle:    WRITE_B1(...); return
    if marker is ChildResource:  WRITE_B2_REFERENCE(...); return
```

## 5. Pattern별 구체 원리

### 5.1 P1 — scalar attribute

null nullable property는 attribute를 생략할 수 있고, 나머지는 type별 formatter를 통과한다. binding 원문이 pending table에 있으면 현재 runtime 값보다 원문이 우선한다.

```xml
<GoButton Text="Start" FontSize="14" Bounds="0,0,100,40" />
<GoValue Value="{Tank.Level:0.0}" />
```

복원 시 attribute name과 public writable property를 대응시키고 목표 type으로 parse한다. binding 문법으로 판정되면 즉시 property setter에 넣지 않고 `PendingBindings[propertyName]`에 보관한다.

### 5.2 P2 — 동종 control list

```text
WRITE_P2:
    items := property value
    if empty: return
    group := Element(property.Name)
    for each control in items:
        group.Add(WRITE_ANY(control))
    owner.Add(group)

READ_P2:
    group := owner.Element(property.Name)
    if absent: return
    for each childElement in group:
        if childElement.Tag not in ControlByTag: continue
        child := new ControlByTag[tag]
        POPULATE_ANY(child, childElement)
        list.Add(child)
```

group을 찾지 못하면 optional empty collection로 처리한다. malformed/unknown control tag는 다른 property로 흘려보내지 않고 해당 group 안에서만 skip한다.

### 5.3 P3 — cell-indexed control collection

P3는 control body와 layout 위치를 분리한다. control 자체에는 layout container에 종속된 column/row property를 추가하지 않고, 직렬화 관계에 attached metadata로 기록한다.

```text
for each (control, column, row, columnSpan, rowSpan):
    child := WRITE_ANY(control)
    if columnSpan = 1 and rowSpan = 1:
        child.Cell := column + "," + row
    else:
        child.Cell := column + "," + row + "," + columnSpan + "," + rowSpan
    group.Add(child)
```

read 시 `Cell`을 정수 tuple로 parse하고 `AddCell` interface를 호출한다. `Cell`이 없는 legacy entry는 collection의 일반 `Controls` fallback에 append할 수 있다. 이 interface 때문에 새 cell layout은 converter 분기를 추가하지 않고 `EnumerateCells`, `AddCell`, `Controls` 계약 구현만으로 참여한다.

### 5.4 P4 — 다형 wrapper list

```text
WRITE_P4:
    group := Element(property.Name)
    for each wrapper in list:
        group.Add(WRITE_WRAPPER(wrapper, runtime-type tag))

READ_P4:
    baseType := generic element type of declared collection
    map := DerivedByBase[baseType]
    for each entry in property group:
        concrete := map[entry.Tag]
        if absent: continue
        wrapper := new concrete()
        POPULATE_ANY(wrapper, entry)
        collection.Add(wrapper)
```

wrapper의 내부에도 child marker가 있으면 같은 dispatch가 재귀 실행된다. 따라서 tree node, tab page, data-grid column과 같은 중첩·다형 구조를 같은 원리로 처리한다.

### 5.5 P5 — keyed map

각 dictionary entry는 value의 type tag와 key인 `Name`을 함께 가진다.

```xml
<Windows>
  <GoWindow Name="AlarmWindow" ... />
</Windows>
```

read 시 `Name`이 없으면 dictionary identity를 복원할 수 없으므로 예외로 처리한다. value가 control이면 control registry 경로를, wrapper이면 wrapper object 경로를 사용한 뒤 `dictionary[name] = value`를 수행한다.

### 5.6 B1 — single child

동일한 `GoSideBar` type이라도 `LeftSideBar`와 `RightSideBar`는 역할이 다르다. 따라서 default tag는 runtime class name이 아니라 property name이다. `[GudxTagName]`이 있으면 그 이름이 우선한다.

```xml
<GoDesign>
  <LeftSideBar ... />
  <RightSideBar ... />
  <Theme ... />
</GoDesign>
```

reader는 이미 생성된 get-only child가 있으면 그 instance를 in-place populate한다. 값이 null이면 concrete type을 생성하고 non-public setter를 포함해 할당을 시도한다. null인데 setter가 없거나 instance 생성이 불가능하면 불완전 객체를 조용히 만들지 않고 예외로 처리한다.

### 5.7 B2 — 외부 resource reference

XML 단계에서는 payload를 쓰지 않고 다음 reference만 생성한다.

```xml
<Images>
  <Image Name="logo" File="resources/logo.png" />
</Images>
<Fonts>
  <Font Name="Pretendard" File="resources/fonts/Pretendard.ttf" />
</Fonts>
```

marker가 tag name, 상대 folder, extension을 제공한다. 실제 file I/O는 design-level split serializer가 reference 생성과 별도 단계로 수행한다. 현재 image dictionary 값에서는 첫 `SKImage`, font dictionary 값에서는 첫 `byte[]` payload를 외부화한다. list 전체를 다중 파일로 저장하는 것으로 과장하면 안 된다.

## 6. 공통 역직렬화 알고리즘

```text
function POPULATE_ANY(obj, element):
    for each XML attribute a:
        p := matching scalar property
        if no p: continue
        if IS_BINDING_EXPRESSION(a.Value):
            obj.PendingBindings[p.Name] := a.Value
        else:
            p.SetValue(obj, PARSE_SCALAR(a.Value, p.Type))

    for each marked child property p:
        DISPATCH_CHILD_READ(obj, element, p, marker)

    if obj is control:
        restore Id through non-public setter when present
        if Id is empty, assign a new Guid on best-effort basis
```

component tag는 일반 control registry에 없을 수 있으므로 component registry도 조회한다. component template 등록이 끝나기 전에는 instance를 정확히 만들 수 없으므로 등록 순서가 기능 요건이다.

## 7. Component template의 구체 원리

### 7.1 정의 등록

master의 `<Components>`를 먼저 순회한다. 각 component definition에서 다음을 저장한다.

```text
ComponentTemplate = {
    Name,
    ParameterDefinitions: [{Name, Type, DefaultLiteral}],
    RootTemplateElements: [XElement clones]
}
```

현재 registry에서 동일 이름이 반복되면 뒤의 정의가 앞 정의를 대체한다. 출원 문서에서는 duplicate 정책을 reject, first-wins, last-wins 중 선택 가능한 실시형태로 일반화할 수 있으나 현재 동작은 last-wins로 기록한다.

### 7.2 instance 생성

일반 control tag 조회가 실패하면 component name인지 검사한다. 일치하면 template root XML을 clone해 각 root를 `ReadElement`로 객체화하고, parameter default 및 instance override를 `ParamValues`에 저장한다.

```text
instance children = Deserialize(Clone(template roots))
instance parameters = Merge(default parameters, instance overrides)
```

clone은 여러 instance가 동일 `XElement`와 runtime control을 공유해 한 instance의 변경이 다른 instance에 전파되는 것을 방지한다.

### 7.3 저장 시 중복 방지

component instance를 저장할 때는 instance가 template로부터 생성한 child tree를 다시 쓰지 않는다. component tag, scalar override, parameter binding 원문만 쓴다. 이로써 template 수정이 모든 instance에 일관되게 적용되고 파일 크기가 불필요하게 증가하지 않는다.

## 8. Binding expression 보존과 runtime 결선

### 8.1 문법 판정

현재 binding은 `{path}` 또는 `{path:format}` 형태를 인식하며 `{{`로 시작하는 escaped literal은 binding으로 보지 않는다.

```text
{Tank.Level}       -> path = Tank.Level
{Tank.Level:0.0}   -> path = Tank.Level, format = 0.0
{{literal}}        -> literal
```

### 8.2 두 단계 복원

1. deserialize 단계: expression 원문을 pending table에 보존한다.
2. wire 단계: 최종 data root와 component scope가 준비된 뒤 getter/setter delegate를 compile한다.

formatted expression은 표시 변환이 포함되므로 read-only binding으로 구성한다. format이 없고 마지막 path property에 setter가 있으면 two-way 결선을 구성할 수 있다.

### 8.3 path cache

`(rootType, path)`를 key로 compile된 accessor를 cache한다. 같은 data model type과 path를 쓰는 다수 control이 reflection path를 매 cycle 다시 해석하지 않도록 한다.

```text
AccessorCache[(RootType, "Tank.Level")] = {
    Getter(root) -> value,
    Setter(root, value)?
}
```

### 8.4 component scope

component instance 내부 binding의 root는 outer root가 아니라 component-local scope다. scope 생성 시 parameter 값이 binding이면 outer root에서 한 번 평가하고, literal이면 선언된 parameter type으로 parse한다. 그 결과와 type map을 사용해 template 내부 path를 compile한다.

현재 outer expression 평가는 scope 생성 시 snapshot이라는 점을 기록해야 한다. outer 값의 지속적인 reactive propagation까지 구현됐다고 표현하면 안 된다.

### 8.5 방어 조건

binder는 tree traversal depth를 32로 제한하여 cyclic component 또는 비정상적으로 깊은 구조가 무한 재귀를 일으키는 것을 막는다. 개별 binding 결선 실패는 debug log를 남기고 해당 binding만 skip하여 나머지 화면의 결선을 계속한다.

## 9. Master/page/window/resource 분할 저장

### 9.1 논리 file tree

```text
Design.gudx
Pages/
  Main.gudx
  Alarm.gudx
Windows/
  Login.gudx
resources/
  logo.png
  fonts/
    Pretendard.ttf
```

master는 design-level scalar, component, single child, resource reference와 page/window file reference를 가진다.

```xml
<GoDesign ...>
  <Pages>
    <GoPageRef Name="Main" File="Pages/Main.gudx" />
  </Pages>
  <Windows>
    <GoWindowRef Name="Login" File="Windows/Login.gudx" />
  </Windows>
</GoDesign>
```

### 9.2 split write 순서

```text
function SERIALIZE_DESIGN_TO_FILES(design, masterPath):
    baseDir := Directory(masterPath)
    pageDir := baseDir/Pages
    windowDir := baseDir/Windows
    resourceRoots := first path segment of each B2 marker folder

    recreate output directories

    master := WRITE_ANY(design, skipChildren={Pages, Windows})

    for each (name, page) in design.Pages:
        file := Pages/SafeName(name).gudx
        pageElement := WRITE_ELEMENT(page)
        pageElement.NameAttribute := name
        write pageElement to file
        add GoPageRef(Name=name, File=file) to master

    for each (name, window) in design.Windows:
        perform the equivalent window steps

    for each marked B2 dictionary entry:
        obtain first supported payload
        encode SKImage as PNG or write byte[] raw
        write payload to marker-derived relative path

    write master to masterPath
```

`SafeName`은 권고 확장이다. 현재 구현이 모든 dictionary key에 대해 path traversal·reserved-name·collision을 완전 차단한다고 단정해서는 안 된다.

현재 구현은 대상 directory를 먼저 삭제해 dictionary와 disk의 stale file 불일치를 제거한다. 그러나 중간 write 실패 시 이전 정상 배포본까지 잃을 수 있으므로 원자성은 없다.

### 9.3 split read 순서

```text
function DESERIALIZE_DESIGN_FROM_FILES(masterPath):
    masterXml := parse(masterPath)
    REGISTER_COMPONENTS(masterXml)          // 반드시 먼저
    design := new GoDesign()
    POPULATE_ANY(design, masterXml, skip Pages/Windows refs)

    for each GoPageRef:
        resolved := ResolveRelativeTo(masterPath, ref.File)
        pageXml := parse(resolved)
        validate root is page-compatible
        page := READ_ELEMENT(pageXml)
        design.Pages[ref.Name] := page

    repeat for windows

    for each B2 reference:
        resolved := ResolveRelativeTo(masterPath, ref.File)
        if file absent: continue
        bytes := read file
        dispatch to AddImage or AddFont by marker/tag

    return design
```

현재 resource file이 없으면 해당 entry를 조용히 skip한다. 필수 resource 오류, placeholder 대체, load 실패 보고서 생성은 확장 정책이다.

## 10. 왕복 불변조건

특허 명세서와 시험계획에서 “동일”은 byte-for-byte XML 동일이 아니라 의미 보존으로 정의한다.

```text
D0 = 원 design object graph
X  = Serialize(D0)
D1 = Deserialize(X)

SemanticEquivalent(D0, D1) = true
```

`SemanticEquivalent`는 적어도 다음을 비교한다.

- 표지된 scalar value
- child collection의 relation과 순서
- cell의 column/row/span
- wrapper의 concrete runtime type
- dictionary key와 value type
- single child의 역할 property
- control identifier
- binding expression 원문
- component name, parameter override 및 template-derived tree
- resource name과 복원 가능한 payload

직렬화 순서나 공백, XML attribute 순서는 의미 동일성에서 제외할 수 있다.

## 11. 오류 처리와 보안 경계

### 11.1 현재 오류 정책

- 알 수 없는 P2/P3 control tag: skip
- 알 수 없는 P4 wrapper tag: skip
- P5 entry의 `Name` 누락: 예외
- B1 생성/할당 불가능: 예외
- 지원하지 않는 wrapper collection type: 예외
- binding compile 실패: 해당 binding log 후 skip
- resource file 누락: skip

silent skip은 forward compatibility에 유리하지만 중요한 화면 요소 누락을 숨길 수 있다. strict mode에서는 unknown tag, missing resource, duplicate key를 오류 report로 승격할 수 있다.

### 11.2 권고 확장 실시형태

다음은 현재 구현이 아니라 명세서에 실시 가능한 변형으로 기재할 항목이다.

1. master에 schema version과 converter version을 기록한다.
2. 각 file의 길이와 cryptographic hash를 manifest에 기록한다.
3. relative path를 normalize한 뒤 base directory 밖으로 벗어나는 reference를 거부한다.
4. 임시 staging directory에 모두 쓴 뒤 검증 성공 시 directory 또는 manifest pointer를 원자적으로 교체한다.
5. 이전 generation을 유지해 load 실패 시 rollback한다.
6. strict/compatible load mode를 제공한다.

이 중 원자적 배포와 generation 전환은 별도 출원 후보 F-05와 중첩될 수 있으므로 P-02 독립항에 무리하게 모두 결합하지 않는다.

## 12. 동시성·성능 원리

- control/wrapper registry와 path accessor cache는 반복 reflection 비용을 줄인다.
- property group은 reader가 전체 sibling을 매 property마다 필터링하는 범위를 줄이고 관계 오소비를 구조적으로 차단한다.
- page/window 분할은 변경 단위와 load 단위를 작게 할 수 있다.
- binary 외부화는 XML parsing과 diff 비용을 줄인다.
- component template 재사용은 반복 child tree의 저장 크기를 줄인다.

현재 component registry가 static/global이라는 점은 multi-design 동시 load에서 namespace 충돌 가능성을 만든다. design-scoped registry 또는 immutable registry snapshot은 확장 실시형태다.

## 13. 구체 실시예

### 13.1 grid page 저장과 복원

입력 객체:

```text
Page "Main"
 └─ Grid
     ├─ Rows[0] = {Height="50%", Columns=["50%","50%"]}
     ├─ Button A at (0,0), span(1,1)
     └─ Value B at (1,0), span(1,1), Value="{Tank.Level:0.0}"
```

출력:

```xml
<GoPage Name="Main">
  <Childrens>
    <GoGridLayoutPanel>
      <Rows>
        <GoGridLayoutPanelRow Height="50%" Columns="50%,50%" />
      </Rows>
      <Childrens>
        <GoButton Cell="0,0" Text="Start" />
        <GoValue Cell="1,0" Value="{Tank.Level:0.0}" />
      </Childrens>
    </GoGridLayoutPanel>
  </Childrens>
</GoPage>
```

reader는 먼저 `Rows`를 P4로, `Childrens`를 P3로 독립 복원한다. `GoValue.Value`는 숫자로 parse하지 않고 pending binding으로 남긴 뒤 data root가 주어졌을 때 `Tank.Level` getter와 연결한다.

### 13.2 component 두 instance

`MotorCard` template가 lamp와 text를 포함하고 `Status`, `Name` parameter를 갖는다고 한다. 두 instance는 template child를 중복 저장하지 않는다.

```xml
<MotorCard Name="M1" Status="{Motors.M1.Run}" />
<MotorCard Name="M2" Status="{Motors.M2.Run}" />
```

load 시 template root를 각각 clone하므로 M1 lamp 상태 변경이 M2 object instance를 직접 변경하지 않는다. 각 component scope는 서로 다른 outer path를 평가한다.

### 13.3 분할 resource

`logo` image를 포함한 design을 저장하면 master에는 `File="resources/logo.png"`가 남고 실제 PNG bytes는 별도 파일에 기록된다. load 시 master 위치를 기준으로 상대 경로를 해석하고 bytes를 image decoder에 전달해 design image dictionary에 재등록한다.

## 14. 기술적 효과와 측정 방법

| 기술적 효과 | 비교 실험 |
|---|---|
| collection 간 silent corruption 방지 | group 없는 구형 fixture와 group 형식에서 P2/P4 혼재 round-trip 비교 |
| 다형 type 보존 | base list에 여러 derived wrapper를 넣고 concrete type 일치율 측정 |
| layout 관계 보존 | cell/span 조합을 저장·복원 후 좌표 tuple 전수 비교 |
| binding 의미 보존 | runtime 값과 expression 원문이 다른 상태에서 round-trip 후 원문 및 결선 확인 |
| 변경 배포량 감소 | 단일 page 변경 시 monolith bytes와 split changed bytes 비교 |
| binary/XML 분리 | 동일 resource set의 XML 크기와 parse time 비교 |
| reflection 비용 절감 | registry/cache cold와 warm의 load time 및 allocation 비교 |

## 15. 도면 구성안

### 도 1 — 전체 system

```text
100 객체 그래프
110 metadata inspector
120 pattern dispatcher
130 scalar converter
140 type registry
150 XML graph writer
160 split-file writer
170 component registry
180 binding compiler
190 graph reader
```

### 도 2 — pattern 선택 flow

property 발견 → scalar/child 판정 → marker subtype 판정 → P1/P2/P3/P4/P5/B1/B2 handler → owner element 결합.

### 도 3 — group 충돌 방지

동일 parent 아래 `Baselines` group과 `Series` group을 분리하고 각 reader의 탐색 범위를 점선 box로 표시한다.

### 도 4 — split deployment

master에서 Pages, Windows, Images, Fonts reference가 각각 외부 file node로 연결되는 구조를 표시한다.

### 도 5 — restore 순서

master parse → component 선등록 → master populate → page/window load → resource load → binding wire 순서를 표시한다.

## 16. 청구항 구성에 대응하는 필수 단계

독립 방법항의 중심 단계는 다음과 같이 잡을 수 있다.

1. 객체 type의 복수 property에서 scalar 표지 또는 관계 표지를 검출하는 단계
2. 관계 표지의 subtype에 따라 서로 다른 직렬화 pattern을 선택하는 단계
3. collection 관계마다 property 식별자에 대응하는 group element를 만들고 해당 group 내부에 entry를 배치하는 단계
4. runtime concrete type, cell 위치, dictionary key 또는 file reference 중 pattern별 관계정보를 기록하는 단계
5. group 식별자와 type registry를 이용해 각 entry를 대응 property에 한정하여 복원하는 단계

종속항 후보:

- pending binding이 존재할 때 runtime value 대신 expression 원문을 기록
- base wrapper type별 derived type map을 자동 생성·cache
- component definition을 instance보다 먼저 등록하고 template root를 clone
- page/window를 master reference와 개별 file로 분리
- binary resource를 marker의 folder/extension 정책에 따라 외부화
- cell collection interface를 통해 container type과 무관하게 cell/span 복원
- identifier가 없을 때 새 identifier를 부여
- binding accessor를 `(rootType,path)`로 cache

독립항을 단순한 “XML serialization”이나 “reflection 사용”으로 쓰지 않고, **관계 표지에 따른 pattern 선택 + property group에 의한 탐색범위 격리 + 다형/관계정보 복원**의 결합으로 구성하는 것이 중요하다.

## 17. 현재 구현 한계와 출원 전 보강 목록

| 항목 | 현재 상태 | 조치 |
|---|---|---|
| directory save | 기존 directory 선삭제 후 write | staging/atomic 교체는 별도 확장으로 구분 |
| integrity | hash/manifest 없음 | 확장 실시형태와 시험 설계 |
| schema evolution | version/migration 없음 | version field와 migration table 설계 |
| resource list | 첫 image/byte payload 중심 | claim에서 list 전체 보존으로 과장 금지 |
| missing resource | 조용히 skip | strict mode 선택안 작성 |
| path safety | 완전한 검증 근거 부족 | normalization 및 base containment 규칙 추가 |
| component registry | static/global | design-scoped registry 변형 기재 |
| outer component parameter | scope 생성 시 snapshot | reactive link로 과장 금지 |
| unknown tag | 일부 pattern에서 skip | 호환 mode와 strict mode 구분 |

## 18. 코드·시험 근거 목록

| 근거 | 위치 |
|---|---|
| registry, `WriteAny`, scalar/binding 저장 | `Going.UI/Gudx/GoGudxConverter.cs` |
| P1~P5/B1/B2 계약 | `Going.UI/Gudx/PATTERNS.md` |
| cell collection abstraction | `Going.UI/Collections/IGoCellIndexedControlCollection.cs` |
| component template/parameter | `Going.UI/Gudx/GoComponentTemplate.cs`, `GoComponentParam.cs` |
| binding 문법과 accessor compile/cache | `Going.UI/Bindings/GudxBindingExpression.cs` |
| tree 결선과 depth 방어 | `Going.UI/Bindings/GudxBinder.cs` |
| component-local scope | `Going.UI/Bindings/GoComponentScope.cs` |
| design split API와 wire entry | `Going.UI/Design/GoDesign.cs` |
| P2/P4 충돌 회귀 | `Going.UI.Tests`의 `GudxP2P4MixTests` |
| P3 cell/span 왕복 | `GudxPattern3_CellIndexedTests` |
| 다형 wrapper 왕복 | `GudxR9PolymorphicWrapperTests` |
| master split | `GudxT12MasterSplitTests` |
| external resource | `GudxB2ResourcesTests` |
| binding 원문 왕복 | `GudxBindingRoundTripTests` |
| component/scope | `GudxComponentTests`, `GudxComponentScopeTests` |

## 19. 변리사·조사자에게 전달할 검색 단위

- attribute-driven serialization of heterogeneous/polymorphic object graph
- property-grouped XML collection deserialization collision prevention
- HMI screen object graph split file serialization
- polymorphic wrapper automatic type registry reflection
- preserving binding expression instead of evaluated value serialization
- component template parameter scoped binding serialization
- cell-indexed control attached metadata round trip
- external binary resource reference object graph serializer

선행기술 조사에서는 개별 요소가 아니라 다음 결합을 우선 비교한다.

```text
속성 subtype dispatch
 + property-name group 격리
 + 다형 type 및 관계 메타데이터 왕복
 + binding/component 의미 보존
 + master/detail/resource 분할 복원
```

