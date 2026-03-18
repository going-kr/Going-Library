# Going.Basis — 통신 코드 패턴

## 목차

| # | 섹션 | 설명 |
|---|------|------|
| 1 | .csproj 패키지 참조 | 패키지 버전 및 참조 방법 |
| 2 | Modbus 아키텍처 | 클래스 선택 기준 (MasterRTU vs ModbusRTUMaster) |
| 3 | MasterRTU 패턴 | 래퍼 — 권장. WordAreas/BitAreas 자동 폴링 |
| 4 | MasterTCP 패턴 | 래퍼 — 권장. TCP 버전 |
| 5 | ModbusRTUMaster 패턴 | 저수준 — 직접 이벤트 처리 필요 시 |
| 6 | ModbusTCPMaster 패턴 | 저수준 TCP 버전 |
| 7 | SlaveRTU 패턴 | 래퍼 — Modbus 슬레이브 (RTU) |
| 8 | SlaveTCP 패턴 | 래퍼 — Modbus 슬레이브 (TCP) |
| 9 | DeviceData 모델 패턴 | MasterRTU/MasterTCP 권장. 슬레이브별 데이터 모델 |
| 10 | CNet 패턴 | LS Electric PLC 시리얼 통신 |
| 11 | MC 패턴 | Mitsubishi PLC 시리얼 통신 |
| 12 | MQTT 패턴 | MQTT 클라이언트 |
| 13 | 자주 하는 실수 | 통신 코드 오류 19개 대조표 |

---

## .csproj 패키지 참조

```xml
<ItemGroup>
  <PackageReference Include="Going.Basis" Version="1.0.4" />
  <PackageReference Include="Going.UI" Version="1.0.8" />
  <PackageReference Include="Going.UI.OpenTK" Version="1.0.8" />
</ItemGroup>
```

> 통신만 사용하는 경우 `Going.Basis`만 참조. UI 포함 시 `Going.UI.OpenTK` (또는 `Going.UI.Forms`) 추가.

---

## Modbus 아키텍처

Going.Basis의 Modbus는 **래퍼(권장) / 저수준** 두 레이어 × **마스터 / 슬레이브** 두 역할로 구성:

| 레이어 | RTU 클래스 | TCP 클래스 | 용도 |
|--------|-----------|-----------|------|
| **마스터 래퍼 (권장)** | `MasterRTU` | `MasterTCP` | WordAreas/BitAreas, GetWord/GetBit, Monitor/Set 메서드, 자동 캐싱 |
| **마스터 저수준** | `ModbusRTUMaster` | `ModbusTCPMaster` | Auto/Manual FC 메서드, 수신 이벤트 직접 처리 |
| **슬레이브 래퍼 (권장)** | `SlaveRTU` | `SlaveTCP` | BitAreas/WordAreas (메모리), 자동 요청 처리 |
| **슬레이브 저수준** | `ModbusRTUSlave` | `ModbusTCPSlave` | Request 이벤트 직접 처리 |

> 래퍼는 내부적으로 저수준 클래스를 감싸며, 마스터는 수신 데이터를 `Devices` 딕셔너리에 자동 캐싱, 슬레이브는 메모리 영역을 자동 응답.

### ⚠ 클래스 선택 기준 — 반드시 확인

**기본: MasterRTU / MasterTCP**
아래 조건 중 하나라도 해당하면 MasterRTU / MasterTCP 사용:
- 레지스터 맵이 있고 주기적으로 폴링하는 경우 (대부분의 케이스)
- GetWord / GetBit로 캐시에서 값을 조회하는 패턴
- DeviceManager에서 의미있는 프로퍼티로 변환하는 패턴

통신 상태 판단: `IsOpen` 프로퍼티 또는 `DeviceOpened` / `DeviceClosed` 이벤트 사용.
DeviceData 모델 사용 가능 — 슬레이브별 의미있는 프로퍼티/Set 메서드를 래핑하여 도메인 모델로 활용.

**예외: ModbusRTUMaster / ModbusTCPMaster**
아래 조건이 명시적으로 요구되는 경우에만 사용:
- 수신 이벤트(WordReadReceived)를 직접 제어해야 하는 경우
- FC 코드 수준의 세밀한 제어가 필요한 경우

위 예외 조건에 해당하지 않으면 ModbusRTUMaster / ModbusTCPMaster를 사용하지 말 것.

---

## MasterRTU 패턴 (래퍼 — 권장)

