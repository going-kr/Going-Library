using Going.Basis.Datas;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Going.Basis.Communications.LS.CNet;

namespace Going.Basis.Communications.LS
{
    #region enum : CNetFunc
    /// <summary>
    /// CNet 프로토콜의 통신 기능 종류를 나타내는 열거형
    /// </summary>
    public enum CNetFunc
    {
        /// <summary>없음</summary>
        NONE,
        /// <summary>개별 디바이스 읽기 (RSS)</summary>
        READ_SINGLE,
        /// <summary>연속 디바이스 블록 읽기 (RSB)</summary>
        READ_BLOCK,
        /// <summary>개별 디바이스 쓰기 (WSS)</summary>
        WRITE_SINGLE,
        /// <summary>연속 디바이스 블록 쓰기 (WSB)</summary>
        WRITE_BLOCK,
    }
    #endregion
    #region class : CNetValue
    /// <summary>
    /// CNet 프로토콜에서 개별 쓰기(WSS) 시 사용되는 디바이스-값 쌍을 나타내는 클래스
    /// </summary>
    /// <param name="dev">PLC 디바이스 주소 (예: "%MW100")</param>
    /// <param name="value">쓰기할 값</param>
    public class CNetValue(string dev, int value)
    {
        /// <summary>PLC 디바이스 주소를 가져옵니다.</summary>
        public string Device { get; private set; } = dev;
        /// <summary>디바이스에 쓰기할 값을 가져옵니다.</summary>
        public int Value { get; private set; } = value;
    }
    #endregion
    #region class : Exception
    /// <summary>
    /// 통신 스케줄러의 루프를 중단시키기 위한 내부 예외 클래스
    /// </summary>
    public class SchedulerStopException : Exception { }
    #endregion

    /// <summary>
    /// LS Electric PLC용 CNet 프로토콜 시리얼 통신 클래스.
    /// RSS(개별읽기), RSB(블록읽기), WSS(개별쓰기), WSB(블록쓰기) 명령을 지원하며,
    /// 자동(Auto) 반복 폴링과 수동(Manual) 일회성 작업 큐를 관리합니다.
    /// </summary>
    public class CNet : IDisposable
    {
        #region const : Special Code
        private const byte ENQ = 0x05;
        private const byte EOT = 0x04;
        private const byte STX = 0x02;
        private const byte ETX = 0x03;
        private const byte ACK = 0x06;
        private const byte NAK = 0x15;
        #endregion
        #region class : Work
        internal class Work(int id, byte[] data, string[]? Devices = null)
        {
            public int MessageID { get; private set; } = id;
            public byte[] Data { get; private set; } = data;
            public int? RepeatCount { get; set; } = null;
            public int? Timeout { get; set; } = null;
            public string[]? Devices { get; set; } = Devices;

