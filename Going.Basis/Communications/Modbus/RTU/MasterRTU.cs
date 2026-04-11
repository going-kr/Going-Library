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
    /// BitAreas/WordAreas 기반 메모리 매핑과 슬레이브별 디바이스 모니터링을 제공합니다.
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

        /// <summary>비트 영역 매핑 (키: Modbus 시작 주소, 값: 영역 코드 접두사)</summary>
        public Dictionary<int, string> BitAreas { get; } = [];
        /// <summary>워드 영역 매핑 (키: Modbus 시작 주소, 값: 영역 코드 접두사)</summary>
        public Dictionary<int, string> WordAreas { get; } = [];

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

            foreach (var baseAddr in BitAreas.Keys.OrderByDescending(x => x))
            {
                var code = BitAreas[baseAddr];
                if (e.StartAddress >= baseAddr)
                {
                    for (int i = 0; i < e.ReceiveData.Length; i++)
                        Devices[e.Slave].Bits[$"{code}{e.StartAddress + i - baseAddr}"] = e.ReceiveData[i];
                }
            }
        }

        private void Modbus_WordReadReceived(object? sender, ModbusRTUMaster.WordReadEventArgs e)
        {
            if (!Devices.ContainsKey(e.Slave)) Devices.Add(e.Slave, new Mems());
            LastReceived[e.Slave] = DateTime.Now;

            foreach (var baseAddr in WordAreas.Keys.OrderByDescending(x => x))
            {
                var code = WordAreas[baseAddr];
                if (e.StartAddress >= baseAddr)
                {
                    for (int i = 0; i < e.ReceiveData.Length; i++)
                        Devices[e.Slave].Words[$"{code}{e.StartAddress + i - baseAddr}"] = e.ReceiveData[i];
                }
            }
        }
        #endregion

        #region Method
        /// <summary>통신을 시작합니다.</summary>
        public void Start() => modbus.Start();
        /// <summary>통신을 중지합니다.</summary>
        public void Stop() => modbus.Stop();

        /// <summary>
        /// FC1을 사용하여 비트 영역 자동 모니터링을 등록합니다.
        /// </summary>
        /// <param name="slave">슬레이브 주소</param>
        /// <param name="startAddr">시작 주소</param>
        /// <param name="length">읽을 비트 수</param>
        public void MonitorBit_F1(int slave, int startAddr, int length) => modbus.AutoBitRead_FC1(0, slave, startAddr, length);
        /// <inheritdoc cref="MonitorBit_F1"/>
        public void MonitorBit_F2(int slave, int startAddr, int length) => modbus.AutoBitRead_FC2(0, slave, startAddr, length);
        /// <summary>
        /// FC3을 사용하여 워드 영역 자동 모니터링을 등록합니다.
        /// </summary>
        /// <param name="slave">슬레이브 주소</param>
        /// <param name="startAddr">시작 주소</param>
        /// <param name="length">읽을 워드 수</param>
        public void MonitorWord_F3(int slave, int startAddr, int length) => modbus.AutoWordRead_FC3(0, slave, startAddr, length);
        /// <inheritdoc cref="MonitorWord_F3"/>
        public void MonitorWord_F4(int slave, int startAddr, int length) => modbus.AutoWordRead_FC4(0, slave, startAddr, length);

        /// <summary>
        /// 영역 코드 기반 주소로 워드 값을 슬레이브에 씁니다.
        /// </summary>
        /// <param name="slave">슬레이브 주소</param>
        /// <param name="addr">영역 코드 기반 주소 (예: "D100")</param>
        /// <param name="value">쓸 값</param>
        public void SetWord(int slave, string addr, int value)
        {
            var v = WordAreas.FirstOrDefault(x => addr.StartsWith(x.Value));
            if (v.Value != null && int.TryParse(addr.AsSpan(v.Value.Length), out var idx))
                modbus.ManualWordWrite_FC6(1, slave, v.Key + idx, value);
        }

        /// <summary>
        /// 영역 코드 기반 주소로 비트 값을 슬레이브에 씁니다.
        /// </summary>
        /// <param name="slave">슬레이브 주소</param>
        /// <param name="addr">영역 코드 기반 주소 (예: "M0")</param>
        /// <param name="value">쓸 값</param>
        public void SetBit(int slave, string addr, bool value)
        {
            var v = BitAreas.FirstOrDefault(x => addr.StartsWith(x.Value));
            if (v.Value != null && int.TryParse(addr.AsSpan(v.Value.Length), out var idx))
                modbus.ManualBitWrite_FC5(1, slave, v.Key + idx, value);
        }

        /// <summary>
        /// 슬레이브의 워드 값을 조회합니다.
        /// </summary>
        /// <param name="slave">슬레이브 주소</param>
        /// <param name="addr">영역 코드 기반 주소</param>
        /// <returns>워드 값, 없으면 null</returns>
        public int? GetWord(int slave, string addr)
        {
            int? ret = null;
            if (Devices.TryGetValue(slave, out var dev) && dev.Words.TryGetValue(addr, out var val)) ret = val;
            return ret;
        }

        /// <summary>
        /// 슬레이브의 비트 값을 조회합니다.
        /// </summary>
        /// <param name="slave">슬레이브 주소</param>
        /// <param name="addr">영역 코드 기반 주소</param>
        /// <returns>비트 값, 없으면 null</returns>
        public bool? GetBit(int slave, string addr)
        {
            bool? ret = null;
            if (Devices.TryGetValue(slave, out var dev) && dev.Bits.TryGetValue(addr, out var val)) ret = val;
            return ret;
        }
        #endregion
    }

    /// <summary>
    /// 슬레이브 장치의 비트/워드 메모리를 저장하는 클래스입니다.
    /// </summary>
    public class Mems
    {
        /// <summary>비트 메모리 저장소 (키: 영역 코드 기반 주소)</summary>
        public Dictionary<string, bool> Bits { get; } = [];
        /// <summary>워드 메모리 저장소 (키: 영역 코드 기반 주소)</summary>
        public Dictionary<string, int> Words { get; } = [];
    }
}
