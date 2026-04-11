using Going.Basis.Communications.LS;
using Going.Basis.Communications.Modbus;
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

namespace Going.Basis.Communications.TextComm.TCP
{
    /// <summary>
    /// TCP 기반 STX/ETX 텍스트 통신 마스터 클래스.
    /// 자동(Auto) 반복 폴링과 수동(Manual) 일회성 작업 큐를 관리하며,
    /// 슬레이브에 요청을 보내고 응답을 수신합니다.
    /// </summary>
    public class TextCommTCPMaster
    {
        #region class : Work
        /// <summary>
        /// 텍스트 통신 작업 항목을 나타내는 클래스
        /// </summary>
        /// <param name="id">메시지 식별자</param>
        /// <param name="data">전송할 패킷 바이트 배열</param>
        /// <param name="slave">슬레이브 ID</param>
        /// <param name="command">명령 코드</param>
        /// <param name="message">메시지 문자열</param>
        public class Work(int id, byte[] data, byte slave, byte command, string message)
        {
            /// <summary>메시지 식별자를 가져옵니다.</summary>
            public int MessageID { get; private set; } = id;
            /// <summary>전송할 패킷 바이트 배열을 가져옵니다.</summary>
            public byte[] Data { get; private set; } = data;
            /// <summary>타임아웃 시 재시도 횟수를 가져오거나 설정합니다.</summary>
            public int? RepeatCount { get; set; } = null;
            /// <summary>응답 대기 타임아웃(밀리초)을 가져오거나 설정합니다.</summary>
            public int? Timeout { get; set; } = null;

            /// <summary>슬레이브 ID를 가져옵니다.</summary>
            public byte Slave { get; private set; } = slave;
            /// <summary>명령 코드를 가져옵니다.</summary>
            public byte Command { get; private set; } = command;
            /// <summary>메시지 문자열을 가져옵니다.</summary>
            public string Message { get; private set; } = message;
        }
        #endregion
        #region class : EventArgs
        /// <summary>
        /// 슬레이브로부터 응답 메시지를 수신했을 때의 이벤트 인자
        /// </summary>
        /// <param name="workItem">요청 작업 항목</param>
        /// <param name="message">수신된 응답 메시지</param>
        public class ReceivedEventArgs(Work workItem, string message) : EventArgs
        {
            /// <summary>메시지 식별자를 가져옵니다.</summary>
            public int MessageID => workItem.MessageID;
            /// <summary>슬레이브 ID를 가져옵니다.</summary>
            public byte Slave => workItem.Slave;
            /// <summary>명령 코드를 가져옵니다.</summary>
            public byte Command => workItem.Command;
            /// <summary>수신된 응답 메시지를 가져옵니다.</summary>
            public string Message { get; private set; } = message;
        }

        /// <summary>
        /// 통신 타임아웃 발생 시의 이벤트 인자
        /// </summary>
        /// <param name="workItem">타임아웃이 발생한 작업 항목</param>
        public class TimeoutEventArgs(Work workItem) : EventArgs
        {
            /// <summary>메시지 식별자를 가져옵니다.</summary>
            public int MessageID => workItem.MessageID;
            /// <summary>슬레이브 ID를 가져옵니다.</summary>
            public byte Slave => workItem.Slave;
            /// <summary>명령 코드를 가져옵니다.</summary>
            public byte Command => workItem.Command;
        }
        #endregion

        #region Properties
        /// <summary>연결 대상 원격 서버의 IP 주소를 가져오거나 설정합니다. 기본값은 "127.0.0.1"입니다.</summary>
        public string RemoteIP { get; set; } = "127.0.0.1";
        /// <summary>연결 대상 원격 서버의 포트 번호를 가져오거나 설정합니다. 기본값은 7897입니다.</summary>
        public int RemotePort { get; set; } = 7897;

        /// <summary>응답 대기 타임아웃 시간(밀리초)을 가져오거나 설정합니다. 기본값은 100입니다.</summary>
        public int Timeout { get; set; } = 100;
        /// <summary>통신 폴링 간격(밀리초)을 가져오거나 설정합니다. 기본값은 10입니다.</summary>
        public int Interval { get; set; } = 10;
        /// <summary>수신 버퍼 크기(바이트)를 가져오거나 설정합니다. 기본값은 1024입니다.</summary>
        public int BufferSize { get; set; } = 1024;
        /// <summary>메시지 문자열의 인코딩을 가져오거나 설정합니다. 기본값은 ASCII입니다.</summary>
        public Encoding MessageEncoding { get; set; } = Encoding.ASCII;

        /// <summary>TCP 소켓이 연결되어 있는지 여부를 가져옵니다.</summary>
        public bool IsOpen => bIsOpen;
        /// <summary>통신 스케줄러가 실행 중인지 여부를 가져옵니다.</summary>
        public bool IsStart { get; private set; }
        /// <summary>연결 끊김 시 자동 재연결 여부를 가져오거나 설정합니다. 기본값은 true입니다.</summary>
        public bool AutoReconnect { get; set; } = true;

        /// <summary>사용자 정의 태그 데이터를 가져오거나 설정합니다.</summary>
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
        /// <summary>슬레이브로부터 응답 메시지를 수신했을 때 발생합니다.</summary>
        public event EventHandler<ReceivedEventArgs>? MessageReceived;
        /// <summary>응답 대기 타임아웃이 발생했을 때 발생합니다.</summary>
        public event EventHandler<TimeoutEventArgs>? TimeoutReceived;

