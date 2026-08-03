# 발명신고서 02 — GUDX 객체 그래프 직렬화 및 분할 배포

## 1. 서지 초안

- 발명의 명칭(국문): **속성 표지형 다형 HMI 객체 그래프의 충돌 방지 직렬화 및 분할 배포 방법 및 시스템**
- 영문 가칭: **Collision-Avoiding Serialization and Split Deployment Method and System for an Attribute-Marked Polymorphic HMI Object Graph**
- 발명자/출원인: 미확정
- 우선순위: 높음
- 국내 출원: 권장
- 해외 확장: claim-level 조사 결과에 따라 검토

## 2. 기술분야

본 발명은 산업용 HMI/SCADA 화면 설계의 저장, 전송, 배포 및 복원에 관한 것이다. 특히 다형 control, layout cell, wrapper, keyed object, single child, binding expression과 binary resource가 함께 존재하는 객체 그래프를 속성 표지에 따라 서로 다른 구조로 직렬화하고, 화면·window·resource를 복수 파일로 분리한 뒤 의미를 보존하여 복원하는 기술에 관한 것이다.

## 3. 배경기술과 문제점

산업용 HMI 설계 객체에는 단순 속성 외에 다음 이질적인 구조가 동시에 존재한다.

- control의 동종 child list
- cell 위치와 span을 가진 layout child
- control이 아닌 wrapper의 다형 list
- 이름을 key로 갖는 page/window map
- 같은 type이지만 역할이 다른 single child
- image/font binary resource
- runtime에 compile되는 binding expression과 component parameter

이들을 parent 바로 아래 같은 형태의 XML element로 평탄화하면 한 collection의 deserializer가 다른 collection의 element를 소비해 silent corruption이 발생할 수 있다. 모든 type을 수동 등록하면 새 control/wrapper 추가 때 serializer 코드가 계속 바뀌고, 하나의 대형 파일에 저장하면 일부 page 수정에도 전체 파일과 resource를 다시 배포해야 한다.

## 4. 발명의 목적

- 서로 다른 child collection이 하나의 parent에 공존해도 잘못 섞이지 않게 한다.
- 객체 property의 의미만 표지하면 serializer가 처리 pattern을 결정하게 한다.
- 다형 type, cell, span, identifier, key와 binding expression을 무손실 왕복한다.
- page, window, image와 font를 별도 파일로 분할 배포한다.
- component와 type을 올바른 순서로 등록해 참조를 복원한다.
- 새 control이 추가될 때 serializer의 특별 분기를 최소화한다.

## 5. 핵심 구성

### 속성 표지 family

| Pattern | 의미 | serialized form |
|---|---|---|
| P1 | scalar property | owner element의 XML attribute |
| P2 | homogeneous control list | property-name group + control elements |
| P3 | cell-indexed control collection | property-name group + `Cell` attached data |
| P4 | non-control wrapper list | property-name group + polymorphic wrapper elements |
| P5 | keyed dictionary | property-name group + `Name` key |
| B1 | single child object | property name 또는 override tag의 단일 element |
| B2 | external binary resource | property-name group + name/file reference, payload는 외부 파일 |

### 충돌 방지 group

각 collection은 owner의 직접 child로 entry를 쓰지 않고 먼저 **property name과 대응하는 group element**를 만든다. deserializer는 해당 property의 group 내부만 읽는다. 이 구조가 P2 control과 P4 wrapper처럼 tag 형태가 겹치는 collection의 상호 소비를 방지한다.

### 분할 배포

- master file에는 design-level property, theme, page/window reference 및 resource reference를 둔다.
- 각 page와 window의 객체 그래프는 개별 GUDX 파일에 저장한다.
- image와 font payload는 resource 폴더에 외부화한다.
- master의 file reference로 이들을 재결합한다.

### 복원 순서

1. 사용할 control type과 wrapper type registry를 준비한다.
2. component template을 먼저 등록한다.
3. master object를 생성하고 P1/B1 등을 채운다.
4. page/window reference의 별도 파일을 읽는다.
5. B2 resource reference의 payload를 읽는다.
6. object identifier, cell, key와 binding expression을 복원한다.
7. binder가 component scope에서 expression path를 compile해 runtime binding을 만든다.

