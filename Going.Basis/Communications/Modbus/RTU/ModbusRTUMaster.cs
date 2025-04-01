using Going.Basis.Communications.LS;
using Going.Basis.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Communications.Modbus.RTU
{
    public class ModbusRTUMaster
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
        public class BitReadEventArgs(Work WorkItem, bool[] Datas) : EventArgs
        {
            public int MessageID => WorkItem.MessageID;
            public int Slave => WorkItem.Data[0];
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[1];
            public int StartAddress => WorkItem.Data[2] << 8 | WorkItem.Data[3];
            public int Length => WorkItem.Data[4] << 8 | WorkItem.Data[5];
            public bool[] ReceiveData => Datas;
        }

        public class WordReadEventArgs(Work WorkItem, int[] Datas) : EventArgs
        {
            public int MessageID => WorkItem.MessageID;
            public int Slave => WorkItem.Data[0];
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[1];
            public int StartAddress => WorkItem.Data[2] << 8 | WorkItem.Data[3];
            public int Length => WorkItem.Data[4] << 8 | WorkItem.Data[5];

            public int[] ReceiveData => Datas;
        }

        public class BitWriteEventArgs(Work WorkItem) : EventArgs
        {
            public int MessageID => WorkItem.MessageID;
            public int Slave => WorkItem.Data[0];
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[1];
            public int StartAddress => WorkItem.Data[2] << 8 | WorkItem.Data[3];
            public bool WriteValue => (WorkItem.Data[4] << 8 | WorkItem.Data[5]) == 0xFF00;
        }

        public class WordWriteEventArgs(Work WorkItem) : EventArgs
        {
            public int MessageID => WorkItem.MessageID;
            public int Slave => WorkItem.Data[0];
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[1];
            public int StartAddress => WorkItem.Data[2] << 8 | WorkItem.Data[3];
            public int WriteValue => WorkItem.Data[4] << 8 | WorkItem.Data[5];
        }

        public class MultiBitWriteEventArgs : EventArgs
        {
            public int MessageID => WorkItem.MessageID;
            public int Slave => WorkItem.Data[0];
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[1];
            public int StartAddress => WorkItem.Data[2] << 8 | WorkItem.Data[3];
            public int Length => WorkItem.Data[4] << 8 | WorkItem.Data[5];

            public bool[] WriteValues { get; private set; }

            private Work WorkItem;

            public MultiBitWriteEventArgs(Work WorkItem)
            {
                this.WorkItem = WorkItem;
                #region WriteValues
                List<bool> ret = new List<bool>();
                for (int i = 7; i < WorkItem.Data.Length - 2; i++)
                {
                    var v = WorkItem.Data[i];
                    for (int j = 0; j < 8; j++)
                        if (ret.Count < Length) ret.Add(v.Bit(j));
                }
                WriteValues = [.. ret];
                #endregion
            }
        }

        public class MultiWordWriteEventArgs : EventArgs
        {
            public int MessageID => WorkItem.MessageID;
            public int Slave => WorkItem.Data[0];
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[1];
            public int StartAddress => WorkItem.Data[2] << 8 | WorkItem.Data[3];
            public int Length => WorkItem.Data[4] << 8 | WorkItem.Data[5];

            public int[] WriteValues { get; private set; }

            private Work WorkItem;

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

        public class WordBitSetEventArgs(Work WorkItem) : EventArgs
        {
            public int MessageID => WorkItem.MessageID;
            public int Slave => WorkItem.Data[0];
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[1];
            public int StartAddress => WorkItem.Data[2] << 8 | WorkItem.Data[3];
            public int BitIndex => WorkItem.Data[4];
            public bool WriteValue => (WorkItem.Data[5] << 8 | WorkItem.Data[6]) == 0xFF00;

        }

        public class TimeoutEventArgs(Work WorkItem) : EventArgs
        {
            public int MessageID => WorkItem.MessageID;
            public int Slave => WorkItem.Data[0];
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[1];
            public int StartAddress => WorkItem.Data[2] << 8 | WorkItem.Data[3];
        }

        public class CRCErrorEventArgs(Work WorkItem) : EventArgs
        {
            public int MessageID => WorkItem.MessageID;
            public int Slave => WorkItem.Data[0];
            public ModbusFunction Function => (ModbusFunction)WorkItem.Data[1];
            public int StartAddress => WorkItem.Data[2] << 8 | WorkItem.Data[3];
        }
        #endregion

        #region Properties
        public string Port { get => ser.PortName; set => ser.PortName = value; }
        public int Baudrate { get => ser.BaudRate; set => ser.BaudRate = value; }
        public Parity Parity { get => ser.Parity; set => ser.Parity = value; }
        public int DataBits { get => ser.DataBits; set => ser.DataBits = value; }
        public StopBits StopBits { get => ser.StopBits; set => ser.StopBits = value; }

        public int Timeout { get; set; } = 100;
        public int Interval { get; set; } = 10;
        public int BufferSize { get; set; } = 1024;

        public bool IsOpen => ser.IsOpen;
        public bool IsStart { get; private set; }
        public bool AutoStart { get; set; }
        #endregion

        #region Member Variable
        SerialPort ser = new() { PortName = "COM1", BaudRate = 115200 };

        Queue<Work> WorkQueue = [];
        List<Work> AutoWorkList = [];
        List<Work> ManualWorkList = [];

        byte[] baResponse = new byte[0];

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
        public event EventHandler<CRCErrorEventArgs>? CRCErrorReceived;

        public event EventHandler? DeviceOpened;
        public event EventHandler? DeviceClosed;
        #endregion

        #region Construct
        public ModbusRTUMaster()
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

                    try { ser.Open(); DeviceOpened?.Invoke(this, EventArgs.Empty); }
                    catch { }

                    if (ser.IsOpen)
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

                    if (ser.IsOpen)
                    {
                        ser.Close();
                        DeviceClosed?.Invoke(this, EventArgs.Empty);
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

        #region AutoBitRead
        public void AutoBitRead_FC1(int id, int slave, int startAddr, int length, int? timeout = null) => AutoBitRead(id, slave, 1, startAddr, length, timeout);
        public void AutoBitRead_FC2(int id, int slave, int startAddr, int length, int? timeout = null) => AutoBitRead(id, slave, 2, startAddr, length, timeout);
        private void AutoBitRead(int id, int slave, byte fn, int startAddr, int length, int? timeout)
        {
            byte[] data = new byte[8];
            byte crcHi = 0xff, crcLo = 0xff;

            data[0] = Convert.ToByte(slave);
            data[1] = fn;
            data[2] = startAddr.Byte1();
            data[3] = startAddr.Byte0();
            data[4] = length.Byte1();
            data[5] = length.Byte0();

            ModbusCRC.GetCRC(data, data.Length - 2, ref crcHi, ref crcLo);
            data[6] = crcHi;
            data[7] = crcLo;

            int nResCount = length / 8;
            if (length % 8 != 0) nResCount++;
            AutoWorkList.Add(new Work(id, data, nResCount + 5) { Timeout = timeout });
        }
        #endregion
        #region AutoWordRead
        public void AutoWordRead_FC3(int id, int slave, int startAddr, int length, int? timeout = null) => AutoWordRead(id, slave, 3, startAddr, length, timeout);
        public void AutoWordRead_FC4(int id, int slave, int startAddr, int length, int? timeout = null) => AutoWordRead(id, slave, 4, startAddr, length, timeout);
        private void AutoWordRead(int id, int slave, byte fn, int startAddr, int length, int? timeout)
        {
            byte[] data = new byte[8];
            byte crcHi = 0xff, crcLo = 0xff;

            data[0] = Convert.ToByte(slave);
            data[1] = fn;
            data[2] = startAddr.Byte1();
            data[3] = startAddr.Byte0();
            data[4] = length.Byte1();
            data[5] = length.Byte0();

            ModbusCRC.GetCRC(data, data.Length - 2, ref crcHi, ref crcLo);
            data[6] = crcHi;
            data[7] = crcLo;

            AutoWorkList.Add(new Work(id, data, length * 2 + 5) { Timeout = timeout });
        }
        #endregion
        #region ManualBitRead
        public void ManualBitRead_FC1(int id, int slave, int startAddr, int length, int? repeatCount = null, int? timeout = null) => ManualBitRead(id, slave, 1, startAddr, length, repeatCount, timeout);
        public void ManualBitRead_FC2(int id, int slave, int startAddr, int length, int? repeatCount = null, int? timeout = null) => ManualBitRead(id, slave, 2, startAddr, length, repeatCount, timeout);
        private void ManualBitRead(int id, int slave, byte fn, int startAddr, int length, int? repeatCount, int? timeout)
        {
            byte[] data = new byte[8];
            byte crcHi = 0xff, crcLo = 0xff;

            data[0] = Convert.ToByte(slave);
            data[1] = fn;
            data[2] = startAddr.Byte1();
            data[3] = startAddr.Byte0();
            data[4] = length.Byte1();
            data[5] = length.Byte0();

            ModbusCRC.GetCRC(data, data.Length - 2, ref crcHi, ref crcLo);
            data[6] = crcHi;
            data[7] = crcLo;

            int nResCount = length / 8;
            if (length % 8 != 0) nResCount++;
            ManualWorkList.Add(new Work(id, data, nResCount + 5) { RepeatCount = repeatCount, Timeout = timeout });
        }
        #endregion
        #region ManualWordRead
        public void ManualWordRead_FC3(int id, int slave, int startAddr, int length, int? repeatCount = null, int? timeout = null) => ManualWordRead(id, slave, 3, startAddr, length, repeatCount, timeout);
        public void ManualWordRead_FC4(int id, int slave, int startAddr, int length, int? repeatCount = null, int? timeout = null) => ManualWordRead(id, slave, 4, startAddr, length, repeatCount, timeout);
        private void ManualWordRead(int id, int slave, byte fn, int startAddr, int length, int? repeatCount, int? timeout)
        {
            byte[] data = new byte[8];
            byte crcHi = 0xff, crcLo = 0xff;

            data[0] = Convert.ToByte(slave);
            data[1] = fn;
            data[2] = startAddr.Byte1();
            data[3] = startAddr.Byte0();
            data[4] = length.Byte1();
            data[5] = length.Byte0();

            ModbusCRC.GetCRC(data, data.Length - 2, ref crcHi, ref crcLo);
            data[6] = crcHi;
            data[7] = crcLo;

            ManualWorkList.Add(new Work(id, data, length * 2 + 5) { RepeatCount = repeatCount, Timeout = timeout });
        }
        #endregion
        #region ManualBitWrite
        public void ManualBitWrite_FC5(int id, int slave, int startAddr, bool value, int? repeatCount = null, int? timeout = null)
        {
            byte[] data = new byte[8];
            byte crcHi = 0xff, crcLo = 0xff;
            int val = value ? 0xFF00 : 0x0000;

            data[0] = Convert.ToByte(slave);
            data[1] = 0x05;
            data[2] = startAddr.Byte1();
            data[3] = startAddr.Byte0();
            data[4] = val.Byte1();
            data[5] = val.Byte0();

            ModbusCRC.GetCRC(data, data.Length - 2, ref crcHi, ref crcLo);
            data[6] = crcHi;
            data[7] = crcLo;
            ManualWorkList.Add(new Work(id, data, 8) { RepeatCount = repeatCount, Timeout = timeout });
        }
        #endregion
        #region ManualWordWrite
        public void ManualWordWrite_FC6(int id, int slave, int startAddr, int value, int? repeatCount = null, int? timeout = null)
        {
            byte[] data = new byte[8];
            byte crcHi = 0xff, crcLo = 0xff;

            data[0] = Convert.ToByte(slave);
            data[1] = 0x06;
            data[2] = startAddr.Byte1();
            data[3] = startAddr.Byte0();
            data[4] = value.Byte1();
            data[5] = value.Byte0();

            ModbusCRC.GetCRC(data, data.Length - 2, ref crcHi, ref crcLo);
            data[6] = crcHi;
            data[7] = crcLo;
            ManualWorkList.Add(new Work(id, data, 8) { RepeatCount = repeatCount, Timeout = timeout });
        }
        #endregion
        #region ManualMultiBitWrite_FC15
        public void ManualMultiBitWrite_FC15(int id, int slave, int startAddr, bool[] values, int? repeatCount = null, int? timeout = null)
        {
            int Length = values.Length / 8;
            Length += values.Length % 8 == 0 ? 0 : 1;

            byte[] data = new byte[9 + Length];
            byte crcHi = 0xff, crcLo = 0xff;


            data[0] = Convert.ToByte(slave);
            data[1] = 0x0F;
            data[2] = startAddr.Byte1();
            data[3] = startAddr.Byte0();
            data[4] = values.Length.Byte1();
            data[5] = values.Length.Byte0();
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
        public void ManualMultiWordWrite_FC16(int id, int slave, int startAddr, int[] values, int? repeatCount = null, int? timeout = null)
        {
            byte[] data = new byte[9 + values.Length * 2];
            byte crcHi = 0xff, crcLo = 0xff;

            data[0] = Convert.ToByte(slave);
            data[1] = 0x10;
            data[2] = startAddr.Byte1();
            data[3] = startAddr.Byte0();
            data[4] = values.Length.Byte1();
            data[5] = values.Length.Byte0();
            data[6] = Convert.ToByte(values.Length * 2);

            for (int i = 0; i < values.Length; i++)
            {
                data[7 + i * 2] = values[i].Byte1();
                data[8 + i * 2] = values[i].Byte0();
            }

            ModbusCRC.GetCRC(data, data.Length - 2, ref crcHi, ref crcLo);
            data[data.Length - 2] = crcHi;
            data[data.Length - 1] = crcLo;

            ManualWorkList.Add(new Work(id, data, 8) { RepeatCount = repeatCount, Timeout = timeout });
        }
        #endregion
        #region ManualWordBitSet_FC26
        public void ManualWordBitSet_FC26(int id, int slave, int startAddr, byte bitIndex, bool value, int? repeatCount = null, int? timeout = null)
        {
            byte[] data = new byte[9];
            byte crcHi = 0xff, crcLo = 0xff;
            int val = value ? 0xFF00 : 0x0000;

            data[0] = Convert.ToByte(slave);
            data[1] = 0x1A;
            data[2] = startAddr.Byte1();
            data[3] = startAddr.Byte0();
            data[4] = bitIndex;
            data[5] = val.Byte1();
            data[6] = val.Byte0();

            ModbusCRC.GetCRC(data, data.Length - 2, ref crcHi, ref crcLo);
            data[7] = crcHi;
            data[8] = crcLo;

            ManualWorkList.Add(new Work(id, data, 8) { RepeatCount = repeatCount, Timeout = timeout });
        }
        #endregion
        #endregion
    }
}
