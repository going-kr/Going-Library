# 구체적 기술원리 05 — 매개변수형 재사용 HMI 컴포넌트

## 0. 지위

- 후보 ID: P-05
- 현재 상태: 핵심 구현 존재
- 단독 권리화: B-, 일반 UI component/template 선행기술 강함
- 권장 위치: P-02 분할 객체 그래프 또는 F-01 통신 계획 compiler의 종속 구성

## 1. 기술적 문제

같은 motor faceplate를 수십 개 복사해 각 tag path를 직접 바꾸면 화면 정의가 중복되고 주소 누락이 생긴다. 반대로 runtime child tree 전체를 instance마다 저장하면 template 변경이 기존 instance에 일관되게 반영되지 않는다.

## 2. 정의 model

```text
ComponentTemplate = {
    Name,
    Parameters: [{Name, DeclaredType, DefaultLiteral}],
    RootTemplateXml: [XElement...]
}

ComponentInstance = {
    ComponentName,
    ParamValues: [Name -> literal or binding expression],
    InstantiatedRoots,
    PendingBindings
}
```

template root는 live control을 저장하는 것이 아니라 복제 가능한 XML definition으로 보유한다.

## 3. 등록 순서

design을 읽을 때 일반 page/window instance보다 `<Components>`를 먼저 처리한다. 이름별 registry에 parameter 정의와 root XML clone을 둔다. 현재 duplicate name은 later definition이 앞 definition을 대체한다.

```text
REGISTER(templateXml):
    validate component name
    parse parameter declarations
    clone all root elements
    Registry[name] = template
```

## 4. instance 생성

reader가 control registry에 없는 tag를 만나면 component registry를 조회한다.

```text
BUILD_INSTANCE(componentTag, instanceXml):
    t := Registry[componentTag]
    values := defaults(t.Parameters)
    overlay values with instance attributes
    roots := [READ_ELEMENT(CLONE(x)) for x in t.RootTemplateXml]
    return ComponentInstance(t.Name, values, roots)
```

각 instance마다 XML과 control을 clone하므로 runtime mutable state를 공유하지 않는다.

## 5. parameter binding

parameter 값은 literal 또는 outer binding이다.

```text
Status="true"                 -> bool literal
Status="{Motors.M1.Run}"     -> outer-root evaluation
Speed="{Motors.M1.Rpm}"      -> outer-root evaluation
```

scope는 parameter name/type/value를 제공하고 template 내부 expression은 이 local scope를 root로 compile한다. 상세 원리는 `technical-principle-04-scoped-binding.md`를 따른다.

## 6. 저장 알고리즘

component instance를 다시 저장할 때 template-derived root tree를 쓰지 않는다.

```text
WRITE_COMPONENT_INSTANCE(ci):
    e := Element(ci.ComponentName)
    emit instance scalar override
    emit parameter literal/expression originals
    omit ci.InstantiatedRoots
```

template definition은 한 번, instance는 이름과 override만 저장하여 중복과 divergence를 줄인다.

## 7. version·호환 확장

현재 구현에 version contract는 없다. 출원용 확장 실시형태는 다음을 포함할 수 있다.

- component definition hash/version
- parameter rename/default/type migration table
- removed parameter warning
- instance가 기대한 component version과 실제 version 비교
- old/new template의 state-transfer rule

이 확장을 원자적 hot deployment와 결합하면 F-05의 일부가 된다.

## 8. 예시

```xml
<Components>
  <GoComponent Name="MotorCard">
    <Params>
      <Param Name="Title" Type="string" Default="Motor" />
      <Param Name="Run" Type="bool" Default="false" />
    </Params>
    <Roots>
      <GoPanel>
        <Childrens>
          <GoLabel Text="{Title}" />
          <GoStateLamp OnOff="{Run}" />
        </Childrens>
      </GoPanel>
    </Roots>
  </GoComponent>
</Components>

<MotorCard Title="Pump 1" Run="{Plant.P1.Run}" />
<MotorCard Title="Pump 2" Run="{Plant.P2.Run}" />
```

## 9. 시험

- 두 instance의 child reference가 서로 다름
- default와 override merge
- parameter type parse 실패
- nested component와 depth 제한
- serialize 후 instance에 template child가 중복되지 않음
- deserialize 후 parameter expression 원문과 local 결선 복원
- duplicate definition 정책

## 10. 청구항 방향

독립항으로는 약하다. 다음 결합을 종속항으로 권장한다.

- 분할 저장된 HMI graph에서 component definition을 instance보다 선등록
- instance별 outer tag binding을 parameter scope로 변환
- template-derived child는 생략하고 parameter expression만 저장
- F-01에서 component instance별 parameter를 실제 protocol address 집합으로 전개
- F-05에서 component version과 poll-plan version을 함께 전환

## 11. 코드 근거

- `Going.UI/Gudx/GoComponentTemplate.cs`
- `Going.UI/Gudx/GoComponentParam.cs`
- `Going.UI/Containers/GoComponentInstance.cs`
- `Going.UI/Bindings/GoComponentScope.cs`
- `Going.UI/Gudx/GoGudxConverter.cs`
- `GudxComponentTests`, `GudxComponentScopeTests`

