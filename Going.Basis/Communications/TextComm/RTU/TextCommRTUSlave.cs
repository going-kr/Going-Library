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
    /// <summary>
    /// 시리얼(RTU) 기반 STX/ETX 텍스트 통신 슬레이브 클래스.
    /// 시리얼 포트에서 마스터의 요청을 수신하고 이벤트를 통해 응답을 처리합니다.
    /// </summary>
    public class TextCommRTUSlave
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
        /// <summary>시리얼 포트 이름을 가져오거나 설정합니다. (예: "COM1")</summary>
        public string Port { get => ser.PortName; set => ser.PortName = value; }
        /// <summary>통신 속도(bps)를 가져오거나 설정합니다.</summary>
        public int Baudrate { get => ser.BaudRate; set => ser.BaudRate = value; }
        /// <summary>패리티 비트 설정을 가져오거나 설정합니다.</summary>
        public Parity Parity { get => ser.Parity; set => ser.Parity = value; }
        /// <summary>데이터 비트 수를 가져오거나 설정합니다.</summary>
        public int DataBits { get => ser.DataBits; set => ser.DataBits = value; }
        /// <summary>스톱 비트 설정을 가져오거나 설정합니다.</summary>
        public StopBits StopBits { get => ser.StopBits; set => ser.StopBits = value; }

        /// <summary>시리얼 포트가 열려 있는지 여부를 가져옵니다.</summary>
        public bool IsOpen => ser.IsOpen;
        /// <summary>슬레이브가 실행 중인지 여부를 가져옵니다.</summary>
        public bool IsStart { get; private set; }

        /// <summary>메시지 문자열의 인코딩을 가져오거나 설정합니다. 기본값은 ASCII입니다.</summary>
        public Encoding MessageEncoding { get; set; } = Encoding.ASCII;

        /// <summary>사용자 정의 태그 데이터를 가져오거나 설정합니다.</summary>
        public object? Tag { get; set; } = null;
        #endregion

        #region Member Variable
        SerialPort ser = new SerialPort() { PortName = "COM1", BaudRate = 115200 };

        Task? task;
        CancellationTokenSource? cancel;
        #endregion

        #region Event
        /// <summary>마스터로부터 메시지 요청을 수신했을 때 발생합니다.</summary>
        public event EventHandler<MessageRequestArgs>? MessageRequest;

        /// <summary>시리얼 포트가 열렸을 때 발생합니다.</summary>
        public event EventHandler? DeviceOpened;
        /// <summary>시리얼 포트가 닫혔을 때 발생합니다.</summary>
        public event EventHandler? DeviceClosed;
        #endregion

        #region Constructor
        /// <summary>
        /// TextCommRTUSlave 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        public TextCommRTUSlave()
        {
        }
        #endregion

        #region Method
        #region Start
        /// <summary>
        /// 시리얼 포트를 열고 마스터 요청 수신을 시작합니다.
        /// 수신된 요청은 MessageRequest 이벤트를 통해 처리됩니다.
        /// </summary>
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

                                    await Task.Delay(10, token);
                                }
                                catch (SchedulerStopException) { break; }
                                catch (Exception) { }
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
        /// <summary>
        /// 슬레이브를 정지하고 시리얼 포트를 닫습니다.
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
        #endregion
    }
}
