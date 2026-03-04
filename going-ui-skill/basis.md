# Going.Basis — 통신 코드 패턴

---

## .csproj 패키지 참조

```xml
<ItemGroup>
  <PackageReference Include="Going.Basis" Version="1.0.0" />
  <PackageReference Include="Going.UI.OpenTK" Version="1.0.4" />
</ItemGroup>
```

> 통신만 사용하는 경우 `Going.Basis`만 참조. UI 포함 시 `Going.UI.OpenTK` (또는 `Going.UI.Forms`) 추가.

---

## Modbus 아키텍처 (두 레이어)

Going.Basis의 Modbus는 **두 레이어**로 구성:

| 레이어 | RTU 클래스 | TCP 클래스 | 용도 |
|--------|-----------|-----------|------|
| **마스터 래퍼 (권장)** | `MasterRTU` | `MasterTCP` | WordAreas/BitAreas, GetWord/GetBit, Monitor/Set 메서드, 자동 캐싱 |
| **마스터 저수준** | `ModbusRTUMaster` | `ModbusTCPMaster` | Auto/Manual FC 메서드, 수신 이벤트 직접 처리 |
| **슬레이브 래퍼 (권장)** | `SlaveRTU` | `SlaveTCP` | BitAreas/WordAreas (메모리), 자동 요청 처리 |
| **슬레이브 저수준** | `ModbusRTUSlave` | `ModbusTCPSlave` | Request 이벤트 직접 처리 |

> 래퍼는 내부적으로 저수준 클래스를 감싸며, 마스터는 수신 데이터를 `Devices` 딕셔너리에 자동 캐싱, 슬레이브는 메모리 영역을 자동 응답.
> **일반적인 사용에는 래퍼를 권장**. 직접 이벤트 처리가 필요하면 저수준 사용.

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

        // 연결 이벤트
        RTU.DeviceOpened += (o, s) => { /* 포트 열림 */ };
        RTU.DeviceClosed += (o, s) => { /* 포트 닫힘 */ };
    }

    public void Start()
    {
        var setting = Main.DataMgr.Setting;
        RTU.Port     = setting.PortName;
        RTU.Baudrate = setting.Baudrate;
        RTU.Timeout  = setting.Timeout;

        // 주기 읽기 등록 (slaveNo, startAddr, length)
        RTU.MonitorWord_F3(1, 0x0000, 50);

        RTU.Start();
    }

    public void Stop()
    {
        RTU.Stop();
    }

    // 값 읽기 (자동 캐싱에서 조회)
    public int? ReadWord(int slave, string addr) => RTU.GetWord(slave, addr);
    public bool? ReadBit(int slave, string addr) => RTU.GetBit(slave, addr);

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
// 주기 읽기 (Monitor)
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

### MasterTCP 고유 프로퍼티

| 프로퍼티 | 타입 | 설명 |
|---------|------|------|
| `RemoteIP` | string | 서버 IP 주소 |
| `RemotePort` | int | 서버 포트 (기본 502) |
| `Interval` | int | 폴링 간격 |
| `Timeout` | int | 타임아웃 |

> 나머지 메서드/이벤트는 MasterRTU와 동일 (Monitor/Set/Get/DeviceOpened/DeviceClosed).

---

## ModbusRTUMaster 패턴 (저수준 — 직접 이벤트 처리)

수신 이벤트를 직접 처리하고 DeviceData 모델로 매핑할 때 사용.

```csharp
public class DeviceManager
{
    public ModbusRTUMaster RTU { get; } = new ModbusRTUMaster();
    public DeviceData Data { get; } = new DeviceData();

    public DeviceManager()
    {
        // 수신 이벤트 (저수준 — WordAreas/GetWord 없음)
        RTU.WordReadReceived += (o, s) =>
        {
            Data.Set(s.Slave, s.StartAddress, s.ReceiveData);
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
        Slave.WordAreas.Add(0x0000, new WordMemories(100));  // 워드 100개
        Slave.BitAreas.Add(0x0000, new BitMemories(32));     // 비트 32개

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
        return mem[offset].Value;            // WordMemories: mem[idx].Value (int)
    }

    public void WriteWord(int baseAddr, int offset, int value)
    {
        var mem = Slave.WordAreas[baseAddr];
        mem[offset].Value = value;
    }

    public bool ReadBit(int baseAddr, int offset)
    {
        var mem = Slave.BitAreas[baseAddr];
        return mem[offset];                  // BitMemories: mem[idx] (bool 직접)
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
| `BitAreas` | Dictionary\<int, BitMemories\> | 비트 메모리 영역 (시작주소 → 메모리) |
| `WordAreas` | Dictionary\<int, WordMemories\> | 워드 메모리 영역 (시작주소 → 메모리) |
| `Tag` | object? | 사용자 데이터 |

### SlaveRTU 이벤트

| 이벤트 | 설명 |
|--------|------|
| `DeviceOpened` | 포트 연결 성공 |
| `DeviceClosed` | 포트 연결 해제 |

### 메모리 인덱서 패턴 (중요)

```csharp
// WordMemories — WORD 객체의 .Value 프로퍼티 사용
var wordMem = new WordMemories(100);
wordMem[0].Value = 1234;           // 쓰기
int val = wordMem[0].Value;        // 읽기