            public int Slave => int.TryParse(Encoding.ASCII.GetString(Data, 1, 2), out var n) ? n : -1;
            public CNetFunc Function => CNet.StringToFunc(Encoding.ASCII.GetString(Data, 3, 3));
        }
        #endregion
        #region class : EventArgs
        /// <summary>
        /// 데이터 읽기 응답 수신 시 발생하는 이벤트 인자
        /// </summary>
        /// <param name="ID">메시지 식별자</param>
        /// <param name="Slave">슬레이브 국번</param>
        /// <param name="Func">실행된 CNet 기능</param>
        /// <param name="Data">읽어온 데이터 배열</param>
        /// <param name="ReadAddress">읽기 대상 디바이스 주소 배열</param>
        public class DataReadEventArgs(int ID, int Slave, CNetFunc Func, int[] Data, string[]? ReadAddress) : EventArgs
        {
            /// <summary>메시지 식별자를 가져옵니다.</summary>
            public int MessageID { get; private set; } = ID;
            /// <summary>슬레이브 국번을 가져옵니다.</summary>
            public int Slave { get; private set; } = Slave;
            /// <summary>실행된 CNet 기능을 가져옵니다.</summary>
            public CNetFunc Function { get; private set; } = Func;
            /// <summary>읽어온 데이터 배열을 가져옵니다.</summary>
            public int[] Data { get; private set; } = Data;
            /// <summary>읽기 대상 디바이스 주소 배열을 가져옵니다.</summary>
            public string[]? ReadAddress { get; private set; } = ReadAddress;
        }
        /// <summary>
        /// 쓰기 응답 수신 시 발생하는 이벤트 인자
        /// </summary>
        /// <param name="ID">메시지 식별자</param>
        /// <param name="Slave">슬레이브 국번</param>
        /// <param name="Func">실행된 CNet 기능</param>
        public class WriteEventArgs(int ID, int Slave, CNetFunc Func) : EventArgs
        {
            /// <summary>메시지 식별자를 가져옵니다.</summary>
            public int MessageID { get; private set; } = ID;
            /// <summary>슬레이브 국번을 가져옵니다.</summary>
            public int Slave { get; private set; } = Slave;
            /// <summary>실행된 CNet 기능을 가져옵니다.</summary>
            public CNetFunc Function { get; private set; } = Func;
        }
        /// <summary>
        /// 통신 타임아웃 발생 시의 이벤트 인자
        /// </summary>
        /// <param name="ID">메시지 식별자</param>
        /// <param name="Slave">슬레이브 국번</param>
        /// <param name="Func">실행된 CNet 기능</param>
        public class TimeoutEventArgs(int ID, int Slave, CNetFunc Func) : EventArgs
        {
            /// <summary>메시지 식별자를 가져옵니다.</summary>
            public int MessageID { get; private set; } = ID;
            /// <summary>슬레이브 국번을 가져옵니다.</summary>
            public int Slave { get; private set; } = Slave;
            /// <summary>실행된 CNet 기능을 가져옵니다.</summary>
            public CNetFunc Function { get; private set; } = Func;
        }
        /// <summary>
        /// BCC(Block Check Character) 오류 발생 시의 이벤트 인자
        /// </summary>
        /// <param name="ID">메시지 식별자</param>
        /// <param name="Slave">슬레이브 국번</param>
        /// <param name="Func">실행된 CNet 기능</param>
        public class BCCErrorEventArgs(int ID, int Slave, CNetFunc Func) : EventArgs
        {
            /// <summary>메시지 식별자를 가져옵니다.</summary>
            public int MessageID { get; private set; } = ID;
            /// <summary>슬레이브 국번을 가져옵니다.</summary>
            public int Slave { get; private set; } = Slave;
            /// <summary>실행된 CNet 기능을 가져옵니다.</summary>
            public CNetFunc Function { get; private set; } = Func;
        }
        /// <summary>
        /// PLC에서 NAK(부정 응답) 수신 시의 이벤트 인자
        /// </summary>
        /// <param name="ID">메시지 식별자</param>
        /// <param name="Slave">슬레이브 국번</param>
        /// <param name="Func">실행된 CNet 기능</param>
        /// <param name="ErrCode">PLC가 반환한 오류 코드</param>
        public class NAKEventArgs(int ID, int Slave, CNetFunc Func, int ErrCode) : EventArgs
        {
            /// <summary>메시지 식별자를 가져옵니다.</summary>
            public int MessageID { get; private set; } = ID;
            /// <summary>슬레이브 국번을 가져옵니다.</summary>
            public int Slave { get; private set; } = Slave;
            /// <summary>실행된 CNet 기능을 가져옵니다.</summary>
            public CNetFunc Function { get; private set; } = Func;
            /// <summary>PLC가 반환한 오류 코드를 가져옵니다.</summary>
            public int ErrorCode { get; private set; } = ErrCode;
        }
        #endregion
        
        #region Properties
        /// <summary>시리얼 포트 이름을 가져오거나 설정합니다. (예: "COM1")</summary>
        public string Port { get => ser.PortName; set => ser.PortName = value; }
        /// <summary>통신 속도(bps)를 가져오거나 설정합니다.</summary>
        public int Baudrate { get => ser.BaudRate; set => ser.BaudRate = value; }
        /// <summary>패리티 비트 설정을 가져오거나 설정합니다.</summary>
        public Parity Parity { get => ser.Parity; set => ser.Parity = value; }
        /// <summary>데이터 비트 수를 가져오거나 설정합니다.</summary>
        public int DataBits { get => ser.DataBits; set => ser.DataBits = value; }
        /// <summary>스톱 비트 설정을 가져오거나 설정합니다.</summary>
        public StopBits StopBits { get => ser.StopBits; set => ser.StopBits = value; }

        /// <summary>응답 대기 타임아웃 시간(밀리초)을 가져오거나 설정합니다. 기본값은 100입니다.</summary>
        public int Timeout { get; set; } = 100;
        /// <summary>통신 폴링 간격(밀리초)을 가져오거나 설정합니다. 기본값은 10입니다.</summary>
        public int Interval { get; set; } = 10;
        /// <summary>수신 버퍼 크기(바이트)를 가져오거나 설정합니다. 기본값은 1024입니다.</summary>
        public int BufferSize { get; set; } = 1024;

