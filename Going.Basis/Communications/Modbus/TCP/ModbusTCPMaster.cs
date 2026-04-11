using Going.Basis.Communications.LS;
using Going.Basis.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Going.Basis.Communications.Modbus.TCP
{
    /// <summary>
    /// TCP 소켓 기반의 Modbus TCP 마스터 통신 클래스입니다.
    /// Auto/Manual 작업 큐를 통해 슬레이브 장치와 폴링 방식으로 통신합니다.
    /// </summary>
    public class ModbusTCPMaster
    {
        #region class : Work
        /// <summary>
        /// Modbus 통신 작업 단위를 나타내는 클래스입니다.
        /// </summary>
        /// <param name="id">메시지 식별자</param>
        /// <param name="data">전송할 Modbus TCP 프레임 데이터</param>
        /// <param name="rescnt">예상 응답 바이트 수</param>
        public class Work(int id, byte[] data, int rescnt)
        {
            /// <summary>작업을 식별하는 메시지 ID</summary>
            public int MessageID { get; private set; } = id;
            /// <summary>전송할 Modbus TCP 프레임 데이터</summary>
            public byte[] Data { get; private set; } = data;
            /// <summary>예상되는 응답 바이트 수</summary>
            public int ResponseCount { get; private set; } = rescnt;
            /// <summary>타임아웃 발생 시 재시도 횟수 (null이면 재시도 안 함)</summary>
            public int? RepeatCount { get; set; } = null;
            /// <summary>이 작업에 대한 개별 타임아웃 값 (ms, null이면 기본값 사용)</summary>
            public int? Timeout { get; set; } = null;
        }
        #endregion
        #region class : EventArgs
        /// <summary>
        /// 비트 읽기(FC1/FC2) 응답 수신 시 전달되는 이벤트 인자입니다.
        /// </summary>
        public class BitReadEventArgs(Work WorkItem, bool[] Datas) : System.EventArgs
        {
            /// <summary>요청 메시지 ID</summary>
            public int MessageID => WorkItem.MessageID;
            /// <summary>슬레이브 주소</summary>
            public int Slave => WorkItem.Data[6];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[7];
            /// <summary>시작 주소</summary>
            public int StartAddress => WorkItem.Data[8] << 8 | WorkItem.Data[9];
            /// <summary>읽기 길이</summary>
            public int Length => WorkItem.Data[10] << 8 | WorkItem.Data[11];
            /// <summary>수신된 비트 데이터 배열</summary>
            public bool[] ReceiveData => Datas;
        }

        /// <summary>
        /// 워드 읽기(FC3/FC4) 응답 수신 시 전달되는 이벤트 인자입니다.
        /// </summary>
        public class WordReadEventArgs(Work WorkItem, int[] Datas) : System.EventArgs
        {
            /// <summary>요청 메시지 ID</summary>
            public int MessageID => WorkItem.MessageID;
            /// <summary>슬레이브 주소</summary>
            public int Slave => WorkItem.Data[6];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[7];
            /// <summary>시작 주소</summary>
            public int StartAddress => WorkItem.Data[8] << 8 | WorkItem.Data[9];
            /// <summary>읽기 길이</summary>
            public int Length => WorkItem.Data[10] << 8 | WorkItem.Data[11];

            /// <summary>수신된 워드 데이터 배열</summary>
            public int[] ReceiveData => Datas;
        }

        /// <summary>
        /// 단일 비트 쓰기(FC5) 응답 수신 시 전달되는 이벤트 인자입니다.
        /// </summary>
        public class BitWriteEventArgs(Work WorkItem) : System.EventArgs
        {
            /// <summary>요청 메시지 ID</summary>
            public int MessageID => WorkItem.MessageID;
            /// <summary>슬레이브 주소</summary>
            public int Slave => WorkItem.Data[6];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[7];
            /// <summary>시작 주소</summary>
            public int StartAddress => WorkItem.Data[8] << 8 | WorkItem.Data[9];
            /// <summary>쓰기 값</summary>
            public bool WriteValue => (WorkItem.Data[10] << 8 | WorkItem.Data[11]) == 0xFF00;
        }

        /// <summary>
        /// 단일 워드 쓰기(FC6) 응답 수신 시 전달되는 이벤트 인자입니다.
        /// </summary>
        public class WordWriteEventArgs(Work WorkItem) : System.EventArgs
        {
            /// <summary>요청 메시지 ID</summary>
            public int MessageID => WorkItem.MessageID;
            /// <summary>슬레이브 주소</summary>
            public int Slave => WorkItem.Data[6];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[7];
            /// <summary>시작 주소</summary>
            public int StartAddress => WorkItem.Data[8] << 8 | WorkItem.Data[9];
            /// <summary>쓰기 값</summary>
            public int WriteValue => WorkItem.Data[10] << 8 | WorkItem.Data[11];
        }

        /// <summary>
        /// 다중 비트 쓰기(FC15) 응답 수신 시 전달되는 이벤트 인자입니다.
        /// </summary>
        public class MultiBitWriteEventArgs : System.EventArgs
        {
            /// <summary>요청 메시지 ID</summary>
            public int MessageID => WorkItem.MessageID;
            /// <summary>슬레이브 주소</summary>
            public int Slave => WorkItem.Data[6];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[7];
            /// <summary>시작 주소</summary>
            public int StartAddress => WorkItem.Data[8] << 8 | WorkItem.Data[9];
            /// <summary>쓰기 길이</summary>
            public int Length => WorkItem.Data[10] << 8 | WorkItem.Data[11];

            /// <summary>쓰기 값 배열</summary>
            public bool[] WriteValues { get; private set; }

            private Work WorkItem;

            /// <summary>
            /// <see cref="MultiBitWriteEventArgs"/> 클래스의 새 인스턴스를 초기화합니다.
            /// </summary>
            /// <param name="WorkItem">원본 작업 항목</param>
            public MultiBitWriteEventArgs(Work WorkItem)
            {
                this.WorkItem = WorkItem;
                #region WriteValues
                List<bool> ret = new List<bool>();
                for (int i = 13; i < WorkItem.Data.Length - 2; i++)
                {
                    var v = WorkItem.Data[i];
                    for (int j = 0; j < 8; j++)
                        if (ret.Count < Length) ret.Add(v.GetBit(j));
                }
                WriteValues = [.. ret];
                #endregion
            }
        }

        /// <summary>
        /// 다중 워드 쓰기(FC16) 응답 수신 시 전달되는 이벤트 인자입니다.
        /// </summary>
        public class MultiWordWriteEventArgs : System.EventArgs
        {
            /// <summary>요청 메시지 ID</summary>
            public int MessageID => WorkItem.MessageID;
            /// <summary>슬레이브 주소</summary>
            public int Slave => WorkItem.Data[6];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[7];
            /// <summary>시작 주소</summary>
            public int StartAddress => WorkItem.Data[8] << 8 | WorkItem.Data[9];
            /// <summary>쓰기 길이</summary>
            public int Length => WorkItem.Data[10] << 8 | WorkItem.Data[11];

            /// <summary>쓰기 값 배열</summary>
            public int[] WriteValues { get; private set; }

            private Work WorkItem;

            /// <summary>
            /// <see cref="MultiWordWriteEventArgs"/> 클래스의 새 인스턴스를 초기화합니다.
            /// </summary>
            /// <param name="WorkItem">원본 작업 항목</param>
            public MultiWordWriteEventArgs(Work WorkItem)
            {
                this.WorkItem = WorkItem;
                #region WriteValues
                List<int> ret = new List<int>();
                for (int i = 13; i < WorkItem.Data.Length - 2; i += 2)
                {
                    ret.Add(WorkItem.Data[i] << 8 | WorkItem.Data[i + 1]);
                }
                WriteValues = [.. ret];
                #endregion
            }
        }

        /// <summary>
        /// 워드 비트 설정(FC26) 응답 수신 시 전달되는 이벤트 인자입니다.
        /// </summary>
        public class WordBitSetEventArgs(Work WorkItem) : System.EventArgs
        {
            /// <summary>요청 메시지 ID</summary>
            public int MessageID => WorkItem.MessageID;
            /// <summary>슬레이브 주소</summary>
            public int Slave => WorkItem.Data[6];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[7];
            /// <summary>시작 주소</summary>
            public int StartAddress => WorkItem.Data[8] << 8 | WorkItem.Data[9];
            /// <summary>대상 비트 인덱스 (0~15)</summary>
            public int BitIndex => WorkItem.Data[10];
            /// <summary>쓰기 값</summary>
            public bool WriteValue => (WorkItem.Data[11] << 8 | WorkItem.Data[12]) == 0xFF00;

        }

        /// <summary>
        /// 통신 타임아웃 발생 시 전달되는 이벤트 인자입니다.
        /// </summary>
        public class TimeoutEventArgs(Work WorkItem) : System.EventArgs
        {
            /// <summary>요청 메시지 ID</summary>
            public int MessageID => WorkItem.MessageID;
            /// <summary>슬레이브 주소</summary>
            public int Slave => WorkItem.Data[6];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[7];
            /// <summary>시작 주소</summary>
            public int StartAddress => WorkItem.Data[8] << 8 | WorkItem.Data[9];
        }

        /// <summary>
        /// CRC 오류 발생 시 전달되는 이벤트 인자입니다.
        /// </summary>
        public class CRCErrorEventArgs(Work WorkItem) : System.EventArgs
        {
            /// <summary>요청 메시지 ID</summary>
            public int MessageID => WorkItem.MessageID;
            /// <summary>슬레이브 주소</summary>
            public int Slave => WorkItem.Data[6];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[7];
            /// <summary>시작 주소</summary>
            public int StartAddress => WorkItem.Data[8] << 8 | WorkItem.Data[9];
        }
        #endregion

        #region Properties
        /// <summary>접속 대상 IP 주소 (기본값: "127.0.0.1")</summary>
        public string RemoteIP { get; set; } = "127.0.0.1";
        /// <summary>접속 대상 포트 번호 (기본값: 502)</summary>
        public int RemotePort { get; set; } = 502;

        /// <summary>응답 타임아웃 (밀리초, 기본값: 100)</summary>
        public int Timeout { get; set; } = 100;
        /// <summary>폴링 간격 (밀리초, 기본값: 10)</summary>
        public int Interval { get; set; } = 10;
        /// <summary>수신 버퍼 크기 (바이트, 기본값: 1024)</summary>
        public int BufferSize { get; set; } = 1024;

        /// <summary>TCP 소켓이 연결되어 있는지 여부</summary>
        public bool IsOpen => bIsOpen;
        /// <summary>통신 루프가 시작되었는지 여부</summary>
        public bool IsStart { get; private set; }
        /// <summary>연결 끊김 시 자동 재접속 여부 (기본값: true)</summary>
        public bool AutoReconnect { get; set; } = true;

        /// <summary>Dispose 되었는지 여부</summary>
        public bool IsDisposed { get; private set; }

        /// <summary>모듈 고유 식별자</summary>
        public string ModuleId { get; } = Guid.NewGuid().ToString();
        /// <summary>사용자 정의 태그 객체</summary>
        public object? Tag { get; set; } = null;
        #endregion

        #region Member Variable
        Socket? client;

        Queue<Work> WorkQueue = [];
        List<Work> AutoWorkList = [];
        List<Work> ManualWorkList = [];

        byte[] baResponse = [];
        bool bIsOpen = false;

        Task? task;
        CancellationTokenSource? cancel;
        #endregion

        #region Event
        /// <summary>비트 읽기(FC1/FC2) 응답 수신 시 발생합니다.</summary>
        public event EventHandler<BitReadEventArgs>? BitReadReceived;
        /// <summary>워드 읽기(FC3/FC4) 응답 수신 시 발생합니다.</summary>
        public event EventHandler<WordReadEventArgs>? WordReadReceived;
        /// <summary>단일 비트 쓰기(FC5) 응답 수신 시 발생합니다.</summary>
        public event EventHandler<BitWriteEventArgs>? BitWriteReceived;
        /// <summary>단일 워드 쓰기(FC6) 응답 수신 시 발생합니다.</summary>
        public event EventHandler<WordWriteEventArgs>? WordWriteReceived;
        /// <summary>다중 비트 쓰기(FC15) 응답 수신 시 발생합니다.</summary>
        public event EventHandler<MultiBitWriteEventArgs>? MultiBitWriteReceived;
        /// <summary>다중 워드 쓰기(FC16) 응답 수신 시 발생합니다.</summary>
        public event EventHandler<MultiWordWriteEventArgs>? MultiWordWriteReceived;
        /// <summary>워드 비트 설정(FC26) 응답 수신 시 발생합니다.</summary>
        public event EventHandler<WordBitSetEventArgs>? WordBitSetReceived;
        /// <summary>통신 타임아웃 발생 시 발생합니다.</summary>
        public event EventHandler<TimeoutEventArgs>? TimeoutReceived;

        /// <summary>TCP 소켓이 연결되었을 때 발생합니다.</summary>
        public event EventHandler<EventArgs>? SocketConnected;
        /// <summary>TCP 소켓 연결이 끊어졌을 때 발생합니다.</summary>
        public event EventHandler<EventArgs>? SocketDisconnected;
        #endregion

        #region Construct
        /// <summary>Modbus TCP 마스터 인스턴스를 생성한다.</summary>
        public ModbusTCPMaster()
        {

        }
        #endregion

        #region Method
        #region Start / Stop
        /// <summary>
        /// TCP 소켓을 연결하고 Modbus 마스터 통신 루프를 시작합니다.
        /// </summary>
        public void Start()
        {
            if (!IsOpen && !IsStart)
            {
                cancel = new CancellationTokenSource();
                task = Task.Run(async () =>
                {
                    var token = cancel.Token;

                    if (!OperatingSystem.IsBrowser())
                    {
                        do
                        {
                            #region Connect
                            try
                            {
                                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                                client.ReceiveTimeout = Timeout;
                                client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, Timeout);
                                client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
                                client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                                await client.ConnectAsync(RemoteIP, RemotePort, token);
                                bIsOpen = client.Connected;
                                SocketConnected?.Invoke(this, new SocketEventArgs(client));
                            }
                            catch { }
                            #endregion

                            if (bIsOpen)
                            {
                                baResponse = new byte[BufferSize];
                                IsStart = true;

                                while (!token.IsCancellationRequested && IsStart && bIsOpen)
                                {
                                    try
                                    {
                                        Process();
                                        await Task.Delay(Interval, token);
                                    }
                                    catch (SchedulerStopException) { break; }
                                    catch { }
                                }
                            }

                            if (bIsOpen && client != null)
                            {
                                client.Close();
                                SocketDisconnected?.Invoke(this, new SocketEventArgs(client));

                                client.Dispose();
                                client = null;
                            }

                        } while (!token.IsCancellationRequested && AutoReconnect && IsStart);
                    }

                }, cancel.Token);
            }
        }

        /// <summary>
        /// 통신 루프를 중지하고 TCP 소켓을 닫습니다.
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

        #region Process
        void Process()
        {
            if (WorkQueue.Count > 0 || ManualWorkList.Count > 0)
            {
                Work? w = null;
                #region Get Work
                if (ManualWorkList.Count > 0)
                {
                    w = ManualWorkList[0];
                    ManualWorkList.RemoveAt(0);
                }
                else w = WorkQueue.Dequeue();
                #endregion

                var bRepeat = true;
                var nTimeoutCount = 0;
                var Timeout = w.Timeout ?? this.Timeout;

                while (bRepeat)
                {
                    #region write
                    try
                    {
                        client?.Send(w.Data);
                    }
                    catch (SocketException ex)
                    {
                        if (ex.SocketErrorCode == SocketError.TimedOut) { }
                        else if (ex.SocketErrorCode == SocketError.ConnectionReset) { bIsOpen = false; }
                        else if (ex.SocketErrorCode == SocketError.ConnectionAborted) { bIsOpen = false; }
                        else if (ex.SocketErrorCode == SocketError.Shutdown) { bIsOpen = false; }
                    }
                    catch (OperationCanceledException) { throw new SchedulerStopException(); }
                    catch { }
                    if (!IsOpen) throw new SchedulerStopException();
                    #endregion

                    #region read
                    var nRecv = 0;
                    var prev = DateTime.Now;
                    var gap = TimeSpan.Zero;
                    var bCollecting = true;
                    while (bCollecting)
                    {
                        try
                        {
                            var len = 0;
                            if (client != null && client.Connected)
                            {
                                client.ReceiveTimeout = Timeout;
                                client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, Timeout);

                                len = client.Receive(baResponse, nRecv, baResponse.Length - nRecv, SocketFlags.None);
                                nRecv += len;
                            }

                            bIsOpen = len > 0;
                        }
                        catch (SocketException ex)
                        {
                            if (ex.SocketErrorCode == SocketError.TimedOut) { }
                            else if (ex.SocketErrorCode == SocketError.ConnectionReset) { bIsOpen = false; }
                            else if (ex.SocketErrorCode == SocketError.ConnectionAborted) { bIsOpen = false; }
                            else if (ex.SocketErrorCode == SocketError.Shutdown) { bIsOpen = false; }
                        }
                        catch (OperationCanceledException) { throw new SchedulerStopException(); }
                        catch { }

                        if (!IsOpen) throw new SchedulerStopException();

                        if (nRecv == w.ResponseCount) bCollecting = false;

                        gap = DateTime.Now - prev;
                        if (gap.TotalMilliseconds >= Timeout) break;
                    }
                    #endregion

                    #region parse
                    if (gap.TotalMilliseconds < Timeout)
                    {
                        int Slave = baResponse[6];
                        ModbusFunction Func = (ModbusFunction)baResponse[7];
                        int StartAddress = Convert.ToInt32((w.Data[8] << 8) | (w.Data[9]));
                        switch (Func)
                        {
                            case ModbusFunction.BITREAD_F1:
                            case ModbusFunction.BITREAD_F2:
                                #region BitRead
                                {
                                    int ByteCount = baResponse[8];
                                    int Length = ((w.Data[10] << 8) | w.Data[11]);
                                    byte[] baData = new byte[ByteCount];
                                    Array.Copy(baResponse, 9, baData, 0, ByteCount);
                                    BitArray ba = new(baData);

                                    bool[] bd = new bool[Length];
                                    for (int i = 0; i < ba.Length && i < Length; i++) bd[i] = ba[i];

                                    BitReadReceived?.Invoke(this, new BitReadEventArgs(w, bd));
                                }
                                #endregion
                                break;
                            case ModbusFunction.WORDREAD_F3:
                            case ModbusFunction.WORDREAD_F4:
                                #region WordRead
                                {
                                    int ByteCount = baResponse[8];
                                    int[] data = new int[ByteCount / 2];
                                    for (int i = 0; i < data.Length; i++) data[i] = Convert.ToUInt16(baResponse[9 + (i * 2)] << 8 | baResponse[10 + (i * 2)]);
                                    WordReadReceived?.Invoke(this, new WordReadEventArgs(w, data));
                                }
                                #endregion
                                break;
                            case ModbusFunction.BITWRITE_F5:
                                #region BitWrite
                                {
                                    BitWriteReceived?.Invoke(this, new BitWriteEventArgs(w));
                                }
                                #endregion
                                break;
                            case ModbusFunction.WORDWRITE_F6:
                                #region WordWrite
                                {
                                    WordWriteReceived?.Invoke(this, new WordWriteEventArgs(w));
                                }
                                #endregion
                                break;
                            case ModbusFunction.MULTIBITWRITE_F15:
                                #region MultiBitWrite
                                {
                                    MultiBitWriteReceived?.Invoke(this, new MultiBitWriteEventArgs(w));
                                }
                                #endregion
                                break;
                            case ModbusFunction.MULTIWORDWRITE_F16:
                                #region MultiWordWrite
                                {
                                    MultiWordWriteReceived?.Invoke(this, new MultiWordWriteEventArgs(w));
                                }
                                #endregion
                                break;
                            case ModbusFunction.WORDBITSET_F26:
                                #region WordBitSet
                                {
                                    WordBitSetReceived?.Invoke(this, new WordBitSetEventArgs(w));
                                }
                                #endregion
                                break;
                        }

                        bRepeat = false;
                    }
                    else
                    {
                        #region Timeout
                        TimeoutReceived?.Invoke(this, new TimeoutEventArgs(w));
                        nTimeoutCount++;
                        if (nTimeoutCount >= (w.RepeatCount ?? 0)) bRepeat = false;
                        #endregion
                    }
                    #endregion
                }
            }
            else
            {
                #region Auto Fill
                foreach (var v in AutoWorkList) WorkQueue.Enqueue(v);
                #endregion
            }
        }
        #endregion

        #region ContainAutoID
        /// <summary>
        /// 자동 작업 목록에 지정된 메시지 ID가 존재하는지 확인합니다.
        /// </summary>
        /// <param name="MessageID">확인할 메시지 ID</param>
        /// <returns>해당 ID가 존재하면 true</returns>
        public bool ContainAutoID(int MessageID)
        {
            bool ret = false;
            for (int i = AutoWorkList.Count - 1; i >= 0; i--)
            {
                if (AutoWorkList[i].MessageID == MessageID)
                {
                    ret = true;
                }
            }
            return ret;
        }
        #endregion
        #region RemoveManual
        /// <summary>
        /// 수동 작업 목록에서 지정된 메시지 ID의 작업을 제거합니다.
        /// </summary>
        /// <param name="MessageID">제거할 메시지 ID</param>
        /// <returns>제거된 항목이 있으면 true</returns>
        public bool RemoveManual(int MessageID)
        {
            bool ret = false;
            for (int i = ManualWorkList.Count - 1; i >= 0; i--)
            {
                if (ManualWorkList[i].MessageID == MessageID)
                {
                    ManualWorkList.RemoveAt(i);
                    ret = true;
                }
            }
            return ret;
        }
        #endregion
        #region RemoveAuto
        /// <summary>
        /// 자동 작업 목록에서 지정된 메시지 ID의 작업을 제거합니다.
        /// </summary>
        /// <param name="MessageID">제거할 메시지 ID</param>
        /// <returns>제거된 항목이 있으면 true</returns>
        public bool RemoveAuto(int MessageID)
        {
            bool ret = false;
            for (int i = AutoWorkList.Count - 1; i >= 0; i--)
            {
                if (AutoWorkList[i].MessageID == MessageID)
                {
                    AutoWorkList.RemoveAt(i);
                    ret = true;
                }
            }
            return ret;
        }
        #endregion
        #region Clear
        /// <summary>수동 작업 목록을 모두 비웁니다.</summary>
        public void ClearManual() { ManualWorkList.Clear(); }
        /// <summary>자동 작업 목록을 모두 비웁니다.</summary>
        public void ClearAuto() { AutoWorkList.Clear(); }
        /// <summary>현재 작업 큐를 모두 비웁니다.</summary>
        public void ClearWorkSchedule() { WorkQueue.Clear(); }
        #endregion

        #region AutoBitRead
        /// <summary>
        /// FC1(코일 읽기)을 사용하여 자동 반복 비트 읽기 작업을 등록합니다.
        /// </summary>
        /// <param name="id">메시지 식별자</param>
        /// <param name="slave">슬레이브 주소</param>
        /// <param name="startAddr">시작 주소</param>
        /// <param name="length">읽을 비트 수</param>
        /// <param name="timeout">개별 타임아웃 (ms, null이면 기본값 사용)</param>
        public void AutoBitRead_FC1(int id, int slave, int startAddr, int length, int? timeout = null) => AutoBitRead(id, slave, 1, startAddr, length, timeout);
        /// <inheritdoc cref="AutoBitRead_FC1"/>
        public void AutoBitRead_FC2(int id, int slave, int startAddr, int length, int? timeout = null) => AutoBitRead(id, slave, 2, startAddr, length, timeout);
        private void AutoBitRead(int id, int slave, byte fn, int startAddr, int length, int? timeout)
        {
            byte[] data = new byte[12];

            data[0] = 0;                                                // TransactionID
            data[1] = 0;
            data[2] = 0;                                                // ProtocolID
            data[3] = 0;
            data[4] = 0x00;                                             // Length (for Next Frame)
            data[5] = 0x06;
            data[6] = Convert.ToByte(slave);
            data[7] = fn;
            data[8] = startAddr.GetByte(1);
            data[9] = startAddr.GetByte(0);
            data[10] = length.GetByte(1);
            data[11] = length.GetByte(0);

            int nResCount = length / 8;
            if (length % 8 != 0) nResCount++;
            AutoWorkList.Add(new Work(id, data, nResCount + 9) { Timeout = timeout });
        }
        #endregion
        #region AutoWordRead
        /// <summary>
        /// FC3(보유 레지스터 읽기)을 사용하여 자동 반복 워드 읽기 작업을 등록합니다.
        /// </summary>
        /// <param name="id">메시지 식별자</param>
        /// <param name="slave">슬레이브 주소</param>
        /// <param name="startAddr">시작 주소</param>
        /// <param name="length">읽을 워드 수</param>
        /// <param name="timeout">개별 타임아웃 (ms, null이면 기본값 사용)</param>
        public void AutoWordRead_FC3(int id, int slave, int startAddr, int length, int? timeout = null) => AutoWordRead(id, slave, 3, startAddr, length, timeout);
        /// <inheritdoc cref="AutoWordRead_FC3"/>
        public void AutoWordRead_FC4(int id, int slave, int startAddr, int length, int? timeout = null) => AutoWordRead(id, slave, 4, startAddr, length, timeout);
        private void AutoWordRead(int id, int slave, byte fn, int startAddr, int length, int? timeout)
        {
            byte[] data = new byte[12];

            data[0] = 0;                                                // TransactionID
            data[1] = 0;
            data[2] = 0;                                                // ProtocolID
            data[3] = 0;
            data[4] = 0x00;                                             // Length (for Next Frame)
            data[5] = 0x06;
            data[6] = Convert.ToByte(slave);
            data[7] = fn;
            data[8] = startAddr.GetByte(1);
            data[9] = startAddr.GetByte(0);
            data[10] = length.GetByte(1);
            data[11] = length.GetByte(0);

            AutoWorkList.Add(new Work(id, data, length * 2 + 9) { Timeout = timeout });
        }
        #endregion
        #region ManualBitRead
        /// <summary>
        /// FC1(코일 읽기)을 사용하여 수동 비트 읽기 작업을 등록합니다.
        /// </summary>
        /// <param name="id">메시지 식별자</param>
        /// <param name="slave">슬레이브 주소</param>
        /// <param name="startAddr">시작 주소</param>
        /// <param name="length">읽을 비트 수</param>
        /// <param name="repeatCount">타임아웃 시 재시도 횟수</param>
        /// <param name="timeout">개별 타임아웃 (ms, null이면 기본값 사용)</param>
        public void ManualBitRead_FC1(int id, int slave, int startAddr, int length, int? repeatCount = null, int? timeout = null) => ManualBitRead(id, slave, 1, startAddr, length, repeatCount, timeout);
        /// <inheritdoc cref="ManualBitRead_FC1"/>
        public void ManualBitRead_FC2(int id, int slave, int startAddr, int length, int? repeatCount = null, int? timeout = null) => ManualBitRead(id, slave, 2, startAddr, length, repeatCount, timeout);
        private void ManualBitRead(int id, int slave, byte fn, int startAddr, int length, int? repeatCount, int? timeout)
        {
            byte[] data = new byte[12];

            data[0] = 0;                                                // TransactionID
            data[1] = 0;
            data[2] = 0;                                                // ProtocolID
            data[3] = 0;
            data[4] = 0x00;                                             // Length (for Next Frame)
            data[5] = 0x06;
            data[6] = Convert.ToByte(slave);
            data[7] = fn;
            data[8] = startAddr.GetByte(1);
            data[9] = startAddr.GetByte(0);
            data[10] = length.GetByte(1);
            data[11] = length.GetByte(0);

            int nResCount = length / 8;
            if (length % 8 != 0) nResCount++;
            ManualWorkList.Add(new Work(id, data, nResCount + 9) { RepeatCount = repeatCount, Timeout = timeout });
        }
        #endregion
        #region ManualWordRead
        /// <summary>
        /// FC3(보유 레지스터 읽기)을 사용하여 수동 워드 읽기 작업을 등록합니다.
        /// </summary>
        /// <param name="id">메시지 식별자</param>
        /// <param name="slave">슬레이브 주소</param>
        /// <param name="startAddr">시작 주소</param>
        /// <param name="length">읽을 워드 수</param>
        /// <param name="repeatCount">타임아웃 시 재시도 횟수</param>
        /// <param name="timeout">개별 타임아웃 (ms, null이면 기본값 사용)</param>
        public void ManualWordRead_FC3(int id, int slave, int startAddr, int length, int? repeatCount = null, int? timeout = null) => ManualWordRead(id, slave, 3, startAddr, length, repeatCount, timeout);
        /// <inheritdoc cref="ManualWordRead_FC3"/>
        public void ManualWordRead_FC4(int id, int slave, int startAddr, int length, int? repeatCount = null, int? timeout = null) => ManualWordRead(id, slave, 4, startAddr, length, repeatCount, timeout);
        private void ManualWordRead(int id, int slave, byte fn, int startAddr, int length, int? repeatCount, int? timeout)
        {
            byte[] data = new byte[12];

            data[0] = 0;                                                // TransactionID
            data[1] = 0;
            data[2] = 0;                                                // ProtocolID
            data[3] = 0;
            data[4] = 0x00;                                             // Length (for Next Frame)
            data[5] = 0x06;
            data[6] = Convert.ToByte(slave);
            data[7] = fn;
            data[8] = startAddr.GetByte(1);
            data[9] = startAddr.GetByte(0);
            data[10] = length.GetByte(1);
            data[11] = length.GetByte(0);

            ManualWorkList.Add(new Work(id, data, length * 2 + 9) { RepeatCount = repeatCount, Timeout = timeout });
        }
        #endregion
        #region ManualBitWrite
        /// <summary>
        /// FC5를 사용하여 단일 비트(코일) 쓰기 작업을 수동 목록에 등록합니다.
        /// </summary>
        /// <param name="id">메시지 식별자</param>
        /// <param name="slave">슬레이브 주소</param>
        /// <param name="startAddr">대상 주소</param>
        /// <param name="value">쓸 값 (true=ON, false=OFF)</param>
        /// <param name="repeatCount">타임아웃 시 재시도 횟수</param>
        /// <param name="timeout">개별 타임아웃 (ms, null이면 기본값 사용)</param>
        public void ManualBitWrite_FC5(int id, int slave, int startAddr, bool value, int? repeatCount = null, int? timeout = null)
        {
            byte[] data = new byte[12];
            int val = value ? 0xFF00 : 0x0000;

            data[0] = 0;                                                // TransactionID
            data[1] = 0;
            data[2] = 0;                                                // ProtocolID
            data[3] = 0;
            data[4] = 0x00;                                             // Length (for Next Frame)
            data[5] = 0x06;
            data[6] = Convert.ToByte(slave);
            data[7] = 0x05;
            data[8] = startAddr.GetByte(1);
            data[9] = startAddr.GetByte(0);
            data[10] = val.GetByte(1);
            data[11] = val.GetByte(0);

            ManualWorkList.Add(new Work(id, data, 12) { RepeatCount = repeatCount, Timeout = timeout });
        }
        #endregion
        #region ManualWordWrite
        /// <summary>
        /// FC6을 사용하여 단일 워드(레지스터) 쓰기 작업을 수동 목록에 등록합니다.
        /// </summary>
        /// <param name="id">메시지 식별자</param>
        /// <param name="slave">슬레이브 주소</param>
        /// <param name="startAddr">대상 주소</param>
        /// <param name="value">쓸 값</param>
        /// <param name="repeatCount">타임아웃 시 재시도 횟수</param>
        /// <param name="timeout">개별 타임아웃 (ms, null이면 기본값 사용)</param>
        public void ManualWordWrite_FC6(int id, int slave, int startAddr, int value, int? repeatCount = null, int? timeout = null)
        {
            byte[] data = new byte[12];

            data[0] = 0;                                                // TransactionID
            data[1] = 0;
            data[2] = 0;                                                // ProtocolID
            data[3] = 0;
            data[4] = 0x00;                                             // Length (for Next Frame)
            data[5] = 0x06;
            data[6] = Convert.ToByte(slave);
            data[7] = 0x06;
            data[8] = startAddr.GetByte(1);
            data[9] = startAddr.GetByte(0);
            data[10] = value.GetByte(1);
            data[11] = value.GetByte(0);

            ManualWorkList.Add(new Work(id, data, 12) { RepeatCount = repeatCount, Timeout = timeout });
        }
        #endregion
        #region ManualMultiBitWrite
        /// <summary>
        /// FC15를 사용하여 다중 비트(코일) 쓰기 작업을 수동 목록에 등록합니다.
        /// </summary>
        /// <param name="id">메시지 식별자</param>
        /// <param name="slave">슬레이브 주소</param>
        /// <param name="startAddr">시작 주소</param>
        /// <param name="values">쓸 비트 값 배열</param>
        /// <param name="repeatCount">타임아웃 시 재시도 횟수</param>
        /// <param name="timeout">개별 타임아웃 (ms, null이면 기본값 사용)</param>
        public void ManualMultiBitWrite_FC15(int id, int slave, int startAddr, bool[] values, int? repeatCount = null, int? timeout = null)
        {
            int Length = values.Length / 8;
            Length += (values.Length % 8 == 0) ? 0 : 1;

            int LengthEx = Length + 0x07;

            byte[] data = new byte[13 + Length];

            data[0] = 0;                                                // TransactionID
            data[1] = 0;
            data[2] = 0;                                                // ProtocolID
            data[3] = 0;
            data[4] = LengthEx.GetByte(1);                                 // Length (for Next Frame)
            data[5] = LengthEx.GetByte(0);
            data[6] = Convert.ToByte(slave);
            data[7] = 0x0F;
            data[8] = startAddr.GetByte(1);
            data[9] = startAddr.GetByte(0);
            data[10] = values.Length.GetByte(1);
            data[11] = values.Length.GetByte(0);
            data[12] = Convert.ToByte(Length);

            for (int i = 0; i < Length; i++)
            {
                byte val = 0;
                int nTemp = 0;
                for (int j = (i * 8); j < values.Length && j < (i * 8) + 8; j++)
                {
                    if (values[j])
                        val |= Convert.ToByte(Math.Pow(2, nTemp));
                    nTemp++;
                }
                data[13 + i] = val;
            }

            ManualWorkList.Add(new Work(id, data, 12) { RepeatCount = repeatCount, Timeout = timeout });
        }
        #endregion
        #region ManualMultiWordWrite
        /// <summary>
        /// FC16을 사용하여 다중 워드(레지스터) 쓰기 작업을 수동 목록에 등록합니다.
        /// </summary>
        /// <param name="id">메시지 식별자</param>
        /// <param name="slave">슬레이브 주소</param>
        /// <param name="startAddr">시작 주소</param>
        /// <param name="values">쓸 워드 값 배열</param>
        /// <param name="repeatCount">타임아웃 시 재시도 횟수</param>
        /// <param name="timeout">개별 타임아웃 (ms, null이면 기본값 사용)</param>
        public void ManualMultiWordWrite_FC16(int id, int slave, int startAddr, int[] values, int? repeatCount = null, int? timeout = null)
        {
            byte[] data = new byte[13 + (values.Length * 2)];

            int LengthEx = values.Length * 2 + 0x07;

            data[0] = 0;                                                // TransactionID
            data[1] = 0;
            data[2] = 0;                                                // ProtocolID
            data[3] = 0;
            data[4] = LengthEx.GetByte(1);                                 // Length (for Next Frame)
            data[5] = LengthEx.GetByte(0);
            data[6] = Convert.ToByte(slave);
            data[7] = 0x10;
            data[8] = startAddr.GetByte(1);
            data[9] = startAddr.GetByte(0);
            data[10] = values.Length.GetByte(1);
            data[11] = values.Length.GetByte(0);
            data[12] = Convert.ToByte(values.Length * 2);

            for (int i = 0; i < values.Length; i++)
            {
                data[13 + (i * 2)] = values[i].GetByte(1);
                data[14 + (i * 2)] = values[i].GetByte(0);
            }

            ManualWorkList.Add(new Work(id, data, 12) { RepeatCount = repeatCount, Timeout = timeout });
        }
        #endregion
        #region ManualWordBitSet
        /// <summary>
        /// FC26을 사용하여 워드 내 특정 비트 설정 작업을 수동 목록에 등록합니다.
        /// </summary>
        /// <param name="id">메시지 식별자</param>
        /// <param name="slave">슬레이브 주소</param>
        /// <param name="startAddr">대상 워드 주소</param>
        /// <param name="bitIndex">설정할 비트 인덱스 (0~15)</param>
        /// <param name="value">설정할 값 (true=ON, false=OFF)</param>
        /// <param name="repeatCount">타임아웃 시 재시도 횟수</param>
        /// <param name="timeout">개별 타임아웃 (ms, null이면 기본값 사용)</param>
        public void ManualWordBitSet_FC26(int id, int slave, int startAddr, byte bitIndex, bool value, int? repeatCount = null, int? timeout = null)
        {
            byte[] data = new byte[13];

            int val = value ? 0xFF00 : 0x0000;

            data[0] = 0;                                                // TransactionID
            data[1] = 0;
            data[2] = 0;                                                // ProtocolID
            data[3] = 0;
            data[4] = 0x00;                                             // Length (for Next Frame)
            data[5] = 0x07;
            data[6] = Convert.ToByte(slave);
            data[7] = 0x1A;
            data[8] = startAddr.GetByte(1);
            data[9] = startAddr.GetByte(0);
            data[10] = bitIndex;
            data[11] = val.GetByte(1);
            data[12] = val.GetByte(0);

            ManualWorkList.Add(new Work(id, data, 12) { RepeatCount = repeatCount, Timeout = timeout });
        }
        #endregion

        #region Dispose
        /// <summary>
        /// 통신을 중지하고 사용 중인 리소스를 해제합니다.
        /// </summary>
        public void Dispose()
        {
            IsDisposed = true;

            if (IsStart) Stop();

            client?.Dispose();
            cancel?.Dispose();
            task?.Dispose();
        }
        #endregion
        #endregion
    }
}