// BitMemories — bool 직접 인덱싱
var bitMem = new BitMemories(32);
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
        Slave.WordAreas.Add(0x0000, new WordMemories(100));
        Slave.BitAreas.Add(0x0000, new BitMemories(32));

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
| `BitAreas` | Dictionary\<int, BitMemories\> | 비트 메모리 영역 |
| `WordAreas` | Dictionary\<int, WordMemories\> | 워드 메모리 영역 |
| `Tag` | object? | 사용자 데이터 |

> **SlaveTCP에는 `IsOpen` 프로퍼티 없음** (TCP는 소켓 기반이므로).

### SlaveTCP 이벤트

| 이벤트 | 설명 |
|--------|------|
| `SocketConnected` | 클라이언트 접속 (**DeviceOpened 아님!**) |
| `SocketDisconnected` | 클라이언트 해제 (**DeviceClosed 아님!**) |

---

## DeviceData 모델 패턴 (Datas/DeviceData.cs)

수신된 원시 레지스터 배열을 의미있는 프로퍼티로 변환.
`CommState`는 마지막 수신 시각으로 통신 상태 판단.
**저수준 API (ModbusRTUMaster)의 WordReadReceived와 함께 사용.**

```csharp
public class DeviceData
{
    // 원시 레지스터 저장소
    private readonly int[] _regs = new int[200];
    private DateTime _recvTime = DateTime.MinValue;

    // 통신 상태 — 마지막 수신 후 3초 이내
    public bool CommState => (DateTime.Now - _recvTime).TotalSeconds < 3;

    // 원시 데이터 업데이트 (WordReadReceived에서 호출)
    public void Set(int slave, int startAddress, int[] data)
    {
        for (int i = 0; i < data.Length; i++)
        {
            var idx = startAddress + i;
            if (idx < _regs.Length) _regs[idx] = data[i];
        }
        _recvTime = DateTime.Now;
    }

    // 의미있는 프로퍼티로 변환
    public bool   RunState    => _regs[0] == 1;
    public int    ErrorCode   => _regs[1];
    public double Temperature => _regs[10] / 10.0;  // 0.1°C 단위
    public int    Pressure    => _regs[11];
}
```

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
| `TCP.Host = "..."` | `TCP.RemoteIP = "..."` | TCP 프로퍼티명 주의 |
| `TCP.Port = 502` | `TCP.RemotePort = 502` | TCP 프로퍼티명 주의 |
| `s.SlaveAddress` | `s.Slave` | 이벤트 인자 프로퍼티명 |
| `s.Message` (MQTT) | `s.Datas` (byte[]) | MQTT 수신은 byte[], UTF-8 디코딩 필요 |
| `MQQosLevel.AtLeastOnce` | `MQQos.LeastOnce` | 열거형 이름과 값 모두 다름 |
| `mqtt.Port = 1883` | (설정 불가) | MQClient에 Port 프로퍼티 없음 |
| `ManualWordWrite_FC16(...)` | `ManualMultiWordWrite_FC16(...)` | FC16은 Multi 접두사 필요 |
| `ManualBitWrite_FC15(...)` | `ManualMultiBitWrite_FC15(...)` | FC15는 Multi 접두사 필요 |
| `GetWord("D", 0)` | `GetWord(1, "D0")` | 첫 인자=slave번호, 둘째="영역+오프셋" 문자열 |
| `SlaveTCP.Port = 502` | `SlaveTCP.LocalPort = 502` | SlaveTCP는 `LocalPort` 사용 |
| `SlaveTCP.DeviceOpened` | `SlaveTCP.SocketConnected` | TCP 슬레이브는 Socket 이벤트 사용 |
| `wordMem[0] = 100` | `wordMem[0].Value = 100` | WordMemories는 `.Value` 프로퍼티 필요 |
| `bitMem[0].Value` | `bitMem[0]` | BitMemories는 bool 직접 반환 (Value 없음) |