### 현재 구현의 경계

- split save는 현재 Pages/Windows/resource root directory를 먼저 삭제한 뒤 파일을 쓰므로 atomic deployment가 아니다. 중간 실패에 대한 staging·commit·rollback은 F-05 확장이다.
- manifest hash, signature, schema version과 migration은 현재 없다.
- B2 image/font dictionary 값은 현재 첫 `SKImage` 또는 첫 `byte[]` payload 중심으로 외부화한다. list 전체를 무손실 저장한다고 과장하지 않는다.
- 누락 resource는 load 중 skip하며 strict integrity failure로 처리하지 않는다.
- component registry는 static/global이고 outer component parameter expression은 scope 생성 시 snapshot으로 평가된다.
- 상세 write/read 의사코드, pattern별 예외와 왕복 불변조건은 `technical-principle-02-gudx.md`를 따른다.

## 6. 코드 근거

| 구성 | 코드 |
|---|---|
| reflection entry와 `WriteAny` | `Going.UI/Gudx/GoGudxConverter.cs` 19~283행 |
| pending binding expression 보존 | 같은 파일 238~260행 및 810·873행 부근 |
| master/page/window/resource split | 같은 파일 327~514행 |
| write dispatch P2→P3→P4→P5→B1→B2 | 같은 파일 576~744행 |
| read dispatch | 같은 파일 856~1105행 |
| wrapper의 재귀 직렬화 | 같은 파일 1120행 이후 |
| pattern contract | `Going.UI/Gudx/PATTERNS.md` |
| scoped binding compile | `Going.UI/Bindings/GudxBindingExpression.cs` |
| component template | 2026-06-06 component design과 관련 class |

## 7. 실시예

### 실시예 1 — P2/P4 충돌 방지

하나의 graph control이 `Baselines` wrapper list와 `Series` wrapper list 또는 control child와 wrapper child를 함께 가진다. serializer는 각각 `<Baselines>`와 `<Series>`/`<Childrens>` group을 만들고, reader는 대응 group만 탐색한다. 따라서 한 pattern의 reader가 sibling collection의 element를 소비하지 않는다.

### 실시예 2 — cell layout

table/grid child를 `<Childrens>` 아래 직렬화하면서 각 child에 `Cell="col,row"` 또는 `Cell="col,row,colSpan,rowSpan"`을 기록한다. identifier도 함께 저장해 dictionary 기반 cell collection을 같은 관계로 복원한다.

### 실시예 3 — page와 resource 분할

design의 page와 window를 각각 파일로 저장하고 master에는 reference만 둔다. image와 font는 `resources/` 아래 binary로 저장한다. 특정 page 변경 때 해당 page 파일과 변경 resource만 배포할 수 있다.

### 실시예 4 — component와 binding roundtrip

component template의 parameter expression과 instance binding을 문자열 상태로 저장한다. deserialize 때 component registry를 먼저 구성하고 tree를 복제한 뒤 instance scope에서 binding path를 compile한다.

## 8. 선행기술 대비와 차별화

| 알려진 기술 | 위험 | 차별화할 결합 |
|---|---|---|
| attribute/reflection XML serialization | 매우 높음 | reflection 자체는 핵심으로 주장하지 않음 |
| UI object graph serialization | 높음 | 이질적 HMI collection별 pattern과 property group 격리 |
| XML HMI configuration | 높음 | 다형 control/wrapper, cell/key/id/binding의 무손실 왕복 |
| external resource file | 높음 | master/page/window/resource split와 graph 복원순서 |
| declarative component/binding | 높음 | serializer가 pending expression을 보존하고 scope에서 재compile |

대표 문헌은 `US7325226B2`, `US7814124B1`, `WO2013070561A1`, `US7676740B2`, `CN114509986A`이다. 정식 조사에서는 각 문헌의 독립항과 family를 다시 확인해야 한다.

## 9. 예비 청구항 골격

### 독립 방법항 A — 충돌 방지 직렬화

산업용 HMI 설계 객체 그래프를 직렬화하는 방법으로서,

