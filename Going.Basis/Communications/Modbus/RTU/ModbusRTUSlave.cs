using Going.Basis.Communications.LS;
using Going.Basis.Extensions;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Going.Basis.Communications.Modbus.RTU
{
    /// <summary>
    /// 시리얼 포트(RS-232/RS-485) 기반의 Modbus RTU 슬레이브 통신 클래스입니다.
    /// 마스터의 요청을 수신하여 이벤트를 통해 처리하고 응답을 반환합니다.
    /// </summary>
    public class ModbusRTUSlave
    {
        #region class : EventArgs
        /// <summary>
        /// 비트 읽기(FC1/FC2) 요청 수신 시 전달되는 이벤트 인자입니다.
        /// </summary>
        public class BitReadRequestArgs(byte[] Data) : System.EventArgs
        {
            /// <summary>슬레이브 주소</summary>
            public int Slave => Data[0];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)Data[1];
            /// <summary>시작 주소</summary>
            public int StartAddress => Data[2] << 8 | Data[3];
            /// <summary>읽기 길이</summary>
            public int Length => Data[4] << 8 | Data[5];

            /// <summary>처리 성공 여부 (핸들러에서 설정)</summary>
            public bool Success { get; set; }
            /// <summary>응답할 비트 데이터 (핸들러에서 설정)</summary>
            public bool[]? ResponseData { get; set; }
        }

        /// <summary>
        /// 워드 읽기(FC3/FC4) 요청 수신 시 전달되는 이벤트 인자입니다.
        /// </summary>
        public class WordReadRequestArgs(byte[] Data) : System.EventArgs
        {
            /// <summary>슬레이브 주소</summary>
            public int Slave => Data[0];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)Data[1];
            /// <summary>시작 주소</summary>
            public int StartAddress => Data[2] << 8 | Data[3];
            /// <summary>읽기 길이</summary>
            public int Length => Data[4] << 8 | Data[5];

            /// <summary>처리 성공 여부 (핸들러에서 설정)</summary>
            public bool Success { get; set; }
            /// <summary>응답할 워드 데이터 (핸들러에서 설정)</summary>
            public int[]? ResponseData { get; set; }
        }

        /// <summary>
        /// 단일 비트 쓰기(FC5) 요청 수신 시 전달되는 이벤트 인자입니다.
        /// </summary>
        public class BitWriteRequestArgs(byte[] Data) : System.EventArgs
        {
            /// <summary>슬레이브 주소</summary>
            public int Slave => Data[0];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)Data[1];
            /// <summary>대상 주소</summary>
            public int StartAddress => Data[2] << 8 | Data[3];
            /// <summary>쓰기 값</summary>
            public bool WriteValue => (Data[4] << 8 | Data[5]) == 0xFF00;

            /// <summary>처리 성공 여부 (핸들러에서 설정)</summary>
            public bool Success { get; set; }
        }

        /// <summary>
        /// 단일 워드 쓰기(FC6) 요청 수신 시 전달되는 이벤트 인자입니다.
        /// </summary>
        public class WordWriteRequestArgs(byte[] Data) : System.EventArgs
        {
            /// <summary>슬레이브 주소</summary>
            public int Slave => Data[0];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)Data[1];
            /// <summary>대상 주소</summary>
            public int StartAddress => Data[2] << 8 | Data[3];
            /// <summary>쓰기 값</summary>
            public ushort WriteValue => Convert.ToUInt16(Data[4] << 8 | Data[5]);

            /// <summary>처리 성공 여부 (핸들러에서 설정)</summary>
            public bool Success { get; set; }
        }

        /// <summary>
        /// 다중 비트 쓰기(FC15) 요청 수신 시 전달되는 이벤트 인자입니다.
        /// </summary>
        public class MultiBitWriteRequestArgs : System.EventArgs
        {
            /// <summary>슬레이브 주소</summary>
            public int Slave => Data[0];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)Data[1];
            /// <summary>시작 주소</summary>
            public int StartAddress => Data[2] << 8 | Data[3];
            /// <summary>쓰기 길이</summary>
            public int Length => Data[4] << 8 | Data[5];
            /// <summary>쓰기 값 배열</summary>
            public bool[] WriteValues { get; private set; }

            /// <summary>처리 성공 여부 (핸들러에서 설정)</summary>
            public bool Success { get; set; }

            byte[] Data;

            /// <summary>
            /// <see cref="MultiBitWriteRequestArgs"/> 클래스의 새 인스턴스를 초기화합니다.
            /// </summary>
            /// <param name="Data">수신된 원본 프레임 데이터</param>
            public MultiBitWriteRequestArgs(byte[] Data)
            {
                this.Data = Data;
                #region WriteValues
                List<bool> ret = new List<bool>();
                for (int i = 7; i < Data.Length - 2; i++)
                    for (int j = 0; j < 8; j++)
                        if (ret.Count < Length) ret.Add(Data[i].GetBit(j));
                WriteValues = ret.ToArray();
                #endregion
            }
        }

        /// <summary>
        /// 다중 워드 쓰기(FC16) 요청 수신 시 전달되는 이벤트 인자입니다.
        /// </summary>
        public class MultiWordWriteRequestArgs : System.EventArgs
        {
            /// <summary>슬레이브 주소</summary>
            public int Slave => Data[0];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)Data[1];
            /// <summary>시작 주소</summary>
            public int StartAddress => Data[2] << 8 | Data[3];
            /// <summary>쓰기 길이</summary>
            public int Length => Data[4] << 8 | Data[5];
            /// <summary>쓰기 값 배열</summary>
            public ushort[] WriteValues { get; private set; }

            /// <summary>처리 성공 여부 (핸들러에서 설정)</summary>
            public bool Success { get; set; }

            byte[] Data;

            /// <summary>
            /// <see cref="MultiWordWriteRequestArgs"/> 클래스의 새 인스턴스를 초기화합니다.
            /// </summary>
            /// <param name="Data">수신된 원본 프레임 데이터</param>
            public MultiWordWriteRequestArgs(byte[] Data)
            {
                this.Data = Data;
                #region WriteValues
                List<ushort> ret = new List<ushort>();
                for (int i = 7; i < Data.Length - 2; i += 2)
                {
                    ret.Add(Convert.ToUInt16(Data[i] << 8 | Data[i + 1]));
                }
                WriteValues = ret.ToArray();
                #endregion
            }
        }

        /// <summary>
        /// 워드 비트 설정(FC26) 요청 수신 시 전달되는 이벤트 인자입니다.
        /// </summary>
        public class WordBitSetRequestArgs(byte[] Data) : System.EventArgs
        {
            /// <summary>슬레이브 주소</summary>
            public int Slave => Data[0];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)Data[1];
            /// <summary>대상 워드 주소</summary>
            public int StartAddress => Data[2] << 8 | Data[3];
            /// <summary>대상 비트 인덱스 (0~15)</summary>
            public int BitIndex => Data[4];
            /// <summary>설정할 값</summary>
            public bool WriteValue => (Data[5] << 8 | Data[6]) == 0xFF00;

            /// <summary>처리 성공 여부 (핸들러에서 설정)</summary>
            public bool Success { get; set; }
        }
        #endregion

        #region Properties
        /// <summary>시리얼 포트 이름 (예: "COM1")</summary>
        public string Port { get => ser.PortName; set => ser.PortName = value; }
        /// <summary>통신 속도 (bps)</summary>
        public int Baudrate { get => ser.BaudRate; set => ser.BaudRate = value; }
        /// <summary>패리티 비트 설정</summary>
        public Parity Parity { get => ser.Parity; set => ser.Parity = value; }
        /// <summary>데이터 비트 수</summary>
        public int DataBits { get => ser.DataBits; set => ser.DataBits = value; }
        /// <summary>정지 비트 설정</summary>
        public StopBits StopBits { get => ser.StopBits; set => ser.StopBits = value; }

        /// <summary>시리얼 포트가 열려 있는지 여부</summary>
        public bool IsOpen => ser.IsOpen;
        /// <summary>통신 루프가 시작되었는지 여부</summary>
        public bool IsStart { get; private set; }

        /// <summary>사용자 정의 태그 객체</summary>
        public object? Tag { get; set; } = null;
        #endregion

        #region Member Variable
        SerialPort ser = new SerialPort() { PortName = "COM1", BaudRate = 115200 };

        Task? task;
        CancellationTokenSource? cancel;
        #endregion

        #region Event
        /// <summary>비트 읽기(FC1/FC2) 요청 수신 시 발생합니다.</summary>
        public event EventHandler<BitReadRequestArgs>? BitReadRequest;
        /// <summary>워드 읽기(FC3/FC4) 요청 수신 시 발생합니다.</summary>
        public event EventHandler<WordReadRequestArgs>? WordReadRequest;
        /// <summary>단일 비트 쓰기(FC5) 요청 수신 시 발생합니다.</summary>
        public event EventHandler<BitWriteRequestArgs>? BitWriteRequest;
        /// <summary>단일 워드 쓰기(FC6) 요청 수신 시 발생합니다.</summary>
        public event EventHandler<WordWriteRequestArgs>? WordWriteRequest;
        /// <summary>다중 비트 쓰기(FC15) 요청 수신 시 발생합니다.</summary>
        public event EventHandler<MultiBitWriteRequestArgs>? MultiBitWriteRequest;
        /// <summary>다중 워드 쓰기(FC16) 요청 수신 시 발생합니다.</summary>
        public event EventHandler<MultiWordWriteRequestArgs>? MultiWordWriteRequest;
        /// <summary>워드 비트 설정(FC26) 요청 수신 시 발생합니다.</summary>
        public event EventHandler<WordBitSetRequestArgs>? WordBitSetRequest;

        /// <summary>시리얼 포트가 열렸을 때 발생합니다.</summary>
        public event EventHandler? DeviceOpened;
        /// <summary>시리얼 포트가 닫혔을 때 발생합니다.</summary>
        public event EventHandler? DeviceClosed;
        #endregion

        #region Constructor
        /// <summary>Modbus RTU 슬레이브 인스턴스를 생성한다.</summary>
        public ModbusRTUSlave()
        {
        }
        #endregion

        #region Method
        #region Start
        /// <summary>
        /// 시리얼 포트를 열고 Modbus 슬레이브 수신 루프를 시작합니다.
        /// </summary>
        public void Start()
        {
            if (!IsOpen && !IsStart)
            {
                cancel = new CancellationTokenSource();
                task = Task.Run(async () =>
                {
                    var token = cancel.Token;

                    do
                    {
                        try { ser.Open(); DeviceOpened?.Invoke(this, System.EventArgs.Empty); }
                        catch { }

                        if (ser.IsOpen)
                        {
                            #region var
                            List<byte> lstResponse = [];
                            var baResponse = new byte[1024 * 8];
                            DateTime prev = DateTime.Now;
                            bool ok = false;
                            #endregion

                            IsStart = true;
                            while (!token.IsCancellationRequested && IsStart)
                            {
                                try
                                {
                                    #region read
                                    try
                                    {
                                        if (ser.BytesToRead > 0)
                                        {
                                            var len = ser.Read(baResponse, 0, baResponse.Length);
                                            for (int i = 0; i < len; i++) lstResponse.Add(baResponse[i]);
                                            prev = DateTime.Now;
                                        }
                                    }
                                    catch (TimeoutException) { }
                                    catch (IOException) { throw new SchedulerStopException(); }
                                    catch (UnauthorizedAccessException) { throw new SchedulerStopException(); }
                                    catch (InvalidOperationException) { throw new SchedulerStopException(); }
                                    catch (OperationCanceledException) { throw new SchedulerStopException(); }
                                    #endregion

                                    #region parse
                                    ok = false;
                                    if (lstResponse.Count >= 4)
                                    {
                                        int Slave = lstResponse[0];
                                        ModbusFunction Function = (ModbusFunction)lstResponse[1];
                                        int StartAddress = lstResponse[2] << 8 | lstResponse[3];

                                        try
                                        {
                                            switch (Function)
                                            {
                                                case ModbusFunction.BITREAD_F1:
                                                case ModbusFunction.BITREAD_F2:
                                                    #region BitRead
                                                    if (lstResponse.Count == 8)
                                                    {
                                                        byte hi = 0xFF, lo = 0xFF;
                                                        ModbusCRC.GetCRC(lstResponse, 0, 6, ref hi, ref lo);
                                                        if (lstResponse[6] == hi && lstResponse[7] == lo)
                                                        {
                                                            if (BitReadRequest != null)
                                                            {
                                                                var args = new BitReadRequestArgs(lstResponse.ToArray());
                                                                BitReadRequest.Invoke(this, args);

                                                                if (args.Success && args.ResponseData != null && args.ResponseData.Length == args.Length)
                                                                {
                                                                    #region MakeData
                                                                    List<byte> Datas = new List<byte>();
                                                                    int nlen = args.ResponseData.Length / 8;
                                                                    nlen += args.ResponseData.Length % 8 == 0 ? 0 : 1;
                                                                    for (int i = 0; i < nlen; i++)
                                                                    {
                                                                        byte val = 0;
                                                                        for (int j = i * 8, nTemp = 0; j < args.ResponseData.Length && j < i * 8 + 8; j++, nTemp++)
                                                                            if (args.ResponseData[j])
                                                                                val |= Convert.ToByte(Math.Pow(2, nTemp));
                                                                        Datas.Add(val);
                                                                    }
                                                                    #endregion
                                                                    #region Serial Write
                                                                    List<byte> ret = new List<byte>();
                                                                    ret.Add((byte)Slave);
                                                                    ret.Add((byte)Function);
                                                                    ret.Add((byte)Datas.Count);
                                                                    ret.AddRange(Datas.ToArray());
                                                                    byte nhi = 0xFF, nlo = 0xFF;
                                                                    ModbusCRC.GetCRC(ret, 0, ret.Count, ref nhi, ref nlo);
                                                                    ret.Add(nhi);
                                                                    ret.Add(nlo);
                                                                    byte[] send = ret.ToArray();
                                                                    ser.Write(send, 0, send.Length);
                                                                    ser.BaseStream.Flush();
                                                                    #endregion
                                                                    ok = true;
                                                                }
                                                            }
                                                            lstResponse.Clear();
                                                        }
                                                    }
                                                    #endregion
                                                    break;
                                                case ModbusFunction.WORDREAD_F3:
                                                case ModbusFunction.WORDREAD_F4:
                                                    #region WordRead
                                                    if (lstResponse.Count == 8)
                                                    {
                                                        byte hi = 0xFF, lo = 0xFF;
                                                        ModbusCRC.GetCRC(lstResponse, 0, 6, ref hi, ref lo);
                                                        if (lstResponse[6] == hi && lstResponse[7] == lo)
                                                        {
                                                            if (WordReadRequest != null)
                                                            {
                                                                var args = new WordReadRequestArgs(lstResponse.ToArray());
                                                                WordReadRequest.Invoke(this, args);

                                                                if (args.Success && args.ResponseData != null && args.ResponseData.Length == args.Length)
                                                                {
                                                                    #region MakeData
                                                                    List<byte> Datas = new List<byte>();
                                                                    for (int i = 0; i < args.ResponseData.Length; i++)
                                                                    {
                                                                        Datas.Add((byte)((args.ResponseData[i] & 0xFF00) >> 8));
                                                                        Datas.Add((byte)(args.ResponseData[i] & 0x00FF));
                                                                    }
                                                                    #endregion
                                                                    #region Serial Write
                                                                    List<byte> ret = new List<byte>();
                                                                    ret.Add((byte)Slave);
                                                                    ret.Add((byte)Function);
                                                                    ret.Add((byte)Datas.Count);
                                                                    ret.AddRange(Datas.ToArray());
                                                                    byte nhi = 0xFF, nlo = 0xFF;
                                                                    ModbusCRC.GetCRC(ret, 0, ret.Count, ref nhi, ref nlo);
                                                                    ret.Add(nhi);
                                                                    ret.Add(nlo);
                                                                    byte[] send = ret.ToArray();
                                                                    ser.Write(send, 0, send.Length);
                                                                    ser.BaseStream.Flush();
                                                                    #endregion
                                                                }
                                                            }
                                                            ok = true;
                                                        }
                                                    }
                                                    #endregion
                                                    break;
                                                case ModbusFunction.BITWRITE_F5:
                                                    #region BitWrite
                                                    if (lstResponse.Count == 8)
                                                    {
                                                        byte hi = 0xFF, lo = 0xFF;
                                                        ModbusCRC.GetCRC(lstResponse, 0, 6, ref hi, ref lo);
                                                        if (lstResponse[6] == hi && lstResponse[7] == lo)
                                                        {
                                                            if (BitWriteRequest != null)
                                                            {
                                                                var args = new BitWriteRequestArgs(lstResponse.ToArray());
                                                                BitWriteRequest.Invoke(this, args);

                                                                if (args.Success)
                                                                {
                                                                    #region Serial Write
                                                                    int nv = args.WriteValue ? 0xFF00 : 0;
                                                                    List<byte> ret = new List<byte>();
                                                                    ret.Add((byte)args.Slave);
                                                                    ret.Add((byte)args.Function);
                                                                    ret.Add((byte)((args.StartAddress & 0xFF00) >> 8));
                                                                    ret.Add((byte)(args.StartAddress & 0x00FF));
                                                                    ret.Add((byte)((nv & 0xFF00) >> 8));
                                                                    ret.Add((byte)(nv & 0x00FF));
                                                                    byte nhi = 0xFF, nlo = 0xFF;
                                                                    ModbusCRC.GetCRC(ret, 0, ret.Count, ref nhi, ref nlo);
                                                                    ret.Add(nhi);
                                                                    ret.Add(nlo);
                                                                    byte[] send = ret.ToArray();
                                                                    ser.Write(send, 0, send.Length);
                                                                    ser.BaseStream.Flush();
                                                                    #endregion
                                                                }
                                                            }
                                                            ok = true;
                                                        }
                                                    }
                                                    #endregion
                                                    break;
                                                case ModbusFunction.WORDWRITE_F6:
                                                    #region WordWrite
                                                    if (lstResponse.Count == 8)
                                                    {
                                                        byte hi = 0xFF, lo = 0xFF;
                                                        ModbusCRC.GetCRC(lstResponse, 0, 6, ref hi, ref lo);
                                                        if (lstResponse[6] == hi && lstResponse[7] == lo)
                                                        {
                                                            if (WordWriteRequest != null)
                                                            {
                                                                var args = new WordWriteRequestArgs(lstResponse.ToArray());
                                                                WordWriteRequest.Invoke(this, args);

                                                                if (args.Success)
                                                                {
                                                                    #region Serial Write
                                                                    List<byte> ret = new List<byte>();
                                                                    ret.Add((byte)args.Slave);
                                                                    ret.Add((byte)args.Function);
                                                                    ret.Add((byte)((args.StartAddress & 0xFF00) >> 8));
                                                                    ret.Add((byte)(args.StartAddress & 0x00FF));
                                                                    ret.Add((byte)((args.WriteValue & 0xFF00) >> 8));
                                                                    ret.Add((byte)(args.WriteValue & 0x00FF));
                                                                    byte nhi = 0xFF, nlo = 0xFF;
                                                                    ModbusCRC.GetCRC(ret, 0, ret.Count, ref nhi, ref nlo);
                                                                    ret.Add(nhi);
                                                                    ret.Add(nlo);
                                                                    byte[] send = ret.ToArray();
                                                                    ser.Write(send, 0, send.Length);
                                                                    ser.BaseStream.Flush();
                                                                    #endregion
                                                                }
                                                            }
                                                            ok = true;
                                                        }
                                                    }
                                                    #endregion
                                                    break;
                                                case ModbusFunction.MULTIBITWRITE_F15:
                                                    #region MultiBitWrite
                                                    if (lstResponse.Count >= 7)
                                                    {
                                                        int Length = lstResponse[4] << 8 | lstResponse[5];
                                                        int ByteCount = lstResponse[6];
                                                        if (lstResponse.Count >= 9 + ByteCount)
                                                        {
                                                            byte hi = 0xFF, lo = 0xFF;
                                                            ModbusCRC.GetCRC(lstResponse, 0, 7 + ByteCount, ref hi, ref lo);
                                                            if (lstResponse[9 + ByteCount - 2] == hi && lstResponse[9 + ByteCount - 1] == lo)
                                                            {
                                                                var args = new MultiBitWriteRequestArgs(lstResponse.ToArray());
                                                                if (MultiBitWriteRequest != null)
                                                                {
                                                                    MultiBitWriteRequest.Invoke(this, args);

                                                                    if (args.Success)
                                                                    {
                                                                        #region Serial Write
                                                                        List<byte> ret = new List<byte>();
                                                                        ret.Add((byte)args.Slave);
                                                                        ret.Add((byte)args.Function);
                                                                        ret.Add((byte)((args.StartAddress & 0xFF00) >> 8));
                                                                        ret.Add((byte)(args.StartAddress & 0x00FF));
                                                                        ret.Add((byte)((args.Length & 0xFF00) >> 8));
                                                                        ret.Add((byte)(args.Length & 0x00FF));
                                                                        byte nhi = 0xFF, nlo = 0xFF;
                                                                        ModbusCRC.GetCRC(ret, 0, ret.Count, ref nhi, ref nlo);
                                                                        ret.Add(nhi);
                                                                        ret.Add(nlo);
                                                                        byte[] send = ret.ToArray();
                                                                        ser.Write(send, 0, send.Length);
                                                                        ser.BaseStream.Flush();
                                                                        #endregion
                                                                    }
                                                                }
                                                                ok = true;
                                                            }
                                                        }
                                                    }
                                                    #endregion
                                                    break;
                                                case ModbusFunction.MULTIWORDWRITE_F16:
                                                    #region MultiWordWrite
                                                    if (lstResponse.Count >= 7)
                                                    {
                                                        int Length = lstResponse[4] << 8 | lstResponse[5];
                                                        int ByteCount = lstResponse[6];
                                                        if (lstResponse.Count >= 9 + ByteCount)
                                                        {
                                                            byte hi = 0xFF, lo = 0xFF;
                                                            ModbusCRC.GetCRC(lstResponse, 0, 7 + ByteCount, ref hi, ref lo);
                                                            if (lstResponse[9 + ByteCount - 2] == hi && lstResponse[9 + ByteCount - 1] == lo)
                                                            {
                                                                if (MultiWordWriteRequest != null)
                                                                {
                                                                    var args = new MultiWordWriteRequestArgs(lstResponse.ToArray());
                                                                    MultiWordWriteRequest.Invoke(this, args);

                                                                    if (args.Success)
                                                                    {
                                                                        #region Serial Write
                                                                        List<byte> ret = new List<byte>();
                                                                        ret.Add((byte)args.Slave);
                                                                        ret.Add((byte)args.Function);
                                                                        ret.Add((byte)((args.StartAddress & 0xFF00) >> 8));
                                                                        ret.Add((byte)(args.StartAddress & 0x00FF));
                                                                        ret.Add((byte)((args.Length & 0xFF00) >> 8));
                                                                        ret.Add((byte)(args.Length & 0x00FF));
                                                                        byte nhi = 0xFF, nlo = 0xFF;
                                                                        ModbusCRC.GetCRC(ret, 0, ret.Count, ref nhi, ref nlo);
                                                                        ret.Add(nhi);
                                                                        ret.Add(nlo);
                                                                        byte[] send = ret.ToArray();
                                                                        ser.Write(send, 0, send.Length);
                                                                        ser.BaseStream.Flush();
                                                                        #endregion
                                                                    }
                                                                }
                                                                ok = true;
                                                            }
                                                        }
                                                    }
                                                    #endregion
                                                    break;
                                                case ModbusFunction.WORDBITSET_F26:
                                                    #region WordBitSet
                                                    if (lstResponse.Count == 9)
                                                    {
                                                        byte hi = 0xFF, lo = 0xFF;
                                                        ModbusCRC.GetCRC(lstResponse, 0, 7, ref hi, ref lo);
                                                        if (lstResponse[7] == hi && lstResponse[8] == lo)
                                                        {
                                                            if (WordBitSetRequest != null)
                                                            {
                                                                var args = new WordBitSetRequestArgs(lstResponse.ToArray());
                                                                WordBitSetRequest.Invoke(this, args);

                                                                if (args.Success)
                                                                {
                                                                    #region Serial Write
                                                                    int nv = args.WriteValue ? 0xFF00 : 0;
                                                                    List<byte> ret = new List<byte>();
                                                                    ret.Add((byte)args.Slave);
                                                                    ret.Add((byte)args.Function);
                                                                    ret.Add((byte)((args.StartAddress & 0xFF00) >> 8));
                                                                    ret.Add((byte)(args.StartAddress & 0x00FF));
                                                                    ret.Add((byte)((nv & 0xFF00) >> 8));
                                                                    ret.Add((byte)(nv & 0x00FF));
                                                                    byte nhi = 0xFF, nlo = 0xFF;
                                                                    ModbusCRC.GetCRC(ret, 0, ret.Count, ref nhi, ref nlo);
                                                                    ret.Add(nhi);
                                                                    ret.Add(nlo);
                                                                    byte[] send = ret.ToArray();
                                                                    ser.Write(send, 0, send.Length);
                                                                    ser.BaseStream.Flush();
                                                                    #endregion
                                                                }
                                                            }
                                                            ok = true;
                                                        }
                                                    }
                                                    #endregion
                                                    break;
                                            }
                                        }
                                        catch (TimeoutException) { }
                                        catch (IOException) { throw new SchedulerStopException(); }
                                        catch (UnauthorizedAccessException) { throw new SchedulerStopException(); }
                                        catch (InvalidOperationException) { throw new SchedulerStopException(); }
                                        catch (OperationCanceledException) { throw new SchedulerStopException(); }
                                    }
                                    #endregion

                                    #region buffer clear
                                    if (ok || ((DateTime.Now - prev).TotalMilliseconds >= 50 && lstResponse.Count > 0))
                                    {
                                        lstResponse.Clear();
                                        try
                                        {
                                            ser.DiscardInBuffer();
                                            ser.BaseStream.Flush();
                                        }
                                        catch (TimeoutException) { }
                                        catch (IOException) { throw new SchedulerStopException(); }
                                        catch (UnauthorizedAccessException) { throw new SchedulerStopException(); }
                                        catch (InvalidOperationException) { throw new SchedulerStopException(); }
                                        catch (OperationCanceledException) { throw new SchedulerStopException(); }
                                    }
                                    #endregion

                                    await Task.Delay(10, token);
                                }
                                catch (SchedulerStopException) { break; }
                                catch (Exception) { }
                            }
                        }

                        if (ser.IsOpen)
                        {
                            ser.Close();
                            DeviceClosed?.Invoke(this, System.EventArgs.Empty);
                        }


                    } while (!token.IsCancellationRequested && IsStart);

                }, cancel.Token);
            }
        }
        #endregion
        #region Stop
        /// <summary>
        /// 수신 루프를 중지하고 시리얼 포트를 닫습니다.
        /// </summary>
        public void Stop()
        {
            IsStart = false;
            cancel?.Cancel(false);

            if (task != null)
            {
                try { if (task.Wait(3000)) task.Dispose(); }
                catch { }
                finally { task = null; }
            }

            cancel?.Dispose();
            cancel = null;
        }
        #endregion
        #endregion

        #region Static Method
        #region ProcessBitReads
        /// <summary>
        /// 비트 읽기 요청을 기본 배열에서 처리합니다.
        /// </summary>
        /// <param name="args">비트 읽기 요청 인자</param>
        /// <param name="BaseAddress">배열의 기준 주소</param>
        /// <param name="BaseArray">비트 데이터 원본 배열</param>
        public static void ProcessBitReads(BitReadRequestArgs args, int BaseAddress, bool[] BaseArray)
        {
            var BA = new bool[Convert.ToInt32(Math.Ceiling(BaseArray.Length / 8.0) * 8.0)];
            Array.Copy(BaseArray, BA, BaseArray.Length);
            if (args.StartAddress >= BaseAddress && args.StartAddress + args.Length < BaseAddress + BA.Length)
            {
                var ret = new bool[args.Length];
                Array.Copy(BA, args.StartAddress - BaseAddress, ret, 0, args.Length);
                args.ResponseData = ret;
                args.Success = true;
            }
        }
        #endregion
        #region ProcessWordReads
        /// <summary>
        /// 워드 읽기 요청을 기본 배열에서 처리합니다.
        /// </summary>
        /// <param name="args">워드 읽기 요청 인자</param>
        /// <param name="BaseAddress">배열의 기준 주소</param>
        /// <param name="BaseArray">워드 데이터 원본 배열</param>
        public static void ProcessWordReads(WordReadRequestArgs args, int BaseAddress, int[] BaseArray)
        {
            if (args.StartAddress >= BaseAddress && args.StartAddress + args.Length < BaseAddress + BaseArray.Length)
            {
                var ret = new int[args.Length];
                Array.Copy(BaseArray, args.StartAddress - BaseAddress, ret, 0, args.Length);
                args.ResponseData = ret;
                args.Success = true;
            }
        }
        #endregion
        #region ProcessBitWrite
        /// <summary>
        /// 단일 비트 쓰기 요청을 기본 배열에 처리합니다.
        /// </summary>
        /// <param name="args">비트 쓰기 요청 인자</param>
        /// <param name="BaseAddress">배열의 기준 주소</param>
        /// <param name="BaseArray">비트 데이터 대상 배열</param>
        public static void ProcessBitWrite(BitWriteRequestArgs args, int BaseAddress, bool[] BaseArray)
        {
            if (args.StartAddress >= BaseAddress && args.StartAddress < BaseAddress + BaseArray.Length)
            {
                BaseArray[args.StartAddress - BaseAddress] = args.WriteValue;
                args.Success = true;
            }
        }
        #endregion
        #region ProcessWordWrite
        /// <summary>
        /// 단일 워드 쓰기 요청을 기본 배열에 처리합니다.
        /// </summary>
        /// <param name="args">워드 쓰기 요청 인자</param>
        /// <param name="BaseAddress">배열의 기준 주소</param>
        /// <param name="BaseArray">워드 데이터 대상 배열</param>
        public static void ProcessWordWrite(WordWriteRequestArgs args, int BaseAddress, int[] BaseArray)
        {
            if (args.StartAddress >= BaseAddress && args.StartAddress < BaseAddress + BaseArray.Length)
            {
                BaseArray[args.StartAddress - BaseAddress] = args.WriteValue;
                args.Success = true;
            }
        }
        #endregion
        #region ProcessMultiBitWrite
        /// <summary>
        /// 다중 비트 쓰기 요청을 기본 배열에 처리합니다.
        /// </summary>
        /// <param name="args">다중 비트 쓰기 요청 인자</param>
        /// <param name="BaseAddress">배열의 기준 주소</param>
        /// <param name="BaseArray">비트 데이터 대상 배열</param>
        public static void ProcessMultiBitWrite(MultiBitWriteRequestArgs args, int BaseAddress, bool[] BaseArray)
        {
            if (args.StartAddress >= BaseAddress && args.StartAddress + args.Length < BaseAddress + BaseArray.Length)
            {
                for (int i = 0; i < args.WriteValues.Length; i++) BaseArray[args.StartAddress - BaseAddress + i] = args.WriteValues[i];
                args.Success = true;
            }
        }
        #endregion
        #region ProcessMultiWordWrite
        /// <summary>
        /// 다중 워드 쓰기 요청을 기본 배열에 처리합니다.
        /// </summary>
        /// <param name="args">다중 워드 쓰기 요청 인자</param>
        /// <param name="BaseAddress">배열의 기준 주소</param>
        /// <param name="BaseArray">워드 데이터 대상 배열</param>
        public static void ProcessMultiWordWrite(MultiWordWriteRequestArgs args, int BaseAddress, int[] BaseArray)
        {
            if (args.StartAddress >= BaseAddress && args.StartAddress + args.Length < BaseAddress + BaseArray.Length)
            {
                for (int i = 0; i < args.WriteValues.Length; i++) BaseArray[args.StartAddress - BaseAddress + i] = args.WriteValues[i];
                args.Success = true;
            }
        }
        #endregion
        #region ProcessWordBitSet
        /// <summary>
        /// 워드 비트 설정 요청을 기본 배열에 처리합니다.
        /// </summary>
        /// <param name="args">워드 비트 설정 요청 인자</param>
        /// <param name="BaseAddress">배열의 기준 주소</param>
        /// <param name="BaseArray">워드 데이터 대상 배열</param>
        public static void ProcessWordBitSet(WordBitSetRequestArgs args, int BaseAddress, int[] BaseArray)
        {
            if (args.StartAddress >= BaseAddress && args.StartAddress < BaseAddress + BaseArray.Length && args.BitIndex >= 0 && args.BitIndex < 16)
            {
                var p = Convert.ToInt32(Math.Pow(2, args.BitIndex));
                if (args.WriteValue) BaseArray[args.StartAddress - BaseAddress] |= p;
                else BaseArray[args.StartAddress - BaseAddress] &= (ushort)~p;
                args.Success = true;
            }
        }
        #endregion
        #endregion
    }
}