```csharp
public class DeviceManager
{
    public MasterRTU RTU { get; } = new MasterRTU();

    public DeviceManager()
    {
        // 영역 매핑 (시작주소 → 영역명)
        RTU.WordAreas.Add(0x0000, "D");  // D0, D1, D2...
        RTU.BitAreas.Add(0x0000, "P");   // P0, P1, P2...

        // 통신 상태는 IsOpen 또는 이벤트로 판단
        RTU.DeviceOpened += (o, s) => { /* 연결됨 */ };
        RTU.DeviceClosed += (o, s) => { /* 연결 해제 */ };
    }

    // 통신 상태 조회
    public bool IsConnected => RTU.IsOpen;

    public void Start()
    {
        var setting = Main.DataMgr.Setting;
        RTU.Port     = setting.PortName;
        RTU.Baudrate = setting.Baudrate;
        RTU.Timeout  = setting.Timeout;

        // 주기 읽기 등록 (slaveNo, startAddr, length)
        // ⚠️ 쓰기 대상 레지스터도 반드시 Monitor로 읽기 등록할 것
        //    읽지 않으면 GetWord로 현재 값 조회 불가 → UI 표시 불가
        RTU.MonitorWord_F3(1, 0x0000, 50);

        RTU.Start();
    }

    public void Stop()
    {
        RTU.Stop();  // 래퍼 클래스는 Stop()만 호출하면 내부에서 자동 정리됨
    }

    // 값 읽기 — 의미있는 프로퍼티로 래핑
    public bool   IsRunning   => RTU.GetWord(1, "D0") == 1;
    public int    ErrorCode   => RTU.GetWord(1, "D1") ?? 0;
    public double Temperature => (RTU.GetWord(1, "D10") ?? 0) / 10.0;  // 0.1°C 단위
    public int    Pressure    => RTU.GetWord(1, "D11") ?? 0;

    // 쓰기
    public void SetOutput(int slave, string addr, int value) => RTU.SetWord(slave, addr, value);
    public void SetBitOutput(int slave, string addr, bool value) => RTU.SetBit(slave, addr, value);
}
```

### MasterRTU 프로퍼티

| 프로퍼티 | 타입 | 설명 |
|---------|------|------|
| `Port` | string | 시리얼 포트명 ("COM1") |
| `Baudrate` | int | 통신 속도 |
| `Parity` | Parity | 패리티 |
| `DataBits` | int | 데이터 비트 |
| `StopBits` | StopBits | 스톱 비트 |
| `Interval` | int | 폴링 간격 |
| `Timeout` | int | 타임아웃 |
| `IsStart` | bool (읽기전용) | 시작 상태 |
| `IsOpen` | bool (읽기전용) | 포트 열림 상태 |
| `WordAreas` | Dictionary\<int, string\> | 워드 영역 매핑 |
| `BitAreas` | Dictionary\<int, string\> | 비트 영역 매핑 |
| `Devices` | Dictionary\<int, Mems\> | 수신 데이터 캐시 |
| `Tag` | object? | 사용자 데이터 |

### MasterRTU 메서드

```csharp
// 주기 읽기 (Monitor) — 쓰기 대상 영역도 반드시 등록
RTU.MonitorWord_F3(slave, startAddr, length);  // FC3: 워드 읽기
RTU.MonitorWord_F4(slave, startAddr, length);  // FC4: 입력 레지스터 읽기
RTU.MonitorBit_F1(slave, startAddr, length);   // FC1: 코일 읽기
RTU.MonitorBit_F2(slave, startAddr, length);   // FC2: 입력 비트 읽기

// 쓰기 (Set)
RTU.SetWord(slave, "D0", 100);     // 영역주소 문자열로 쓰기
RTU.SetBit(slave, "P0", true);     // 영역주소 문자열로 쓰기

// 읽기 (Get — 캐시에서 조회)
int? val = RTU.GetWord(slave, "D0");   // null = 아직 수신 안됨
bool? bit = RTU.GetBit(slave, "P3");   // null = 아직 수신 안됨

// 제어
RTU.Start();
RTU.Stop();
```

### GetWord / GetBit 시그니처

```csharp
int? GetWord(int slave, string addr);   // slave=슬레이브번호, addr="D0" 형식
bool? GetBit(int slave, string addr);   // slave=슬레이브번호, addr="P3" 형식
```

> **주의**: 첫 번째 인자는 `slave` (슬레이브 번호). 주소는 `"영역명+오프셋"` 문자열.

### MasterRTU 이벤트

| 이벤트 | 설명 |
|--------|------|
| `DeviceOpened` | 포트 연결 성공 |
| `DeviceClosed` | 포트 연결 해제 |

---

## MasterTCP 패턴 (래퍼 — 권장)

MasterRTU와 동일한 구조. 프로퍼티만 다름.

