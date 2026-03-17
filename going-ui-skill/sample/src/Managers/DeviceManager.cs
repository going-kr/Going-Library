using Going.Basis.Communications.Modbus.RTU;
using SenvasSample.Datas;

namespace SenvasSample.Managers
{
    public class DeviceManager
    {
        public MasterRTU RTU { get; } = new MasterRTU();
        public DeviceData Data { get; private set; }

        public bool IsConnected => RTU.IsOpen;

        public DeviceManager()
        {
            // WordAreas: base address 0x7001 maps to area "D"
            // D0 = 0x7001 (BoardStatus), D1 = 0x7002 (AlarmStatus), ...
            RTU.WordAreas.Add(0x7001, "D");

            // DeviceData for slave 1
            Data = new DeviceData(RTU, 1);

            RTU.DeviceOpened += (o, s) => { };
            RTU.DeviceClosed += (o, s) => { };
        }

        public void Start()
        {
            var setting = Main.DataMgr.Setting;
            RTU.Port = setting.PortName;
            RTU.Baudrate = setting.Baudrate;
            RTU.Timeout = setting.Timeout;

            // Read 0x7001~0x7012 (18 registers) for real-time monitoring
            RTU.MonitorWord_F3(setting.SlaveId, 0x7001, 18);

            // Read 0x7015~0x703F (43 registers) for warning/correction settings
            RTU.MonitorWord_F3(setting.SlaveId, 0x7015, 43);

            RTU.Start();
        }

        public void Stop()
        {
            RTU.Stop();
        }
    }
}
