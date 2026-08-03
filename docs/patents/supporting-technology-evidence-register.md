# 통신·UI 기반기술 증거대장

## 0. 문서 목적

이 문서는 C-01~C-15와 U-01~U-14를 독립 발명으로 과장하지 않고, P/F 발명의 어느 구성요소·실시예·종속항에 쓰는지 추적하는 대장이다. 각 항목은 출원 전 실제 코드 line, commit, 시험결과, 최초 공개일을 채워야 한다.

## 1. 증거 수집 공통 양식

```text
기술 ID / 담당자
기능이 처음 구현된 commit 및 작성자
최초 private 구현일 / 최초 public 공개일
현재 source file·symbol·line
관련 test·sample·packet capture
P/F 발명에서의 역할
표준/선행기술과 다른 결합점
미구현 gap과 과장 금지사항
```

## 2. 통신·제어 기반기술

| ID | 현재 기술과 근거 | 출원에서의 역할 | 추가 증거·주의 |
|---|---|---|---|
| C-01 | RTU/TCP/CNet/MC의 manual·auto work queue와 manual-first 선택 | P-01 command 우선, F-01/F-02 scheduler | queue selection timeline, starvation 시험; 단독 우선순위 scheduler로 청구하지 않음 |
| C-02 | master loop의 timeout, retry, reconnect | P-01 timeout/retry, F-03/F-07 reconciliation | retry 전/후 device 적용 여부 packet fixture 필요 |
| C-03 | `MasterRTU`, `MasterTCP`의 response-event 기반 cache | P-01 confirmed value 핵심 | 현재 full request/response correlation 부재를 명시하고 보완시험 확보 |
| C-04 | zero-based address와 device별 bit/word cache | protocol-neutral logical tag mapping | one-based 표시주소와 wire offset 혼동 회귀시험 |
| C-05 | Modbus FC1/2/3/4/5/6/15/16/26 master/slave | P-01 protocol 실시예, F-10 simulator | 표준 function 구현 자체는 특허 포인트 아님 |
| C-06 | slave area를 `BitMemory`/`WordMemory`에 mapping | F-10 자동 simulator | schema에서 memory map 생성하는 결합으로 사용 |
| C-07 | LS CNet frame, BCC, ACK/NAK, work schedule | F-01/F-06 이종 protocol 실시예 | 표준 규격과 custom layer 구분 |
| C-08 | Mitsubishi MC frame, checksum, ACK/NAK, schedule | F-01/F-06 이종 protocol 실시예 | protocol 자체 독립항 제외 |
| C-09 | MQTT reconnect, resubscribe, alive publish | F-06 read path, F-07/F-05 reconnect 실시예 | QoS/session 정책과 duplicate event capture |
| C-10 | TextComm STX/ETX/DLE stuffing과 checksum | custom protocol path 실시예 | framing 자체는 일반 기술 |
| C-11 | 한 byte array의 word/dword/int/float/string/bit view | F-01 decode map과 F-10 schema | alias overlap/type-width test, thread safety 확인 |
| C-12 | bit-packed memory와 aligned access | Modbus coil/discrete simulator | boundary·concurrent access 시험 |
| C-13 | ABCD/BADC/CDAB/DCBA endian 변환 | F-01 range/decode, F-10 공동검증 | 모든 type round-trip 및 잘못된 endian 검출 fixture |
| C-14 | 안정시간과 chattering 제거 | F-04 sensor quality, F-08 derived quality | sampling rate별 false transition 측정 |
| C-15 | last receive time과 connection alive state | F-02/F-06/F-08 freshness/health score | monotonic clock 사용 여부와 reconnect transition 증거 |

## 3. 통신 path 공통 상관검증 checklist

P-01/F-03/F-06/F-07에서 “확인응답”이라고 쓰려면 protocol별로 다음을 확인한다.

| Protocol | 요청-응답 대조 후보 |
|---|---|
| Modbus RTU | slave, function/exception, address, quantity 또는 echoed value, CRC |
| Modbus TCP | transaction ID, protocol ID, length, unit ID, function/exception, payload |
| CNet/MC | station/device, command, address/range, sequence 가능 필드, checksum/BCC, ACK/NAK |
| MQTT | topic, correlation/command ID, QoS delivery와 application-level device feedback |
| TextComm | endpoint/session, frame type, sequence/correlation field, checksum, semantic payload |