```csharp
public class DeviceManager
{
    public MasterTCP TCP { get; } = new MasterTCP();

    public DeviceManager()
    {
        TCP.WordAreas.Add(0x0000, "D");
        TCP.BitAreas.Add(0x0000, "P");

        TCP.DeviceOpened += (o, s) => { /* 소켓 연결 */ };
        TCP.DeviceClosed += (o, s) => { /* 소켓 해제 */ };
    }

    public void Start()
    {
        var setting = Main.DataMgr.Setting;
        TCP.RemoteIP   = setting.Host;       // "192.168.0.1"
        TCP.RemotePort = setting.Port;       // 502
        TCP.Timeout    = setting.Timeout;

        TCP.MonitorWord_F3(1, 0x0000, 50);

        TCP.Start();
    }

    public void Stop()
    {
        TCP.Stop();
    }
}
```

### MasterTCP 프로퍼티 (MasterRTU와 다른 부분)

| 프로퍼티 | 타입 | 설명 |
|---------|------|------|
| `RemoteIP` | string | 서버 IP 주소 (**TCP 전용**) |
| `RemotePort` | int | 서버 포트 (기본 502) (**TCP 전용**) |

> `Interval`, `Timeout`, `IsStart`, `IsOpen`, `WordAreas`, `BitAreas`, `Devices`, `Tag`은 MasterRTU와 동일.
> 메서드/이벤트도 동일 (Monitor/Set/Get/DeviceOpened/DeviceClosed).

---

## ModbusRTUMaster 패턴 (저수준 — 직접 이벤트 처리)

수신 이벤트를 직접 처리해야 할 때 사용.

```csharp
public class DeviceManager
{
    public ModbusRTUMaster RTU { get; } = new ModbusRTUMaster();

    public DeviceManager()
    {
        // 수신 이벤트 (저수준 — WordAreas/GetWord 없음)
        RTU.WordReadReceived += (o, s) =>
        {
            // s.Slave, s.StartAddress, s.ReceiveData (int[])
        };

        RTU.BitReadReceived += (o, s) =>
        {
            // s.Slave, s.StartAddress, s.ReceiveData (bool[])
        };

        RTU.TimeoutReceived += (o, s) =>
        {
            // 통신 타임아웃
        };
    }

    public void Start()
    {
        var setting = Main.DataMgr.Setting;
        RTU.Port     = setting.PortName;
        RTU.Baudrate = setting.Baudrate;
        RTU.Timeout  = setting.Timeout;

        // 주기 읽기 등록 (id, slaveNo, startAddr, length)
        RTU.AutoWordRead_FC3(1, 1, 0x0000, 50);
        RTU.AutoBitRead_FC1(2, 1, 0x0000, 16);

        RTU.Start();
    }

    // ⚠ 저수준 클래스는 Stop() 외에 스케줄/큐 수동 정리 필요 (래퍼와 다름)
    public void Stop()
    {
        RTU.Stop();
        RTU.ClearAuto();
        RTU.ClearManual();
        RTU.ClearWorkSchedule();
    }

    // 쓰기 메서드 (FC코드 직접 지정)
    public void WriteWord(int address, int value)
    {
        RTU.ManualWordWrite_FC6(1, 1, address, value);
    }

    public void WriteMultiWord(int address, int[] values)
    {
        RTU.ManualMultiWordWrite_FC16(1, 1, address, values);
    }
}
```

### 저수준 읽기 (Auto — 주기적)

```csharp
// FC3: 워드 읽기 (id, slaveNo, startAddr, length)
RTU.AutoWordRead_FC3(1, 1, 0x0000, 50);

// FC1: 코일 읽기 (id, slaveNo, startAddr, length)
RTU.AutoBitRead_FC1(2, 1, 0x0000, 16);
```

### 저수준 쓰기 (Manual — 즉시)

```csharp
// FC6: 단일 워드 쓰기 (id, slaveNo, address, value)
RTU.ManualWordWrite_FC6(1, 1, 0x0064, 100);

// FC16: 다중 워드 쓰기 (id, slaveNo, address, values)
RTU.ManualMultiWordWrite_FC16(1, 1, 0x0064, new int[] { 100, 200 });

// FC5: 단일 비트 쓰기 (id, slaveNo, address, value)
RTU.ManualBitWrite_FC5(1, 1, 0x0000, true);

// FC15: 다중 비트 쓰기 (id, slaveNo, address, values)
RTU.ManualMultiBitWrite_FC15(1, 1, 0x0000, new bool[] { true, false, true });
```

### 저수준 수신 이벤트

