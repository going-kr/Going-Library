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

        public object? Tag { get; set; } = null;
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
                                    catch (Exception ex) { }
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
                            catch (Exception ex) { }
                        }
                        client = null;

                    }, cancel.Token);
                }
            }
            catch (Exception ex) { }
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
