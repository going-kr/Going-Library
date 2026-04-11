using Going.Basis.Communications.LS;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using static Going.Basis.Communications.LS.CNet;

namespace Going.Basis.Communications.Mitsubishi
{
    #region enum : MITSUBISHI FX Function
    /// <summary>
    /// Mitsubishi FX 시리즈 MC 프로토콜의 통신 기능 종류를 나타내는 열거형
    /// </summary>
    public enum MCFunc
    {
        /// <summary>없음</summary>
        None,
        /// <summary>비트 디바이스 읽기 (BR)</summary>
        BitRead,
        /// <summary>워드 디바이스 읽기 (WR)</summary>
        WordRead,
        /// <summary>비트 디바이스 쓰기 (BW)</summary>
        BitWrite,
        /// <summary>워드 디바이스 쓰기 (WW)</summary>
        WordWrite,
    }
    #endregion

    /// <summary>
    /// Mitsubishi FX 시리즈 PLC용 MC 프로토콜 시리얼 통신 클래스.
    /// 비트(BR/BW) 및 워드(WR/WW) 디바이스 읽기/쓰기를 지원하며,
    /// 자동(Auto) 반복 폴링과 수동(Manual) 일회성 작업 큐를 관리합니다.
    /// </summary>
    public class MC
    {
        #region const : Special Code
        private const byte ENQ = 0x05;
        private const byte EOT = 0x04;
        private const byte STX = 0x02;
        private const byte ETX = 0x03;
        private const byte ACK = 0x06;
        private const byte NAK = 0x15;
        private const byte CR = 0x0D;
        private const byte LF = 0x0A;
        private const byte CL = 0x0C;
        #endregion
        #region class : Work
        internal class Work(int ID, byte[] Data)
        {
            public int MessageID { get; private set; } = ID;
            public byte[] Data { get; private set; } = Data;

            public int? RepeatCount { get; set; } = null;
            public int? Timeout { get; set; } = null;
        }
        #endregion
        #region class : EventArgs
        /// <summary>
        /// 워드 디바이스 데이터 읽기 응답 수신 시 발생하는 이벤트 인자
        /// </summary>
        /// <param name="ID">메시지 식별자</param>
        /// <param name="Slave">슬레이브 국번</param>
        /// <param name="Func">실행된 MC 기능</param>
        /// <param name="Data">읽어온 워드 데이터 배열</param>
        public class WordDataReadEventArgs(int ID, int Slave, MCFunc Func, int[] Data) : EventArgs
        {
            /// <summary>메시지 식별자를 가져옵니다.</summary>
            public int MessageID { get; private set; } = ID;
            /// <summary>슬레이브 국번을 가져옵니다.</summary>
            public int Slave { get; private set; } = Slave;
            /// <summary>실행된 MC 기능을 가져옵니다.</summary>
            public MCFunc Function { get; private set; } = Func;
            /// <summary>읽어온 워드 데이터 배열을 가져옵니다.</summary>
            public int[] Data { get; private set; } = Data;
        }
        /// <summary>
        /// 비트 디바이스 데이터 읽기 응답 수신 시 발생하는 이벤트 인자
        /// </summary>
        /// <param name="ID">메시지 식별자</param>
        /// <param name="Slave">슬레이브 국번</param>
        /// <param name="Func">실행된 MC 기능</param>
        /// <param name="Data">읽어온 비트 데이터 배열</param>
        public class BitDataReadEventArgs(int ID, int Slave, MCFunc Func, bool[] Data) : EventArgs
        {
            /// <summary>메시지 식별자를 가져옵니다.</summary>
            public int MessageID { get; private set; } = ID;
            /// <summary>슬레이브 국번을 가져옵니다.</summary>
            public int Slave { get; private set; } = Slave;
            /// <summary>실행된 MC 기능을 가져옵니다.</summary>
            public MCFunc Function { get; private set; } = Func;
            /// <summary>읽어온 비트 데이터 배열을 가져옵니다.</summary>
            public bool[] Data { get; private set; } = Data;
        }
        /// <summary>
        /// 쓰기 응답 수신 시 발생하는 이벤트 인자
        /// </summary>
        /// <param name="ID">메시지 식별자</param>
        /// <param name="Slave">슬레이브 국번</param>
        /// <param name="Func">실행된 MC 기능</param>
        public class WriteEventArgs(int ID, int Slave, MCFunc Func) : EventArgs
        {
            /// <summary>메시지 식별자를 가져옵니다.</summary>
            public int MessageID { get; private set; } = ID;
            /// <summary>슬레이브 국번을 가져옵니다.</summary>
            public int Slave { get; private set; } = Slave;
            /// <summary>실행된 MC 기능을 가져옵니다.</summary>
            public MCFunc Function { get; private set; } = Func;
        }
        /// <summary>
        /// 통신 타임아웃 발생 시의 이벤트 인자
        /// </summary>
        /// <param name="ID">메시지 식별자</param>
        /// <param name="Slave">슬레이브 국번</param>
        /// <param name="Func">실행된 MC 기능</param>
        public class TimeoutEventArgs(int ID, int Slave, MCFunc Func) : EventArgs
        {
            /// <summary>메시지 식별자를 가져옵니다.</summary>
            public int MessageID { get; private set; } = ID;
            /// <summary>슬레이브 국번을 가져옵니다.</summary>
            public int Slave { get; private set; } = Slave;
            /// <summary>실행된 MC 기능을 가져옵니다.</summary>
            public MCFunc Function { get; private set; } = Func;
        }
        /// <summary>
        /// 체크섬 오류 발생 시의 이벤트 인자
        /// </summary>
        /// <param name="ID">메시지 식별자</param>
        /// <param name="Slave">슬레이브 국번</param>
        /// <param name="Func">실행된 MC 기능</param>
        public class CheckSumErrorEventArgs(int ID, int Slave, MCFunc Func) : EventArgs
        {
            /// <summary>메시지 식별자를 가져옵니다.</summary>
            public int MessageID { get; private set; } = ID;
            /// <summary>슬레이브 국번을 가져옵니다.</summary>
            public int Slave { get; private set; } = Slave;
            /// <summary>실행된 MC 기능을 가져옵니다.</summary>
            public MCFunc Function { get; private set; } = Func;
        }
        /// <summary>
        /// PLC에서 NAK(부정 응답) 수신 시의 이벤트 인자
        /// </summary>
        /// <param name="ID">메시지 식별자</param>
        /// <param name="Slave">슬레이브 국번</param>
        /// <param name="Func">실행된 MC 기능</param>
        public class NakErrorEventArgs(int ID, int Slave, MCFunc Func) : EventArgs
        {
            /// <summary>메시지 식별자를 가져옵니다.</summary>
            public int MessageID { get; private set; } = ID;
            /// <summary>슬레이브 국번을 가져옵니다.</summary>
            public int Slave { get; private set; } = Slave;
            /// <summary>실행된 MC 기능을 가져옵니다.</summary>
            public MCFunc Function { get; private set; } = Func;
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