checksum 통과는 원 명령의 장치 적용 확인과 동일하지 않다. transport ACK, protocol ACK, device accepted, process feedback을 단계별로 분리한다.

## 4. UI·편집기·렌더링 기반기술

| ID | 현재 기술과 근거 | 출원에서의 역할 | 추가 증거·주의 |
|---|---|---|---|
| U-01 | SkiaSharp 기반 platform-independent control tree | P-01/P-03 표시부 실시예 | 일반 rendering framework로 단독 청구하지 않음 |
| U-02 | WinForms/OpenTK adapter | system/platform 종속항 | adapter pattern 자체 제외 |
| U-03 | 산업 control family | P-01 control별 manipulation detector | slider/knob/switch/input의 종료조건 시험 |
| U-04 | editor drag/drop/property/C# 생성 | F-01 compiler source, P-03 topology editor | 편집 변경→plan rebuild traceability 필요 |
| U-05 | table/grid cell+span collection | P-02 P3 | cell/span round-trip 전수 fixture |
| U-06 | 다형 series/column/tree wrapper | P-02 P4 | derived runtime type 보존 fixture |
| U-07 | image/font 외부화, page/window split | P-02 핵심, F-05 bundle | 현재 atomic/integrity 아님을 명시 |
| U-08 | component와 parameter | P-05, F-01 instance tag 전개 | static global registry와 snapshot scope 한계 |
| U-09 | ItemList item-scope template | P-06, F-01 dynamic schema | 현재 생성 row와 template 저장 구분 |
| U-10 | vector shape/path rendering | P-03 geometry 표시 | 일반 graphics로 독립항 제외 |
| U-11 | chart/trend/sparkline/gauge | F-08 quality-aware series 표시 | downsampling/quality gap algorithm 추가 전 독립성 낮음 |
| U-12 | image canvas zoom/pan/select/edit | topology/editor 실시예 | 일반 editor 기능 |
| U-13 | touch keyboard/embedded input | P-01 operator input | 오류방지·권한 algorithm 없으면 독립항 제외 |
| U-14 | theme/layout/window/dialog | P-02 object graph 다양성 | 일반 UI framework |

## 5. 후보별 구성요소 mapping

| 발명 | 필수 기반기술 |
|---|---|
| P-01 | C-01, C-03, C-04, C-05, C-15, U-03 |
| P-02 | U-05, U-06, U-07, U-08, U-09 |
| P-03 | U-01, U-04, U-10 및 topology object |
| P-04 | U-08과 GUDX binding compiler |
| P-05 | U-08, U-07 |
| P-06 | U-09, U-05 |
| F-01 | C-04, C-05, C-11, C-13, U-04, U-08, U-09 |
| F-02 | C-01, C-02, C-15, U-14 |
| F-03 | C-02, C-03, C-15, U-03 |
| F-04 | C-14, C-15, P-03 topology |
| F-05 | U-07, C-01, C-03, C-09 |
| F-06 | C-03, C-07, C-08, C-09, C-15 |
| F-07 | C-02, C-03, C-09, C-15 |
| F-08 | C-03, C-14, C-15, U-08, U-11 |
| F-09 | C-01, C-02, C-05, C-15 |
| F-10 | C-05, C-06, C-11, C-13, U-04 |

## 6. 출원 전 증거 우선순위

1. P-01 full packet→cache→binding timeline과 final manipulation flush 통합시험
2. P-02 semantic round-trip, group collision 회귀, split failure 자료
3. P-03 cycle/branch/merge 현재 실패 fixture와 보완 solver 비교
4. F-01 수동 scan plan 대비 frame·byte·오류 감소 benchmark
5. F-03/F-07 disconnect 시 device applied/unknown/duplicate scenario
6. F-04 fault injection dataset
7. F-05 crash consistency와 rollback journal

## 7. 공개일 감사 열

각 ID마다 다음 날짜를 별도 spreadsheet 또는 disclosure form에 채운다.

- 최초 private commit
- 최초 public commit/push
- public package binary 포함일
- sample/document에서 원리가 이해 가능해진 날
- 고객·파트너 제공일 및 NDA
- 논문·발표·영상·전시일

같은 source file이 공개됐더라도 해당 결합 원리가 실제로 공중에게 이용 가능했는지는 변리사와 별도 판단한다.

