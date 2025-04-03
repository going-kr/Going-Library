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
    public class TextCommTCPMaster
    {
        #region class : Work
        public class Work(int id, byte[] data, byte slave, byte command, string message)
        {
            public int MessageID { get; private set; } = id;
            public byte[] Data { get; private set; } = data;
            public int? RepeatCount { get; set; } = null;
            public int? Timeout { get; set; } = null;

            public byte Slave { get; private set; } = slave;
            public byte Command { get; private set; } = command;
            public string Message { get; private set; } = message;
        }
        #endregion
        #region class : EventArgs
        public class ReceivedEventArgs(Work workItem, string message) : EventArgs
        {
            public int MessageID => workItem.MessageID;
            public byte Slave => workItem.Slave;
            public byte Command => workItem.Command;
            public string Message { get; private set; } = message;
        }

        public class TimeoutEventArgs(Work workItem) : EventArgs
        {
            public int MessageID => workItem.MessageID;
            public byte Slave => workItem.Slave;
            public byte Command => workItem.Command;
        }
        #endregion

        #region Properties
        public string RemoteIP { get; set; } = "127.0.0.1";
        public int RemotePort { get; set; } = 7897;

        public int Timeout { get; set; } = 100;
        public int Interval { get; set; } = 10;
        public int BufferSize { get; set; } = 1024;
        public Encoding MessageEncoding { get; set; } = Encoding.ASCII;

        public bool IsOpen => bIsOpen;
        public bool IsStart { get; private set; }
        public bool AutoStart { get; set; }
        #endregion

        #region Member Variable
        Socket? client;

        Queue<Work> WorkQueue = [];
        List<Work> AutoWorkList = [];
        List<Work> ManualWorkList = [];

        byte[] baResponse = new byte[0];
        bool bIsOpen = false;

        Task? task;
        CancellationTokenSource? cancel;
        #endregion

        #region Event
        public event EventHandler<ReceivedEventArgs>? MessageReceived;
        public event EventHandler<TimeoutEventArgs>? TimeoutReceived;

        public event EventHandler<SocketEventArgs>? SocketConnected;
        public event EventHandler<SocketEventArgs>? SocketDisconnected;
        #endregion

        #region Construct
        public TextCommTCPMaster()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    if (!IsStart && AutoStart)
                    {
                        _Start();
                    }
                    await Task.Delay(1000);
                }
            });
        }
        #endregion

        #region Method
        #region Start / Stop
        public void Start()
        {
            if (AutoStart) throw new Exception("AutoStart가 true일 땐 Start/Stop 을 할 수 없습니다.");
            else _Start();
        }

        public void Stop()
        {
            if (AutoStart) throw new Exception("AutoStart가 true일 땐 Start/Stop 을 할 수 없습니다.");
            else _Stop();
        }

        private void _Start()
        {
            if (!IsOpen && !IsStart)
            {
                cancel = new CancellationTokenSource();
                task = Task.Run(async () =>
                {
                    var token = cancel.Token;

                    #region Connect
                    try
                    {
                        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        client.ReceiveTimeout = Timeout;
                        client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, Timeout);
                        client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
                        client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                        client.Connect(RemoteIP, RemotePort);
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
                            }
                            catch (SchedulerStopException) { break; }
                            await Task.Delay(Interval);
                        }
                    }

                    if (bIsOpen && client != null)
                    {
                        client.Close();
                        SocketDisconnected?.Invoke(this, new SocketEventArgs(client));
                    }

                    IsStart = false;

                }, cancel.Token);
            }
        }

        private void _Stop()
        {
            try { cancel?.Cancel(false); }
            finally
            {
                cancel?.Dispose();
                cancel = null;
            }

            if (task != null)
            {
                try { task.Wait(); }
                catch { }
                finally { task = null; }
            }
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

                    if (WorkQueue.Count > 0)
                    {
                        var w = WorkQueue.Dequeue();
                        var bRepeat = true;
                        var nTimeoutCount = 0;
                        var Timeout = w.Timeout ?? this.Timeout;

                        while (bRepeat)
                        {
                            #region write
                            try
                            {
                                EndPoint ipep = new IPEndPoint(IPAddress.Parse(RemoteIP), RemotePort);
                                client?.SendTo(w.Data, ipep);
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

                                        EndPoint ipep = new IPEndPoint(IPAddress.Parse(RemoteIP), RemotePort);

                                        len = client.ReceiveFrom(baResponse, 0, baResponse.Length, SocketFlags.None, ref ipep);
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
                                        if (slave == w.Slave && cmd == w.Command)
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
        public void ClearManual() { ManualWorkList.Clear(); }
        public void ClearAuto() { AutoWorkList.Clear(); }
        public void ClearWorkSchedule() { WorkQueue.Clear(); }
        #endregion

        #region Auto
        public void AutoSend(int MessageID, byte Slave, byte Command, string Message, int? timeout = null)
        {
            var ba = TextCommPacket.MakePacket(MessageEncoding, Slave, Command, Message);
            AutoWorkList.Add(new Work(MessageID, ba, Slave, Command, Message) { Timeout = timeout });
        }
        #endregion
        #region Manual
        public void ManualSend(int MessageID, byte Slave, byte Command, string Message, int? repeatCount = null, int? timeout = null)
        {
            var ba = TextCommPacket.MakePacket(MessageEncoding, Slave, Command, Message);
            ManualWorkList.Add(new Work(MessageID, ba, Slave, Command, Message) { RepeatCount = repeatCount, Timeout = timeout });
        }
        #endregion
        #endregion
    }
}
