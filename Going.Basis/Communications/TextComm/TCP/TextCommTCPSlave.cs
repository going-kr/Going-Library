using Going.Basis.Communications.Modbus;
using Going.Basis.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Communications.TextComm.TCP
{
    /// <summary>
    /// TCP 기반 STX/ETX 텍스트 통신 슬레이브 클래스.
    /// TCP 서버로 동작하며, 마스터의 요청을 수신하고 이벤트를 통해 응답을 처리합니다.
    /// </summary>
    public class TextCommTCPSlave
    {
        #region class : EventArgs
        /// <summary>
        /// 마스터로부터 메시지 요청을 수신했을 때의 이벤트 인자.
        /// ResponseMessage에 응답을 설정하면 마스터에게 자동으로 전송됩니다.
        /// </summary>
        /// <param name="Slave">슬레이브 ID</param>
        /// <param name="Command">명령 코드</param>
        /// <param name="Message">요청 메시지 문자열</param>
        public class MessageRequestArgs(int Slave, int Command, string Message) : EventArgs
        {
            /// <summary>슬레이브 ID를 가져옵니다.</summary>
            public int Slave { get; private set; } = Slave;
            /// <summary>명령 코드를 가져옵니다.</summary>
            public int Command { get; private set; } = Command;
            /// <summary>마스터로부터 수신한 요청 메시지를 가져옵니다.</summary>
            public string RequestMessage { get; private set; } = Message;
            /// <summary>마스터에게 전송할 응답 메시지를 가져오거나 설정합니다. null이면 응답을 전송하지 않습니다.</summary>
            public string? ResponseMessage { get; set; } = null;
        }
        #endregion

        #region Properties
        /// <summary>수신 대기할 로컬 포트 번호를 가져오거나 설정합니다. 기본값은 7897입니다.</summary>
        public int LocalPort { get; set; } = 7897;
        /// <summary>수신 버퍼 크기(바이트)를 가져오거나 설정합니다. 기본값은 8192입니다.</summary>
        public int BufferSize { get; set; } = 8192;

        /// <summary>슬레이브 서버가 실행 중인지 여부를 가져옵니다.</summary>
        public bool IsStart { get; private set; }

        /// <summary>메시지 문자열의 인코딩을 가져오거나 설정합니다. 기본값은 ASCII입니다.</summary>
        public Encoding MessageEncoding { get; set; } = Encoding.ASCII;

        /// <summary>사용자 정의 태그 데이터를 가져오거나 설정합니다.</summary>
        public object? Tag { get; set; } = null;
        #endregion

        #region Member Variable
        Socket? server;

        Task? task;
        CancellationTokenSource? cancel;
        #endregion

        #region Event
        /// <summary>마스터로부터 메시지 요청을 수신했을 때 발생합니다.</summary>
        public event EventHandler<MessageRequestArgs>? MessageRequest;
        /// <summary>마스터 클라이언트가 연결되었을 때 발생합니다.</summary>
        public event EventHandler<SocketEventArgs>? SocketConnected;
        /// <summary>마스터 클라이언트 연결이 끊어졌을 때 발생합니다.</summary>
        public event EventHandler<SocketEventArgs>? SocketDisconnected;
        #endregion

        #region Constructor
        /// <summary>
        /// TextCommTCPSlave 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        public TextCommTCPSlave()
        {
        }
        #endregion

        #region Method
        #region Start
        /// <summary>
        /// TCP 서버 소켓을 열고 마스터 연결 수신을 시작합니다.
        /// 연결된 각 마스터 클라이언트는 별도 태스크에서 처리됩니다.
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
        /// TCP 서버를 정지하고 모든 클라이언트 연결을 종료합니다.
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
            var baResponse = new byte[BufferSize];
            var prev = DateTime.Now;
            var isConnected = sock.Connected;
            var ok = false;
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
                            for (int i = 0; i < n; i++)
                            {
                                var v = baResponse[i];
                                if (v == 0x02) lstResponse.Clear();
                                lstResponse.Add(v);

                            }
                            prev = DateTime.Now;

                            if (n == 0) isConnected = false;
                        }
                        catch (TimeoutException) { }
                    }
                    #endregion

                    #region parse
                    ok = false;
                    if (lstResponse.Count >= 2 && lstResponse.FirstOrDefault() == 0x02 && lstResponse.LastOrDefault() == 0x03)
                    {
                        var ls = TextCommPacket.ParsePacket([.. lstResponse])?.ToList();
                        if (ls != null)
                        {
                            var sum = (byte)(ls.GetRange(0, ls.Count - 1).Select(x => (int)x).Sum() & 0xFF);
                            if (sum == ls[ls.Count - 1])
                            {
                                byte slave = ls[0];
                                byte cmd = ls[1];
                                string msg = MessageEncoding.GetString(ls.ToArray(), 2, ls.Count - 3);

                                if (MessageRequest != null)
                                {
                                    var args = new MessageRequestArgs(slave, cmd, msg);
                                    MessageRequest?.Invoke(this, args);

                                    if (!string.IsNullOrEmpty(args.ResponseMessage))
                                    {
                                        var snd = TextCommPacket.MakePacket(MessageEncoding, slave, cmd, args.ResponseMessage);
                                        sock.Send(snd.ToArray());
                                    }
                                }
                            }

                            ok = true;
                        }
                    }
                    #endregion

                    #region Buffer Clear
                    if (ok || ((DateTime.Now - prev).TotalMilliseconds >= 50 && lstResponse.Count > 0)) lstResponse.Clear();
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
                catch { }
            }

            if (sock.Connected) sock.Close();
            SocketDisconnected?.Invoke(this, new SocketEventArgs(sock));
        }
        #endregion
        #endregion
    }
}
