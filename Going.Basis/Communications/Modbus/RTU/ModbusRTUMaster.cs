using Going.Basis.Communications.LS;
using Going.Basis.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Communications.Modbus.RTU
{
    /// <summary>
    /// 시리얼 포트(RS-232/RS-485) 기반의 Modbus RTU 마스터 통신 클래스입니다.
    /// Auto/Manual 작업 큐를 통해 슬레이브 장치와 폴링 방식으로 통신합니다.
    /// </summary>
    public class ModbusRTUMaster : IDisposable
    {
        #region class : Work
        /// <summary>
        /// Modbus 통신 작업 단위를 나타내는 클래스입니다.
        /// </summary>
        /// <param name="id">메시지 식별자</param>
        /// <param name="data">전송할 Modbus 프레임 데이터</param>
        /// <param name="rescnt">예상 응답 바이트 수</param>
        public class Work(int id, byte[] data, int rescnt)
        {
            /// <summary>작업을 식별하는 메시지 ID</summary>
            public int MessageID { get; private set; } = id;
            /// <summary>전송할 Modbus 프레임 데이터</summary>
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
            public int Slave => WorkItem.Data[0];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[1];
            /// <summary>시작 주소</summary>
            public int StartAddress => WorkItem.Data[2] << 8 | WorkItem.Data[3];
            /// <summary>읽기 길이</summary>
            public int Length => WorkItem.Data[4] << 8 | WorkItem.Data[5];
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
            public int Slave => WorkItem.Data[0];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[1];
            /// <summary>시작 주소</summary>
            public int StartAddress => WorkItem.Data[2] << 8 | WorkItem.Data[3];
            /// <summary>읽기 길이</summary>
            public int Length => WorkItem.Data[4] << 8 | WorkItem.Data[5];

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
            public int Slave => WorkItem.Data[0];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[1];
            /// <summary>시작 주소</summary>
            public int StartAddress => WorkItem.Data[2] << 8 | WorkItem.Data[3];
            /// <summary>쓰기 값</summary>
            public bool WriteValue => (WorkItem.Data[4] << 8 | WorkItem.Data[5]) == 0xFF00;
        }

        /// <summary>
        /// 단일 워드 쓰기(FC6) 응답 수신 시 전달되는 이벤트 인자입니다.
        /// </summary>
        public class WordWriteEventArgs(Work WorkItem) : System.EventArgs
        {
            /// <summary>요청 메시지 ID</summary>
            public int MessageID => WorkItem.MessageID;
            /// <summary>슬레이브 주소</summary>
            public int Slave => WorkItem.Data[0];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[1];
            /// <summary>시작 주소</summary>
            public int StartAddress => WorkItem.Data[2] << 8 | WorkItem.Data[3];
            /// <summary>쓰기 값</summary>
            public int WriteValue => WorkItem.Data[4] << 8 | WorkItem.Data[5];
        }

        /// <summary>
        /// 다중 비트 쓰기(FC15) 응답 수신 시 전달되는 이벤트 인자입니다.
        /// </summary>
        public class MultiBitWriteEventArgs : System.EventArgs
        {
            /// <summary>요청 메시지 ID</summary>
            public int MessageID => WorkItem.MessageID;
            /// <summary>슬레이브 주소</summary>
            public int Slave => WorkItem.Data[0];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[1];
            /// <summary>시작 주소</summary>
            public int StartAddress => WorkItem.Data[2] << 8 | WorkItem.Data[3];
            /// <summary>쓰기 길이</summary>
            public int Length => WorkItem.Data[4] << 8 | WorkItem.Data[5];

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
                for (int i = 7; i < WorkItem.Data.Length - 2; i++)
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
            public int Slave => WorkItem.Data[0];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[1];
            /// <summary>시작 주소</summary>
            public int StartAddress => WorkItem.Data[2] << 8 | WorkItem.Data[3];
            /// <summary>쓰기 길이</summary>
            public int Length => WorkItem.Data[4] << 8 | WorkItem.Data[5];

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
                for (int i = 7; i < WorkItem.Data.Length - 2; i += 2)
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
            public int Slave => WorkItem.Data[0];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[1];
            /// <summary>시작 주소</summary>
            public int StartAddress => WorkItem.Data[2] << 8 | WorkItem.Data[3];
            /// <summary>대상 비트 인덱스 (0~15)</summary>
            public int BitIndex => WorkItem.Data[4];
            /// <summary>쓰기 값</summary>
            public bool WriteValue => (WorkItem.Data[5] << 8 | WorkItem.Data[6]) == 0xFF00;

        }

        /// <summary>
        /// 통신 타임아웃 발생 시 전달되는 이벤트 인자입니다.
        /// </summary>
        public class TimeoutEventArgs(Work WorkItem) : System.EventArgs
        {
            /// <summary>요청 메시지 ID</summary>
            public int MessageID => WorkItem.MessageID;
            /// <summary>슬레이브 주소</summary>
            public int Slave => WorkItem.Data[0];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[1];
            /// <summary>시작 주소</summary>
            public int StartAddress => WorkItem.Data[2] << 8 | WorkItem.Data[3];
        }

        /// <summary>
        /// CRC 오류 발생 시 전달되는 이벤트 인자입니다.
        /// </summary>
        public class CRCErrorEventArgs(Work WorkItem) : System.EventArgs
        {
            /// <summary>요청 메시지 ID</summary>
            public int MessageID => WorkItem.MessageID;
            /// <summary>슬레이브 주소</summary>
            public int Slave => WorkItem.Data[0];
            /// <summary>Modbus 함수 코드</summary>
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[1];
            /// <summary>시작 주소</summary>
            public int StartAddress => WorkItem.Data[2] << 8 | WorkItem.Data[3];
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

        /// <summary>응답 타임아웃 (밀리초, 기본값: 100)</summary>
        public int Timeout { get; set; } = 100;
        /// <summary>폴링 간격 (밀리초, 기본값: 10)</summary>
        public int Interval { get; set; } = 10;
        /// <summary>수신 버퍼 크기 (바이트, 기본값: 1024)</summary>
        public int BufferSize { get; set; } = 1024;

        /// <summary>시리얼 포트가 열려 있는지 여부</summary>
        public bool IsOpen => !OperatingSystem.IsBrowser() && ser.IsOpen;
        /// <summary>통신 루프가 시작되었는지 여부</summary>
        public bool IsStart { get; private set; }
        /// <summary>연결 끊김 시 자동 재접속 여부</summary>
        public bool AutoReconnect { get; set; }

        /// <summary>Dispose 되었는지 여부</summary>
        public bool IsDisposed { get; private set; }

        /// <summary>모듈 고유 식별자</summary>
        public string ModuleId { get; } = Guid.NewGuid().ToString();
        /// <summary>사용자 정의 태그 객체</summary>
        public object? Tag { get; set; } = null;
        #endregion

        #region Member Variable
        SerialPort ser = new() { PortName = "COM1", BaudRate = 115200 };

        Queue<Work> WorkQueue = [];
        List<Work> AutoWorkList = [];
        List<Work> ManualWorkList = [];

        byte[] baResponse = [];

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
        /// <summary>CRC 오류 발생 시 발생합니다.</summary>
        public event EventHandler<CRCErrorEventArgs>? CRCErrorReceived;

        /// <summary>시리얼 포트가 열렸을 때 발생합니다.</summary>
        public event EventHandler? DeviceOpened;
        /// <summary>시리얼 포트가 닫혔을 때 발생합니다.</summary>
        public event EventHandler? DeviceClosed;
        #endregion

        #region Construct
        /// <summary>Modbus RTU 마스터 인스턴스를 생성한다.</summary>
        public ModbusRTUMaster()
        {

        }
        #endregion

        #region Method
        #region Start / Stop
        /// <summary>
        /// 시리얼 포트를 열고 Modbus 마스터 통신 루프를 시작합니다.
        /// </summary>
        public void Start()
        {
            if (!IsOpen && !IsStart)
            {
                cancel = new CancellationTokenSource();
                task = Task.Run(async () =>
                {
                    IsStart = true;

                    var token = cancel.Token;

                    if (!OperatingSystem.IsBrowser())
                    {
                        do
                        {
                            try { ser.Open(); DeviceOpened?.Invoke(this, System.EventArgs.Empty); }
                            catch { }

                            if (ser.IsOpen)
                            {
                                baResponse = new byte[BufferSize];

                                while (!token.IsCancellationRequested && IsStart && ser.IsOpen)
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

                            if (ser.IsOpen)
                            {
                                ser.Close();
                                DeviceClosed?.Invoke(this, System.EventArgs.Empty);
                            }

                        } while (!token.IsCancellationRequested && AutoReconnect && IsStart);


                    }

                }, cancel.Token);
            }
        }

        /// <summary>
        /// 통신 루프를 중지하고 시리얼 포트를 닫습니다.
        /// </summary>
        public void Stop()
        {
            IsStart = false;
            cancel?.Cancel(false);

            if (task != null)
            {
                try
                {
                    if (task.Wait(3000)) task.Dispose();
                }
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
            #region Manual Fill (삭제)
            /*
            if (ManualWorkList.Count > 0)
            {
                for (int i = 0; i < ManualWorkList.Count; i++) WorkQueue.Enqueue(ManualWorkList[i]);
                ManualWorkList.Clear();
            }
            */
            #endregion

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
                        ser.Write(w.Data, 0, w.Data.Length);
                        ser.BaseStream.Flush();
                    }
                    catch (TimeoutException) { }
                    catch (IOException) { throw new SchedulerStopException(); }
                    catch (UnauthorizedAccessException) { throw new SchedulerStopException(); }
                    catch (InvalidOperationException) { throw new SchedulerStopException(); }
                    catch (OperationCanceledException) { throw new SchedulerStopException(); }
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
                            ser.ReadTimeout = Timeout;
                            var len = ser.Read(baResponse, nRecv, baResponse.Length - nRecv);
                            nRecv += len;
                        }
                        catch (TimeoutException) { }
                        catch (IOException) { throw new SchedulerStopException(); }
                        catch (UnauthorizedAccessException) { throw new SchedulerStopException(); }
                        catch (InvalidOperationException) { throw new SchedulerStopException(); }
                        catch (OperationCanceledException) { throw new SchedulerStopException(); }

                        if (nRecv == w.ResponseCount) bCollecting = false;

                        gap = DateTime.Now - prev;
                        if (gap.TotalMilliseconds >= Timeout) break;
                    }
                    #endregion

                    #region parse
                    if (gap.TotalMilliseconds < Timeout)
                    {
                        if (nRecv > 2)
                        {
                            byte crcHi = 0, crcLo = 0;
                            ModbusCRC.GetCRC(baResponse, nRecv - 2, ref crcHi, ref crcLo);
                            if (crcHi == baResponse[nRecv - 2] && crcLo == baResponse[nRecv - 1])
                            {
                                ModbusFunction Func = (ModbusFunction)baResponse[1];
                                switch (Func)
                                {
                                    case ModbusFunction.BITREAD_F1:
                                    case ModbusFunction.BITREAD_F2:
                                        #region BitRead
                                        {
                                            int ByteCount = baResponse[2];
                                            int Length = w.Data[4] << 8 | w.Data[5];
                                            byte[] baData = new byte[ByteCount];
                                            Array.Copy(baResponse, 3, baData, 0, ByteCount);
                                            BitArray ba = new BitArray(baData);

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
                                            int ByteCount = baResponse[2];
                                            if (baResponse.Length < 3 + ByteCount) throw new FormatException("WordRead response too short");
                                            int[] data = new int[ByteCount / 2];
                                            for (int i = 0; i < data.Length; i++) data[i] = Convert.ToUInt16(baResponse[3 + i * 2] << 8 | baResponse[4 + i * 2]);

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
                            }
                            else CRCErrorReceived?.Invoke(this, new CRCErrorEventArgs(w));
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
            byte[] data = new byte[8];
            byte crcHi = 0xff, crcLo = 0xff;

            data[0] = Convert.ToByte(slave);
            data[1] = fn;
            data[2] = startAddr.GetByte(1);
            data[3] = startAddr.GetByte(0);
            data[4] = length.GetByte(1);
            data[5] = length.GetByte(0);

            ModbusCRC.GetCRC(data, data.Length - 2, ref crcHi, ref crcLo);
            data[6] = crcHi;
            data[7] = crcLo;

            int nResCount = length / 8;
            if (length % 8 != 0) nResCount++;
            AutoWorkList.Add(new Work(id, data, nResCount + 5) { Timeout = timeout });
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
            byte[] data = new byte[8];
            byte crcHi = 0xff, crcLo = 0xff;

            data[0] = Convert.ToByte(slave);
            data[1] = fn;
            data[2] = startAddr.GetByte(1);
            data[3] = startAddr.GetByte(0);
            data[4] = length.GetByte(1);
            data[5] = length.GetByte(0);

            ModbusCRC.GetCRC(data, data.Length - 2, ref crcHi, ref crcLo);
            data[6] = crcHi;
            data[7] = crcLo;

            AutoWorkList.Add(new Work(id, data, length * 2 + 5) { Timeout = timeout });
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
            byte[] data = new byte[8];
            byte crcHi = 0xff, crcLo = 0xff;

            data[0] = Convert.ToByte(slave);
            data[1] = fn;
            data[2] = startAddr.GetByte(1);
            data[3] = startAddr.GetByte(0);
            data[4] = length.GetByte(1);
            data[5] = length.GetByte(0);

            ModbusCRC.GetCRC(data, data.Length - 2, ref crcHi, ref crcLo);
            data[6] = crcHi;
            data[7] = crcLo;

            int nResCount = length / 8;
            if (length % 8 != 0) nResCount++;
            ManualWorkList.Add(new Work(id, data, nResCount + 5) { RepeatCount = repeatCount, Timeout = timeout });
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
            byte[] data = new byte[8];
            byte crcHi = 0xff, crcLo = 0xff;

            data[0] = Convert.ToByte(slave);
            data[1] = fn;
            data[2] = startAddr.GetByte(1);
            data[3] = startAddr.GetByte(0);
            data[4] = length.GetByte(1);
            data[5] = length.GetByte(0);

            ModbusCRC.GetCRC(data, data.Length - 2, ref crcHi, ref crcLo);
            data[6] = crcHi;
            data[7] = crcLo;

            ManualWorkList.Add(new Work(id, data, length * 2 + 5) { RepeatCount = repeatCount, Timeout = timeout });
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
            byte[] data = new byte[8];
            byte crcHi = 0xff, crcLo = 0xff;
            int val = value ? 0xFF00 : 0x0000;

            data[0] = Convert.ToByte(slave);
            data[1] = 0x05;
            data[2] = startAddr.GetByte(1);
            data[3] = startAddr.GetByte(0);
            data[4] = val.GetByte(1);
            data[5] = val.GetByte(0);

            ModbusCRC.GetCRC(data, data.Length - 2, ref crcHi, ref crcLo);
            data[6] = crcHi;
            data[7] = crcLo;
            ManualWorkList.Add(new Work(id, data, 8) { RepeatCount = repeatCount, Timeout = timeout });
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
            byte[] data = new byte[8];
            byte crcHi = 0xff, crcLo = 0xff;

            data[0] = Convert.ToByte(slave);
            data[1] = 0x06;
            data[2] = startAddr.GetByte(1);
            data[3] = startAddr.GetByte(0);
            data[4] = value.GetByte(1);
            data[5] = value.GetByte(0);

            ModbusCRC.GetCRC(data, data.Length - 2, ref crcHi, ref crcLo);
            data[6] = crcHi;
            data[7] = crcLo;
            ManualWorkList.Add(new Work(id, data, 8) { RepeatCount = repeatCount, Timeout = timeout });
        }
        #endregion
        #region ManualMultiBitWrite_FC15
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
            Length += values.Length % 8 == 0 ? 0 : 1;

            byte[] data = new byte[9 + Length];
            byte crcHi = 0xff, crcLo = 0xff;


            data[0] = Convert.ToByte(slave);
            data[1] = 0x0F;
            data[2] = startAddr.GetByte(1);
            data[3] = startAddr.GetByte(0);
            data[4] = values.Length.GetByte(1);
            data[5] = values.Length.GetByte(0);
            data[6] = Convert.ToByte(Length);

            for (int i = 0; i < Length; i++)
            {
                byte val = 0;
                int nTemp = 0;
                for (int j = i * 8; j < values.Length && j < i * 8 + 8; j++)
                {
                    if (values[j])
                        val |= Convert.ToByte(Math.Pow(2, nTemp));
                    nTemp++;
                }
                data[7 + i] = val;
            }

            ModbusCRC.GetCRC(data, data.Length - 2, ref crcHi, ref crcLo);
            data[data.Length - 2] = crcHi;
            data[data.Length - 1] = crcLo;

            ManualWorkList.Add(new Work(id, data, 8) { RepeatCount = repeatCount, Timeout = timeout });
        }
        #endregion
        #region ManualMultiWordWrite_FC16
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
            byte[] data = new byte[9 + values.Length * 2];
            byte crcHi = 0xff, crcLo = 0xff;

            data[0] = Convert.ToByte(slave);
            data[1] = 0x10;
            data[2] = startAddr.GetByte(1);
            data[3] = startAddr.GetByte(0);
            data[4] = values.Length.GetByte(1);
            data[5] = values.Length.GetByte(0);
            data[6] = Convert.ToByte(values.Length * 2);

            for (int i = 0; i < values.Length; i++)
            {
                data[7 + i * 2] = values[i].GetByte(1);
                data[8 + i * 2] = values[i].GetByte(0);
            }

            ModbusCRC.GetCRC(data, data.Length - 2, ref crcHi, ref crcLo);
            data[data.Length - 2] = crcHi;
            data[data.Length - 1] = crcLo;

            ManualWorkList.Add(new Work(id, data, 8) { RepeatCount = repeatCount, Timeout = timeout });
        }
        #endregion
        #region ManualWordBitSet_FC26
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
            byte[] data = new byte[9];
            byte crcHi = 0xff, crcLo = 0xff;
            int val = value ? 0xFF00 : 0x0000;

            data[0] = Convert.ToByte(slave);
            data[1] = 0x1A;
            data[2] = startAddr.GetByte(1);
            data[3] = startAddr.GetByte(0);
            data[4] = bitIndex;
            data[5] = val.GetByte(1);
            data[6] = val.GetByte(0);

            ModbusCRC.GetCRC(data, data.Length - 2, ref crcHi, ref crcLo);
            data[7] = crcHi;
            data[8] = crcLo;

            ManualWorkList.Add(new Work(id, data, 8) { RepeatCount = repeatCount, Timeout = timeout });
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

            ser?.Dispose();
            cancel?.Dispose();
            task?.Dispose();
        }
        #endregion
        #endregion
    }
}
