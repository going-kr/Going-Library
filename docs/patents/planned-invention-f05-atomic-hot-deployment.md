# 장래 발명 F-05 — UI·바인딩·통신 계획의 원자적 핫 배포

## 0. 지위

- 등급: A-
- 상태: 미구현; 현재 GUDX split save는 directory 선삭제 방식으로 비원자적
- 핵심: 화면 file 교체가 아니라 물리장치 연결 상태와 pending command까지 포함한 version transaction

## 1. deployment bundle

```text
Bundle = {
  Version,
  MasterGudx, Page/Window files, Resources,
  BindingPlan,
  PollPlan,
  SchemaVersion,
  FileHashes,
  CompatibilityAndStateTransferRules
}
```

## 2. prepare 단계

1. staging location에 모든 file을 기록한다.
2. hash/signature와 path containment를 검증한다.
3. GUDX graph를 off-screen deserialize한다.
4. component/binding을 compile한다.
5. F-01 poll plan을 compile하거나 bundle plan과 UI hash의 일치를 확인한다.
6. 현재 version과 diff를 계산해 control/tag/resource added/removed/changed를 분류한다.

prepare 실패는 active version에 영향을 주지 않는다.

## 3. quiescence와 state transfer

```text
PreservableState = confirmed cache, alarm acknowledgement,
                   control navigation state, compatible trend buffer
NonTransferable = incompatible type/endian tag, removed command target
```

pending command마다 다음을 결정한다.

- 동일 logical tag와 feedback mapping 유지: 새 version으로 승계
- target 변경/삭제: confirmation 완료까지 전환 대기 또는 명시적 cancel/rollback
- safety command: 별도 policy

## 4. commit

safe point에서 active pointer를 한 번 바꾼다.

```text
ActiveRuntime := {
  UIGraph=newUI,
  BindingGraph=newBindings,
  PollPlan=newPollPlan,
  Version=newVersion
}
```

renderer, scheduler, binding pump가 서로 다른 version을 보지 않도록 immutable runtime aggregate 또는 epoch barrier를 사용한다.

## 5. rollback

commit 후 health window 안에 binding exception, poll-plan startup failure, critical resource failure가 발생하면 aggregate 전체를 이전 version으로 되돌린다. 장치에 이미 전송된 command는 file rollback으로 취소됐다고 간주하지 않고 command journal로 계속 reconcile한다.

## 6. 시험

- page write 중 process crash
- resource hash mismatch
- binding compile failure
- pending command 중 tag 유지/삭제/type 변경
- commit 직전 connection loss
- UI만 새 버전, poll plan은 구버전인 혼합상태가 관찰되지 않음
- rollback 뒤 command 중복 전송 없음

## 7. 청구항 중심

새 UI graph, binding graph, poll plan을 사전 검증하고 pending command의 mapping 호환성에 따라 quiescence/state transfer를 결정한 뒤 하나의 version으로 원자 전환·공동 rollback하는 방법.

