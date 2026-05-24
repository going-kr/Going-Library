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
    /// Function code별 0-base BitMemory/WordMemory 영역을 사용하여 마스터 요청에 자동 응답합니다.
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

        /// <summary>FC1 코일 영역. Modbus 주소를 0-base 인덱스로 사용합니다.</summary>
        public BitMemory Coils { get; set; } = new(65536);
        /// <summary>FC2 이산 입력 영역. Modbus 주소를 0-base 인덱스로 사용합니다.</summary>
        public BitMemory Contacts { get; set; } = new(65536);
        /// <summary>FC3 보유 레지스터 영역. Modbus 주소를 0-base 인덱스로 사용합니다.</summary>
        public WordMemory HoldingRegister { get; set; } = new(65536);
        /// <summary>FC4 입력 레지스터 영역. Modbus 주소를 0-base 인덱스로 사용합니다.</summary>
        public WordMemory InputRegister { get; set; } = new(65536);

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
            {
                var mem = args.Function == ModbusFunction.BITREAD_F1 ? Coils :
                          args.Function == ModbusFunction.BITREAD_F2 ? Contacts :
                          null;

                if (mem != null && args.StartAddress >= 0 && args.StartAddress + args.Length <= mem.Count)
                {
                    var ret = new bool[args.Length];
                    for (int i = 0; i < args.Length; i++)
                    {
                        var sidx = args.StartAddress + i;
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
            {
                var mem = args.Function == ModbusFunction.WORDREAD_F3 ? HoldingRegister :
                          args.Function == ModbusFunction.WORDREAD_F4 ? InputRegister :
                          null;

                if (mem != null && args.StartAddress >= 0 && args.StartAddress + args.Length <= mem.Count)
                {
                    var ret = new int[args.Length];
                    for (int i = 0; i < args.Length; i++)
                    {
                        var sidx = args.StartAddress + i;
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
            {
                if (args.StartAddress >= 0 && args.StartAddress < Coils.Count)
                {
                    Coils[args.StartAddress] = args.WriteValue;
                    args.Success = true;
                }
            }
        }

        private void Modbus_WordWriteRequest(object? sender, ModbusTCPSlave.WordWriteRequestArgs args)
        {
            if (args.Slave == Slave)
            {
                if (args.StartAddress >= 0 && args.StartAddress < HoldingRegister.Count)
                {
                    HoldingRegister[args.StartAddress].W = args.WriteValue;
                    args.Success = true;
                }
            }
        }

        private void Modbus_MultiBitWriteRequest(object? sender, ModbusTCPSlave.MultiBitWriteRequestArgs args)
        {
            if (args.Slave == Slave)
            {
                if (args.StartAddress >= 0 && args.StartAddress + args.Length <= Coils.Count)
                {
                    for (int i = 0; i < args.WriteValues.Length; i++) Coils[args.StartAddress + i] = args.WriteValues[i];
                    args.Success = true;
                }
            }
        }

        private void Modbus_MultiWordWriteRequest(object? sender, ModbusTCPSlave.MultiWordWriteRequestArgs args)
        {
            if (args.Slave == Slave)
            {
                if (args.StartAddress >= 0 && args.StartAddress + args.Length <= HoldingRegister.Count)
                {
                    for (int i = 0; i < args.WriteValues.Length; i++) HoldingRegister[args.StartAddress + i].W = args.WriteValues[i];
                    args.Success = true;
                }
            }
        }

        private void Modbus_WordBitSetRequest(object? sender, ModbusTCPSlave.WordBitSetRequestArgs args)
        {
            if (args.Slave == Slave)
            {
                if (args.StartAddress >= 0 && args.StartAddress < HoldingRegister.Count && args.BitIndex >= 0 && args.BitIndex < 16)
                {
                    var p = Convert.ToUInt16(Math.Pow(2, args.BitIndex));
                    if (args.WriteValue) HoldingRegister[args.StartAddress].W |= p;
                    else HoldingRegister[args.StartAddress].W &= (ushort)~p;

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
