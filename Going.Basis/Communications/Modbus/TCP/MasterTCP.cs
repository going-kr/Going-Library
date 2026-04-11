using Going.Basis.Communications.Modbus.RTU;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Communications.Modbus.TCP
{
    /// <summary>
    /// Modbus TCP 마스터 래퍼 클래스입니다.
    /// BitAreas/WordAreas 기반 메모리 매핑과 슬레이브별 디바이스 모니터링을 제공합니다.
    /// </summary>
    public class MasterTCP
    {
        #region Properties
        /// <summary>접속 대상 IP 주소</summary>
        public string RemoteIP { get => modbus.RemoteIP; set => modbus.RemoteIP = value; }
        /// <summary>접속 대상 포트 번호</summary>
        public int RemotePort { get => modbus.RemotePort; set => modbus.RemotePort = value; }

        /// <summary>폴링 간격 (밀리초)</summary>
        public int Interval { get => modbus.Interval; set => modbus.Interval = value; }
        /// <summary>응답 타임아웃 (밀리초)</summary>
        public int Timeout { get => modbus.Timeout; set => modbus.Timeout = value; }

        /// <summary>통신 루프가 시작되었는지 여부</summary>
        public bool IsStart => modbus.IsStart;
        /// <summary>TCP 소켓이 연결되어 있는지 여부</summary>
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
        private ModbusTCPMaster modbus;
        #endregion

        #region Event
        /// <summary>TCP 소켓 연결이 끊어졌을 때 발생합니다.</summary>
        public event EventHandler<EventArgs>? DeviceClosed;
        /// <summary>TCP 소켓이 연결되었을 때 발생합니다.</summary>
        public event EventHandler<EventArgs>? DeviceOpened;
        #endregion

        #region Constructor
        /// <summary>Modbus TCP 마스터 래퍼 인스턴스를 생성한다.</summary>
        public MasterTCP()
        {
            modbus = new ModbusTCPMaster();

            modbus.BitReadReceived += Modbus_BitReadReceived;
            modbus.WordReadReceived += Modbus_WordReadReceived;

            modbus.SocketConnected += (o, s) => DeviceOpened?.Invoke(this, s);
            modbus.SocketDisconnected += (o, s) => DeviceClosed?.Invoke(this, s);
        }
        #endregion

        #region Handler
        private void Modbus_BitReadReceived(object? sender, ModbusTCPMaster.BitReadEventArgs e)
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

        private void Modbus_WordReadReceived(object? sender, ModbusTCPMaster.WordReadEventArgs e)
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
}
