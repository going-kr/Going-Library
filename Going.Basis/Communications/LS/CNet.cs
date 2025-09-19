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
    public enum CNetFunc
    {
        NONE, READ_SINGLE, READ_BLOCK, WRITE_SINGLE, WRITE_BLOCK,
    }
    #endregion
    #region class : CNetValue
    public class CNetValue(string dev, int value)
    {
        public string Device { get; private set; } = dev;
        public int Value { get; private set; } = value;
    }
    #endregion
    #region class : Exception
    public class SchedulerStopException : Exception { }
    #endregion

    public class CNet
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
        public class DataReadEventArgs(int ID, int Slave, CNetFunc Func, int[] Data, string[]? ReadAddress) : EventArgs
        {
            public int MessageID { get; private set; } = ID;
            public int Slave { get; private set; } = Slave;
            public CNetFunc Function { get; private set; } = Func;
            public int[] Data { get; private set; } = Data;
            public string[]? ReadAddress { get; private set; } = ReadAddress;
        }
        public class WriteEventArgs(int ID, int Slave, CNetFunc Func) : EventArgs
        {
            public int MessageID { get; private set; } = ID;
            public int Slave { get; private set; } = Slave;
            public CNetFunc Function { get; private set; } = Func;
        }
        public class TimeoutEventArgs(int ID, int Slave, CNetFunc Func) : EventArgs
        {
            public int MessageID { get; private set; } = ID;
            public int Slave { get; private set; } = Slave;
            public CNetFunc Function { get; private set; } = Func;
        }
        public class BCCErrorEventArgs(int ID, int Slave, CNetFunc Func) : EventArgs
        {
            public int MessageID { get; private set; } = ID;
            public int Slave { get; private set; } = Slave;
            public CNetFunc Function { get; private set; } = Func;
        }
        public class NAKEventArgs(int ID, int Slave, CNetFunc Func, int ErrCode) : EventArgs
        {
            public int MessageID { get; private set; } = ID;
            public int Slave { get; private set; } = Slave;
            public CNetFunc Function { get; private set; } = Func;
            public int ErrorCode { get; private set; } = ErrCode;
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
        public event EventHandler<DataReadEventArgs>? DataReceived;
        public event EventHandler<WriteEventArgs>? WriteResponseReceived;
        public event EventHandler<TimeoutEventArgs>? TimeoutReceived;
        public event EventHandler<BCCErrorEventArgs>? BCCErrorReceived;
        public event EventHandler<NAKEventArgs>? NAKReceived;

        public event EventHandler? DeviceOpened;
        public event EventHandler? DeviceClosed;
        #endregion

        #region Construct
        public CNet()
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
                                string strFunc = new([(char)baResponse[3], (char)baResponse[4], (char)w.Data[5]]);
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

        #region AutoRSS(id, slave, device)
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
            int n = Convert.ToInt32(device.Substring(3));
            for (int i = 0; i < length; i++) d.Add(device.Substring(0, 3) + (n + i));

            AutoWorkList.Add(new Work(id, data) { Devices = [.. d], Timeout = timeout });
        }
        #endregion
        #region ManualRSS(id, slave, device)
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
            int n = Convert.ToInt32(device.Substring(3));
            for (int i = 0; i < length; i++)
                d.Add(device.Substring(0, 3) + (n + i));

            ManualWorkList.Add(new Work(id, data) { Devices = d.ToArray(), RepeatCount = repeatCount, Timeout = timeout });
        }
        #endregion
        #region ManualWSS(id, slave, device, value)
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