```csharp
RTU.WordReadReceived += (o, s) =>
{
    // s.Slave         — 슬레이브 번호 (int)
    // s.StartAddress  — 시작 주소 (int)
    // s.ReceiveData   — int[] 수신 데이터
};

RTU.BitReadReceived += (o, s) =>
{
    // s.Slave         — 슬레이브 번호 (int)
    // s.StartAddress  — 시작 주소 (int)
    // s.ReceiveData   — bool[] 수신 데이터
};

RTU.TimeoutReceived += (o, s) =>
{
    // 통신 타임아웃
};
```

### 저수준 정리/종료

```csharp
RTU.Stop();              // 통신 종료
RTU.ClearAuto();         // 주기 읽기 모두 제거
RTU.ClearManual();       // 수동 작업 큐 클리어
RTU.ClearWorkSchedule(); // 작업 스케줄 클리어
```

---

## ModbusTCPMaster 패턴 (저수준)

ModbusRTUMaster와 동일한 메서드/이벤트. 프로퍼티만 다름.

```csharp
var tcp = new ModbusTCPMaster();
tcp.RemoteIP   = "192.168.0.1";   // Host가 아닌 RemoteIP
tcp.RemotePort = 502;             // Port가 아닌 RemotePort
tcp.Timeout    = 1000;
```

---

## SlaveRTU 패턴 (래퍼 — 권장)

Modbus 슬레이브(서버) 역할. `BitAreas`/`WordAreas`에 메모리를 등록하면 마스터 요청을 자동 응답.

```csharp
public class DeviceManager
{
    public SlaveRTU Slave { get; } = new SlaveRTU();

    public DeviceManager()
    {
        // 메모리 영역 등록 (시작주소 → 메모리)
        Slave.WordAreas.Add(0x0000, new WordMemory(100));  // 워드 100개
        Slave.BitAreas.Add(0x0000, new BitMemory(32));     // 비트 32개

        // 연결 이벤트
        Slave.DeviceOpened += (o, s) => { /* 포트 열림 */ };
        Slave.DeviceClosed += (o, s) => { /* 포트 닫힘 */ };
    }

    public void Start()
    {
        Slave.Slave    = 1;              // 슬레이브 번호
        Slave.Port     = "COM1";
        Slave.Baudrate = 9600;

        Slave.Start();
    }

    public void Stop() => Slave.Stop();

    // 메모리 읽기/쓰기
    public int ReadWord(int baseAddr, int offset)
    {
        var mem = Slave.WordAreas[baseAddr];
        return mem[offset].W;                // WordMemory: mem[idx].W (ushort)
    }

    public void WriteWord(int baseAddr, int offset, int value)
    {
        var mem = Slave.WordAreas[baseAddr];
        mem[offset].W = (ushort)value;
    }

    public bool ReadBit(int baseAddr, int offset)
    {
        var mem = Slave.BitAreas[baseAddr];
        return mem[offset];                  // BitMemory: mem[idx] (bool 직접 인덱싱)
    }

    public void WriteBit(int baseAddr, int offset, bool value)
    {
        var mem = Slave.BitAreas[baseAddr];
        mem[offset] = value;
    }
}
```

### SlaveRTU 프로퍼티

| 프로퍼티 | 타입 | 설명 |
|---------|------|------|
| `Slave` | int (기본 1) | 슬레이브 번호 |
| `Port` | string | 시리얼 포트명 ("COM1") |
| `Baudrate` | int | 통신 속도 |
| `Parity` | Parity | 패리티 |
| `DataBits` | int | 데이터 비트 |
| `StopBits` | StopBits | 스톱 비트 |
| `IsStart` | bool (읽기전용) | 시작 상태 |
| `IsOpen` | bool (읽기전용) | 포트 열림 상태 |
| `BitAreas` | Dictionary\<int, BitMemory\> | 비트 메모리 영역 (시작주소 → 메모리) |
| `WordAreas` | Dictionary\<int, WordMemory\> | 워드 메모리 영역 (시작주소 → 메모리) |
| `Tag` | object? | 사용자 데이터 |

### SlaveRTU 이벤트

| 이벤트 | 설명 |
|--------|------|
| `DeviceOpened` | 포트 연결 성공 |
| `DeviceClosed` | 포트 연결 해제 |

### 메모리 인덱서 패턴 (중요)

```csharp
// WordMemory — WordRef 객체의 .W 프로퍼티 사용
var wordMem = new WordMemory(100);
wordMem[0].W = 1234;              // 쓰기 (ushort)
ushort val = wordMem[0].W;        // 읽기
short sval = wordMem[0].IW;       // 부호 있는 읽기
wordMem[0].DW = 100000;           // 더블워드 (uint, 2워드 사용)
wordMem[0].R = 3.14f;             // 실수 (float, 2워드 사용)
wordMem[0].Bit[0] = true;         // 워드 내 비트 접근

// BitMemory — bool 직접 인덱싱
var bitMem = new BitMemory(32);
bitMem[0] = true;                  // 쓰기
bool bit = bitMem[0];              // 읽기
```

