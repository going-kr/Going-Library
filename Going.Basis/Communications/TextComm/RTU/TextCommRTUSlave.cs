using Going.Basis.Communications.LS;
using Going.Basis.Communications.Modbus;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Communications.TextComm.RTU
{
    public class TextCommRTUSlave
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
        public string Port { get => ser.PortName; set => ser.PortName = value; }
        public int Baudrate { get => ser.BaudRate; set => ser.BaudRate = value; }
        public Parity Parity { get => ser.Parity; set => ser.Parity = value; }
        public int DataBits { get => ser.DataBits; set => ser.DataBits = value; }
        public StopBits StopBits { get => ser.StopBits; set => ser.StopBits = value; }

        public bool IsOpen => ser.IsOpen;
        public bool IsStart { get; private set; }

        public Encoding MessageEncoding { get; set; } = Encoding.ASCII;

        public object? Tag { get; set; } = null;
        #endregion

        #region Member Variable
        SerialPort ser = new SerialPort() { PortName = "COM1", BaudRate = 115200 };

        Task? task;
        CancellationTokenSource? cancel;
        #endregion

        #region Event
        public event EventHandler<MessageRequestArgs>? MessageRequest;

        public event EventHandler? DeviceOpened;
        public event EventHandler? DeviceClosed;
        #endregion

        #region Constructor
        public TextCommRTUSlave()
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

                    do
                    {
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
                                            for (int i = 0; i < len; i++)
                                            {
                                                var v = baResponse[i];
                                                if (v == 0x02) lstResponse.Clear();
                                                lstResponse.Add(v);
                                            }
                                            prev = DateTime.Now;
                                        }
                                        catch (IOException) { throw new SchedulerStopException(); }
                                        catch (UnauthorizedAccessException) { throw new SchedulerStopException(); }
                                        catch (InvalidOperationException) { throw new SchedulerStopException(); }
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
                                                        ser.Write(snd.ToArray(), 0, snd.Length);
                                                        ser.BaseStream.Flush();
                                                    }
                                                }
                                            }

                                            ok = true;
                                        }
                                    }
                                    #endregion

                                    #region buffer clear
                                    if (ok || ((DateTime.Now - prev).TotalMilliseconds >= 50 && lstResponse.Count > 0))
                                    {
                                        lstResponse.Clear();
                                        ser.DiscardInBuffer();
                                        ser.BaseStream.Flush();
                                    }
                                    #endregion
                                }
                                catch (SchedulerStopException) { break; }
                                catch (Exception) { }
                                await Task.Delay(10, token);
                            }
                        }

                        if (ser.IsOpen)
                        {
                            ser.Close();
                            DeviceClosed?.Invoke(this, EventArgs.Empty);
                        }


                    } while (!token.IsCancellationRequested && IsStart);

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
        #endregion
    }
}
