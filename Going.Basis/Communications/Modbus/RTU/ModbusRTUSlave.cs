using Going.Basis.Communications.LS;
using Going.Basis.Extensions;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Going.Basis.Communications.Modbus.RTU
{
    public class ModbusRTUSlave
    {
        #region class : EventArgs
        public class BitReadRequestArgs(byte[] Data) : EventArgs
        {
            public int Slave => Data[0];
            public ModbusFunction Function => (ModbusFunction)Data[1];
            public int StartAddress => Data[2] << 8 | Data[3];
            public int Length => Data[4] << 8 | Data[5];

            public bool Success { get; set; }
            public bool[]? ResponseData { get; set; }
        }

        public class WordReadRequestArgs(byte[] Data) : EventArgs
        {
            public int Slave => Data[0];
            public ModbusFunction Function => (ModbusFunction)Data[1];
            public int StartAddress => Data[2] << 8 | Data[3];
            public int Length => Data[4] << 8 | Data[5];

            public bool Success { get; set; }
            public int[]? ResponseData { get; set; }
        }

        public class BitWriteRequestArgs(byte[] Data) : EventArgs
        {
            public int Slave => Data[0];
            public ModbusFunction Function => (ModbusFunction)Data[1];
            public int StartAddress => Data[2] << 8 | Data[3];
            public bool WriteValue => (Data[4] << 8 | Data[5]) == 0xFF00;

            public bool Success { get; set; }
        }

        public class WordWriteRequestArgs(byte[] Data) : EventArgs
        {
            public int Slave => Data[0];
            public ModbusFunction Function => (ModbusFunction)Data[1];
            public int StartAddress => Data[2] << 8 | Data[3];
            public ushort WriteValue => Convert.ToUInt16(Data[4] << 8 | Data[5]);

            public bool Success { get; set; }
        }

        public class MultiBitWriteRequestArgs : EventArgs
        {
            public int Slave => Data[0];
            public ModbusFunction Function => (ModbusFunction)Data[1];
            public int StartAddress => Data[2] << 8 | Data[3];
            public int Length => Data[4] << 8 | Data[5];
            public bool[] WriteValues { get; private set; }

            public bool Success { get; set; }

            byte[] Data;

            public MultiBitWriteRequestArgs(byte[] Data)
            {
                this.Data = Data;
                #region WriteValues
                List<bool> ret = new List<bool>();
                for (int i = 7; i < Data.Length - 2; i++)
                    for (int j = 0; j < 8; j++)
                        if (ret.Count < Length) ret.Add(Data[i].Bit(j));
                WriteValues = ret.ToArray();
                #endregion
            }
        }

        public class MultiWordWriteRequestArgs : EventArgs
        {
            public int Slave => Data[0];
            public ModbusFunction Function => (ModbusFunction)Data[1];
            public int StartAddress => Data[2] << 8 | Data[3];
            public int Length => Data[4] << 8 | Data[5];
            public ushort[] WriteValues { get; private set; }

            public bool Success { get; set; }

            byte[] Data;

            public MultiWordWriteRequestArgs(byte[] Data)
            {
                this.Data = Data;
                #region WriteValues
                List<ushort> ret = new List<ushort>();
                for (int i = 7; i < Data.Length - 2; i += 2)
                {
                    ret.Add(Convert.ToUInt16(Data[i] << 8 | Data[i + 1]));
                }
                WriteValues = ret.ToArray();
                #endregion
            }
        }

        public class WordBitSetRequestArgs(byte[] Data) : EventArgs
        {
            public int Slave => Data[0];
            public ModbusFunction Function => (ModbusFunction)Data[1];
            public int StartAddress => Data[2] << 8 | Data[3];
            public int BitIndex => Data[4];
            public bool WriteValue => (Data[5] << 8 | Data[6]) == 0xFF00;

            public bool Success { get; set; }
        }
        #endregion

        #region Properties
        public string Port { get => ser.PortName; set => ser.PortName = value; }
        public int Baudrate { get => ser.BaudRate; set => ser.BaudRate = value; }
        public Parity Parity { get => ser.Parity; set => ser.Parity = value; }
        public int DataBits { get => ser.DataBits; set => ser.DataBits = value; }
        public StopBits StopBits { get => ser.StopBits; set => ser.StopBits = value; }

        public bool IsOpen => ser.IsOpen;
        public bool IsStart { get; private set; }
        #endregion

        #region Member Variable
        SerialPort ser = new SerialPort() { PortName = "COM1", BaudRate = 115200 };

        Task? task;
        CancellationTokenSource? cancel;
        #endregion

        #region Event
        public event EventHandler<BitReadRequestArgs>? BitReadRequest;
        public event EventHandler<WordReadRequestArgs>? WordReadRequest;
        public event EventHandler<BitWriteRequestArgs>? BitWriteRequest;
        public event EventHandler<WordWriteRequestArgs>? WordWriteRequest;
        public event EventHandler<MultiBitWriteRequestArgs>? MultiBitWriteRequest;
        public event EventHandler<MultiWordWriteRequestArgs>? MultiWordWriteRequest;
        public event EventHandler<WordBitSetRequestArgs>? WordBitSetRequest;

        public event EventHandler? DeviceOpened;
        public event EventHandler? DeviceClosed;
        #endregion

        #region Constructor
        public ModbusRTUSlave()
        {
        }
        #endregion

        #region Method
        #region Start
        public void Start()
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
                        #region var
                        List<byte> lstResponse = [];
                        var baResponse = new byte[1024 * 8];
                        DateTime prev = DateTime.Now;
                        bool ok = false;
                        #endregion

                        IsStart = true;
                        while (!token.IsCancellationRequested && IsStart)
                        {
                            try
                            {
                                #region read
                                if (ser.BytesToRead > 0)
                                {
                                    try
                                    {
                                        var len = ser.Read(baResponse, 0, baResponse.Length);
                                        for (int i = 0; i < len; i++) lstResponse.Add(baResponse[i]);
                                        prev = DateTime.Now;
                                    }
                                    catch (IOException) { throw new SchedulerStopException(); }
                                    catch (UnauthorizedAccessException) { throw new SchedulerStopException(); }
                                    catch (InvalidOperationException) { throw new SchedulerStopException(); }
                                }
                                #endregion

                                #region parse
                                ok = false;
                                if (lstResponse.Count >= 4)
                                {
                                    int Slave = lstResponse[0];
                                    ModbusFunction Function = (ModbusFunction)lstResponse[1];
                                    int StartAddress = lstResponse[2] << 8 | lstResponse[3];

                                    switch (Function)
                                    {
                                        case ModbusFunction.BITREAD_F1:
                                        case ModbusFunction.BITREAD_F2:
                                            #region BitRead
                                            if (lstResponse.Count == 8)
                                            {
                                                byte hi = 0xFF, lo = 0xFF;
                                                ModbusCRC.GetCRC(lstResponse, 0, 6, ref hi, ref lo);
                                                if (lstResponse[6] == hi && lstResponse[7] == lo)
                                                {
                                                    if (BitReadRequest != null)
                                                    {
                                                        var args = new BitReadRequestArgs(lstResponse.ToArray());
                                                        BitReadRequest.Invoke(this, args);

                                                        if (args.Success && args.ResponseData != null && args.ResponseData.Length == args.Length)
                                                        {
                                                            #region MakeData
                                                            List<byte> Datas = new List<byte>();
                                                            int nlen = args.ResponseData.Length / 8;
                                                            nlen += args.ResponseData.Length % 8 == 0 ? 0 : 1;
                                                            for (int i = 0; i < nlen; i++)
                                                            {
                                                                byte val = 0;
                                                                for (int j = i * 8, nTemp = 0; j < args.ResponseData.Length && j < i * 8 + 8; j++, nTemp++)
                                                                    if (args.ResponseData[j])
                                                                        val |= Convert.ToByte(Math.Pow(2, nTemp));
                                                                Datas.Add(val);
                                                            }
                                                            #endregion
                                                            #region Serial Write
                                                            List<byte> ret = new List<byte>();
                                                            ret.Add((byte)Slave);
                                                            ret.Add((byte)Function);
                                                            ret.Add((byte)Datas.Count);
                                                            ret.AddRange(Datas.ToArray());
                                                            byte nhi = 0xFF, nlo = 0xFF;
                                                            ModbusCRC.GetCRC(ret, 0, ret.Count, ref nhi, ref nlo);
                                                            ret.Add(nhi);
                                                            ret.Add(nlo);
                                                            byte[] send = ret.ToArray();
                                                            ser.Write(send, 0, send.Length);
                                                            ser.BaseStream.Flush();
                                                            #endregion
                                                            ok = true;
                                                        }
                                                    }
                                                    lstResponse.Clear();
                                                }
                                            }
                                            #endregion
                                            break;
                                        case ModbusFunction.WORDREAD_F3:
                                        case ModbusFunction.WORDREAD_F4:
                                            #region WordRead
                                            if (lstResponse.Count == 8)
                                            {
                                                byte hi = 0xFF, lo = 0xFF;
                                                ModbusCRC.GetCRC(lstResponse, 0, 6, ref hi, ref lo);
                                                if (lstResponse[6] == hi && lstResponse[7] == lo)
                                                {
                                                    if (WordReadRequest != null)
                                                    {
                                                        var args = new WordReadRequestArgs(lstResponse.ToArray());
                                                        WordReadRequest.Invoke(this, args);

                                                        if (args.Success && args.ResponseData != null && args.ResponseData.Length == args.Length)
                                                        {
                                                            #region MakeData
                                                            List<byte> Datas = new List<byte>();
                                                            for (int i = 0; i < args.ResponseData.Length; i++)
                                                            {
                                                                Datas.Add((byte)((args.ResponseData[i] & 0xFF00) >> 8));
                                                                Datas.Add((byte)(args.ResponseData[i] & 0x00FF));
                                                            }
                                                            #endregion
                                                            #region Serial Write
                                                            List<byte> ret = new List<byte>();
                                                            ret.Add((byte)Slave);
                                                            ret.Add((byte)Function);
                                                            ret.Add((byte)Datas.Count);
                                                            ret.AddRange(Datas.ToArray());
                                                            byte nhi = 0xFF, nlo = 0xFF;
                                                            ModbusCRC.GetCRC(ret, 0, ret.Count, ref nhi, ref nlo);
                                                            ret.Add(nhi);
                                                            ret.Add(nlo);
                                                            byte[] send = ret.ToArray();
                                                            ser.Write(send, 0, send.Length);
                                                            ser.BaseStream.Flush();
                                                            #endregion
                                                        }
                                                    }
                                                    ok = true;
                                                }
                                            }
                                            #endregion
                                            break;
                                        case ModbusFunction.BITWRITE_F5:
                                            #region BitWrite
                                            if (lstResponse.Count == 8)
                                            {
                                                byte hi = 0xFF, lo = 0xFF;
                                                ModbusCRC.GetCRC(lstResponse, 0, 6, ref hi, ref lo);
                                                if (lstResponse[6] == hi && lstResponse[7] == lo)
                                                {
                                                    if (BitWriteRequest != null)
                                                    {
                                                        var args = new BitWriteRequestArgs(lstResponse.ToArray());
                                                        BitWriteRequest.Invoke(this, args);

                                                        if (args.Success)
                                                        {
                                                            #region Serial Write
                                                            int nv = args.WriteValue ? 0xFF00 : 0;
                                                            List<byte> ret = new List<byte>();
                                                            ret.Add((byte)args.Slave);
                                                            ret.Add((byte)args.Function);
                                                            ret.Add((byte)((args.StartAddress & 0xFF00) >> 8));
                                                            ret.Add((byte)(args.StartAddress & 0x00FF));
                                                            ret.Add((byte)((nv & 0xFF00) >> 8));
                                                            ret.Add((byte)(nv & 0x00FF));
                                                            byte nhi = 0xFF, nlo = 0xFF;
                                                            ModbusCRC.GetCRC(ret, 0, ret.Count, ref nhi, ref nlo);
                                                            ret.Add(nhi);
                                                            ret.Add(nlo);
                                                            byte[] send = ret.ToArray();
                                                            ser.Write(send, 0, send.Length);
                                                            ser.BaseStream.Flush();
                                                            #endregion
                                                        }
                                                    }
                                                    ok = true;
                                                }
                                            }
                                            #endregion
                                            break;
                                        case ModbusFunction.WORDWRITE_F6:
                                            #region WordWrite
                                            if (lstResponse.Count == 8)
                                            {
                                                byte hi = 0xFF, lo = 0xFF;
                                                ModbusCRC.GetCRC(lstResponse, 0, 6, ref hi, ref lo);
                                                if (lstResponse[6] == hi && lstResponse[7] == lo)
                                                {
                                                    if (WordWriteRequest != null)
                                                    {
                                                        var args = new WordWriteRequestArgs(lstResponse.ToArray());
                                                        WordWriteRequest.Invoke(this, args);

                                                        if (args.Success)
                                                        {
                                                            #region Serial Write
                                                            List<byte> ret = new List<byte>();
                                                            ret.Add((byte)args.Slave);
                                                            ret.Add((byte)args.Function);
                                                            ret.Add((byte)((args.StartAddress & 0xFF00) >> 8));
                                                            ret.Add((byte)(args.StartAddress & 0x00FF));
                                                            ret.Add((byte)((args.WriteValue & 0xFF00) >> 8));
                                                            ret.Add((byte)(args.WriteValue & 0x00FF));
                                                            byte nhi = 0xFF, nlo = 0xFF;
                                                            ModbusCRC.GetCRC(ret, 0, ret.Count, ref nhi, ref nlo);
                                                            ret.Add(nhi);
                                                            ret.Add(nlo);
                                                            byte[] send = ret.ToArray();
                                                            ser.Write(send, 0, send.Length);
                                                            ser.BaseStream.Flush();
                                                            #endregion
                                                        }
                                                    }
                                                    ok = true;
                                                }
                                            }
                                            #endregion
                                            break;
                                        case ModbusFunction.MULTIBITWRITE_F15:
                                            #region MultiBitWrite
                                            if (lstResponse.Count >= 7)
                                            {
                                                int Length = lstResponse[4] << 8 | lstResponse[5];
                                                int ByteCount = lstResponse[6];
                                                if (lstResponse.Count >= 9 + ByteCount)
                                                {
                                                    byte hi = 0xFF, lo = 0xFF;
                                                    ModbusCRC.GetCRC(lstResponse, 0, 7 + ByteCount, ref hi, ref lo);
                                                    if (lstResponse[9 + ByteCount - 2] == hi && lstResponse[9 + ByteCount - 1] == lo)
                                                    {
                                                        var args = new MultiBitWriteRequestArgs(lstResponse.ToArray());
                                                        if (MultiBitWriteRequest != null)
                                                        {
                                                            MultiBitWriteRequest.Invoke(this, args);

                                                            if (args.Success)
                                                            {
                                                                #region Serial Write
                                                                List<byte> ret = new List<byte>();
                                                                ret.Add((byte)args.Slave);
                                                                ret.Add((byte)args.Function);
                                                                ret.Add((byte)((args.StartAddress & 0xFF00) >> 8));
                                                                ret.Add((byte)(args.StartAddress & 0x00FF));
                                                                ret.Add((byte)((args.Length & 0xFF00) >> 8));
                                                                ret.Add((byte)(args.Length & 0x00FF));
                                                                byte nhi = 0xFF, nlo = 0xFF;
                                                                ModbusCRC.GetCRC(ret, 0, ret.Count, ref nhi, ref nlo);
                                                                ret.Add(nhi);
                                                                ret.Add(nlo);
                                                                byte[] send = ret.ToArray();
                                                                ser.Write(send, 0, send.Length);
                                                                ser.BaseStream.Flush();
                                                                #endregion
                                                            }
                                                        }
                                                        ok = true;
                                                    }
                                                }
                                            }
                                            #endregion
                                            break;
                                        case ModbusFunction.MULTIWORDWRITE_F16:
                                            #region MultiWordWrite
                                            if (lstResponse.Count >= 7)
                                            {
                                                int Length = lstResponse[4] << 8 | lstResponse[5];
                                                int ByteCount = lstResponse[6];
                                                if (lstResponse.Count >= 9 + ByteCount)
                                                {
                                                    byte hi = 0xFF, lo = 0xFF;
                                                    ModbusCRC.GetCRC(lstResponse, 0, 7 + ByteCount, ref hi, ref lo);
                                                    if (lstResponse[9 + ByteCount - 2] == hi && lstResponse[9 + ByteCount - 1] == lo)
                                                    {
                                                        if (MultiWordWriteRequest != null)
                                                        {
                                                            var args = new MultiWordWriteRequestArgs(lstResponse.ToArray());
                                                            MultiWordWriteRequest.Invoke(this, args);

                                                            if (args.Success)
                                                            {
                                                                #region Serial Write
                                                                List<byte> ret = new List<byte>();
                                                                ret.Add((byte)args.Slave);
                                                                ret.Add((byte)args.Function);
                                                                ret.Add((byte)((args.StartAddress & 0xFF00) >> 8));
                                                                ret.Add((byte)(args.StartAddress & 0x00FF));
                                                                ret.Add((byte)((args.Length & 0xFF00) >> 8));
                                                                ret.Add((byte)(args.Length & 0x00FF));
                                                                byte nhi = 0xFF, nlo = 0xFF;
                                                                ModbusCRC.GetCRC(ret, 0, ret.Count, ref nhi, ref nlo);
                                                                ret.Add(nhi);
                                                                ret.Add(nlo);
                                                                byte[] send = ret.ToArray();
                                                                ser.Write(send, 0, send.Length);
                                                                ser.BaseStream.Flush();
                                                                #endregion
                                                            }
                                                        }
                                                        ok = true;
                                                    }
                                                }
                                            }
                                            #endregion
                                            break;
                                        case ModbusFunction.WORDBITSET_F26:
                                            #region WordBitSet
                                            if (lstResponse.Count == 9)
                                            {
                                                byte hi = 0xFF, lo = 0xFF;
                                                ModbusCRC.GetCRC(lstResponse, 0, 7, ref hi, ref lo);
                                                if (lstResponse[7] == hi && lstResponse[8] == lo)
                                                {
                                                    if (WordBitSetRequest != null)
                                                    {
                                                        var args = new WordBitSetRequestArgs(lstResponse.ToArray());
                                                        WordBitSetRequest.Invoke(this, args);

                                                        if (args.Success)
                                                        {
                                                            #region Serial Write
                                                            int nv = args.WriteValue ? 0xFF00 : 0;
                                                            List<byte> ret = new List<byte>();
                                                            ret.Add((byte)args.Slave);
                                                            ret.Add((byte)args.Function);
                                                            ret.Add((byte)((args.StartAddress & 0xFF00) >> 8));
                                                            ret.Add((byte)(args.StartAddress & 0x00FF));
                                                            ret.Add((byte)((nv & 0xFF00) >> 8));
                                                            ret.Add((byte)(nv & 0x00FF));
                                                            byte nhi = 0xFF, nlo = 0xFF;
                                                            ModbusCRC.GetCRC(ret, 0, ret.Count, ref nhi, ref nlo);
                                                            ret.Add(nhi);
                                                            ret.Add(nlo);
                                                            byte[] send = ret.ToArray();
                                                            ser.Write(send, 0, send.Length);
                                                            ser.BaseStream.Flush();
                                                            #endregion
                                                        }
                                                    }
                                                    ok = true;
                                                }
                                            }
                                            #endregion
                                            break;
                                    }
                                }
                                #endregion

                                #region buffer clear
                                if (ok || (DateTime.Now - prev).TotalMilliseconds >= 20 && lstResponse.Count > 0)
                                {
                                    lstResponse.Clear();
                                    ser.DiscardInBuffer();
                                    ser.BaseStream.Flush();
                                }
                                #endregion
                            }
                            catch (SchedulerStopException) { break; }
                            await Task.Delay(1);
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
        #endregion
        #region Stop
        public void Stop()
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
        #endregion

        #region Static Method
        #region ProcessBitReads
        public static void ProcessBitReads(BitReadRequestArgs args, int BaseAddress, bool[] BaseArray)
        {
            var BA = new bool[Convert.ToInt32(Math.Ceiling(BaseArray.Length / 8.0) * 8.0)];
            Array.Copy(BaseArray, BA, BaseArray.Length);
            if (args.StartAddress >= BaseAddress && args.StartAddress + args.Length < BaseAddress + BA.Length)
            {
                var ret = new bool[args.Length];
                Array.Copy(BA, args.StartAddress - BaseAddress, ret, 0, args.Length);
                args.ResponseData = ret;
                args.Success = true;
            }
        }
        #endregion
        #region ProcessWordReads
        public static void ProcessWordReads(WordReadRequestArgs args, int BaseAddress, int[] BaseArray)
        {
            if (args.StartAddress >= BaseAddress && args.StartAddress + args.Length < BaseAddress + BaseArray.Length)
            {
                var ret = new int[args.Length];
                Array.Copy(BaseArray, args.StartAddress - BaseAddress, ret, 0, args.Length);
                args.ResponseData = ret;
                args.Success = true;
            }
        }
        #endregion
        #region ProcessBitWrite
        public static void ProcessBitWrite(BitWriteRequestArgs args, int BaseAddress, bool[] BaseArray)
        {
            if (args.StartAddress >= BaseAddress && args.StartAddress < BaseAddress + BaseArray.Length)
            {
                BaseArray[args.StartAddress - BaseAddress] = args.WriteValue;
                args.Success = true;
            }
        }
        #endregion
        #region ProcessWordWrite
        public static void ProcessWordWrite(WordWriteRequestArgs args, int BaseAddress, int[] BaseArray)
        {
            if (args.StartAddress >= BaseAddress && args.StartAddress < BaseAddress + BaseArray.Length)
            {
                BaseArray[args.StartAddress - BaseAddress] = args.WriteValue;
                args.Success = true;
            }
        }
        #endregion
        #region ProcessMultiBitWrite
        public static void ProcessMultiBitWrite(MultiBitWriteRequestArgs args, int BaseAddress, bool[] BaseArray)
        {
            if (args.StartAddress >= BaseAddress && args.StartAddress + args.Length < BaseAddress + BaseArray.Length)
            {
                for (int i = 0; i < args.WriteValues.Length; i++) BaseArray[args.StartAddress - BaseAddress + i] = args.WriteValues[i];
                args.Success = true;
            }
        }
        #endregion
        #region ProcessMultiWordWrite
        public static void ProcessMultiWordWrite(MultiWordWriteRequestArgs args, int BaseAddress, int[] BaseArray)
        {
            if (args.StartAddress >= BaseAddress && args.StartAddress + args.Length < BaseAddress + BaseArray.Length)
            {
                for (int i = 0; i < args.WriteValues.Length; i++) BaseArray[args.StartAddress - BaseAddress + i] = args.WriteValues[i];
                args.Success = true;
            }
        }
        #endregion
        #region ProcessWordBitSet
        public static void ProcessWordBitSet(WordBitSetRequestArgs args, int BaseAddress, int[] BaseArray)
        {
            if (args.StartAddress >= BaseAddress && args.StartAddress < BaseAddress + BaseArray.Length && args.BitIndex >= 0 && args.BitIndex < 16)
            {
                var p = Convert.ToInt32(Math.Pow(2, args.BitIndex));
                if (args.WriteValue) BaseArray[args.StartAddress - BaseAddress] |= p;
                else BaseArray[args.StartAddress - BaseAddress] &= (ushort)~p;
                args.Success = true;
            }
        }
        #endregion
        #endregion
    }
}
