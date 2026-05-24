using Going.Basis.Datas;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Communications.Modbus.RTU
{
    /// <summary>
    /// Modbus RTU 마스터 래퍼 클래스입니다.
    /// Function code별 0-base 메모리 매핑과 슬레이브별 디바이스 모니터링을 제공합니다.
    /// </summary>
    public class MasterRTU
    {
        #region Properties
        /// <summary>시리얼 포트 이름 (예: "COM1")</summary>
        public string Port { get => modbus.Port; set => modbus.Port = value; }
        /// <summary>통신 속도 (bps)</summary>
        public int Baudrate { get => modbus.Baudrate; set => modbus.Baudrate = value; }
        /// <summary>패리티 비트 설정</summary>
        public Parity Parity { get => modbus.Parity; set => modbus.Parity = value; }
        /// <summary>데이터 비트 수</summary>
        public int DataBits { get => modbus.DataBits; set => modbus.DataBits = value; }
        /// <summary>정지 비트 설정</summary>
        public StopBits StopBits { get => modbus.StopBits; set => modbus.StopBits = value; }

        /// <summary>폴링 간격 (밀리초)</summary>
        public int Interval { get => modbus.Interval; set => modbus.Interval = value; }
        /// <summary>응답 타임아웃 (밀리초)</summary>
        public int Timeout { get => modbus.Timeout; set => modbus.Timeout = value; }

        /// <summary>통신 루프가 시작되었는지 여부</summary>
        public bool IsStart => modbus.IsStart;
        /// <summary>시리얼 포트가 열려 있는지 여부</summary>
        public bool IsOpen => modbus.IsOpen;
        /// <summary>연결 끊김 시 자동 재연결 여부 (기본값: false)</summary>
        public bool AutoReconnect { get => modbus.AutoReconnect; set => modbus.AutoReconnect = value; }

        /// <summary>슬레이브별 메모리 저장소 (키: 슬레이브 주소)</summary>
        public Dictionary<int, Mems> Devices { get; } = [];
        /// <summary>슬레이브별 마지막 수신 시각 (키: 슬레이브 주소)</summary>
        public Dictionary<int, DateTime> LastReceived { get; } = [];
        /// <summary>사용자 정의 태그 객체</summary>
        public object? Tag { get; set; } = null;
        #endregion

        #region Member Variable
        private ModbusRTUMaster modbus;
        #endregion

        #region Event
        /// <summary>시리얼 포트가 닫혔을 때 발생합니다.</summary>
        public event EventHandler? DeviceClosed;
        /// <summary>시리얼 포트가 열렸을 때 발생합니다.</summary>
        public event EventHandler? DeviceOpened;
        #endregion

        #region Constructor
        /// <summary>Modbus RTU 마스터 래퍼 인스턴스를 생성한다.</summary>
        public MasterRTU()
        {
            modbus = new ModbusRTUMaster();

            modbus.BitReadReceived += Modbus_BitReadReceived;
            modbus.WordReadReceived += Modbus_WordReadReceived;

            modbus.DeviceOpened += (o, s) => DeviceOpened?.Invoke(this, System.EventArgs.Empty);
            modbus.DeviceClosed += (o, s) => DeviceClosed?.Invoke(this, System.EventArgs.Empty);
        }
        #endregion

        #region Handler
        private void Modbus_BitReadReceived(object? sender, ModbusRTUMaster.BitReadEventArgs e)
        {
            if (!Devices.ContainsKey(e.Slave)) Devices.Add(e.Slave, new Mems());
            LastReceived[e.Slave] = DateTime.Now;

            var area = e.Function == ModbusFunction.BITREAD_F1 ? Devices[e.Slave].Coils :
                       e.Function == ModbusFunction.BITREAD_F2 ? Devices[e.Slave].Contacts :
                       null;

            if (area != null)
                for (int i = 0; i < e.ReceiveData.Length; i++)
                    area[e.StartAddress + i] = e.ReceiveData[i];
        }

        private void Modbus_WordReadReceived(object? sender, ModbusRTUMaster.WordReadEventArgs e)
        {
            if (!Devices.ContainsKey(e.Slave)) Devices.Add(e.Slave, new Mems());
            LastReceived[e.Slave] = DateTime.Now;

            var area = e.Function == ModbusFunction.WORDREAD_F3 ? Devices[e.Slave].HoldingRegister :
                       e.Function == ModbusFunction.WORDREAD_F4 ? Devices[e.Slave].InputRegister :
                       null;

            if (area != null)
                for (int i = 0; i < e.ReceiveData.Length; i++)
                    area[e.StartAddress + i] = e.ReceiveData[i];
        }
        #endregion

        #region Method
        /// <summary>통신을 시작합니다.</summary>
        public void Start() => modbus.Start();
        /// <summary>통신을 중지합니다.</summary>
        public void Stop() => modbus.Stop();

        /// <summary>FC1을 사용하여 코일 영역 자동 모니터링을 등록합니다.</summary>
        public void MonitorCoils_FC1(int slave, int startAddr, int length) => modbus.AutoBitRead_FC1(0, slave, startAddr, length);
        /// <summary>FC2를 사용하여 접점 영역 자동 모니터링을 등록합니다.</summary>
        public void MonitorContacts_FC2(int slave, int startAddr, int length) => modbus.AutoBitRead_FC2(0, slave, startAddr, length);
        /// <summary>FC3을 사용하여 보유 레지스터 영역 자동 모니터링을 등록합니다.</summary>
        public void MonitorHoldingRegister_FC3(int slave, int startAddr, int length) => modbus.AutoWordRead_FC3(0, slave, startAddr, length);
        /// <summary>FC4를 사용하여 입력 레지스터 영역 자동 모니터링을 등록합니다.</summary>
        public void MonitorInputRegister_FC4(int slave, int startAddr, int length) => modbus.AutoWordRead_FC4(0, slave, startAddr, length);

        /// <summary>FC5를 사용하여 0-base 코일 주소에 값을 씁니다.</summary>
        public void WriteCoil(int slave, int address, bool value) => modbus.ManualBitWrite_FC5(1, slave, address, value);
        /// <summary>FC15를 사용하여 0-base 코일 주소부터 여러 값을 씁니다.</summary>
        public void WriteCoils(int slave, int address, bool[] values) => modbus.ManualMultiBitWrite_FC15(1, slave, address, values);
        /// <summary>FC6을 사용하여 0-base 보유 레지스터 주소에 값을 씁니다.</summary>
        public void WriteHoldingRegister(int slave, int address, int value) => modbus.ManualWordWrite_FC6(1, slave, address, value);
        /// <summary>FC16을 사용하여 0-base 보유 레지스터 주소부터 여러 값을 씁니다.</summary>
        public void WriteHoldingRegisters(int slave, int address, int[] values) => modbus.ManualMultiWordWrite_FC16(1, slave, address, values);

        /// <summary>FC1 코일 읽기 결과를 0-base 주소로 조회합니다.</summary>
        public bool? GetCoil(int slave, int address) => Devices.TryGetValue(slave, out var dev) && dev.Coils.TryGetValue(address, out var val) ? val : null;
        /// <summary>FC2 접점 읽기 결과를 0-base 주소로 조회합니다.</summary>
        public bool? GetContact(int slave, int address) => Devices.TryGetValue(slave, out var dev) && dev.Contacts.TryGetValue(address, out var val) ? val : null;
        /// <summary>FC3 보유 레지스터 읽기 결과를 0-base 주소로 조회합니다.</summary>
        public int? GetHoldingRegister(int slave, int address) => Devices.TryGetValue(slave, out var dev) && dev.HoldingRegister.TryGetValue(address, out var val) ? val : null;
        /// <summary>FC4 입력 레지스터 읽기 결과를 0-base 주소로 조회합니다.</summary>
        public int? GetInputRegister(int slave, int address) => Devices.TryGetValue(slave, out var dev) && dev.InputRegister.TryGetValue(address, out var val) ? val : null;
        #endregion
    }

    /// <summary>
    /// 슬레이브 장치의 비트/워드 메모리를 저장하는 클래스입니다.
    /// </summary>
    public class Mems
    {
        /// <summary>FC1 코일 영역 (키: 0-base 주소)</summary>
        public Dictionary<int, bool> Coils { get; } = [];
        /// <summary>FC2 이산 입력 영역 (키: 0-base 주소)</summary>
        public Dictionary<int, bool> Contacts { get; } = [];
        /// <summary>FC3 보유 레지스터 영역 (키: 0-base 주소)</summary>
        public Dictionary<int, int> HoldingRegister { get; } = [];
        /// <summary>FC4 입력 레지스터 영역 (키: 0-base 주소)</summary>
        public Dictionary<int, int> InputRegister { get; } = [];
    }
}
