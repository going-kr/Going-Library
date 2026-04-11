using Going.Basis.Communications.LS;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace Going.Basis.Communications.Mqtt
{
    #region enum : MQQos
    /// <summary>
    /// MQTT 메시지의 서비스 품질(QoS) 레벨을 나타내는 열거형
    /// </summary>
    public enum MQQos : byte
    {
        /// <summary>최소 한 번 전달 (QoS 1)</summary>
        LeastOnce = MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE,
        /// <summary>최대 한 번 전달 (QoS 0)</summary>
        MostOnce = MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE,
        /// <summary>정확히 한 번 전달 (QoS 2)</summary>
        ExactlyOnce = MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE,
        /// <summary>구독 실패</summary>
        GrantedFailure = MqttMsgBase.QOS_LEVEL_GRANTED_FAILURE
    }
    #endregion
    #region class : MQSubscribe
    /// <summary>
    /// MQTT 구독 정보를 나타내는 클래스
    /// </summary>
    /// <param name="topic">구독할 토픽 경로</param>
    /// <param name="qos">서비스 품질 레벨. 기본값은 MostOnce(QoS 0)</param>
    public class MQSubscribe(string topic, MQQos qos = MQQos.MostOnce)
    {
        /// <summary>구독 토픽 경로를 가져옵니다.</summary>
        public string Topic { get; private set; } = topic;
        /// <summary>서비스 품질 레벨을 가져옵니다.</summary>
        public MQQos Qos { get; private set; } = qos;
    }
    #endregion
    #region class : MQReceiveArgs
    /// <summary>
    /// MQTT 메시지 수신 시 발생하는 이벤트 인자
    /// </summary>
    /// <param name="Topic">수신된 메시지의 토픽</param>
    /// <param name="Datas">수신된 메시지의 페이로드 바이트 배열</param>
    public class MQReceiveArgs(string Topic, byte[] Datas) : EventArgs
    {
        /// <summary>수신된 메시지의 토픽을 가져옵니다.</summary>
        public string Topic { get; private set; } = Topic;
        /// <summary>수신된 메시지의 페이로드 바이트 배열을 가져옵니다.</summary>
        public byte[] Datas { get; private set; } = Datas;
    }
    #endregion

    /// <summary>
    /// MQTT 브로커에 연결하여 메시지를 발행(Publish) 및 구독(Subscribe)하는 클라이언트 클래스.
    /// M2Mqtt 라이브러리를 기반으로 자동 재연결 기능을 제공합니다.
    /// </summary>
    public class MQClient
    {
        #region Properties
        /// <summary>MQTT 브로커의 호스트 이름 또는 IP 주소를 가져오거나 설정합니다. 기본값은 "127.0.0.1"입니다.</summary>
        public string BrokerHostName { get; set; } = "127.0.0.1";
        /// <summary>클라이언트가 시작되었는지 여부를 가져옵니다.</summary>
        public bool IsStart { get; private set; }
        /// <summary>MQTT 브로커에 연결되어 있는지 여부를 가져옵니다.</summary>
        public bool IsConnected => client != null ? client.IsConnected : false;

        /// <summary>MQTT 클라이언트 식별자를 가져옵니다.</summary>
        public string? ClientID { get; private set; }
        /// <summary>인증에 사용되는 사용자 이름을 가져옵니다.</summary>
        public string? UserName { get; private set; }
        /// <summary>인증에 사용되는 비밀번호를 가져옵니다.</summary>
        public string? Password { get; private set; }
        /// <summary>현재 등록된 구독 목록을 가져옵니다.</summary>
        public List<MQSubscribe> Subscribes { get; } = [];

        /// <summary>사용자 정의 태그 데이터를 가져오거나 설정합니다.</summary>
        public object? Tag { get; set; } = null;
        #endregion

        #region Member Variable
        MqttClient? client;

        Task? task;
        CancellationTokenSource? cancel;
        #endregion

        #region Event
        /// <summary>MQTT 브로커에 연결되었을 때 발생합니다.</summary>
        public event EventHandler? Connected;
        /// <summary>MQTT 브로커와의 연결이 끊어졌을 때 발생합니다.</summary>
        public event EventHandler? Disconnected;
        /// <summary>구독 중인 토픽에서 메시지를 수신했을 때 발생합니다.</summary>
        public event EventHandler<MQReceiveArgs>? Received;
        #endregion

        #region Constructor
        /// <summary>
        /// MQClient 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        public MQClient()
        {
        }
        #endregion

        #region Method
        #region Start / Stop
        /// <summary>
        /// 자동 생성된 클라이언트 ID로 MQTT 브로커에 연결을 시작합니다.
        /// </summary>
        public void Start() => Start(null, null, null);
        /// <summary>
        /// 지정한 클라이언트 ID로 MQTT 브로커에 연결을 시작합니다.
        /// </summary>
        /// <param name="clientID">MQTT 클라이언트 식별자. null이면 GUID가 자동 생성됩니다.</param>
        public void Start(string? clientID) => Start(clientID, null, null);
        /// <summary>
        /// 지정한 클라이언트 ID와 인증 정보로 MQTT 브로커에 연결을 시작합니다.
        /// 연결이 끊어지면 자동으로 재연결을 시도합니다.
        /// </summary>
        /// <param name="clientID">MQTT 클라이언트 식별자. null이면 GUID가 자동 생성됩니다.</param>
        /// <param name="userName">인증 사용자 이름. null이면 인증 없이 연결합니다.</param>
        /// <param name="password">인증 비밀번호</param>
        public void Start(string? clientID, string? userName, string? password)
        {
            try
            {
                if (!IsStart)
                {
                    cancel = new CancellationTokenSource();
                    task = Task.Run(async () =>
                    {
                        ClientID = clientID ?? Guid.NewGuid().ToString();
                        UserName = userName;
                        Password = password;
                        IsStart = true;

                        var token = cancel.Token;

                        double sec = 1.0;
                        while (!token.IsCancellationRequested && IsStart)
                        {
                            if (client != null)
                            {
                                if (client.IsConnected)
                                {
                                    sec = 1.0;
                                    #region Alive
                                    client.Publish("/test-alive", [0]);
                                    #endregion
                                }
                                else
                                {
                                    sec = 1.0;
                                    #region Reconnect
                                    try
                                    {
                                        int? result = null;
                                        if (!string.IsNullOrWhiteSpace(Password) && !string.IsNullOrWhiteSpace(UserName)) result = client.Connect(ClientID, UserName, Password);
                                        else result = client.Connect(ClientID);

                                        if (result == 0)
                                        {
                                            if (Subscribes.Count > 0)
                                                client.Subscribe(Subscribes.Select(x => x.Topic).ToArray(), Subscribes.Select(x => (byte)x.Qos).ToArray());

                                            Connected?.Invoke(this, EventArgs.Empty);
                                            client.ConnectionClosed += (o, s) => Disconnected?.Invoke(this, EventArgs.Empty);
                                            client.MqttMsgPublishReceived += (o, s) => Received?.Invoke(this, new MQReceiveArgs(s.Topic, s.Message));
                                        }
                                    }
                                    catch { }
                                    #endregion
                                }
                            }
                            else
                            {
                                sec = 0.1;
                                #region new
                                if (BrokerHostName != null)
                                {
                                    client = new MqttClient(BrokerHostName);
                                }
                                #endregion
                            }

                            await Task.Delay(TimeSpan.FromSeconds(sec), token);
                        }


                        if (client != null && client.IsConnected)
                        {
                            try { client.Disconnect(); }
                            catch { }
                        }
                        client = null;

                    }, cancel.Token);
                }
            }
            catch { }
        }

        /// <summary>
        /// MQTT 브로커와의 연결을 종료하고 클라이언트를 정지합니다.
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

        #region Publish
        /// <summary>
        /// 지정한 토픽에 바이트 배열 메시지를 발행합니다.
        /// </summary>
        /// <param name="Topic">발행할 토픽 경로</param>
        /// <param name="Data">발행할 메시지 바이트 배열</param>
        /// <param name="qos">서비스 품질 레벨. 기본값은 MostOnce(QoS 0)</param>
        /// <param name="retain">메시지 보존 여부</param>
        public void Publish(string Topic, byte[] Data, MQQos qos = MQQos.MostOnce, bool retain = false)
        {
            if (client != null && client.IsConnected)
                client.Publish(Topic, Data, (byte)qos, retain);
        }

        /// <summary>
        /// 지정한 토픽에 문자열 메시지를 UTF-8로 인코딩하여 발행합니다.
        /// </summary>
        /// <param name="Topic">발행할 토픽 경로</param>
        /// <param name="Data">발행할 메시지 문자열</param>
        /// <param name="qos">서비스 품질 레벨. 기본값은 MostOnce(QoS 0)</param>
        /// <param name="retain">메시지 보존 여부</param>
        public void Publish(string Topic, string Data, MQQos qos = MQQos.MostOnce, bool retain = false) => Publish(Topic, Encoding.UTF8.GetBytes(Data), qos, retain);
        #endregion

        #region Subscribe
        /// <summary>
        /// 지정한 구독 정보로 토픽을 구독합니다. 이미 연결되어 있으면 즉시 구독이 적용됩니다.
        /// </summary>
        /// <param name="sub">구독할 토픽과 QoS 정보</param>
        public void Subscribe(MQSubscribe sub)
        {
            Subscribes.Add(sub);
            if (client != null && client.IsConnected)
                client.Subscribe([sub.Topic], [(byte)sub.Qos]);
        }
        /// <summary>
        /// 지정한 토픽을 기본 QoS(MostOnce)로 구독합니다.
        /// </summary>
        /// <param name="Topic">구독할 토픽 경로</param>
        public void Subscribe(string Topic) => Subscribe(new MQSubscribe(Topic));
        /// <summary>
        /// 지정한 토픽을 지정한 QoS 레벨로 구독합니다.
        /// </summary>
        /// <param name="Topic">구독할 토픽 경로</param>
        /// <param name="Qos">서비스 품질 레벨</param>
        public void Subscribe(string Topic, MQQos Qos) => Subscribe(new MQSubscribe(Topic, Qos));
        #endregion

        #region Unsubscribe
        /// <summary>
        /// 지정한 토픽의 구독을 해제합니다.
        /// </summary>
        /// <param name="Topic">구독 해제할 토픽 경로</param>
        public void Unsubscribe(string Topic)
        {
            var v = Subscribes.FirstOrDefault(x => x.Topic == Topic);
            if (v != null)
            {
                Subscribes.Remove(v);
                if (client != null && client.IsConnected) client.Unsubscribe([v.Topic]);
            }
        }

        /// <summary>
        /// 모든 토픽의 구독을 해제하고 구독 목록을 비웁니다.
        /// </summary>
        public void UnsubscribeClear()
        {
            if (Subscribes.Count > 0)
            {
                var tops = Subscribes.Select(x => x.Topic).ToArray();
                Subscribes.Clear();
                if (client != null && client.IsConnected) client.Unsubscribe(tops);
            }
        }
        #endregion
        #endregion

        #region Static Method
        /// <summary>
        /// 지정한 MQTT 브로커에 테스트 연결을 시도합니다.
        /// </summary>
        /// <param name="BrokerIP">브로커 IP 주소</param>
        /// <param name="ClientID">테스트에 사용할 클라이언트 식별자</param>
        /// <returns>연결에 성공하면 true, 실패하면 false</returns>
        public static bool Test(string BrokerIP, string ClientID)
        {
            bool ret = false;
            try
            {
                var client = new uPLibrary.Networking.M2Mqtt.MqttClient(BrokerIP);
                if (client.Connect(ClientID) == 0)
                {
                    ret = true;
                    System.Threading.Thread.Sleep(500);
                    client.Disconnect();
                }
            }
            catch (Exception) { }
            return ret;
        }
        #endregion
    }
}
