using Going.Basis.Datas;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Communications.Modbus.RTU
{
    public class SlaveRTU
    {
        #region Properties
        public int Slave { get; set; } = 1;
        public string Port { get => modbus.Port; set => modbus.Port = value; }
        public int Baudrate { get => modbus.Baudrate; set => modbus.Baudrate = value; }
        public Parity Parity { get => modbus.Parity; set => modbus.Parity = value; }
        public int DataBits { get => modbus.DataBits; set => modbus.DataBits = value; }
        public StopBits StopBits { get => modbus.StopBits; set => modbus.StopBits = value; }

        public bool IsStart => modbus.IsStart;
        public bool IsOpen => modbus.IsOpen;

        public Dictionary<int, BitMemories> BitAreas { get; } = [];
        public Dictionary<int, WordMemories> WordAreas { get; } = [];
        public object? Tag { get; set; } = null;
        #endregion

        #region Member Variable
        private ModbusRTUSlave modbus;
        #endregion

        #region Event
        public event EventHandler? DeviceClosed;
        public event EventHandler? DeviceOpened;
        #endregion

        #region Constructor
        public SlaveRTU()
        {
            modbus = new ModbusRTUSlave();

            modbus.BitReadRequest += Modbus_BitReadRequest;
            modbus.WordReadRequest += Modbus_WordReadRequest;
            modbus.BitWriteRequest += Modbus_BitWriteRequest;
            modbus.WordWriteRequest += Modbus_WordWriteRequest;
            modbus.MultiBitWriteRequest += Modbus_MultiBitWriteRequest;
            modbus.MultiWordWriteRequest += Modbus_MultiWordWriteRequest;
            modbus.WordBitSetRequest += Modbus_WordBitSetRequest;

            modbus.DeviceOpened += (o, s) => DeviceOpened?.Invoke(this, System.EventArgs.Empty);
            modbus.DeviceClosed += (o, s) => DeviceClosed?.Invoke(this, System.EventArgs.Empty);
        }
        #endregion

        #region Handler
        private void Modbus_BitReadRequest(object? sender, ModbusRTUSlave.BitReadRequestArgs args)
        {
            if (args.Slave == Slave)
                foreach (var BaseAddress in BitAreas.Keys)
                {
                    var mem = BitAreas[BaseAddress];

                    if (args.StartAddress >= BaseAddress && args.StartAddress + args.Length < BaseAddress + mem.Size)
                    {
                        var ret = new bool[args.Length];
                        for (int i = 0; i < args.Length; i++)
                        {
                            var sidx = args.StartAddress - BaseAddress + i;
                            ret[i] = mem[sidx];
                        }

                        args.ResponseData = ret;
                        args.Success = true;
                    }
                }
        }

        private void Modbus_WordReadRequest(object? sender, ModbusRTUSlave.WordReadRequestArgs args)
        {
            if (args.Slave == Slave)
                foreach (var BaseAddress in WordAreas.Keys)
                {
                    var mem = WordAreas[BaseAddress];

                    if (args.StartAddress >= BaseAddress && args.StartAddress + args.Length < BaseAddress + mem.Size)
                    {
                        var ret = new int[args.Length];
                        for (int i = 0; i < args.Length; i++)
                        {
                            var sidx = args.StartAddress - BaseAddress + i;
                            ret[i] = mem[sidx].Value;
                        }
                        args.ResponseData = ret;
                        args.Success = true;
                    }
                }

        }

        private void Modbus_BitWriteRequest(object? sender, ModbusRTUSlave.BitWriteRequestArgs args)
        {
            if (args.Slave == Slave)
                foreach (var BaseAddress in BitAreas.Keys)
                {
                    var mem = BitAreas[BaseAddress];

                    if (args.StartAddress >= BaseAddress && args.StartAddress < BaseAddress + mem.Size)
                    {
                        mem[args.StartAddress - BaseAddress] = args.WriteValue;
                        args.Success = true;
                    }
                }
        }

        private void Modbus_WordWriteRequest(object? sender, ModbusRTUSlave.WordWriteRequestArgs args)
        {
            if (args.Slave == Slave)
                foreach (var BaseAddress in WordAreas.Keys)
                {
                    var mem = WordAreas[BaseAddress];

                    if (args.StartAddress >= BaseAddress && args.StartAddress < BaseAddress + mem.Size)
                    {
                        mem[args.StartAddress - BaseAddress].Value = args.WriteValue;
                        args.Success = true;
                    }
                }
        }

        private void Modbus_MultiBitWriteRequest(object? sender, ModbusRTUSlave.MultiBitWriteRequestArgs args)
        {
            if (args.Slave == Slave)
                foreach (var BaseAddress in BitAreas.Keys)
                {
                    var mem = BitAreas[BaseAddress];

                    if (args.StartAddress >= BaseAddress && args.StartAddress + args.Length < BaseAddress + mem.Size)
                    {
                        for (int i = 0; i < args.WriteValues.Length; i++) mem[args.StartAddress - BaseAddress + i] = args.WriteValues[i];
                        args.Success = true;
                    }
                }
        }

        private void Modbus_MultiWordWriteRequest(object? sender, ModbusRTUSlave.MultiWordWriteRequestArgs args)
        {
            if (args.Slave == Slave)
                foreach (var BaseAddress in WordAreas.Keys)
                {
                    var mem = WordAreas[BaseAddress];

                    if (args.StartAddress >= BaseAddress && args.StartAddress + args.Length < BaseAddress + mem.Size)
                    {
                        for (int i = 0; i < args.WriteValues.Length; i++) mem[args.StartAddress - BaseAddress + i].Value = args.WriteValues[i];
                        args.Success = true;
                    }
                }
        }

        private void Modbus_WordBitSetRequest(object? sender, ModbusRTUSlave.WordBitSetRequestArgs args)
        {
            if (args.Slave == Slave)
                foreach (var BaseAddress in WordAreas.Keys)
                {
                    var mem = WordAreas[BaseAddress];

                    if (args.StartAddress >= BaseAddress && args.StartAddress < BaseAddress + mem.Size && args.BitIndex >= 0 && args.BitIndex < 16)
                    {
                        var p = Convert.ToUInt16(Math.Pow(2, args.BitIndex));
                        if (args.WriteValue) mem[args.StartAddress - BaseAddress].Value |= p;
                        else mem[args.StartAddress - BaseAddress].Value &= (ushort)~p;

                        args.Success = true;
                    }
                }
        }
        #endregion

        #region Method
        public void Start() => modbus.Start();
        public void Stop() => modbus.Stop();
        #endregion
    }
}
