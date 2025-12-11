using Going.Basis.Datas;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Communications.Modbus.RTU
{
    public class MasterRTU
    {
        #region Properties
        public string Port { get => modbus.Port; set => modbus.Port = value; }
        public int Baudrate { get => modbus.Baudrate; set => modbus.Baudrate = value; }
        public Parity Parity { get => modbus.Parity; set => modbus.Parity = value; }
        public int DataBits { get => modbus.DataBits; set => modbus.DataBits = value; }
        public StopBits StopBits { get => modbus.StopBits; set => modbus.StopBits = value; }

        public int Interval { get => modbus.Interval; set => modbus.Interval = value; }
        public int Timeout { get => modbus.Timeout; set => modbus.Timeout = value; }

        public bool IsStart => modbus.IsStart;
        public bool IsOpen => modbus.IsOpen;

        public Dictionary<int, string> BitAreas { get; } = [];
        public Dictionary<int, string> WordAreas { get; } = [];

        public Dictionary<int, Mems> Devices { get; } = [];
        public object? Tag { get; set; } = null;
        #endregion

        #region Member Variable
        private ModbusRTUMaster modbus;
        #endregion

        #region Event
        public event EventHandler? DeviceClosed;
        public event EventHandler? DeviceOpened;
        #endregion

        #region Constructor
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
        public void Start() => modbus.Start();
        public void Stop() => modbus.Stop();

        public void MonitorBit_F1(int slave, int startAddr, int length) => modbus.AutoBitRead_FC1(0, slave, startAddr, length);
        public void MonitorBit_F2(int slave, int startAddr, int length) => modbus.AutoBitRead_FC2(0, slave, startAddr, length);
        public void MonitorWord_F3(int slave, int startAddr, int length) => modbus.AutoWordRead_FC3(0, slave, startAddr, length);
        public void MonitorWord_F4(int slave, int startAddr, int length) => modbus.AutoWordRead_FC4(0, slave, startAddr, length);

        public void SetWord(int slave, string addr, int value)
        {
            var v = WordAreas.FirstOrDefault(x => addr.StartsWith(x.Value));
            if (v.Value != null && int.TryParse(addr.AsSpan(v.Value.Length), out var idx))
                modbus.ManualWordWrite_FC6(1, slave, v.Key + idx, value);
        }

        public void SetBit(int slave, string addr, bool value)
        {
            var v = BitAreas.FirstOrDefault(x => addr.StartsWith(x.Value));
            if (v.Value != null && int.TryParse(addr.AsSpan(v.Value.Length), out var idx))
                modbus.ManualBitWrite_FC5(1, slave, v.Key + idx, value);
        }

        public int? GetWord(int slave, string addr)
        {
            int? ret = null;
            if (Devices.TryGetValue(slave, out var dev) && dev.Words.TryGetValue(addr, out var val)) ret = val;
            return ret;
        }

        public bool? GetBit(int slave, string addr)
        {
            bool? ret = null;
            if (Devices.TryGetValue(slave, out var dev) && dev.Bits.TryGetValue(addr, out var val)) ret = val;
            return ret;
        }
        #endregion
    }

    public class Mems
    {
        public Dictionary<string, bool> Bits { get; } = [];
        public Dictionary<string, int> Words { get; } = [];
    }
}