---

## SlaveTCP 패턴 (래퍼 — 권장)

SlaveRTU와 동일한 구조. 프로퍼티/이벤트만 다름.

```csharp
public class DeviceManager
{
    public SlaveTCP Slave { get; } = new SlaveTCP();

    public DeviceManager()
    {
        Slave.WordAreas.Add(0x0000, new WordMemory(100));
        Slave.BitAreas.Add(0x0000, new BitMemory(32));

        // TCP는 Socket 이벤트 (DeviceOpened/Closed 아님!)
        Slave.SocketConnected += (o, s) => { /* 클라이언트 접속 */ };
        Slave.SocketDisconnected += (o, s) => { /* 클라이언트 해제 */ };
    }

    public void Start()
    {
        Slave.Slave     = 1;
        Slave.LocalPort = 502;   // Port가 아닌 LocalPort!

        Slave.Start();
    }

    public void Stop() => Slave.Stop();
}
```

### SlaveTCP 고유 프로퍼티

| 프로퍼티 | 타입 | 설명 |
|---------|------|------|
| `Slave` | int (기본 1) | 슬레이브 번호 |
| `LocalPort` | int | 리슨 포트 (기본 502). **`Port` 아님!** |
| `IsStart` | bool (읽기전용) | 시작 상태 |
| `BitAreas` | Dictionary\<int, BitMemory\> | 비트 메모리 영역 |
| `WordAreas` | Dictionary\<int, WordMemory\> | 워드 메모리 영역 |
| `Tag` | object? | 사용자 데이터 |

> **SlaveTCP에는 `IsOpen` 프로퍼티 없음** (TCP는 소켓 기반이므로).

### SlaveTCP 이벤트

| 이벤트 | 설명 |
|--------|------|
| `SocketConnected` | 클라이언트 접속 (**DeviceOpened 아님!**) |
| `SocketDisconnected` | 클라이언트 해제 (**DeviceClosed 아님!**) |

---

## DeviceData 모델 패턴 (Datas/DeviceData.cs)

슬레이브별 데이터를 도메인 모델로 래핑하는 패턴. **MasterRTU / MasterTCP와 함께 사용**.

### DeviceData 필수 규칙

- DeviceData는 항상 `ID` 프로퍼티(slaveId)를 가진다
- 읽기 프로퍼티: MasterRTU/MasterTCP의 `GetWord(ID, ...)` / `GetBit(ID, ...)`를 래핑
- 쓰기 메서드: `Set{기능이름}()` 형태로 MasterRTU/MasterTCP의 `SetWord` / `SetBit`를 래핑
- 통신 상태: MasterRTU/MasterTCP 사용 시 `IsOpen` 또는 `DeviceOpened/DeviceClosed` 이벤트로 판단

### MasterRTU + DeviceData (권장)

```csharp
public class DeviceData
{
    public int ID { get; set; }  // slaveId — 항상 존재

    private MasterRTU _rtu;

    public DeviceData(MasterRTU rtu, int slaveId)
    {
        _rtu = rtu;
        ID = slaveId;
    }

    // 읽기 — GetWord/GetBit로 캐시 조회 (slaveId 내장)
    public bool   RunState    => _rtu.GetBit(ID, "P0") ?? false;
    public int    ErrorCode   => _rtu.GetWord(ID, "D1") ?? 0;
    public double Temperature => (_rtu.GetWord(ID, "D10") ?? 0) / 10.0;  // 0.1°C 단위
    public int    Pressure    => _rtu.GetWord(ID, "D11") ?? 0;

    // 쓰기 — Set{기능이름}() 메서드로 SetWord/SetBit 래핑
    public void SetTargetTemp(double value)
        => _rtu.SetWord(ID, "D20", (int)(value * 10));
    public void SetRun(bool value)
        => _rtu.SetBit(ID, "P10", value);
    public void SetPressure(int value)
        => _rtu.SetWord(ID, "D21", value);
}
```

### DeviceManager + DeviceData 조합 예시

