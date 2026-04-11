using Going.Basis.Memories;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Communications.Modbus.RTU
{
    /// <summary>
    /// Modbus RTU 슬레이브 래퍼 클래스입니다.
    /// BitMemory/WordMemory 기반 메모리 영역을 사용하여 마스터 요청에 자동 응답합니다.
    /// </summary>
    public class SlaveRTU
    {
        #region Properties
        /// <summary>슬레이브 주소 (기본값: 1)</summary>
        public int Slave { get; set; } = 1;
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

        /// <summary>통신 루프가 시작되었는지 여부</summary>
        public bool IsStart => modbus.IsStart;
        /// <summary>시리얼 포트가 열려 있는지 여부</summary>
        public bool IsOpen => modbus.IsOpen;

        /// <summary>비트 메모리 영역 (키: Modbus 시작 주소, 값: BitMemory 인스턴스)</summary>
        public Dictionary<int, BitMemory> BitAreas { get; } = [];
        /// <summary>워드 메모리 영역 (키: Modbus 시작 주소, 값: WordMemory 인스턴스)</summary>
        public Dictionary<int, WordMemory> WordAreas { get; } = [];
        /// <summary>사용자 정의 태그 객체</summary>
        public object? Tag { get; set; } = null;
        #endregion

        #region Member Variable
        private ModbusRTUSlave modbus;
        #endregion

        #region Event
        /// <summary>시리얼 포트가 닫혔을 때 발생합니다.</summary>
        public event EventHandler? DeviceClosed;
        /// <summary>시리얼 포트가 열렸을 때 발생합니다.</summary>
        public event EventHandler? DeviceOpened;
        #endregion

        #region Constructor
        /// <summary>Modbus RTU 슬레이브 래퍼 인스턴스를 생성한다.</summary>
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

                    if (args.StartAddress >= BaseAddress && args.StartAddress + args.Length < BaseAddress + mem.Count)
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

                    if (args.StartAddress >= BaseAddress && args.StartAddress + args.Length < BaseAddress + mem.Count)
                    {
                        var ret = new int[args.Length];
                        for (int i = 0; i < args.Length; i++)
                        {
                            var sidx = args.StartAddress - BaseAddress + i;
                            ret[i] = mem[sidx].W;
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

                    if (args.StartAddress >= BaseAddress && args.StartAddress < BaseAddress + mem.Count)
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

                    if (args.StartAddress >= BaseAddress && args.StartAddress < BaseAddress + mem.Count)
                    {
                        mem[args.StartAddress - BaseAddress].W = args.WriteValue;
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

                    if (args.StartAddress >= BaseAddress && args.StartAddress + args.Length < BaseAddress + mem.Count)
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

                    if (args.StartAddress >= BaseAddress && args.StartAddress + args.Length < BaseAddress + mem.Count)
                    {
                        for (int i = 0; i < args.WriteValues.Length; i++) mem[args.StartAddress - BaseAddress + i].W = args.WriteValues[i];
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

                    if (args.StartAddress >= BaseAddress && args.StartAddress < BaseAddress + mem.Count && args.BitIndex >= 0 && args.BitIndex < 16)
                    {
                        var p = Convert.ToUInt16(Math.Pow(2, args.BitIndex));
                        if (args.WriteValue) mem[args.StartAddress - BaseAddress].W |= p;
                        else mem[args.StartAddress - BaseAddress].W &= (ushort)~p;

                        args.Success = true;
                    }
                }
        }
        #endregion

        #region Method
        /// <summary>통신을 시작합니다.</summary>
        public void Start() => modbus.Start();
        /// <summary>통신을 중지합니다.</summary>
        public void Stop() => modbus.Stop();
        #endregion
    }
}