        /// <summary>CR/LF 제어 시퀀스 사용 여부를 가져오거나 설정합니다. 기본값은 false입니다.</summary>
        public bool UseControlSequence { get; set; } = false;
        /// <summary>체크섬 사용 여부를 가져오거나 설정합니다. 기본값은 false입니다.</summary>
        public bool UseCheckSum { get; set; } = false;

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
        /// <summary>PLC로부터 워드 디바이스 데이터 읽기 응답을 수신했을 때 발생합니다.</summary>
        public event EventHandler<WordDataReadEventArgs>? WordDataReceived;
        /// <summary>PLC로부터 비트 디바이스 데이터 읽기 응답을 수신했을 때 발생합니다.</summary>
        public event EventHandler<BitDataReadEventArgs>? BitDataReceived;
        /// <summary>PLC에 쓰기 완료 응답을 수신했을 때 발생합니다.</summary>
        public event EventHandler<WriteEventArgs>? WriteResponseReceived;
        /// <summary>응답 대기 타임아웃이 발생했을 때 발생합니다.</summary>
        public event EventHandler<TimeoutEventArgs>? TimeoutReceived;
        /// <summary>체크섬 검증 오류가 발생했을 때 발생합니다.</summary>
        public event EventHandler<CheckSumErrorEventArgs>? CheckSumErrorReceived;
        /// <summary>PLC로부터 NAK(부정 응답)을 수신했을 때 발생합니다.</summary>
        public event EventHandler<NakErrorEventArgs>? NakErrorReceived;

        /// <summary>시리얼 포트가 열렸을 때 발생합니다.</summary>
        public event EventHandler? DeviceOpened;
        /// <summary>시리얼 포트가 닫혔을 때 발생합니다.</summary>
        public event EventHandler? DeviceClosed;
        #endregion

        #region Construct
        /// <summary>
        /// MC 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        public MC()
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
                        catch (IOException) { throw new SchedulerStopException(); }
                        catch (UnauthorizedAccessException) { throw new SchedulerStopException(); }
                        catch (InvalidOperationException) { throw new SchedulerStopException(); }
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
                            catch (IOException) { throw new SchedulerStopException(); }
                            catch (UnauthorizedAccessException) { throw new SchedulerStopException(); }
                            catch (InvalidOperationException) { throw new SchedulerStopException(); }

