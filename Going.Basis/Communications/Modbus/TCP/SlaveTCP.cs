using Going.Basis.Communications.Modbus.RTU;
using Going.Basis.Memories;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Communications.Modbus.TCP
{
    /// <summary>
    /// Modbus TCP 슬레이브 래퍼 클래스입니다.
    /// BitMemory/WordMemory 기반 메모리 영역을 사용하여 마스터 요청에 자동 응답합니다.
    /// </summary>
    public class SlaveTCP
    {
        #region Properties
        /// <summary>슬레이브 주소 (기본값: 1)</summary>
        public int Slave { get; set; } = 1;
        /// <summary>수신 대기 포트 번호</summary>
        public int LocalPort { get => modbus.LocalPort; set => modbus.LocalPort = value; }

        /// <summary>통신 루프가 시작되었는지 여부</summary>
        public bool IsStart => modbus.IsStart;

        /// <summary>비트 메모리 영역 (키: Modbus 시작 주소, 값: BitMemory 인스턴스)</summary>
        public Dictionary<int, BitMemory> BitAreas { get; } = [];
        /// <summary>워드 메모리 영역 (키: Modbus 시작 주소, 값: WordMemory 인스턴스)</summary>
        public Dictionary<int, WordMemory> WordAreas { get; } = [];

        /// <summary>사용자 정의 태그 객체</summary>
        public object? Tag { get; set; } = null;
        #endregion

        #region Member Variable
        private ModbusTCPSlave modbus;
        #endregion

        #region Event
        /// <summary>클라이언트 TCP 소켓이 연결되었을 때 발생합니다.</summary>
        public event EventHandler<EventArgs>? SocketConnected;
        /// <summary>클라이언트 TCP 소켓 연결이 끊어졌을 때 발생합니다.</summary>
        public event EventHandler<EventArgs>? SocketDisconnected;
        #endregion

        #region Constructor
        /// <summary>Modbus TCP 슬레이브 래퍼 인스턴스를 생성한다.</summary>
        public SlaveTCP()
        {
            modbus = new ModbusTCPSlave();

            modbus.BitReadRequest += Modbus_BitReadRequest;
            modbus.WordReadRequest += Modbus_WordReadRequest;
            modbus.BitWriteRequest += Modbus_BitWriteRequest;
            modbus.WordWriteRequest += Modbus_WordWriteRequest;
            modbus.MultiBitWriteRequest += Modbus_MultiBitWriteRequest;
            modbus.MultiWordWriteRequest += Modbus_MultiWordWriteRequest;
            modbus.WordBitSetRequest += Modbus_WordBitSetRequest;

            modbus.SocketConnected += (o, s) => SocketConnected?.Invoke(this, s);
            modbus.SocketDisconnected += (o, s) => SocketDisconnected?.Invoke(this, s);
        }
        #endregion

        #region Handler
        private void Modbus_BitReadRequest(object? sender, ModbusTCPSlave.BitReadRequestArgs args)
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

        private void Modbus_WordReadRequest(object? sender, ModbusTCPSlave.WordReadRequestArgs args)
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

        private void Modbus_BitWriteRequest(object? sender, ModbusTCPSlave.BitWriteRequestArgs args)
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

        private void Modbus_WordWriteRequest(object? sender, ModbusTCPSlave.WordWriteRequestArgs args)
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

        private void Modbus_MultiBitWriteRequest(object? sender, ModbusTCPSlave.MultiBitWriteRequestArgs args)
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

        private void Modbus_MultiWordWriteRequest(object? sender, ModbusTCPSlave.MultiWordWriteRequestArgs args)
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

        private void Modbus_WordBitSetRequest(object? sender, ModbusTCPSlave.WordBitSetRequestArgs args)
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
