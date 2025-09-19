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
    public enum MCFunc
    {
        None,
        BitRead,
        WordRead,
        BitWrite,
        WordWrite,
    }
    #endregion

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
        public class WordDataReadEventArgs(int ID, int Slave, MCFunc Func, int[] Data) : EventArgs
        {
            public int MessageID { get; private set; } = ID;
            public int Slave { get; private set; } = Slave;
            public MCFunc Function { get; private set; } = Func;
            public int[] Data { get; private set; } = Data;
        }
        public class BitDataReadEventArgs(int ID, int Slave, MCFunc Func, bool[] Data) : EventArgs
        {
            public int MessageID { get; private set; } = ID;
            public int Slave { get; private set; } = Slave;
            public MCFunc Function { get; private set; } = Func;
            public bool[] Data { get; private set; } = Data;
        }
        public class WriteEventArgs(int ID, int Slave, MCFunc Func) : EventArgs
        {
            public int MessageID { get; private set; } = ID;
            public int Slave { get; private set; } = Slave;
            public MCFunc Function { get; private set; } = Func;
        }
        public class TimeoutEventArgs(int ID, int Slave, MCFunc Func) : EventArgs
        {
            public int MessageID { get; private set; } = ID;
            public int Slave { get; private set; } = Slave;
            public MCFunc Function { get; private set; } = Func;
        }
        public class CheckSumErrorEventArgs(int ID, int Slave, MCFunc Func) : EventArgs
        {
            public int MessageID { get; private set; } = ID;
            public int Slave { get; private set; } = Slave;
            public MCFunc Function { get; private set; } = Func;
        }
        public class NakErrorEventArgs(int ID, int Slave, MCFunc Func) : EventArgs
        {
            public int MessageID { get; private set; } = ID;
            public int Slave { get; private set; } = Slave;
            public MCFunc Function { get; private set; } = Func;
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

        public bool UseControlSequence { get; set; } = false;
        public bool UseCheckSum { get; set; } = false;
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
        public event EventHandler<WordDataReadEventArgs>? WordDataReceived;
        public event EventHandler<BitDataReadEventArgs>? BitDataReceived;
        public event EventHandler<WriteEventArgs>? WriteResponseReceived;
        public event EventHandler<TimeoutEventArgs>? TimeoutReceived;
        public event EventHandler<CheckSumErrorEventArgs>? CheckSumErrorReceived;
        public event EventHandler<NakErrorEventArgs>? NakErrorReceived;

        public event EventHandler? DeviceOpened;
        public event EventHandler? DeviceClosed;
        #endregion

        #region Construct
        public MC()
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
        public void ManualBitWrite(int id, int slave, string device, bool value, int waitTime = 0, int? repeatCount = null, int? timeout = null) => ManualBitWrite(id, slave, device, new bool[] { value }, waitTime, repeatCount, timeout);
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
        public void ManualWordWrite(int id, int slave, string device, int value, int waitTime = 0, int? repeatCount = null, int? timeout = null) { ManualWordWrite(id, slave, device, new int[] { value }, waitTime, repeatCount, timeout); }
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
