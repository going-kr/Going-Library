# 구체적 기술원리 04 — 컴포넌트 스코프형 선언 바인딩

## 0. 지위

- 후보 ID: P-04
- 현재 상태: 핵심 구현 존재
- 단독 권리화: B, 선행기술 강함
- 권장 위치: P-02의 binding 복원 종속항 또는 F-01의 tag 전개 구성

일반적인 data context와 path binding 자체는 오래된 기술이다. 이 문서의 의미는 GUDX 원문 보존, component parameter scope, accessor compile/cache, object graph 복원 후 결선 순서를 구체적으로 남기는 데 있다.

## 1. 입력·출력

```text
입력:
  TargetObject, TargetProperty
  Expression = "{Path}" | "{Path:Format}"
  OuterRoot 또는 ComponentScope

출력:
  Getter(root) -> target value
  선택적 Setter(root,value)
  runtime binding record
```

`{{`로 시작하는 text는 escaped literal로 보고 binding에서 제외한다. `:Format`이 있으면 display 변환이므로 기본적으로 read-only로 구성한다.

## 2. 두 단계 처리

### 2.1 deserialize 단계

XML attribute가 binding 문법이면 목표 property type으로 parse하거나 즉시 setter에 넣지 않는다.

```text
PendingBindings[targetPropertyName] = originalExpression
```

이 단계의 불변조건은 평가값이 아니라 expression 원문을 보존하는 것이다.

### 2.2 wire 단계

전체 object tree와 data root가 준비된 뒤 tree를 순회한다.

```text
for each control in tree:
    scope := inherited root
    if control is ComponentInstance:
        scope := BuildComponentScope(control, inherited root)

    for each pending binding:
        compile or reuse path accessor
        attach runtime binding

    recurse into children with scope
```

component nesting의 비정상 순환을 막기 위해 현재 traversal depth를 32로 제한한다. 개별 expression 결선 실패는 log 후 skip하여 나머지 tree를 계속 처리한다.

## 3. path accessor 생성

path `Tank.Level.Value`를 segment로 나누고 root type부터 각 property를 순서대로 찾는다. getter는 null intermediate에 대한 정책을 포함한 expression tree로 compile한다.

```text
AccessorKey = (RootRuntimeType, NormalizedPath)
AccessorCache[AccessorKey] = {Getter, OptionalSetter, ResultType}
```

마지막 property가 writable이고 format이 없을 때 setter를 생성한다. cache hit 시 reflection/path parsing을 반복하지 않는다.

## 4. component scope

```text
ComponentScope = {
    Values[ParameterName] = evaluated or parsed value,
    Types[ParameterName]  = declared parameter type,
    OuterRoot
}
```

parameter override가 expression이면 outer root에서 평가하고 literal이면 선언 type으로 parse한다. template 내부 `{Status}`는 outer model의 `Status`가 아니라 component scope의 parameter를 읽는다.

현재 outer expression은 scope 생성 시 평가된 snapshot이다. outer parameter 변화가 자동으로 scope에 계속 전파되는 reactive link까지 구현됐다고 주장하지 않는다.

## 5. 예시

```xml
<MotorCard Name="M1" Status="{Motors.M1.Run}" Speed="{Motors.M1.Rpm:0}" />
```

1. `MotorCard` template를 clone한다.
2. outer root에서 `Motors.M1.Run`, `Motors.M1.Rpm`을 평가한다.
3. local scope에 `Status`, `Speed`를 둔다.
4. template 내부 lamp의 `{Status}`와 label의 `{Speed}`를 local scope accessor에 연결한다.

## 6. 오류 조건

- 존재하지 않는 path segment
- intermediate null
- formatted expression에 write 시도
- 목표 property와 result type 불일치
- component nesting depth 초과
- 동일 scope parameter의 type 불일치

strict mode는 load 실패로, compatible mode는 해당 binding skip과 진단목록 생성으로 처리할 수 있다.

## 7. 기술효과·시험

- 직렬화 전후 expression 원문 일치
- 동일 template의 서로 다른 instance가 다른 data root를 참조
- `(rootType,path)` cache hit에 따른 binding 구성시간 감소
- 한 binding 오류가 전체 화면 결선을 중단하지 않음
- 32 depth 초과 구조의 유한 종료

## 8. 청구항 방향

단독항보다 다음 결합이 현실적이다.

1. 객체 그래프에서 평가된 값 대신 binding 원문을 저장
2. component 정의를 선등록하고 instance별 parameter scope 생성
3. 복원 완료 후 scope type과 path를 이용해 accessor compile
4. 동일 root type/path accessor cache
5. command path에는 P-01 pending 상태기계를 연결하거나 F-01 통신 tag를 전개

## 9. 코드 근거

- `Going.UI/Bindings/GudxBindingExpression.cs`
- `Going.UI/Bindings/GudxBinder.cs`
- `Going.UI/Bindings/GoComponentScope.cs`
- `Going.UI/Gudx/GoGudxConverter.cs`
- `Going.UI.Tests`의 `GudxBindingRoundTripTests`, `GudxComponentScopeTests`

