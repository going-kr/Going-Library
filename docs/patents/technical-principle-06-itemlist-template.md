# 구체적 기술원리 06 — 컬렉션 항목 스코프형 HMI 템플릿

## 0. 지위

- 후보 ID: P-06
- 현재 상태: 핵심 구현 존재
- 단독 권리화: B-, ItemsControl류 선행기술 강함
- 권장 위치: P-02/P-04/F-01 종속항

## 1. 기술적 문제

alarm, recipe, device list처럼 runtime collection의 항목마다 복합 HMI row가 필요하다. 단순 text list로는 lamp, button, graph를 함께 표현하기 어렵고, 생성된 각 row를 직렬화하면 원 template와 runtime data가 뒤섞인다.

## 2. model

```text
ItemList = {
    Items,                    // runtime data collection
    ItemTemplateXml,          // 원본 row template
    GeneratedRows,            // item별 clone 결과
    Dirty                     // rebuild 필요
}
```

각 row의 data root는 전체 page model이 아니라 해당 item이다.

## 3. rebuild algorithm

```text
if Items reference/count/change event changed:
    Dirty := true

if Dirty during update/layout:
    dispose or detach previous GeneratedRows
    rows := []
    for each item in Items:
        xml := Clone(ItemTemplateXml)
        row := GoGudxConverter.ReadElement(xml)
        GudxBinder.WireTree(row, item)
        rows.Add(row)
    GeneratedRows := rows
    Dirty := false
```

template XML을 item별로 다시 읽으므로 control ID와 mutable state 정책을 명확히 해야 한다. 저장된 template ID를 그대로 복제하면 instance 간 ID 충돌이 생길 수 있으므로 instance ID 재발급 또는 composite identity `(ItemListId,itemKey,templateLocalId)`가 확장안이다.

## 4. 저장 원리

serializer는 현재 생성된 rows를 저장하지 않고 원 `ItemTemplateXml`을 저장한다. runtime item data도 design file에 무조건 포함하지 않는다.

```text
design artifact = template + binding expressions
runtime artifact = Items + generated row instances
```

이 분리가 round-trip 후에도 item별 binding을 다시 만들 수 있게 한다.

## 5. item scope

template 내부 `{Name}`, `{Alarm.Active}`는 현재 item을 root로 compile한다. 같은 template라도 item A와 item B의 accessor 호출 root가 다르므로 결과와 write target이 분리된다.

formatted expression은 display용 read-only로, writable unformatted path는 item property setter에 연결할 수 있다. 산업 명령으로 이어지는 write는 P-01 command binding을 명시적으로 적용해야 하며 단순 property setter와 구별한다.

## 6. collection 변화 정책

현재 구현을 발전시킬 때 변화 종류별 정책을 둔다.

| 변화 | 권장 처리 |
|---|---|
| add | 새 item row만 clone/wire |
| remove | 해당 row detach/dispose |
| move | row 재사용, visual order만 변경 |
| replace | old row dispose 후 new row 생성 |
| reset/template change | 전체 rebuild |
| item property change | binding update, tree rebuild 없음 |

현재 구현의 dirty 전체 rebuild와 위 incremental 실시형태를 구분한다.

## 7. virtualization 확장

대량 alarm list는 모든 item row를 만들지 않고 viewport와 overscan 범위만 materialize할 수 있다.

```text
VisibleRange = IndexRange(scrollOffset, viewportHeight, estimatedRowHeight)
Materialized = VisibleRange + Overscan
```

row 재사용 시 이전 item binding을 완전히 detach한 뒤 새 item scope로 rewire해야 한다. command pending 중인 row를 recycle할 때는 pending state를 row가 아니라 item/tag identity에 보존한다.

## 8. 시험

- N개 item에서 N개의 독립 row와 scope 생성
- template expression 원문 왕복
- add/remove/reset 후 row 수와 dispose 여부
- 두 row의 write가 서로 다른 item을 변경
- malformed template 한 항목의 실패 격리 정책
- 큰 collection의 rebuild 시간/allocation
- virtualization 시 scroll 후 binding 누수 없음
- pending command row recycle 안전성

## 9. 청구항 방향

단순 “collection마다 template 복제”는 약하다. 다음 결합을 권장한다.

- GUDX object graph에서 runtime row 대신 template 원문과 binding 식을 보존
- item별 scope에서 path accessor를 재사용하되 root instance를 분리
- F-01 compiler가 template binding을 item/tag schema에 따라 통신 주소로 전개
- virtualization/recycle 시 P-01 pending command를 logical item/tag identity로 승계

## 10. 코드 근거

- `Going.UI/Controls/GoItemList.cs`
- `Going.UI/Gudx/GoGudxConverter.cs`
- `Going.UI/Bindings/GudxBinder.cs`
- `Going.UI.Tests`의 `GoItemListTests`