        /// <summary>시리얼 포트가 열려 있는지 여부를 가져옵니다.</summary>
        public bool IsOpen => ser.IsOpen;
        /// <summary>통신 스케줄러가 실행 중인지 여부를 가져옵니다.</summary>
        public bool IsStart { get; private set; }
        /// <summary>연결 끊김 시 자동 재연결 여부를 가져오거나 설정합니다.</summary>
        public bool AutoReconnect { get; set; }

        /// <summary>객체가 Dispose되었는지 여부를 가져옵니다.</summary>
        public bool IsDisposed { get; private set; }

        /// <summary>사용자 정의 태그 데이터를 가져오거나 설정합니다.</summary>
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
        /// <summary>PLC로부터 데이터 읽기 응답을 수신했을 때 발생합니다.</summary>
        public event EventHandler<DataReadEventArgs>? DataReceived;
        /// <summary>PLC에 쓰기 완료 응답을 수신했을 때 발생합니다.</summary>
        public event EventHandler<WriteEventArgs>? WriteResponseReceived;
        /// <summary>응답 대기 타임아웃이 발생했을 때 발생합니다.</summary>
        public event EventHandler<TimeoutEventArgs>? TimeoutReceived;
        /// <summary>BCC(Block Check Character) 검증 오류가 발생했을 때 발생합니다.</summary>
        public event EventHandler<BCCErrorEventArgs>? BCCErrorReceived;
        /// <summary>PLC로부터 NAK(부정 응답)을 수신했을 때 발생합니다.</summary>
        public event EventHandler<NAKEventArgs>? NAKReceived;

        /// <summary>시리얼 포트가 열렸을 때 발생합니다.</summary>
        public event EventHandler? DeviceOpened;
        /// <summary>시리얼 포트가 닫혔을 때 발생합니다.</summary>
        public event EventHandler? DeviceClosed;
        #endregion

        #region Construct
        /// <summary>
        /// CNet 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        public CNet()
        {

        }
        #endregion

        #region Method
        #region Start / Stop
        /// <summary>
        /// 시리얼 포트를 열고 통신 스케줄러를 시작합니다.
        /// 자동 작업 큐에 등록된 명령을 반복 실행하며, 수동 작업을 우선 처리합니다.
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

                                while (!token.IsCancellationRequested && IsStart)
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
        /// 통신 스케줄러를 정지하고 시리얼 포트를 닫습니다.
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
            try
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

                            if (nRecv > 3 && nRecv < 256 && baResponse[nRecv - 3] == ETX) bCollecting = false;

                            gap = DateTime.Now - prev;
                            if (gap.TotalMilliseconds >= Timeout) break;
                        }
                        #endregion

