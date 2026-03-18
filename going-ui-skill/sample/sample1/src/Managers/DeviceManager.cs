using Going.Basis.Communications.Modbus.RTU;
using TspMonitor.Datas;

namespace TspMonitor.Managers
{
    public class DeviceManager
    {
        public MasterRTU RTU { get; } = new MasterRTU();
        public DeviceData Data { get; private set; }

        public DeviceManager()
        {
            RTU.WordAreas.Add(0x7001, "D");
            Data = new DeviceData(RTU, Main.DataMgr.Setting.SlaveID);
        }

        public bool IsConnected => RTU.IsOpen;

        public void Start()
        {
            var s = Main.DataMgr.Setting;
            RTU.Port = s.PortName;
            RTU.Baudrate = s.Baudrate;
            RTU.Timeout = s.Timeout;

            RTU.MonitorWord_F3(s.SlaveID, 0x7001, 63);

            RTU.Start();
        }

        public void Stop() => RTU.Stop();
    }
}