        /// <summary>TCP 소켓이 연결되었을 때 발생합니다.</summary>
        public event EventHandler<SocketEventArgs>? SocketConnected;
        /// <summary>TCP 소켓 연결이 끊어졌을 때 발생합니다.</summary>
        public event EventHandler<SocketEventArgs>? SocketDisconnected;
        #endregion

        #region Construct
        /// <summary>
        /// TextCommTCPMaster 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        public TextCommTCPMaster()
        {

        }
        #endregion

        #region Method
        #region Start / Stop
        /// <summary>
        /// TCP 소켓을 연결하고 통신 스케줄러를 시작합니다.
        /// 자동 작업 큐에 등록된 명령을 반복 실행하며, 수동 작업을 우선 처리합니다.
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
        /// 통신 스케줄러를 정지하고 TCP 소켓을 닫습니다.
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
                if (client != null)
                {
                    #region Manual Fill
                    if (ManualWorkList.Count > 0)
                    {
                        for (int i = 0; i < ManualWorkList.Count; i++) WorkQueue.Enqueue(ManualWorkList[i]);
                        ManualWorkList.Clear();
                    }
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
                                client?.Send(w.Data);
                            }
                            catch (SocketException ex)
                            {
                                if (ex.SocketErrorCode == SocketError.TimedOut) { }
                                else if (ex.SocketErrorCode == SocketError.ConnectionReset) { bIsOpen = false; }
                                else if (ex.SocketErrorCode == SocketError.ConnectionAborted) { bIsOpen = false; }
                                else if (ex.SocketErrorCode == SocketError.Shutdown) { bIsOpen = false; }
                            }
                            catch { }
                            if (!IsOpen) throw new SchedulerStopException();
                            #endregion

                            #region read
                            var lsResponse = new List<byte>();
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

                                        len = client.Receive(baResponse, 0, baResponse.Length, SocketFlags.None);
                                        for (int i = 0; i < len; i++)
                                        {
                                            var v = baResponse[i];
                                            if (v == 0x02) lsResponse.Clear();
                                            lsResponse.Add(v);
                                        }
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
                                catch { }

                                if (!IsOpen) throw new SchedulerStopException();

                                if (lsResponse.Count >= 2 && lsResponse.FirstOrDefault() == 0x02 && lsResponse.LastOrDefault() == 0x03) bCollecting = false;

                                gap = DateTime.Now - prev;
                                if (gap.TotalMilliseconds >= Timeout) break;
                            }
                            #endregion

                            #region parse
                            if (gap.TotalMilliseconds < Timeout)
                            {
                                var ls = TextCommPacket.ParsePacket(lsResponse.ToArray())?.ToList();
                                if (ls != null)
                                {
                                    byte sum = (byte)(ls.GetRange(0, ls.Count - 1).Select(x => (int)x).Sum() & 0xFF);

                                    if (sum == ls[^1])
                                    {
                                        byte slave = ls[0];
                                        byte cmd = ls[1];
                                        if (slave == w.Slave && cmd == w.Command && ls.Count >= 3)
                                        {
                                            var msg = MessageEncoding.GetString(ls.GetRange(2, ls.Count - 3).ToArray());
                                            MessageReceived?.Invoke(this, new ReceivedEventArgs(w, msg));
                                        }
                                    }
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

        #region Auto
        /// <summary>
        /// 자동 작업 목록에 메시지 전송 명령을 등록합니다. 반복적으로 실행됩니다.
        /// </summary>
        /// <param name="MessageID">메시지 식별자</param>
        /// <param name="Slave">슬레이브 ID</param>
        /// <param name="Command">명령 코드</param>
        /// <param name="Message">전송할 메시지 문자열</param>
        /// <param name="timeout">응답 대기 타임아웃(밀리초). null이면 기본값 사용</param>
        public void AutoSend(int MessageID, byte Slave, byte Command, string Message, int? timeout = null)
        {
            var ba = TextCommPacket.MakePacket(MessageEncoding, Slave, Command, Message);
            AutoWorkList.Add(new Work(MessageID, ba, Slave, Command, Message) { Timeout = timeout });
        }
        #endregion
        #region Manual
        /// <summary>
        /// 수동 작업 목록에 메시지 전송 명령을 등록합니다. 일회성으로 실행됩니다.
        /// </summary>
        /// <param name="MessageID">메시지 식별자</param>
        /// <param name="Slave">슬레이브 ID</param>
        /// <param name="Command">명령 코드</param>
        /// <param name="Message">전송할 메시지 문자열</param>
        /// <param name="repeatCount">타임아웃 시 재시도 횟수. null이면 재시도하지 않음</param>
        /// <param name="timeout">응답 대기 타임아웃(밀리초). null이면 기본값 사용</param>
        public void ManualSend(int MessageID, byte Slave, byte Command, string Message, int? repeatCount = null, int? timeout = null)
        {
            var ba = TextCommPacket.MakePacket(MessageEncoding, Slave, Command, Message);
            ManualWorkList.Add(new Work(MessageID, ba, Slave, Command, Message) { RepeatCount = repeatCount, Timeout = timeout });
        }
        #endregion
        #endregion
    }
}