```csharp
public class DeviceManager
{
    public MasterRTU RTU { get; } = new MasterRTU();
    public DeviceData Device1 { get; private set; }
    public DeviceData Device2 { get; private set; }

    public DeviceManager()
    {
        RTU.WordAreas.Add(0x0000, "D");
        RTU.BitAreas.Add(0x0000, "P");

        // 슬레이브별 DeviceData 생성
        Device1 = new DeviceData(RTU, 1);
        Device2 = new DeviceData(RTU, 2);

        RTU.DeviceOpened += (o, s) => { /* 연결됨 */ };
        RTU.DeviceClosed += (o, s) => { /* 연결 해제 */ };
    }

    public bool IsConnected => RTU.IsOpen;

    public void Start()
    {
        var setting = Main.DataMgr.Setting;
        RTU.Port     = setting.PortName;
        RTU.Baudrate = setting.Baudrate;
        RTU.Timeout  = setting.Timeout;

        // 슬레이브별 주기 읽기 등록
        RTU.MonitorWord_F3(1, 0x0000, 50);
        RTU.MonitorBit_F1(1, 0x0000, 16);
        RTU.MonitorWord_F3(2, 0x0000, 50);
        RTU.MonitorBit_F1(2, 0x0000, 16);

        RTU.Start();
    }

    public void Stop() => RTU.Stop();
}
```

---

## CNet 패턴 (LS Electric PLC 시리얼 통신)

LS Electric PLC와 CNet 프로토콜로 통신. Modbus와 동일한 Auto/Manual 스케줄링 구조.

```csharp
public class DeviceManager
{
    public CNet LS { get; } = new CNet();

    public DeviceManager()
    {
        // 수신 이벤트
        LS.DataReceived += (o, s) =>
        {
            // s.MessageID  — 요청 ID (int)
            // s.Slave      — 슬레이브 번호 (int)
            // s.Function   — CNetFunc (READ_SINGLE, READ_BLOCK, WRITE_SINGLE, WRITE_BLOCK)
            // s.Data       — int[] 수신 데이터
            // s.ReadAddress — string[] 읽기 주소
        };

        LS.WriteResponseReceived += (o, s) => { /* 쓰기 완료 */ };
        LS.TimeoutReceived += (o, s) => { /* 타임아웃 */ };
        LS.BCCErrorReceived += (o, s) => { /* BCC 체크섬 오류 */ };
        LS.NAKReceived += (o, s) => { /* NAK 응답, s.ErrorCode */ };

        LS.DeviceOpened += (o, s) => { /* 포트 열림 */ };
        LS.DeviceClosed += (o, s) => { /* 포트 닫힘 */ };
    }

    public void Start()
    {
        var setting = Main.DataMgr.Setting;
        LS.Port     = setting.PortName;
        LS.Baudrate = setting.Baudrate;
        LS.Timeout  = setting.Timeout;

        // 주기 읽기 (Auto)
        LS.AutoRSS(1, 1, "%MW100");                          // 단일 디바이스
        LS.AutoRSS(2, 1, new[] { "%MW100", "%MW101" });      // 복수 디바이스
        LS.AutoRSB(3, 1, "%MW200", 10);                      // 블록 읽기 (시작주소, 길이)

        LS.Start();
    }

    public void Stop() => LS.Stop();

    // 수동 쓰기
    public void WriteValue(string device, int value)
    {
        LS.ManualWSS(10, 1, device, value);                   // 단일 쓰기
    }

    public void WriteBlock(string device, int[] values)
    {
        LS.ManualWSB(11, 1, device, values);                  // 블록 쓰기
    }
}
```

### CNet 주요 메서드

```csharp
// Auto (주기적 반복)
LS.AutoRSS(id, slave, device);                    // 단일 읽기
LS.AutoRSS(id, slave, devices[]);                 // 복수 읽기
LS.AutoRSB(id, slave, device, length);            // 블록 읽기

// Manual (1회 실행)
LS.ManualRSS(id, slave, device);                  // 단일 읽기
LS.ManualRSB(id, slave, device, length);          // 블록 읽기
LS.ManualWSS(id, slave, device, value);           // 단일 쓰기
LS.ManualWSS(id, slave, CNetValue[]);             // 복수 쓰기
LS.ManualWSB(id, slave, device, values[]);        // 블록 쓰기

// 큐 관리
LS.RemoveAuto(id);       LS.ClearAuto();
LS.RemoveManual(id);     LS.ClearManual();
LS.ClearWorkSchedule();
```

### CNet 프로퍼티

| 프로퍼티 | 타입 | 설명 |
|---------|------|------|
| `Port` | string | 시리얼 포트명 ("COM1") |
| `Baudrate` | int | 통신 속도 (기본 115200) |
| `Parity` | Parity | 패리티 |
| `DataBits` | int | 데이터 비트 |
| `StopBits` | StopBits | 스톱 비트 |
| `Timeout` | int | 타임아웃 (기본 100ms) |
| `Interval` | int | 폴링 간격 (기본 10ms) |
| `BufferSize` | int | 버퍼 크기 (기본 1024) |
| `IsStart` | bool (읽기전용) | 시작 상태 |
| `IsOpen` | bool (읽기전용) | 포트 열림 상태 |
| `AutoReconnect` | bool | 자동 재연결 |

