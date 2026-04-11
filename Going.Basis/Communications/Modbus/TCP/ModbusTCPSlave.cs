using Going.Basis.Communications.LS;
using Going.Basis.Extensions;
using Going.Basis.Tools;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Communications.Modbus.TCP
{
    /// <summary>
    /// TCP 소켓 기반의 Modbus TCP 슬레이브 통신 클래스입니다.
    /// 마스터의 TCP 접속을 수락하여 요청을 이벤트를 통해 처리하고 응답을 반환합니다.
    /// </summary>
    public class ModbusTCPSlave
    {
        #region class : EventArgs
        /// <summary>
        /// 비트 읽기(FC1/FC2) 요청 수신 시 전달되는 이벤트 인자입니다.
        /// </summary>
        public class BitReadRequestArgs(byte[] Data) : System.EventArgs
        {
            /// <summary>슬레이브 주소</summary>
            public int Slave => Data[6];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)Data[7];
            /// <summary>시작 주소</summary>
            public int StartAddress => (Data[8] << 8) | Data[9];
            /// <summary>읽기 길이</summary>
            public int Length => (Data[10] << 8) | Data[11];

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
            public int Slave => Data[6];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)Data[7];
            /// <summary>시작 주소</summary>
            public int StartAddress => (Data[8] << 8) | Data[9];
            /// <summary>읽기 길이</summary>
            public int Length => (Data[10] << 8) | Data[11];

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
            public int Slave => Data[6];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)Data[7];
            /// <summary>대상 주소</summary>
            public int StartAddress => (Data[8] << 8) | Data[9];
            /// <summary>쓰기 값</summary>
            public bool WriteValue => ((Data[10] << 8) | Data[11]) == 0xFF00;

            /// <summary>처리 성공 여부 (핸들러에서 설정)</summary>
            public bool Success { get; set; }
        }
        /// <summary>
        /// 단일 워드 쓰기(FC6) 요청 수신 시 전달되는 이벤트 인자입니다.
        /// </summary>
        public class WordWriteRequestArgs(byte[] Data) : System.EventArgs
        {
            /// <summary>슬레이브 주소</summary>
            public int Slave => Data[6];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)Data[7];
            /// <summary>대상 주소</summary>
            public int StartAddress => (Data[8] << 8) | Data[9];
            /// <summary>쓰기 값</summary>
            public ushort WriteValue => Convert.ToUInt16((Data[10] << 8) | Data[11]);

            /// <summary>처리 성공 여부 (핸들러에서 설정)</summary>
            public bool Success { get; set; }
        }
        /// <summary>
        /// 다중 비트 쓰기(FC15) 요청 수신 시 전달되는 이벤트 인자입니다.
        /// </summary>
        public class MultiBitWriteRequestArgs : System.EventArgs
        {
            /// <summary>슬레이브 주소</summary>
            public int Slave => Data[6];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)Data[7];
            /// <summary>시작 주소</summary>
            public int StartAddress => (Data[8] << 8) | Data[9];
            /// <summary>쓰기 길이</summary>
            public int Length => (Data[10] << 8) | Data[11];
            /// <summary>데이터 바이트 수</summary>
            public int ByteCount => Data[12];
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
                for (int i = 13; i < Data.Length; i++)
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
            public int Slave => Data[6];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)Data[7];
            /// <summary>시작 주소</summary>
            public int StartAddress => (Data[8] << 8) | Data[9];
            /// <summary>쓰기 길이</summary>
            public int Length => (Data[10] << 8) | Data[11];
            /// <summary>데이터 바이트 수</summary>
            public int ByteCount => Data[12];
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
                for (int i = 13; i < Data.Length; i += 2)
                {
                    ret.Add(Convert.ToUInt16((Data[i] << 8) | Data[i + 1]));
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
            public int Slave => Data[6];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)Data[7];
            /// <summary>대상 워드 주소</summary>
            public int StartAddress => (Data[8] << 8) | Data[9];
            /// <summary>대상 비트 인덱스 (0~15)</summary>
            public int BitIndex => Data[10];
            /// <summary>설정할 값</summary>
            public bool WriteValue => ((Data[11] << 8) | Data[12]) == 0xFF00;

            /// <summary>처리 성공 여부 (핸들러에서 설정)</summary>
            public bool Success { get; set; }
        }
        #endregion

        #region Properties
        /// <summary>수신 대기 포트 번호 (기본값: 502)</summary>
        public int LocalPort { get; set; } = 502;

        /// <summary>통신 루프가 시작되었는지 여부</summary>
        public bool IsStart { get; private set; }

        /// <summary>사용자 정의 태그 객체</summary>
        public object? Tag { get; set; } = null;
        #endregion

        #region Member Variable
        Socket? server;

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

        /// <summary>클라이언트 TCP 소켓이 연결되었을 때 발생합니다.</summary>
        public event EventHandler<EventArgs>? SocketConnected;
        /// <summary>클라이언트 TCP 소켓 연결이 끊어졌을 때 발생합니다.</summary>
        public event EventHandler<EventArgs>? SocketDisconnected;
        #endregion

        #region Constructor
        /// <summary>Modbus TCP 슬레이브 인스턴스를 생성한다.</summary>
        public ModbusTCPSlave()
        {
        }
        #endregion

        #region Method
        #region Start
        /// <summary>
        /// TCP 서버 소켓을 열고 Modbus 슬레이브 수신 루프를 시작합니다.
        /// </summary>
        public void Start()
        {
            if (!IsStart)
            {
                cancel = new CancellationTokenSource();
                task = Task.Run(async () =>
                {
                    var token = cancel.Token;

                    try
                    {
                        #region server listen
                        server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, LocalPort);
                        server.Bind(ipEndPoint);
                        server.Listen(10);
                        #endregion

                        IsStart = true;
                        while (!token.IsCancellationRequested && IsStart)
                        {
                            try
                            {
                                var sock = await server.AcceptAsync(token);
                                _ = Task.Run(async () => await run(sock, token), token);
                                await Task.Delay(100, token);
                            }
                            catch { }
                        }

                        server.Close();
                    }
                    catch { }

                    IsStart = false;

                }, cancel.Token);
            }
        }
        #endregion
        #region Stop
        /// <summary>
        /// 수신 루프를 중지하고 TCP 서버 소켓을 닫습니다.
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

        #region Run
        async Task run(Socket sock, CancellationToken cancel)
        {
            SocketConnected?.Invoke(this, new SocketEventArgs(sock));

            #region var 
            var lstResponse = new List<byte>();
            var baResponse = new byte[1024];
            var prev = DateTime.Now;
            var isConnected = sock.Connected;
            #endregion

            while (!cancel.IsCancellationRequested && IsStart && isConnected)
            {
                try
                {
                    #region DataRead
                    if (sock.Available > 0)
                    {
                        try
                        {
                            int n = sock.Receive(baResponse);
                            for (int i = 0; i < n; i++) lstResponse.Add(baResponse[i]);
                            prev = DateTime.Now;

                            if (n == 0) isConnected = false;
                        }
                        catch (TimeoutException) { }
                    }
                    #endregion

                    #region Modbus Parse
                    if (lstResponse.Count >= 10)
                    {
                        int Slave = lstResponse[6];
                        ModbusFunction Function = (ModbusFunction)lstResponse[7];
                        int StartAddress = (lstResponse[8] << 8) | lstResponse[9];

                        switch (Function)
                        {
                            case ModbusFunction.BITREAD_F1:
                            case ModbusFunction.BITREAD_F2:
                                #region BitRead
                                if (lstResponse.Count == 12)
                                {
                                    int Length = (lstResponse[10] << 8) | lstResponse[11];

                                    if (BitReadRequest != null)
                                    {
                                        var args = new BitReadRequestArgs(lstResponse.ToArray());
                                        BitReadRequest?.Invoke(this, args);

                                        if (args.Success && args.ResponseData != null && args.ResponseData.Length == args.Length)
                                        {
                                            #region MakeData
                                            List<byte> Datas = new List<byte>();
                                            int nlen = args.ResponseData.Length / 8;
                                            nlen += (args.ResponseData.Length % 8 == 0) ? 0 : 1;
                                            for (int i = 0; i < nlen; i++)
                                            {
                                                byte val = 0;
                                                for (int j = (i * 8), nTemp = 0; j < args.ResponseData.Length && j < (i * 8) + 8; j++, nTemp++)
                                                    if (args.ResponseData[j])
                                                        val |= Convert.ToByte(Math.Pow(2, nTemp));
                                                Datas.Add(val);
                                            }
                                            #endregion
                                            #region Write
                                            List<byte> ret = new List<byte>();
                                            ret.Add(lstResponse[0]);
                                            ret.Add(lstResponse[1]);
                                            ret.Add(0);
                                            ret.Add(0);
                                            ret.Add((byte)(((nlen + 3) & 0xFF00) >> 8));
                                            ret.Add((byte)(((nlen + 3) & 0x00FF)));
                                            ret.Add((byte)Slave);
                                            ret.Add((byte)Function);
                                            ret.Add((byte)Datas.Count);
                                            ret.AddRange(Datas.ToArray());

                                            byte[] send = ret.ToArray();
                                            sock.Send(send);
                                            #endregion
                                        }
                                    }
                                    lstResponse.Clear();
                                }
                                #endregion
                                break;
                            case ModbusFunction.WORDREAD_F3:
                            case ModbusFunction.WORDREAD_F4:
                                #region WordRead
                                if (lstResponse.Count == 12)
                                {
                                    int Length = (lstResponse[10] << 8) | lstResponse[11];

                                    if (WordReadRequest != null)
                                    {
                                        var args = new WordReadRequestArgs(lstResponse.ToArray());
                                        WordReadRequest?.Invoke(this, args);

                                        if (args.Success && args.ResponseData != null && args.ResponseData.Length == args.Length)
                                        {
                                            #region MakeData
                                            List<byte> Datas = new List<byte>();
                                            for (int i = 0; i < args.ResponseData.Length; i++)
                                            {
                                                Datas.Add((byte)((args.ResponseData[i] & 0xFF00) >> 8));
                                                Datas.Add((byte)((args.ResponseData[i] & 0x00FF)));
                                            }
                                            #endregion
                                            #region Write
                                            int nlen = Length * 2;
                                            List<byte> ret = new List<byte>();
                                            ret.Add(lstResponse[0]);
                                            ret.Add(lstResponse[1]);
                                            ret.Add(0);
                                            ret.Add(0);
                                            ret.Add((byte)(((nlen + 3) & 0xFF00) >> 8));
                                            ret.Add((byte)(((nlen + 3) & 0x00FF)));
                                            ret.Add((byte)Slave);
                                            ret.Add((byte)Function);
                                            ret.Add((byte)Datas.Count);
                                            ret.AddRange(Datas.ToArray());

                                            byte[] send = ret.ToArray();
                                            sock.Send(send);
                                            #endregion
                                        }
                                    }
                                    lstResponse.Clear();
                                }
                                #endregion
                                break;
                            case ModbusFunction.BITWRITE_F5:
                                #region BitWrite
                                if (lstResponse.Count == 12)
                                {
                                    int WriteValue = (lstResponse[10] << 8) | lstResponse[11];
                                    if (BitWriteRequest != null)
                                    {
                                        var args = new BitWriteRequestArgs(lstResponse.ToArray());
                                        BitWriteRequest?.Invoke(this, args);

                                        if (args.Success)
                                        {
                                            #region Write
                                            int nv = args.WriteValue ? 0xFF00 : 0;
                                            List<byte> ret = new List<byte>();
                                            ret.Add(lstResponse[0]);
                                            ret.Add(lstResponse[1]);
                                            ret.Add(0);
                                            ret.Add(0);
                                            ret.Add(0);
                                            ret.Add(6);
                                            ret.Add((byte)Slave);
                                            ret.Add((byte)Function);
                                            ret.Add((byte)((StartAddress & 0xFF00) >> 8));
                                            ret.Add((byte)((StartAddress & 0x00FF)));
                                            ret.Add((byte)((nv & 0xFF00) >> 8));
                                            ret.Add((byte)((nv & 0x00FF)));

                                            byte[] send = ret.ToArray();
                                            sock.Send(send);
                                            #endregion
                                        }
                                    }
                                    lstResponse.Clear();
                                }
                                #endregion
                                break;
                            case ModbusFunction.WORDWRITE_F6:
                                #region WordWrite
                                if (lstResponse.Count == 12)
                                {
                                    int WriteValue = (lstResponse[10] << 8) | lstResponse[11];

                                    if (WordWriteRequest != null)
                                    {
                                        var args = new WordWriteRequestArgs(lstResponse.ToArray());
                                        WordWriteRequest?.Invoke(this, args);

                                        if (args.Success)
                                        {
                                            #region Write
                                            List<byte> ret = new List<byte>();
                                            ret.Add(lstResponse[0]);
                                            ret.Add(lstResponse[1]);
                                            ret.Add(0);
                                            ret.Add(0);
                                            ret.Add(0);
                                            ret.Add(6);
                                            ret.Add((byte)args.Slave);
                                            ret.Add((byte)args.Function);
                                            ret.Add((byte)((args.StartAddress & 0xFF00) >> 8));
                                            ret.Add((byte)((args.StartAddress & 0x00FF)));
                                            ret.Add((byte)((args.WriteValue & 0xFF00) >> 8));
                                            ret.Add((byte)((args.WriteValue & 0x00FF)));

                                            byte[] send = ret.ToArray();
                                            sock.Send(send);
                                            #endregion
                                        }
                                    }
                                    lstResponse.Clear();
                                }
                                #endregion
                                break;
                            case ModbusFunction.MULTIBITWRITE_F15:
                                #region MultiBitWrite
                                if (lstResponse.Count >= 13)
                                {
                                    int ByteCount = lstResponse[12];
                                    if (lstResponse.Count >= 13 + ByteCount)
                                    {
                                        var args = new MultiBitWriteRequestArgs(lstResponse.ToArray());
                                        if (MultiBitWriteRequest != null)
                                        {
                                            MultiBitWriteRequest?.Invoke(this, args);

                                            if (args.Success)
                                            {
                                                #region Write
                                                List<byte> ret = new List<byte>();
                                                ret.Add(lstResponse[0]);
                                                ret.Add(lstResponse[1]);
                                                ret.Add(0);
                                                ret.Add(0);
                                                ret.Add(0);
                                                ret.Add(6);
                                                ret.Add((byte)args.Slave);
                                                ret.Add((byte)args.Function);
                                                ret.Add((byte)((args.StartAddress & 0xFF00) >> 8));
                                                ret.Add((byte)((args.StartAddress & 0x00FF)));
                                                ret.Add((byte)((args.Length & 0xFF00) >> 8));
                                                ret.Add((byte)((args.Length & 0x00FF)));

                                                byte[] send = ret.ToArray();
                                                sock.Send(send);
                                                #endregion
                                            }
                                        }
                                        lstResponse.Clear();
                                    }
                                }
                                #endregion
                                break;
                            case ModbusFunction.MULTIWORDWRITE_F16:
                                #region MultiWordWrite
                                if (lstResponse.Count >= 13)
                                {
                                    int ByteCount = lstResponse[12];
                                    if (lstResponse.Count >= 13 + ByteCount)
                                    {
                                        if (MultiWordWriteRequest != null)
                                        {
                                            var args = new MultiWordWriteRequestArgs(lstResponse.ToArray());
                                            MultiWordWriteRequest?.Invoke(this, args);

                                            if (args.Success)
                                            {
                                                #region Write
                                                List<byte> ret = new List<byte>();
                                                ret.Add(lstResponse[0]);
                                                ret.Add(lstResponse[1]);
                                                ret.Add(0);
                                                ret.Add(0);
                                                ret.Add(0);
                                                ret.Add(6);
                                                ret.Add((byte)args.Slave);
                                                ret.Add((byte)args.Function);
                                                ret.Add((byte)((args.StartAddress & 0xFF00) >> 8));
                                                ret.Add((byte)((args.StartAddress & 0x00FF)));
                                                ret.Add((byte)((args.Length & 0xFF00) >> 8));
                                                ret.Add((byte)((args.Length & 0x00FF)));

                                                byte[] send = ret.ToArray();
                                                sock.Send(send);
                                                #endregion
                                            }
                                        }
                                        lstResponse.Clear();
                                    }
                                }
                                #endregion
                                break;
                            case ModbusFunction.WORDBITSET_F26:
                                #region WordBitSet
                                if (lstResponse.Count == 13)
                                {
                                    if (WordBitSetRequest != null)
                                    {
                                        var args = new WordBitSetRequestArgs(lstResponse.ToArray());
                                        WordBitSetRequest?.Invoke(this, args);

                                        if (args.Success)
                                        {
                                            #region Write
                                            int nv = args.WriteValue ? 0xFF00 : 0;
                                            List<byte> ret = new List<byte>();
                                            ret.Add(lstResponse[0]);
                                            ret.Add(lstResponse[1]);
                                            ret.Add(0);
                                            ret.Add(0);
                                            ret.Add(0);
                                            ret.Add(6);
                                            ret.Add((byte)args.Slave);
                                            ret.Add((byte)args.Function);
                                            ret.Add((byte)((args.StartAddress & 0xFF00) >> 8));
                                            ret.Add((byte)((args.StartAddress & 0x00FF)));
                                            ret.Add((byte)((nv & 0xFF00) >> 8));
                                            ret.Add((byte)((nv & 0x00FF)));

                                            byte[] send = ret.ToArray();
                                            sock.Send(send);
                                            #endregion
                                        }
                                    }
                                    lstResponse.Clear();
                                }
                                #endregion
                                break;
                        }
                    }
                    #endregion

                    #region Buffer Clear
                    if ((DateTime.Now - prev).TotalMilliseconds >= 50 && lstResponse.Count > 0) lstResponse.Clear();
                    #endregion

                    isConnected = NetworkTool.IsSocketConnected(sock, 10000);

                    await Task.Delay(10, cancel);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.TimedOut) { }
                    else if (ex.SocketErrorCode == SocketError.ConnectionReset) { isConnected = false; }
                    else if (ex.SocketErrorCode == SocketError.ConnectionAborted) { isConnected = false; }
                    else if (ex.SocketErrorCode == SocketError.Shutdown) { isConnected = false; }
                }
                catch (OperationCanceledException) { isConnected = false; }
                catch { }
            }

            if (sock.Connected) sock.Close();
            SocketDisconnected?.Invoke(this, new SocketEventArgs(sock));
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
            var BA = new bool[Convert.ToInt32(Math.Ceiling((double)BaseArray.Length / 8.0) * 8.0)];
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
            if (args.StartAddress >= BaseAddress && args.StartAddress < BaseAddress + BaseArray.Length && (args.BitIndex >= 0 && args.BitIndex < 16))
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