                            #region CollectCompleteCheck
                            string Func = Encoding.ASCII.GetString(w.Data, 5, 2);
                            if (Func == "WR" || Func == "BR")
                            {
                                int nLastOffset = UseCheckSum ? 3 : 1;
                                if (nRecv > 0 && nRecv - nLastOffset >= 0)
                                    if (baResponse[nRecv - nLastOffset] == ETX)
                                        bCollecting = false;
                            }
                            else
                            {
                                if (nRecv >= 5) bCollecting = false;
                            }
                            #endregion

                            gap = DateTime.Now - prev;
                            if (gap.TotalMilliseconds >= Timeout) break;
                        }
                        #endregion

                        #region parse
                        if (gap.TotalMilliseconds < Timeout)
                        {
                            if (nRecv > 7)
                            {
                                try
                                {
                                    #region Proc
                                    string Func = Encoding.ASCII.GetString(w.Data, 5, 2);
                                    switch (Func)
                                    {
                                        #region BR
                                        case "BR":
                                            if (UseCheckSum)
                                            {
                                                #region Set
                                                string Data = Encoding.ASCII.GetString(baResponse, 5, nRecv - (8 + (UseControlSequence ? 2 : 0)));
                                                string CheckSum = Encoding.ASCII.GetString(baResponse, nRecv - (2 + (UseControlSequence ? 2 : 0)), 2);
                                                #endregion
                                                #region Calc Checksum
                                                int nChksum = 0;
                                                for (int i = 1; i < nRecv; i++)
                                                {
                                                    nChksum += baResponse[i];
                                                    if (baResponse[i] == ETX) break;
                                                }
                                                nChksum = nChksum & 0xFF;
                                                string CalcCheckSum = nChksum.ToString("X2");
                                                #endregion

                                                if (CalcCheckSum == CheckSum)
                                                {
                                                    #region BitDataReceived
                                                    if (BitDataReceived != null)
                                                    {
                                                        string strSlave = new string(new char[] { (char)w.Data[1], (char)w.Data[2] });
                                                        string strFunc = new string(new char[] { (char)w.Data[5], (char)w.Data[6] });
                                                        int slave = int.Parse(strSlave, System.Globalization.NumberStyles.AllowHexSpecifier);
                                                        MCFunc func = StringToFunc(strFunc);

                                                        bool[] realData = new bool[Data.Length];
                                                        for (int i = 0; i < realData.Length; i++) realData[i] = (Data[i] == '1');

                                                        if (BitDataReceived != null)
                                                            BitDataReceived.Invoke(this, new BitDataReadEventArgs(w.MessageID, slave, func, realData));
                                                    }
                                                    #endregion
                                                }
                                                else
                                                {
                                                    #region Checksum Error
                                                    if (CheckSumErrorReceived != null)
                                                    {
                                                        string strSlave = new string(new char[] { (char)w.Data[1], (char)w.Data[2] });
                                                        string strFunc = new string(new char[] { (char)w.Data[5], (char)w.Data[6] });
                                                        int slave = Convert.ToInt32(strSlave, 16);
                                                        MCFunc func = StringToFunc(strFunc);
                                                        if (CheckSumErrorReceived != null)
                                                            CheckSumErrorReceived.Invoke(this, new CheckSumErrorEventArgs(w.MessageID, slave, func));
                                                    }
                                                    #endregion
                                                }
                                            }
                                            else
                                            {
                                                #region BitDataReceived
                                                string Data = Encoding.ASCII.GetString(baResponse, 5, nRecv - (6 + (UseControlSequence ? 2 : 0)));
                                                if (BitDataReceived != null)
                                                {
                                                    string strSlave = new string(new char[] { (char)w.Data[1], (char)w.Data[2] });
                                                    string strFunc = new string(new char[] { (char)w.Data[5], (char)w.Data[6] });
                                                    int slave = int.Parse(strSlave, System.Globalization.NumberStyles.AllowHexSpecifier);
                                                    MCFunc func = StringToFunc(strFunc);

                                                    bool[] realData = new bool[Data.Length];
                                                    for (int i = 0; i < realData.Length; i++)
                                                        realData[i] = (Data[i] == '1');

                                                    if (BitDataReceived != null)
                                                        BitDataReceived.Invoke(this, new BitDataReadEventArgs(w.MessageID, slave, func, realData));
                                                }
                                                #endregion
                                            }
                                            break;
                                        #endregion
                                        #region WR
                                        case "WR":
                                            if (UseCheckSum)
                                            {
                                                #region Set
                                                string Data = Encoding.ASCII.GetString(baResponse, 5, nRecv - (8 + (UseControlSequence ? 2 : 0)));
                                                string CheckSum = Encoding.ASCII.GetString(baResponse, nRecv - (2 + (UseControlSequence ? 2 : 0)), 2);
                                                #endregion
                                                #region Calc Checksum
                                                int nChksum = 0;
                                                for (int i = 1; i < nRecv; i++)
                                                {
                                                    nChksum += baResponse[i];
                                                    if (baResponse[i] == ETX) break;
                                                }
                                                nChksum = nChksum & 0xFF;
                                                string CalcCheckSum = nChksum.ToString("X2");
                                                #endregion
                                                if (CalcCheckSum == CheckSum)
                                                {
                                                    #region WordDataReceived
                                                    if (WordDataReceived != null)
                                                    {
                                                        string strSlave = new string(new char[] { (char)w.Data[1], (char)w.Data[2] });
                                                        string strFunc = new string(new char[] { (char)w.Data[5], (char)w.Data[6] });
                                                        int slave = int.Parse(strSlave, System.Globalization.NumberStyles.AllowHexSpecifier);
                                                        MCFunc func = StringToFunc(strFunc);

                                                        int[] realData = new int[Data.Length / 4];
                                                        for (int i = 0; i < Data.Length; i += 4) realData[i / 4] = Convert.ToInt32(Data.Substring(i, 4), 16);

                                                        if (WordDataReceived != null)
                                                            WordDataReceived.Invoke(this, new WordDataReadEventArgs(w.MessageID, slave, func, realData));
                                                    }
                                                    #endregion
                                                }
                                                else
                                                {
                                                    #region Checksum Error
                                                    if (CheckSumErrorReceived != null)
                                                    {
                                                        string strSlave = new string(new char[] { (char)w.Data[1], (char)w.Data[2] });
                                                        string strFunc = new string(new char[] { (char)w.Data[5], (char)w.Data[6] });
                                                        int slave = int.Parse(strSlave, System.Globalization.NumberStyles.AllowHexSpecifier);
                                                        MCFunc func = StringToFunc(strFunc);
                                                        if (CheckSumErrorReceived != null)
                                                            CheckSumErrorReceived.Invoke(this, new CheckSumErrorEventArgs(w.MessageID, slave, func));
                                                    }
                                                    #endregion
                                                }
                                            }
                                            else
                                            {
                                                #region WordDataReceived
                                                string Data = Encoding.ASCII.GetString(baResponse, 5, nRecv - (6 + (UseControlSequence ? 2 : 0)));
                                                if (WordDataReceived != null)
                                                {
                                                    string strSlave = new string(new char[] { (char)w.Data[1], (char)w.Data[2] });
                                                    string strFunc = new string(new char[] { (char)w.Data[5], (char)w.Data[6] });
                                                    int slave = int.Parse(strSlave, System.Globalization.NumberStyles.AllowHexSpecifier);
                                                    MCFunc func = StringToFunc(strFunc);

                                                    int[] realData = new int[Data.Length / 4];
                                                    for (int i = 0; i < Data.Length; i += 4)
                                                        realData[i / 4] = int.Parse(Data.Substring(i, 4), System.Globalization.NumberStyles.AllowHexSpecifier);

                                                    if (BitDataReceived != null)
                                                        WordDataReceived.Invoke(this, new WordDataReadEventArgs(w.MessageID, slave, func, realData));
                                                }
                                                #endregion
                                            }
                                            break;
                                        #endregion
                                        #region BW/WW
                                        case "BW":
                                        case "WW":
                                            if (baResponse[0] == ACK)
                                            {
                                                #region WriteResponseReceived
                                                if (WriteResponseReceived != null)
                                                {
                                                    string strSlave = new string(new char[] { (char)w.Data[1], (char)w.Data[2] });
                                                    string strFunc = new string(new char[] { (char)w.Data[5], (char)w.Data[6] });
                                                    int slave = int.Parse(strSlave, System.Globalization.NumberStyles.AllowHexSpecifier);
                                                    MCFunc func = StringToFunc(strFunc);
                                                    if (WriteResponseReceived != null)
                                                        WriteResponseReceived.Invoke(this, new WriteEventArgs(w.MessageID, slave, func));
                                                }
                                                #endregion
                                            }
                                            else
                                            {
                                                #region NakErrorReceived
                                                if (NakErrorReceived != null)
                                                {
                                                    string strSlave = new string(new char[] { (char)w.Data[1], (char)w.Data[2] });
                                                    string strFunc = new string(new char[] { (char)w.Data[5], (char)w.Data[6] });
                                                    int slave = int.Parse(strSlave, System.Globalization.NumberStyles.AllowHexSpecifier);
                                                    MCFunc func = StringToFunc(strFunc);
                                                    if (NakErrorReceived != null)
                                                        NakErrorReceived.Invoke(this, new NakErrorEventArgs(w.MessageID, slave, func));
                                                }
                                                #endregion
                                            }
                                            break;
                                            #endregion
                                    }
                                    #endregion
                                }
                                catch (Exception) { }
                            }

                            bRepeat = false;
                        }
                        else
                        {
                            #region Timeout
                            string strSlave = new string(new char[] { (char)w.Data[1], (char)w.Data[2] });
                            string strFunc = new string(new char[] { (char)w.Data[5], (char)w.Data[6] });
                            int slave = int.Parse(strSlave, System.Globalization.NumberStyles.AllowHexSpecifier);
                            MCFunc func = StringToFunc(strFunc);

                            TimeoutReceived?.Invoke(this, new TimeoutEventArgs(w.MessageID, slave, func));
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

        #region AutoBitRead
        /// <summary>
        /// 자동 작업 목록에 비트 디바이스 읽기(BR) 명령을 등록합니다.
        /// </summary>
        /// <param name="id">메시지 식별자</param>
        /// <param name="slave">슬레이브 국번</param>
        /// <param name="device">시작 디바이스 주소 (예: "M100")</param>
        /// <param name="length">읽을 비트 디바이스 개수 (최대 255)</param>
        /// <param name="waitTime">PLC 응답 대기 시간 (0~15)</param>
        /// <param name="timeout">응답 대기 타임아웃(밀리초). null이면 기본값 사용</param>
        public void AutoBitRead(int id, int slave, string device, int length, int waitTime = 0, int? timeout = null)
        {
            device = device.Substring(0, 1) + Convert.ToInt32(device.Substring(1)).ToString("0000");

            if (waitTime > 15) waitTime = 15;
            if (length > 255) length = 255;
            if (slave > 255) slave = 255;

            StringBuilder strbul = new StringBuilder();
            strbul.Append((char)ENQ);
            strbul.Append(slave.ToString("X2"));
            strbul.Append("FF");
            strbul.Append("BR");
            strbul.Append(waitTime.ToString("X"));
            strbul.Append(device);
            strbul.Append(length.ToString("X2"));
            if (UseCheckSum) strbul.Append("00");
            if (UseControlSequence) strbul.Append("\r\n");

            byte[] data = Encoding.ASCII.GetBytes(strbul.ToString());

            if (UseCheckSum)
            {
                int CheckSum = 0;
                int lastIndex = data.Length - 1;
                if (UseCheckSum) lastIndex -= 2;
                if (UseControlSequence) lastIndex -= 2;

                for (int i = 1; i <= lastIndex; i++) CheckSum += data[i];
                CheckSum &= 0xFF;
                string bcc = CheckSum.ToString("X2");
                if (UseControlSequence)
                {
                    data[data.Length - 4] = (byte)bcc[0];
                    data[data.Length - 3] = (byte)bcc[1];
                }
                else
                {
                    data[data.Length - 2] = (byte)bcc[0];
                    data[data.Length - 1] = (byte)bcc[1];
                }
            }
            AutoWorkList.Add(new Work(id, data) { Timeout = timeout });
        }
        #endregion
        #region AutoWordRead
        /// <summary>
        /// 자동 작업 목록에 워드 디바이스 읽기(WR) 명령을 등록합니다.
        /// </summary>
        /// <param name="id">메시지 식별자</param>
        /// <param name="slave">슬레이브 국번</param>
        /// <param name="device">시작 디바이스 주소 (예: "D100")</param>
        /// <param name="length">읽을 워드 디바이스 개수 (최대 255)</param>
        /// <param name="waitTime">PLC 응답 대기 시간 (0~15)</param>
        /// <param name="timeout">응답 대기 타임아웃(밀리초). null이면 기본값 사용</param>
        public void AutoWordRead(int id, int slave, string device, int length, int waitTime = 0, int? timeout = null)
        {
            device = device.Substring(0, 1) + Convert.ToInt32(device.Substring(1)).ToString("0000");

            if (waitTime > 15) waitTime = 15;
            if (length > 255) length = 255;
            if (slave > 255) slave = 255;

            StringBuilder strbul = new StringBuilder();
            strbul.Append((char)ENQ);
            strbul.Append(slave.ToString("X2"));
            strbul.Append("FF");
            strbul.Append("WR");
            strbul.Append(waitTime.ToString("X"));
            strbul.Append(device);
            strbul.Append(length.ToString("X2"));
            if (UseCheckSum) strbul.Append("00");
            if (UseControlSequence) strbul.Append("\r\n");

            byte[] data = Encoding.ASCII.GetBytes(strbul.ToString());

            if (UseCheckSum)
            {
                int CheckSum = 0;
                int lastIndex = data.Length - 1;
                if (UseCheckSum) lastIndex -= 2;
                if (UseControlSequence) lastIndex -= 2;

                for (int i = 1; i <= lastIndex; i++) CheckSum += data[i];
                CheckSum &= 0xFF;
                string bcc = CheckSum.ToString("X2");

                if (UseControlSequence)
                {
                    data[data.Length - 4] = (byte)bcc[0];
                    data[data.Length - 3] = (byte)bcc[1];
                }
                else
                {
                    data[data.Length - 2] = (byte)bcc[0];
                    data[data.Length - 1] = (byte)bcc[1];
                }
            }

            AutoWorkList.Add(new Work(id, data) { Timeout = timeout });
        }
        #endregion
        #region ManualBitRead
        /// <summary>
        /// 수동 작업 목록에 비트 디바이스 읽기(BR) 명령을 등록합니다. 일회성으로 실행됩니다.
        /// </summary>
        /// <param name="id">메시지 식별자</param>
        /// <param name="slave">슬레이브 국번</param>
        /// <param name="device">시작 디바이스 주소 (예: "M100")</param>
        /// <param name="length">읽을 비트 디바이스 개수 (최대 255)</param>
        /// <param name="waitTime">PLC 응답 대기 시간 (0~15)</param>
        /// <param name="repeatCount">타임아웃 시 재시도 횟수. null이면 재시도하지 않음</param>
        /// <param name="timeout">응답 대기 타임아웃(밀리초). null이면 기본값 사용</param>
        public void ManualBitRead(int id, int slave, string device, int length, int waitTime = 0, int? repeatCount = null, int? timeout = null)
        {
            device = device.Substring(0, 1) + Convert.ToInt32(device.Substring(1)).ToString("0000");

            if (waitTime > 15) waitTime = 15;
            if (length > 255) length = 255;
            if (slave > 255) slave = 255;

            StringBuilder strbul = new StringBuilder();
            strbul.Append((char)ENQ);
            strbul.Append(slave.ToString("X2"));
            strbul.Append("FF");
            strbul.Append("BR");
            strbul.Append(waitTime.ToString("X"));
            strbul.Append(device);
            strbul.Append(length.ToString("X2"));
            if (UseCheckSum) strbul.Append("00");
            if (UseControlSequence) strbul.Append("\r\n");

            byte[] data = Encoding.ASCII.GetBytes(strbul.ToString());

            if (UseCheckSum)
            {
                int CheckSum = 0;
                int lastIndex = data.Length - 1;
                if (UseCheckSum) lastIndex -= 2;
                if (UseControlSequence) lastIndex -= 2;

                for (int i = 1; i <= lastIndex; i++) CheckSum += data[i];
                CheckSum &= 0xFF;
                string bcc = CheckSum.ToString("X2");

                if (UseControlSequence)
                {
                    data[data.Length - 4] = (byte)bcc[0];
                    data[data.Length - 3] = (byte)bcc[1];
                }
                else
                {
                    data[data.Length - 2] = (byte)bcc[0];
                    data[data.Length - 1] = (byte)bcc[1];
                }
            }

            ManualWorkList.Add(new Work(id, data) { RepeatCount = repeatCount, Timeout = timeout });
        }
        #endregion
        #region ManualBitWrite
        /// <summary>
        /// 수동 작업 목록에 단일 비트 디바이스 쓰기(BW) 명령을 등록합니다. 일회성으로 실행됩니다.
        /// </summary>
        /// <param name="id">메시지 식별자</param>
        /// <param name="slave">슬레이브 국번</param>
        /// <param name="device">쓰기 대상 디바이스 주소</param>
        /// <param name="value">쓰기할 비트 값</param>
        /// <param name="waitTime">PLC 응답 대기 시간 (0~15)</param>
        /// <param name="repeatCount">타임아웃 시 재시도 횟수. null이면 재시도하지 않음</param>
        /// <param name="timeout">응답 대기 타임아웃(밀리초). null이면 기본값 사용</param>
        public void ManualBitWrite(int id, int slave, string device, bool value, int waitTime = 0, int? repeatCount = null, int? timeout = null) => ManualBitWrite(id, slave, device, new bool[] { value }, waitTime, repeatCount, timeout);
        /// <summary>
        /// 수동 작업 목록에 복수 비트 디바이스 쓰기(BW) 명령을 등록합니다. 일회성으로 실행됩니다.
        /// </summary>
        /// <param name="id">메시지 식별자</param>
        /// <param name="slave">슬레이브 국번</param>
        /// <param name="device">시작 디바이스 주소</param>
        /// <param name="value">쓰기할 비트 값 배열</param>
        /// <param name="waitTime">PLC 응답 대기 시간 (0~15)</param>
        /// <param name="repeatCount">타임아웃 시 재시도 횟수. null이면 재시도하지 않음</param>
        /// <param name="timeout">응답 대기 타임아웃(밀리초). null이면 기본값 사용</param>
        public void ManualBitWrite(int id, int slave, string device, bool[] value, int waitTime = 0, int? repeatCount = null, int? timeout = null)
        {
            device = device.Substring(0, 1) + Convert.ToInt32(device.Substring(1)).ToString("0000");
            int Length = value.Length;
            string strValue = "";
            for (int i = 0; i < value.Length; i++) strValue += value[i] ? "1" : "0";

            if (waitTime > 15) waitTime = 15;
            if (Length > 255) Length = 255;
            if (slave > 255) slave = 255;

            StringBuilder strbul = new StringBuilder();
            strbul.Append((char)ENQ);
            strbul.Append(slave.ToString("X2"));
            strbul.Append("FF");
            strbul.Append("BW");
            strbul.Append(waitTime.ToString("X"));
            strbul.Append(device);
            strbul.Append(Length.ToString("X2"));
            strbul.Append(strValue);

            if (UseCheckSum) strbul.Append("00");
            if (UseControlSequence) strbul.Append("\r\n");

            byte[] data = Encoding.ASCII.GetBytes(strbul.ToString());

            if (UseCheckSum)
            {
                int CheckSum = 0;
                int lastIndex = data.Length - 1;
                if (UseCheckSum) lastIndex -= 2;
                if (UseControlSequence) lastIndex -= 2;

                for (int i = 1; i <= lastIndex; i++) CheckSum += data[i];
                CheckSum &= 0xFF;
                string bcc = CheckSum.ToString("X2");
                if (UseControlSequence)
                {
                    data[data.Length - 4] = (byte)bcc[0];
                    data[data.Length - 3] = (byte)bcc[1];
                }
                else
                {
                    data[data.Length - 2] = (byte)bcc[0];
                    data[data.Length - 1] = (byte)bcc[1];
                }
            }

            ManualWorkList.Add(new Work(id, data) { RepeatCount = repeatCount, Timeout = timeout });
        }
        #endregion
        #region ManualWordRead
        /// <summary>
        /// 수동 작업 목록에 워드 디바이스 읽기(WR) 명령을 등록합니다. 일회성으로 실행됩니다.
        /// </summary>
        /// <param name="id">메시지 식별자</param>
        /// <param name="slave">슬레이브 국번</param>
        /// <param name="device">시작 디바이스 주소 (예: "D100")</param>
        /// <param name="length">읽을 워드 디바이스 개수 (최대 255)</param>
        /// <param name="waitTime">PLC 응답 대기 시간 (0~15)</param>
        /// <param name="repeatCount">타임아웃 시 재시도 횟수. null이면 재시도하지 않음</param>
        /// <param name="timeout">응답 대기 타임아웃(밀리초). null이면 기본값 사용</param>
        public void ManualWordRead(int id, int slave, string device, int length, int waitTime = 0, int? repeatCount = null, int? timeout = null)
        {
            device = device.Substring(0, 1) + Convert.ToInt32(device.Substring(1)).ToString("0000");

            if (waitTime > 15) waitTime = 15;
            if (length > 255) length = 255;
            if (slave > 255) slave = 255;

            StringBuilder strbul = new StringBuilder();
            strbul.Append((char)ENQ);
            strbul.Append(slave.ToString("X2"));
            strbul.Append("FF");
            strbul.Append("WR");
            strbul.Append(waitTime.ToString("X"));
            strbul.Append(device);
            strbul.Append(length.ToString("X2"));

            if (UseCheckSum) strbul.Append("00");
            if (UseControlSequence) strbul.Append("\r\n");

            byte[] data = Encoding.ASCII.GetBytes(strbul.ToString());

            if (UseCheckSum)
            {
                int CheckSum = 0;
                int lastIndex = data.Length - 1;
                if (UseCheckSum) lastIndex -= 2;
                if (UseControlSequence) lastIndex -= 2;

                for (int i = 1; i <= lastIndex; i++) CheckSum += data[i];
                CheckSum &= 0xFF;
                string bcc = CheckSum.ToString("X2");
                if (UseControlSequence)
                {
                    data[data.Length - 4] = (byte)bcc[0];
                    data[data.Length - 3] = (byte)bcc[1];
                }
                else
                {
                    data[data.Length - 2] = (byte)bcc[0];
                    data[data.Length - 1] = (byte)bcc[1];
                }
            }

            ManualWorkList.Add(new Work(id, data) { RepeatCount = repeatCount, Timeout = timeout });
        }
        #endregion
        #region ManualWordWrite
        /// <summary>
        /// 수동 작업 목록에 단일 워드 디바이스 쓰기(WW) 명령을 등록합니다. 일회성으로 실행됩니다.
        /// </summary>
        /// <param name="id">메시지 식별자</param>
        /// <param name="slave">슬레이브 국번</param>
        /// <param name="device">쓰기 대상 디바이스 주소</param>
        /// <param name="value">쓰기할 워드 값</param>
        /// <param name="waitTime">PLC 응답 대기 시간 (0~15)</param>
        /// <param name="repeatCount">타임아웃 시 재시도 횟수. null이면 재시도하지 않음</param>
        /// <param name="timeout">응답 대기 타임아웃(밀리초). null이면 기본값 사용</param>
        public void ManualWordWrite(int id, int slave, string device, int value, int waitTime = 0, int? repeatCount = null, int? timeout = null) { ManualWordWrite(id, slave, device, new int[] { value }, waitTime, repeatCount, timeout); }
        /// <summary>
        /// 수동 작업 목록에 복수 워드 디바이스 쓰기(WW) 명령을 등록합니다. 일회성으로 실행됩니다.
        /// </summary>
        /// <param name="id">메시지 식별자</param>
        /// <param name="slave">슬레이브 국번</param>
        /// <param name="device">시작 디바이스 주소</param>
        /// <param name="value">쓰기할 워드 값 배열</param>
        /// <param name="waitTime">PLC 응답 대기 시간 (0~15)</param>
        /// <param name="repeatCount">타임아웃 시 재시도 횟수. null이면 재시도하지 않음</param>
        /// <param name="timeout">응답 대기 타임아웃(밀리초). null이면 기본값 사용</param>
        public void ManualWordWrite(int id, int slave, string device, int[] value, int waitTime = 0, int? repeatCount = null, int? timeout = null)
        {
            device = device.Substring(0, 1) + Convert.ToInt32(device.Substring(1)).ToString("0000");
            int Length = value.Length;
            string strValue = "";
            for (int i = 0; i < value.Length; i++) strValue += value[i].ToString("X4");

            if (waitTime > 15) waitTime = 15;
            if (Length > 255) Length = 255;
            if (slave > 255) slave = 255;

            StringBuilder strbul = new StringBuilder();
            strbul.Append((char)ENQ);
            strbul.Append(slave.ToString("X2"));
            strbul.Append("FF");
            strbul.Append("WW");
            strbul.Append(waitTime.ToString("X"));
            strbul.Append(device);
            strbul.Append(Length.ToString("X2"));
            strbul.Append(strValue);

            if (UseCheckSum) strbul.Append("00");
            if (UseControlSequence) strbul.Append("\r\n");

            byte[] data = Encoding.ASCII.GetBytes(strbul.ToString());

            if (UseCheckSum)
            {
                int CheckSum = 0;
                int lastIndex = data.Length - 1;
                if (UseCheckSum) lastIndex -= 2;
                if (UseControlSequence) lastIndex -= 2;

                for (int i = 1; i <= lastIndex; i++)
                {
                    CheckSum += data[i];
                }
                CheckSum &= 0xFF;
                string bcc = CheckSum.ToString("X2");
                if (UseControlSequence)
                {
                    data[data.Length - 4] = (byte)bcc[0];
                    data[data.Length - 3] = (byte)bcc[1];
                }
                else
                {
                    data[data.Length - 2] = (byte)bcc[0];
                    data[data.Length - 1] = (byte)bcc[1];
                }
            }

            ManualWorkList.Add(new Work(id, data) { RepeatCount = repeatCount, Timeout = timeout });
        }
        #endregion
        #endregion

        #region Static Method
        internal static string FuncToString(MCFunc func)
        {
            string ret = "";
            switch (func)
            {
                case MCFunc.BitRead: ret = "BR"; break;
                case MCFunc.BitWrite: ret = "BW"; break;
                case MCFunc.WordRead: ret = "WR"; break;
                case MCFunc.WordWrite: ret = "WW"; break;
            }
            return ret;
        }
        internal static MCFunc StringToFunc(string func)
        {
            MCFunc ret = MCFunc.None;
            switch (func)
            {
                case "BR": ret = MCFunc.BitRead; break;
                case "BW": ret = MCFunc.BitWrite; break;
                case "WR": ret = MCFunc.WordRead; break;
                case "WW": ret = MCFunc.WordWrite; break;
            }
            return ret;
        }
        #endregion
    }
}
