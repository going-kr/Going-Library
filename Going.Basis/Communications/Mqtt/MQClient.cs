using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt;
using Going.Basis.Communications.LS;
using System.Runtime.ConstrainedExecution;

namespace Going.Basis.Communications.Mqtt
{
    #region enum : MQQos
    public enum MQQos : byte
    {
        LeastOnce = MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE,
        MostOnce = MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE,
        ExactlyOnce = MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE,
        GrantedFailure = MqttMsgBase.QOS_LEVEL_GRANTED_FAILURE
    }
    #endregion
    #region class : MQSubscribe
    public class MQSubscribe(string topic, MQQos qos = MQQos.MostOnce)
    {
        public string Topic { get; private set; } = topic;
        public MQQos Qos { get; private set; } = qos;
    }
    #endregion
    #region class : MQReceiveArgs
    public class MQReceiveArgs(string Topic, byte[] Datas) : EventArgs
    {
        public string Topic { get; private set; } = Topic;
        public byte[] Datas { get; private set; } = Datas;
    }
    #endregion

    public class MQClient
    {
        #region Properties
        public string BrokerHostName { get; set; } = "127.0.0.1";
        public bool IsStart { get; private set; }
        public bool IsConnected => client != null ? client.IsConnected : false;

        public string? ClientID { get; private set; }
        public string? UserName { get; private set; }
        public string? Password { get; private set; }
        public List<MQSubscribe> Subscribes { get; } = [];
        #endregion

        #region Member Variable
        MqttClient? client;

        Task? task;
        CancellationTokenSource? cancel;
        #endregion

        #region Event
        public event EventHandler? Connected;
        public event EventHandler? Disconnected;
        public event EventHandler<MQReceiveArgs>? Received;
        #endregion

        #region Constructor
        public MQClient()
        {
        }
        #endregion

        #region Method
        #region Start / Stop
        public void Start() => Start(null, null, null);
        public void Start(string? clientID) => Start(clientID, null, null);
        public void Start(string? clientID, string? userName, string? password)
        {
            try
            {
                if (!IsStart)
                {
                    ClientID = clientID ?? Guid.NewGuid().ToString();
                    UserName = userName;
                    Password = password;

                    cancel = new CancellationTokenSource();
                    task = Task.Run(async () =>
                    {
                        var token = cancel.Token;
                        var prev = DateTime.Now;
                        IsStart = true;
                        client = new MqttClient(BrokerHostName);
                        
                        while (!token.IsCancellationRequested && IsStart)
                        {
                            if (!client.IsConnected)
                            {
                                try
                                {
                                    if (!string.IsNullOrWhiteSpace(Password) && !string.IsNullOrWhiteSpace(UserName))
                                    {
                                        if (client.Connect(ClientID, UserName, Password) == 0)
                                        {
                                            #region Subscribe
                                            if (Subscribes.Count > 0)
                                            {
                                                var ts = Subscribes.Select(x => x.Topic).ToArray();
                                                var qs = Subscribes.Select(x => (byte)x.Qos).ToArray();
                                                client.Subscribe(ts, qs);
                                            }
                                            #endregion

                                            Connected?.Invoke(this, EventArgs.Empty);
                                            client.ConnectionClosed += (o, s) => Disconnected?.Invoke(this, EventArgs.Empty);
                                            client.MqttMsgPublishReceived += (o, s) => Received?.Invoke(this, new MQReceiveArgs(s.Topic, s.Message));
                                        }
                                    }
                                    else
                                    {
                                        if (client.Connect(ClientID) == 0)
                                        {
                                            #region Subscribe
                                            if (Subscribes.Count > 0)
                                            {
                                                var ts = Subscribes.Select(x => x.Topic).ToArray();
                                                var qs = Subscribes.Select(x => (byte)x.Qos).ToArray();
                                                client.Subscribe(ts, qs);
                                            }
                                            #endregion
                                            
                                            Connected?.Invoke(this, EventArgs.Empty);
                                            client.ConnectionClosed += (o, s) => Disconnected?.Invoke(this, EventArgs.Empty);
                                            client.MqttMsgPublishReceived += (o, s) => Received?.Invoke(this, new MQReceiveArgs(s.Topic, s.Message));
                                        }
                                    }
                                }
                                catch (Exception ex) { }
                                await Task.Delay(1000);
                            }
                            else
                            {
                                if ((DateTime.Now - prev).TotalSeconds > 1)
                                {
                                    client.Publish("/test-alive", [0]);
                                    prev = DateTime.Now;
                                }
                                await Task.Delay(10);
                            }
                        }
                       
                        if (client != null && client.IsConnected)
                        {
                            try { client.Disconnect(); }
                            catch (Exception ex) { }
                        }
                        client = null;

                    }, cancel.Token);
                }
            }
            catch (Exception ex) { }
        }

        private void Stop()
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

        #region Publish
        public void Publish(string Topic, byte[] Data, MQQos qos = MQQos.MostOnce, bool retain = false)
        {
            if (client != null && client.IsConnected)
                client.Publish(Topic, Data, (byte)qos, retain);
        }

        public void Publish(string Topic, string Data, MQQos qos = MQQos.MostOnce, bool retain = false) => Publish(Topic, Encoding.UTF8.GetBytes(Data), qos, retain);
        #endregion

        #region Subscribe
        public void Subscribe(MQSubscribe sub)
        {
            Subscribes.Add(sub);
            if (client != null && client.IsConnected)
                client.Subscribe([sub.Topic], [(byte)sub.Qos]);
        }
        public void Subscribe(string Topic) => Subscribe(new MQSubscribe(Topic));
        public void Subscribe(string Topic, MQQos Qos) => Subscribe(new MQSubscribe(Topic, Qos));
        #endregion

        #region Unsubscribe
        public void Unsubscribe(string Topic)
        {
            var v = Subscribes.FirstOrDefault(x => x.Topic == Topic);
            if (v != null)
            {
                Subscribes.Remove(v);
                if (client != null && client.IsConnected) client.Unsubscribe([v.Topic]);
            }
        }

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
