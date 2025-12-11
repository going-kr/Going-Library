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
    public class ModbusTCPMaster
    {
        #region class : Work
        public class Work(int id, byte[] data, int rescnt)
        {
            public int MessageID { get; private set; } = id;
            public byte[] Data { get; private set; } = data;
            public int ResponseCount { get; private set; } = rescnt;
            public int? RepeatCount { get; set; } = null;
            public int? Timeout { get; set; } = null;
        }
        #endregion
        #region class : EventArgs
        public class BitReadEventArgs(Work WorkItem, bool[] Datas) : System.EventArgs
        {
            public int MessageID => WorkItem.MessageID;
            public int Slave => WorkItem.Data[6];
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[7];
            public int StartAddress => WorkItem.Data[8] << 8 | WorkItem.Data[9];
            public int Length => WorkItem.Data[10] << 8 | WorkItem.Data[11];
            public bool[] ReceiveData => Datas;
        }

        public class WordReadEventArgs(Work WorkItem, int[] Datas) : System.EventArgs
        {
            public int MessageID => WorkItem.MessageID;
            public int Slave => WorkItem.Data[6];
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[7];
            public int StartAddress => WorkItem.Data[8] << 8 | WorkItem.Data[9];
            public int Length => WorkItem.Data[10] << 8 | WorkItem.Data[11];

            public int[] ReceiveData => Datas;
        }

        public class BitWriteEventArgs(Work WorkItem) : System.EventArgs
        {
            public int MessageID => WorkItem.MessageID;
            public int Slave => WorkItem.Data[6];
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[7];
            public int StartAddress => WorkItem.Data[8] << 8 | WorkItem.Data[9];
            public bool WriteValue => (WorkItem.Data[10] << 8 | WorkItem.Data[11]) == 0xFF00;
        }

        public class WordWriteEventArgs(Work WorkItem) : System.EventArgs
        {
            public int MessageID => WorkItem.MessageID;
            public int Slave => WorkItem.Data[6];
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[7];
            public int StartAddress => WorkItem.Data[8] << 8 | WorkItem.Data[9];
            public int WriteValue => WorkItem.Data[10] << 8 | WorkItem.Data[11];
        }

        public class MultiBitWriteEventArgs : System.EventArgs
        {
            public int MessageID => WorkItem.MessageID;
            public int Slave => WorkItem.Data[6];
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[7];
            public int StartAddress => WorkItem.Data[8] << 8 | WorkItem.Data[9];
            public int Length => WorkItem.Data[10] << 8 | WorkItem.Data[11];

            public bool[] WriteValues { get; private set; }

            private Work WorkItem;

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

        public class MultiWordWriteEventArgs : System.EventArgs
        {
            public int MessageID => WorkItem.MessageID;
            public int Slave => WorkItem.Data[6];
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[7];
            public int StartAddress => WorkItem.Data[8] << 8 | WorkItem.Data[9];
            public int Length => WorkItem.Data[10] << 8 | WorkItem.Data[11];

            public int[] WriteValues { get; private set; }

            private Work WorkItem;

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

        public class WordBitSetEventArgs(Work WorkItem) : System.EventArgs
        {
            public int MessageID => WorkItem.MessageID;
            public int Slave => WorkItem.Data[6];
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[7];
            public int StartAddress => WorkItem.Data[8] << 8 | WorkItem.Data[9];
            public int BitIndex => WorkItem.Data[10];
            public bool WriteValue => (WorkItem.Data[11] << 8 | WorkItem.Data[12]) == 0xFF00;

        }

        public class TimeoutEventArgs(Work WorkItem) : System.EventArgs
        {
            public int MessageID => WorkItem.MessageID;
            public int Slave => WorkItem.Data[6];
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[7];
            public int StartAddress => WorkItem.Data[8] << 8 | WorkItem.Data[9];
        }

        public class CRCErrorEventArgs(Work WorkItem) : System.EventArgs
        {
            public int MessageID => WorkItem.MessageID;
            public int Slave => WorkItem.Data[6];
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[7];
            public int StartAddress => WorkItem.Data[8] << 8 | WorkItem.Data[9];
        }
        #endregion

        #region Properties
        public string RemoteIP { get; set; } = "127.0.0.1";
        public int RemotePort { get; set; } = 502;

        public int Timeout { get; set; } = 100;
        public int Interval { get; set; } = 10;
        public int BufferSize { get; set; } = 1024;

        public bool IsOpen => bIsOpen;
        public bool IsStart { get; private set; }
        public bool AutoReconnect { get; set; } = true;

        public bool IsDisposed { get; private set; }

        public string ModuleId { get; } = Guid.NewGuid().ToString();
        public object? Tag { get; set; } = null;
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
        public event EventHandler<BitReadEventArgs>? BitReadReceived;
        public event EventHandler<WordReadEventArgs>? WordReadReceived;
        public event EventHandler<BitWriteEventArgs>? BitWriteReceived;
        public event EventHandler<WordWriteEventArgs>? WordWriteReceived;
        public event EventHandler<MultiBitWriteEventArgs>? MultiBitWriteReceived;
        public event EventHandler<MultiWordWriteEventArgs>? MultiWordWriteReceived;
        public event EventHandler<WordBitSetEventArgs>? WordBitSetReceived;
        public event EventHandler<TimeoutEventArgs>? TimeoutReceived;

        public event EventHandler<EventArgs>? SocketConnected;
        public event EventHandler<EventArgs>? SocketDisconnected;
        #endregion

        #region Construct
        public ModbusTCPMaster()
        {

        }
        #endregion

        #region Method
        #region Start / Stop
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
                                    }
                                    catch (SchedulerStopException) { break; }
                                    catch (Exception ex) { }
                                    await Task.Delay(Interval, token);
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

                                EndPoint ipep = new IPEndPoint(IPAddress.Parse(RemoteIP), RemotePort);

                                len = client.ReceiveFrom(baResponse, nRecv, baResponse.Length - nRecv, SocketFlags.None, ref ipep);
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

        #region AutoBitRead
        public void AutoBitRead_FC1(int id, int slave, int startAddr, int length, int? timeout = null) => AutoBitRead(id, slave, 1, startAddr, length, timeout);
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
        public void AutoWordRead_FC3(int id, int slave, int startAddr, int length, int? timeout = null) => AutoWordRead(id, slave, 3, startAddr, length, timeout);
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
        public void ManualBitRead_FC1(int id, int slave, int startAddr, int length, int? repeatCount = null, int? timeout = null) => ManualBitRead(id, slave, 1, startAddr, length, repeatCount, timeout);
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
        public void ManualWordRead_FC3(int id, int slave, int startAddr, int length, int? repeatCount = null, int? timeout = null) => ManualWordRead(id, slave, 3, startAddr, length, repeatCount, timeout);
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