---

## MC 패턴 (Mitsubishi PLC 시리얼 통신)

Mitsubishi PLC와 MC 프로토콜로 통신. 비트/워드가 별도 이벤트로 분리된 것이 CNet과의 주요 차이.

```csharp
public class DeviceManager
{
    public MC Mitsubishi { get; } = new MC();

    public DeviceManager()
    {
        // 워드/비트 수신 이벤트가 분리됨
        Mitsubishi.WordDataReceived += (o, s) =>
        {
            // s.MessageID, s.Slave, s.Function (MCFunc), s.Data (int[])
        };

        Mitsubishi.BitDataReceived += (o, s) =>
        {
            // s.MessageID, s.Slave, s.Function (MCFunc), s.Data (bool[])
        };

        Mitsubishi.WriteResponseReceived += (o, s) => { /* 쓰기 완료 */ };
        Mitsubishi.TimeoutReceived += (o, s) => { /* 타임아웃 */ };
        Mitsubishi.CheckSumErrorReceived += (o, s) => { /* 체크섬 오류 */ };
        Mitsubishi.NakErrorReceived += (o, s) => { /* NAK 오류 */ };

        Mitsubishi.DeviceOpened += (o, s) => { /* 포트 열림 */ };
        Mitsubishi.DeviceClosed += (o, s) => { /* 포트 닫힘 */ };
    }

    public void Start()
    {
        var setting = Main.DataMgr.Setting;
        Mitsubishi.Port     = setting.PortName;
        Mitsubishi.Baudrate = setting.Baudrate;
        Mitsubishi.Timeout  = setting.Timeout;

        // 주기 읽기 (Auto) — device, length 필수, waitTime 선택
        Mitsubishi.AutoWordRead(1, 1, "D0", 50);       // 워드 D0~D49 주기 읽기
        Mitsubishi.AutoBitRead(2, 1, "M0", 16);        // 비트 M0~M15 주기 읽기

        Mitsubishi.Start();
    }

    public void Stop() => Mitsubishi.Stop();

    // 수동 쓰기
    public void WriteWord(string device, int value)
    {
        Mitsubishi.ManualWordWrite(10, 1, device, value);
    }

    public void WriteBit(string device, bool value)
    {
        Mitsubishi.ManualBitWrite(11, 1, device, value);
    }
}
```

### MC 주요 메서드

```csharp
// Auto (주기적 반복)
Mitsubishi.AutoWordRead(id, slave, device, length);
Mitsubishi.AutoBitRead(id, slave, device, length);

// Manual (1회 실행)
Mitsubishi.ManualWordRead(id, slave, device, length);
Mitsubishi.ManualBitRead(id, slave, device, length);
Mitsubishi.ManualWordWrite(id, slave, device, value);       // int 단일
Mitsubishi.ManualWordWrite(id, slave, device, values[]);    // int[] 복수
Mitsubishi.ManualBitWrite(id, slave, device, value);        // bool 단일
Mitsubishi.ManualBitWrite(id, slave, device, values[]);     // bool[] 복수

// 큐 관리 (CNet과 동일)
Mitsubishi.RemoveAuto(id);       Mitsubishi.ClearAuto();
Mitsubishi.RemoveManual(id);     Mitsubishi.ClearManual();
Mitsubishi.ClearWorkSchedule();
```

### MC 프로퍼티 (CNet과 공통 + 추가)

| 프로퍼티 | 타입 | 설명 |
|---------|------|------|
| `UseControlSequence` | bool | CR/LF 제어 시퀀스 사용 (기본 false) |
| `UseCheckSum` | bool | 체크섬 검증 사용 (기본 false) |

> 나머지 프로퍼티(Port, Baudrate, Parity, DataBits, StopBits, Timeout, Interval, BufferSize, IsStart, IsOpen, AutoReconnect)는 CNet과 동일.

---

## MQTT 패턴

```csharp
var mqtt = new MQClient();
mqtt.BrokerHostName = "192.168.0.100";
// Port 프로퍼티 없음 — 기본 1883 사용

// 연결/해제 이벤트
mqtt.Connected += (o, s) => { /* 연결됨 */ };
mqtt.Disconnected += (o, s) => { /* 해제됨 */ };

// 수신 이벤트
mqtt.Received += (o, s) =>
{
    // s.Topic  — string
    // s.Datas  — byte[] (문자열 아님!)
    var message = System.Text.Encoding.UTF8.GetString(s.Datas);
    Console.WriteLine($"{s.Topic}: {message}");
};

// 연결 (3가지 오버로드)
mqtt.Start();                                      // 자동 ClientID
mqtt.Start("myClient");                            // ClientID 지정
mqtt.Start("myClient", "user", "password");        // 인증 포함

// 구독
mqtt.Subscribe("sensor/temperature");                      // 기본 QoS
mqtt.Subscribe("sensor/temperature", MQQos.LeastOnce);     // QoS 지정

// 발행
mqtt.Publish("command/start", "1", MQQos.LeastOnce);                 // string 데이터
mqtt.Publish("command/data", new byte[] { 0x01, 0x02 }, MQQos.LeastOnce);  // byte[] 데이터

// 구독 해제
mqtt.Unsubscribe("sensor/temperature");
mqtt.UnsubscribeClear();  // 전체 구독 해제

// 종료
mqtt.Stop();
```

