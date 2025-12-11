using Going.Basis.Communications.Modbus.RTU;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Communications.Modbus.TCP
{
    public class MasterTCP
    {
        #region Properties
        public string RemoteIP { get => modbus.RemoteIP; set => modbus.RemoteIP = value; }
        public int RemotePort { get => modbus.RemotePort; set => modbus.RemotePort = value; }

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
        private ModbusTCPMaster modbus;
        #endregion

        #region Event
        public event EventHandler<EventArgs>? DeviceClosed;
        public event EventHandler<EventArgs>? DeviceOpened;
        #endregion

        #region Constructor
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
}