                        #region parse
                        if (gap.TotalMilliseconds < Timeout)
                        {
                            if (nRecv > 6)
                            {
                                #region var
                                string strSlave = new([(char)baResponse[1], (char)baResponse[2]]);
                                string strFunc = new([(char)baResponse[3], (char)baResponse[4], (char)baResponse[5]]);
                                string strBCC = new([(char)baResponse[nRecv - 2], (char)baResponse[nRecv - 1]]);

                                var slave = int.Parse(strSlave, System.Globalization.NumberStyles.AllowHexSpecifier);
                                var func = StringToFunc(strFunc);
                                var bcc = int.Parse(strBCC, System.Globalization.NumberStyles.AllowHexSpecifier);
                                #endregion

                                #region calc bcc
                                int calcbcc = 0;
                                for (int i = 0; i < nRecv; i++)
                                {
                                    calcbcc += baResponse[i];
                                    if (baResponse[i] == ETX) break;
                                }
                                calcbcc &= 0xFF;
                                #endregion

                                if (calcbcc == bcc)
                                {
                                    if (baResponse[0] == ACK)
                                    {
                                        switch (func)
                                        {
                                            #region READ_BLACK
                                            case CNetFunc.READ_BLOCK:
                                                {
                                                    if (baResponse.Length < 10) throw new FormatException("READ_BLOCK response too short");
                                                    int BlockCount = int.Parse(new string(new char[] { (char)baResponse[6], (char)baResponse[7] }));
                                                    List<int> Data = new List<int>();
                                                    int nIndexOffset = 0;
                                                    int DataType = 0;
                                                    switch ((char)w.Data[10])
                                                    {
                                                        case 'X': DataType = 1; break;
                                                        case 'B': DataType = 1; break;
                                                        case 'W': DataType = 2; break;
                                                        case 'D': DataType = 4; break;
                                                        case 'L': DataType = 8; break;
                                                    }
                                                    for (int i = 0; i < BlockCount; i++)
                                                    {
                                                        int nSize = int.Parse(new string(new char[] { (char)baResponse[8 + nIndexOffset], (char)baResponse[9 + nIndexOffset] }), System.Globalization.NumberStyles.AllowHexSpecifier);
                                                        for (int j = 0; j < nSize * 2; j += (DataType * 2))
                                                        {
                                                            string str = "";
                                                            for (int k = j; k < j + (DataType * 2); k++) str += (char)baResponse[10 + k];
                                                            Data.Add(Convert.ToInt32(str, 16));
                                                        }
                                                        nIndexOffset += ((nSize * 2) + 2);
                                                    }
                                                    DataReceived?.Invoke(this, new DataReadEventArgs(w.MessageID, slave, func, Data.ToArray(), w.Devices));
                                                }
                                                break;
                                            #endregion
                                            #region READ_SINGLE
                                            case CNetFunc.READ_SINGLE:
                                                {
                                                    if (baResponse.Length < 10) throw new FormatException("READ_SINGLE response too short");
                                                    int BlockCount = int.Parse(new string(new char[] { (char)baResponse[6], (char)baResponse[7] }));
                                                    List<int> Data = new List<int>();
                                                    int nIndexOffset = 0;
                                                    string[] street = Encoding.ASCII.GetString(w.Data).Split('%');
                                                    for (int i = 0; i < BlockCount; i++)
                                                    {
                                                        int DataType = 0;
                                                        switch ((char)street[i + 1][1])
                                                        {
                                                            case 'X': DataType = 1; break;
                                                            case 'B': DataType = 1; break;
                                                            case 'W': DataType = 2; break;
                                                            case 'D': DataType = 4; break;
                                                            case 'L': DataType = 8; break;
                                                        }
                                                        string stre = Encoding.ASCII.GetString(baResponse).Trim();
                                                        int nSize = int.Parse(new string(new char[] { (char)baResponse[8 + nIndexOffset], (char)baResponse[9 + nIndexOffset] }), System.Globalization.NumberStyles.AllowHexSpecifier);
                                                        for (int j = 0; j < nSize * 2; j += (DataType * 2))
                                                        {
                                                            string str = "";
                                                            for (int k = j; k < j + (DataType * 2); k++) str += (char)baResponse[10 + nIndexOffset + k];
                                                            Data.Add(int.Parse(str, System.Globalization.NumberStyles.AllowHexSpecifier));
                                                        }
                                                        nIndexOffset += ((nSize * 2) + 2);
                                                    }
                                                    DataReceived?.Invoke(this, new DataReadEventArgs(w.MessageID, slave, func, Data.ToArray(), w.Devices));
                                                }
                                                break;
                                            #endregion
                                            #region WRITE_BLOCK
                                            case CNetFunc.WRITE_BLOCK:
                                                {
                                                    WriteResponseReceived?.Invoke(this, new WriteEventArgs(w.MessageID, slave, func));
                                                }
                                                break;
                                            #endregion
                                            #region WRITE_SINGLE
                                            case CNetFunc.WRITE_SINGLE:
                                                {
                                                    WriteResponseReceived?.Invoke(this, new WriteEventArgs(w.MessageID, slave, func));
                                                }
                                                break;
                                                #endregion
                                        }
                                    }
                                    else if (baResponse[0] == NAK)
                                    {
                                        #region NAK
                                        int ErrorCode = 0;
                                        string str = "";
                                        for (int i = 6; i < nRecv - 3; i++) str += (char)baResponse[i];
                                        ErrorCode = Convert.ToInt32(str, 16);

                                        NAKReceived?.Invoke(this, new NAKEventArgs(w.MessageID, slave, func, ErrorCode));
                                        #endregion
                                    }
                                }
                                else
                                {
                                    #region BCC ERROR
                                    BCCErrorReceived?.Invoke(this, new BCCErrorEventArgs(w.MessageID, slave, func));
                                    #endregion
                                }
                            }

                            bRepeat = false;
                        }
                        else
                        {
                            #region Timeout
                            TimeoutReceived?.Invoke(this, new TimeoutEventArgs(w.MessageID, w.Slave, w.Function));
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
            catch { }
        }
        #endregion