### MQQos 열거형

| 값 | 이름 | 설명 |
|----|------|------|
| 0 | `MostOnce` | 최대 1회 (보장 없음) |
| 1 | `LeastOnce` | 최소 1회 (중복 가능) |
| 2 | `ExactlyOnce` | 정확히 1회 |
| 128 | `GrantedFailure` | 구독 실패 |

### MQReceiveArgs 프로퍼티

| 프로퍼티 | 타입 | 설명 |
|---------|------|------|
| `Topic` | string | 수신 토픽 |
| `Datas` | byte[] | 수신 데이터 (UTF-8 디코딩 필요) |

### MQClient 프로퍼티

| 프로퍼티 | 타입 | 설명 |
|---------|------|------|
| `BrokerHostName` | string | 브로커 주소 (기본 "127.0.0.1") |
| `IsStart` | bool | 시작 상태 |
| `IsConnected` | bool | 연결 상태 |
| `Subscribes` | List\<MQSubscribe\> | 활성 구독 목록 |

---

## 자주 하는 실수

| 잘못된 사용 | 올바른 사용 | 설명 |
|------------|-----------|------|
| `ModbusRTUMaster`에서 `WordAreas` 사용 | `MasterRTU`에서 `WordAreas` 사용 | WordAreas는 래퍼에만 있음 |
| `MasterRTU`에서 `AutoWordRead_FC3` 호출 | `RTU.MonitorWord_F3(slave, addr, len)` | 래퍼는 Monitor 메서드 사용 |
| `MasterTCP.Host = "..."` | `MasterTCP.RemoteIP = "..."` | MasterTCP 프로퍼티명 주의 |
| `MasterTCP.Port = 502` | `MasterTCP.RemotePort = 502` | MasterTCP 프로퍼티명 주의 |
| `s.SlaveAddress` | `s.Slave` | 이벤트 인자 프로퍼티명 |
| `s.Message` (MQTT) | `s.Datas` (byte[]) | MQTT 수신은 byte[], UTF-8 디코딩 필요 |
| `MQQosLevel.AtLeastOnce` | `MQQos.LeastOnce` | 열거형 이름과 값 모두 다름 |
| `mqtt.Port = 1883` | (설정 불가) | MQClient에 Port 프로퍼티 없음 |
| `ManualWordWrite_FC16(...)` | `ManualMultiWordWrite_FC16(...)` | FC16은 Multi 접두사 필요 |
| `ManualBitWrite_FC15(...)` | `ManualMultiBitWrite_FC15(...)` | FC15는 Multi 접두사 필요 |
| `GetWord("D", 0)` | `GetWord(1, "D0")` | 첫 인자=slave번호, 둘째="영역+오프셋" 문자열 |
| `SlaveTCP.Port = 502` | `SlaveTCP.LocalPort = 502` | SlaveTCP는 `LocalPort` 사용 |
| `SlaveTCP.DeviceOpened` | `SlaveTCP.SocketConnected` | TCP 슬레이브는 Socket 이벤트 사용 |
| `wordMem[0] = 100` | `wordMem[0].W = 100` | WordMemory는 `.W` 프로퍼티 필요 (WordRef 반환) |
| `bitMem[0].W` | `bitMem[0]` | BitMemory는 bool 직접 반환 (.W 없음) |
| `wordMem[0].Value` | `wordMem[0].W` | 구 API의 `.Value`는 `.W`(ushort)로 변경됨 |
| 일반 폴링에 `ModbusRTUMaster` 사용 | `MasterRTU` 사용 | 저수준은 특수 목적 전용 |
| DeviceData에 `ID`(slaveId) 없음 | DeviceData에 항상 `ID` 프로퍼티 포함 | DeviceData는 반드시 slaveId를 가짐 |
| `MasterRTU`에서 통신 상태를 `CommState`로 판단 | `RTU.IsOpen` 또는 `DeviceOpened/Closed` 이벤트 | MasterRTU는 IsOpen으로 판단 |
