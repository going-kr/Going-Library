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
    public class TextCommTCPSlave
    {
        #region class : EventArgs
        public class MessageRequestArgs(int Slave, int Command, string Message) : EventArgs
        {
            public int Slave { get; private set; } = Slave;
            public int Command { get; private set; } = Command;
            public string RequestMessage { get; private set; } = Message;
            public string? ResponseMessage { get; set; } = null;
        }
        #endregion

        #region Properties
        public int LocalPort { get; set; } = 7897;
        public int BufferSize { get; set; } = 8192;

        public bool IsStart { get; private set; }

        public Encoding MessageEncoding { get; set; } = Encoding.ASCII;
        #endregion

        #region Member Variable
        Socket? server;

        Task? task;
        CancellationTokenSource? cancel;
        #endregion

        #region Event
        public event EventHandler<MessageRequestArgs>? MessageRequest;
        public event EventHandler<SocketEventArgs>? SocketConnected;
        public event EventHandler<SocketEventArgs>? SocketDisconnected;
        #endregion

        #region Constructor
        public TextCommTCPSlave()
        {
        }
        #endregion

        #region Method
        #region Start
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
        public void Stop()
        {
            try { IsStart = false; cancel?.Cancel(false); }
            finally
            {
                cancel?.Dispose();
                cancel = null;
            }

            if (task != null)
            {
                try { task.Wait(); task.Dispose(); }
                catch { }
                finally { task = null; }
            }
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
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.TimedOut) { }
                    else if (ex.SocketErrorCode == SocketError.ConnectionReset) { isConnected = false; }
                    else if (ex.SocketErrorCode == SocketError.ConnectionAborted) { isConnected = false; }
                    else if (ex.SocketErrorCode == SocketError.Shutdown) { isConnected = false; }
                }
                catch { }
                await Task.Delay(10, cancel);
            }

            if (sock.Connected) sock.Close();
            SocketDisconnected?.Invoke(this, new SocketEventArgs(sock));
        }
        #endregion
        #endregion
    }
}