1. 객체의 복수 property 각각에 부여된 복수 종류의 child 표지를 판독하는 단계;
2. 표지 종류에 따라 scalar, control list, cell-indexed collection, wrapper list, keyed map, single child 및 external resource 중 하나의 처리 pattern을 선택하는 단계;
3. collection property마다 property identifier에 대응하는 group element를 생성하는 단계;
4. 각 collection의 entry를 대응 group element 내부에만 기록함으로써 동일 parent의 이종 collection entry를 구조적으로 격리하는 단계;
5. runtime type과, 선택된 pattern에 따른 cell, span, identifier 또는 key를 기록하는 단계;

를 포함하는 방법.

### 독립 방법항 B — 분할 배포·복원

1. HMI design 객체 그래프에서 page 객체, window 객체 및 binary resource를 식별하는 단계;
2. page/window 객체를 각각 별도 문서로 직렬화하고 binary resource를 외부 파일로 기록하는 단계;
3. master 문서에서 이들을 file reference로 치환하는 단계;
4. component 및 polymorphic type을 등록한 뒤 master와 참조 문서를 읽는 단계;
5. identifier, cell, key와 binding expression을 이용해 원래의 객체 관계 및 runtime binding을 복원하는 단계;

를 포함하는 방법.

### 시스템·프로그램항

- object graph analyzer
- attribute dispatcher
- group generator
- polymorphic type registry
- split-file deployer
- graph restorer 및 binding compiler
- 위 method를 실행하는 program/recording medium

### 종속항 후보

1. P2 homogeneous control list
2. P3 cell/span attached information
3. P4 polymorphic wrapper auto-discovery
4. P5 multiple keyed maps의 group 격리
5. B1 property-name tag로 same-type sibling 구별
6. B2 attribute에 folder·extension을 선언
7. page/window를 master reference로 치환
8. image/font payload 외부화
9. binding expression을 property value로 보존
10. component template을 instance보다 먼저 등록
11. generic numeric control의 safe XML alias
12. 알 수 없는 type 또는 malformed element를 registry로 제한

## 10. 청구범위 방어선

| 단계 | 구성 |
|---|---|
| 넓은 안 | attribute pattern dispatch + property group isolation |
| 1차 축소 | polymorphic HMI control/wrapper + cell/key/id preservation |
| 2차 축소 | master/page/window/resource split 추가 |
| 3차 축소 | binding expression + component pre-registration 추가 |
| 4차 축소 | P2와 P4가 공존할 때 silent corruption을 방지하는 구체 구조 |

## 11. 기술효과와 시험

### 입증할 효과

- P2/P4 또는 복수 map 공존 시 잘못된 child 소비 0건
- 원본과 복원 graph의 type/id/cell/key/binding 동등성
- 일부 page 수정 시 전체 단일파일 대비 전송 byte 감소
- resource 중복/inline 대비 master size 감소
- 새 wrapper/control 추가 때 converter 수정량 감소

### 필수 시험

- [ ] 모든 P1~P5/B1/B2 조합 roundtrip
- [ ] P2+P4 충돌 regression
- [ ] 복수 P5/B2 property 격리
- [ ] 3단계 다형 wrapper 및 recursive tree
- [ ] page/window/resource 누락·손상 대응
- [ ] component parameter와 scoped binding roundtrip
- [ ] 대형 실제 HMI design의 size/load/deploy 비교

## 12. 도면 초안

- 도 1: HMI object graph의 이질적 property 구조
- 도 2: attribute type에 따른 pattern dispatch
- 도 3: group 도입 전 entry 충돌과 도입 후 격리
- 도 4: P1~P5/B1/B2 serialized form
- 도 5: master-page-window-resource 분할 구조
- 도 6: serialize 순서도
- 도 7: component pre-registration을 포함한 restore 순서도
- 도 8: binding expression의 저장과 runtime compile

## 13. 출원 전 남은 일

- [ ] 최초 공개일: 2026-04-29 및 2026-06-06 기능을 구분해 확인
- [ ] GUDX라는 명칭의 상표·기존 용례와 무관하게 명세서에서는 일반 용어 사용
- [ ] 선행 serializer 특허의 claims를 element별로 charting
- [ ] collision regression의 전/후 XML과 실패 log 보존
- [ ] 실제 design benchmark 작성
- [ ] P-02 한 건에 binding/component를 포함할지 후속 분할할지 변리사와 결정