        #region ContainAutoID
        /// <summary>
        /// 지정한 메시지 ID가 자동 작업 목록에 존재하는지 확인합니다.
        /// </summary>
        /// <param name="MessageID">확인할 메시지 식별자</param>
        /// <returns>자동 작업 목록에 존재하면 true, 그렇지 않으면 false</returns>
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
        /// 지정한 메시지 ID의 수동 작업을 목록에서 제거합니다.
        /// </summary>
        /// <param name="MessageID">제거할 메시지 식별자</param>
        /// <returns>제거에 성공하면 true, 해당 ID가 없으면 false</returns>
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
        /// 지정한 메시지 ID의 자동 작업을 목록에서 제거합니다.
        /// </summary>
        /// <param name="MessageID">제거할 메시지 식별자</param>
        /// <returns>제거에 성공하면 true, 해당 ID가 없으면 false</returns>
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
        /// <summary>현재 대기 중인 작업 큐를 모두 비웁니다.</summary>
        public void ClearWorkSchedule() { WorkQueue.Clear(); }
        #endregion

        #region AutoRSS(id, slave, device)
        /// <summary>
        /// 자동 작업 목록에 단일 디바이스 개별 읽기(RSS) 명령을 등록합니다.
        /// </summary>
        /// <param name="id">메시지 식별자</param>
        /// <param name="slave">슬레이브 국번</param>
        /// <param name="device">읽기 대상 디바이스 주소 (예: "%MW100")</param>
        /// <param name="timeout">응답 대기 타임아웃(밀리초). null이면 기본값 사용</param>
        public void AutoRSS(int id, int slave, string device, int? timeout = null)
        {
            var strbul = new StringBuilder();
            strbul.Append((char)ENQ);
            strbul.Append(slave.ToString("X2"));
            strbul.Append("rSS");
            strbul.Append("01");
            strbul.Append(device.Length.ToString("X2"));
            strbul.Append(device);
            strbul.Append((char)EOT);
            strbul.Append("00");

            byte[] data = Encoding.ASCII.GetBytes(strbul.ToString());

            int nBCC = 0;
            for (int i = 0; i < data.Length; i++)
            {
                nBCC += data[i];
                if (data[i] == EOT) break;
            }
            nBCC &= 0xFF;
            string bcc = nBCC.ToString("X2");
            data[data.Length - 2] = (byte)bcc[0];
            data[data.Length - 1] = (byte)bcc[1];

            AutoWorkList.Add(new Work(id, data) { Devices = [device], Timeout = timeout });
        }
        #endregion
        #region AutoRSS(id, slave, devices)
        /// <summary>
        /// 자동 작업 목록에 복수 디바이스 개별 읽기(RSS) 명령을 등록합니다.
        /// </summary>
        /// <param name="id">메시지 식별자</param>
        /// <param name="slave">슬레이브 국번</param>
        /// <param name="devices">읽기 대상 디바이스 주소 배열</param>
        /// <param name="timeout">응답 대기 타임아웃(밀리초). null이면 기본값 사용</param>
        public void AutoRSS(int id, int slave, string[] devices, int? timeout = null)
        {
            var strbul = new StringBuilder();
            strbul.Append((char)ENQ);
            strbul.Append(slave.ToString("X2"));
            strbul.Append("rSS");
            strbul.Append(devices.Length.ToString("X2"));
            for (int i = 0; i < devices.Length; i++)
            {
                strbul.Append(devices[i].Length.ToString("X2"));
                strbul.Append(devices[i]);
            }
            strbul.Append((char)EOT);
            strbul.Append("00");

            byte[] data = Encoding.ASCII.GetBytes(strbul.ToString());

            int nBCC = 0;
            for (int i = 0; i < data.Length; i++)
            {
                nBCC += data[i];
                if (data[i] == EOT) break;
            }
            nBCC &= 0xFF;
            string bcc = nBCC.ToString("X2");
            data[data.Length - 2] = (byte)bcc[0];
            data[data.Length - 1] = (byte)bcc[1];

            AutoWorkList.Add(new Work(id, data) { Devices = devices, Timeout = timeout });
        }
        #endregion
        #region AutoRSB(id, slave, device, Length)
        /// <summary>
        /// 자동 작업 목록에 연속 디바이스 블록 읽기(RSB) 명령을 등록합니다.
        /// </summary>
        /// <param name="id">메시지 식별자</param>
        /// <param name="slave">슬레이브 국번</param>
        /// <param name="device">시작 디바이스 주소 (예: "%MW100")</param>
        /// <param name="length">연속으로 읽을 디바이스 개수</param>
        /// <param name="timeout">응답 대기 타임아웃(밀리초). null이면 기본값 사용</param>
        public void AutoRSB(int id, int slave, string device, int length, int? timeout = null)
        {
            var strbul = new StringBuilder();
            strbul.Append((char)ENQ);
            strbul.Append(slave.ToString("X2"));
            strbul.Append("rSB");
            strbul.Append(device.Length.ToString("X2"));
            strbul.Append(device);
            strbul.Append(length.ToString("X2"));
            strbul.Append((char)EOT);
            strbul.Append("00");

            byte[] data = Encoding.ASCII.GetBytes(strbul.ToString());

            int nBCC = 0;
            for (int i = 0; i < data.Length; i++)
            {
                nBCC += data[i];
                if (data[i] == EOT) break;
            }
            nBCC &= 0xFF;
            string bcc = nBCC.ToString("X2");
            data[data.Length - 2] = (byte)bcc[0];
            data[data.Length - 1] = (byte)bcc[1];

            List<string> d = [];
            if (device.Length < 4) throw new FormatException($"Invalid device format: '{device}'");
            int n = Convert.ToInt32(device.Substring(3));
            for (int i = 0; i < length; i++) d.Add(device.Substring(0, 3) + (n + i));

            AutoWorkList.Add(new Work(id, data) { Devices = [.. d], Timeout = timeout });
        }
        #endregion
        #region ManualRSS(id, slave, device)
        /// <summary>
        /// 수동 작업 목록에 단일 디바이스 개별 읽기(RSS) 명령을 등록합니다. 일회성으로 실행됩니다.
        /// </summary>
        /// <param name="id">메시지 식별자</param>
        /// <param name="slave">슬레이브 국번</param>
        /// <param name="device">읽기 대상 디바이스 주소</param>
        /// <param name="repeatCount">타임아웃 시 재시도 횟수. null이면 재시도하지 않음</param>
        /// <param name="timeout">응답 대기 타임아웃(밀리초). null이면 기본값 사용</param>
        public void ManualRSS(int id, int slave, string device, int? repeatCount = null, int? timeout = null)
        {
            var strbul = new StringBuilder();
            strbul.Append((char)ENQ);
            strbul.Append(slave.ToString("X2"));
            strbul.Append("rSS");
            strbul.Append("01");
            strbul.Append(device.Length.ToString("X2"));
            strbul.Append(device);
            strbul.Append((char)EOT);
            strbul.Append("00");

            byte[] data = Encoding.ASCII.GetBytes(strbul.ToString());

            int nBCC = 0;
            for (int i = 0; i < data.Length; i++)
            {
                nBCC += data[i];
                if (data[i] == EOT) break;
            }
            nBCC &= 0xFF;
            string bcc = nBCC.ToString("X2");
            data[data.Length - 2] = (byte)bcc[0];
            data[data.Length - 1] = (byte)bcc[1];

            ManualWorkList.Add(new Work(id, data) { Devices = [device], RepeatCount = repeatCount, Timeout = timeout });
        }
        #endregion
        #region ManualRSS(id, slave, devices)
        /// <summary>
        /// 수동 작업 목록에 복수 디바이스 개별 읽기(RSS) 명령을 등록합니다. 일회성으로 실행됩니다.
        /// </summary>
        /// <param name="id">메시지 식별자</param>
        /// <param name="slave">슬레이브 국번</param>
        /// <param name="devices">읽기 대상 디바이스 주소 배열</param>
        /// <param name="repeatCount">타임아웃 시 재시도 횟수. null이면 재시도하지 않음</param>
        /// <param name="timeout">응답 대기 타임아웃(밀리초). null이면 기본값 사용</param>
        public void ManualRSS(int id, int slave, string[] devices, int? repeatCount = null, int? timeout = null)
        {
            var strbul = new StringBuilder();
            strbul.Append((char)ENQ);
            strbul.Append(slave.ToString("X2"));
            strbul.Append("rSS");
            strbul.Append(devices.Length.ToString("X2"));
            for (int i = 0; i < devices.Length; i++)
            {
                strbul.Append(devices[i].Length.ToString("X2"));
                strbul.Append(devices[i]);
            }
            strbul.Append((char)EOT);
            strbul.Append("00");

            byte[] data = Encoding.ASCII.GetBytes(strbul.ToString());

            int nBCC = 0;
            for (int i = 0; i < data.Length; i++)
            {
                nBCC += data[i];
                if (data[i] == EOT) break;
            }
            nBCC &= 0xFF;
            string bcc = nBCC.ToString("X2");
            data[data.Length - 2] = (byte)bcc[0];
            data[data.Length - 1] = (byte)bcc[1];

            ManualWorkList.Add(new Work(id, data) { Devices = devices, RepeatCount = repeatCount, Timeout = timeout });
        }
        #endregion
        #region ManualRSB(id, slave, device, length)
        /// <summary>
        /// 수동 작업 목록에 연속 디바이스 블록 읽기(RSB) 명령을 등록합니다. 일회성으로 실행됩니다.
        /// </summary>
        /// <param name="id">메시지 식별자</param>
        /// <param name="slave">슬레이브 국번</param>
        /// <param name="device">시작 디바이스 주소</param>
        /// <param name="length">연속으로 읽을 디바이스 개수</param>
        /// <param name="repeatCount">타임아웃 시 재시도 횟수. null이면 재시도하지 않음</param>
        /// <param name="timeout">응답 대기 타임아웃(밀리초). null이면 기본값 사용</param>
        public void ManualRSB(int id, int slave, string device, int length, int? repeatCount = null, int? timeout = null)
        {
            var strbul = new StringBuilder();
            strbul.Append((char)ENQ);
            strbul.Append(slave.ToString("X2"));
            strbul.Append("rSB");
            strbul.Append(device.Length.ToString("X2"));
            strbul.Append(device);
            strbul.Append(length.ToString("X2"));
            strbul.Append((char)EOT);
            strbul.Append("00");

            byte[] data = Encoding.ASCII.GetBytes(strbul.ToString());

            int nBCC = 0;
            for (int i = 0; i < data.Length; i++)
            {
                nBCC += data[i];
                if (data[i] == EOT) break;
            }
            nBCC &= 0xFF;
            string bcc = nBCC.ToString("X2");
            data[data.Length - 2] = (byte)bcc[0];
            data[data.Length - 1] = (byte)bcc[1];

            List<string> d = [];
            if (device.Length < 4) throw new FormatException($"Invalid device format: '{device}'");
            int n = Convert.ToInt32(device.Substring(3));
            for (int i = 0; i < length; i++)
                d.Add(device.Substring(0, 3) + (n + i));

            ManualWorkList.Add(new Work(id, data) { Devices = d.ToArray(), RepeatCount = repeatCount, Timeout = timeout });
        }
        #endregion
        #region ManualWSS(id, slave, device, value)
        /// <summary>
        /// 수동 작업 목록에 단일 디바이스 개별 쓰기(WSS) 명령을 등록합니다. 일회성으로 실행됩니다.
        /// </summary>
        /// <param name="id">메시지 식별자</param>
        /// <param name="slave">슬레이브 국번</param>
        /// <param name="device">쓰기 대상 디바이스 주소</param>
        /// <param name="value">쓰기할 값</param>
        /// <param name="repeatCount">타임아웃 시 재시도 횟수. null이면 재시도하지 않음</param>
        /// <param name="timeout">응답 대기 타임아웃(밀리초). null이면 기본값 사용</param>
        public void ManualWSS(int id, int slave, string device, int value, int? repeatCount = null, int? timeout = null)
        {
            var strbul = new StringBuilder();
            strbul.Append((char)ENQ);
            strbul.Append(slave.ToString("X2"));
            strbul.Append("wSS");
            strbul.Append("01");
            strbul.Append(device.Length.ToString("X2"));
            strbul.Append(device);
            switch (device[2])
            {
                case 'X': strbul.Append(value.ToString("X2")); break;
                case 'B': strbul.Append(value.ToString("X2")); break;
                case 'W': strbul.Append(value.ToString("X4")); break;
                case 'D': strbul.Append(value.ToString("X8")); break;
                case 'L': strbul.Append(value.ToString("X16")); break;
            }
            strbul.Append((char)EOT);
            strbul.Append("00");

            byte[] data = Encoding.ASCII.GetBytes(strbul.ToString());

            int nBCC = 0;
            for (int i = 0; i < data.Length; i++)
            {
                nBCC += data[i];
                if (data[i] == EOT) break;
            }
            nBCC &= 0xFF;
            string bcc = nBCC.ToString("X2");
            data[data.Length - 2] = (byte)bcc[0];
            data[data.Length - 1] = (byte)bcc[1];

            ManualWorkList.Add(new Work(id, data) { RepeatCount = repeatCount, Timeout = timeout });
        }
        #endregion
        #region ManualWSS(id, slave, values)
        /// <summary>
        /// 수동 작업 목록에 복수 디바이스 개별 쓰기(WSS) 명령을 등록합니다. 일회성으로 실행됩니다.
        /// </summary>
        /// <param name="id">메시지 식별자</param>
        /// <param name="slave">슬레이브 국번</param>
        /// <param name="values">쓰기할 디바이스-값 쌍 컬렉션</param>
        /// <param name="repeatCount">타임아웃 시 재시도 횟수. null이면 재시도하지 않음</param>
        /// <param name="timeout">응답 대기 타임아웃(밀리초). null이면 기본값 사용</param>
        public void ManualWSS(int id, int slave, IEnumerable<CNetValue> values, int? repeatCount = null, int? timeout = null)
        {
            var strbul = new StringBuilder();
            strbul.Append((char)ENQ);
            strbul.Append(slave.ToString("X2"));
            strbul.Append("wSS");
            strbul.Append(values.Count().ToString("X2"));
            foreach (var v in values)
            {
                strbul.Append(v.Device.Length.ToString("X2"));
                strbul.Append(v.Device);
                if (v.Device[2] == 'W') strbul.Append(v.Value.ToString("X4"));
                if (v.Device[2] == 'X') strbul.Append(v.Value.ToString("X2"));
            }
            strbul.Append((char)EOT);
            strbul.Append("00");

            byte[] data = Encoding.ASCII.GetBytes(strbul.ToString());

            int nBCC = 0;
            for (int i = 0; i < data.Length; i++)
            {
                nBCC += data[i];
                if (data[i] == EOT) break;
            }
            nBCC &= 0xFF;
            string bcc = nBCC.ToString("X2");
            data[data.Length - 2] = (byte)bcc[0];
            data[data.Length - 1] = (byte)bcc[1];

            ManualWorkList.Add(new Work(id, data) { RepeatCount = repeatCount, Timeout = timeout });
        }
        #endregion
        #region ManualWSB(id, slave, device, values)
        /// <summary>
        /// 수동 작업 목록에 연속 디바이스 블록 쓰기(WSB) 명령을 등록합니다. 일회성으로 실행됩니다.
        /// </summary>
        /// <param name="id">메시지 식별자</param>
        /// <param name="slave">슬레이브 국번</param>
        /// <param name="device">시작 디바이스 주소</param>
        /// <param name="values">쓰기할 값 배열</param>
        /// <param name="repeatCount">타임아웃 시 재시도 횟수. null이면 재시도하지 않음</param>
        /// <param name="timeout">응답 대기 타임아웃(밀리초). null이면 기본값 사용</param>
        public void ManualWSB(int id, int slave, string device, int[] values, int? repeatCount = null, int? timeout = null)
        {
            var strbul = new StringBuilder();
            strbul.Append((char)ENQ);
            strbul.Append(slave.ToString("X2"));
            strbul.Append("wSB");
            strbul.Append(device.Length.ToString("X2"));
            strbul.Append(values.Length);
            for (int i = 0; i < values.Length; i++)
                strbul.Append(values[i].ToString("X4"));
            strbul.Append((char)EOT);
            strbul.Append("00");

            byte[] data = Encoding.ASCII.GetBytes(strbul.ToString());

            int nBCC = 0;
            for (int i = 0; i < data.Length; i++)
            {
                nBCC += data[i];
                if (data[i] == EOT) break;
            }
            nBCC &= 0xFF;
            string bcc = nBCC.ToString("X2");
            data[data.Length - 2] = (byte)bcc[0];
            data[data.Length - 1] = (byte)bcc[1];

            ManualWorkList.Add(new Work(id, data) { RepeatCount = repeatCount, Timeout = timeout });
        }
        #endregion

        #region Dispose
        /// <summary>
        /// 통신을 정지하고 시리얼 포트 등 관련 리소스를 해제합니다.
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

        #region Static Method
        #region FuncToString
        internal static string FuncToString(CNetFunc func)
        {
            string ret = "";
            switch (func)
            {
                case CNetFunc.READ_SINGLE: ret = "rSS"; break;
                case CNetFunc.READ_BLOCK: ret = "rSB"; break;
                case CNetFunc.WRITE_SINGLE: ret = "wSS"; break;
                case CNetFunc.WRITE_BLOCK: ret = "wSB"; break;
            }
            return ret;
        }
        #endregion
        #region StringToFunc
        internal static CNetFunc StringToFunc(string func)
        {
            CNetFunc ret = CNetFunc.NONE;
            switch (func)
            {
                case "rSS": ret = CNetFunc.READ_SINGLE; break;
                case "rSB": ret = CNetFunc.READ_BLOCK; break;
                case "wSS": ret = CNetFunc.WRITE_SINGLE; break;
                case "wSB": ret = CNetFunc.WRITE_BLOCK; break;
            }
            return ret;
        }
        #endregion
        #endregion
    }
}
