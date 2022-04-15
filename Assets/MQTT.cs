using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using M2MqttUnity;
using Newtonsoft.Json.Linq;
using System.Linq;
using Newtonsoft.Json;

namespace Dashboard
{
    public class Status_Data
    {
        public List<data_ss> data_ss { get; set; }
        public Status_Data()
        {
            this.data_ss = new List<data_ss>();
        }
        
    }

    public class data_ss
    {
        public string ss_name { get; set; }
        public string ss_unit { get; set; }
        public string ss_value { get; set; }

        public data_ss(string name, string unit, string value)
        {
            ss_name = name;
            ss_unit = unit;
            ss_value = value;
        }
        
    }

    public class Config_Data
    {
        public string device { get; set; }
        public string status { get; set; }

        public Config_Data(string theDevice, string theStatus)
        {
            this.device = theDevice;
            this.status = theStatus;
        }
    }

    public class MQTT : M2MqttUnityClient
    {
        public List<string> topics = new List<string>();

        private List<string> eventMessages = new List<string>();
        [SerializeField]
        public Status_Data _status_data;
        [SerializeField]
        public Config_Data _config_data;

        public data_ss temperature;
        public data_ss humidity;
        public System.Random rnd;

        public override void Connect()
        {
            base.Connect();
            GameObject.Find("Manager").GetComponent<Manager>().ClearErrorMessage();
            this.brokerAddress = "";
            this.mqttUserName = "";
            this.mqttPassword = "";
            GameObject g = GameObject.Find("Manager");
            this.brokerAddress = g.GetComponent<Manager>().GetBrokerText();
            this.mqttUserName = g.GetComponent<Manager>().GetUsernameText();
            this.mqttPassword = g.GetComponent<Manager>().GetPasswordText();
        }

        protected override void OnConnecting()
        {

            base.OnConnecting();
        }

        public override void Disconnect()
        {
            base.Disconnect();
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            GameObject g = GameObject.Find("Manager");
            g.GetComponent<Manager>().SwitchLayer();
            rnd = new System.Random();
            temperature = new data_ss("temp", "°C", rnd.Next(-30, 100).ToString());
            humidity = new data_ss("humi", "%", rnd.Next(1, 100).ToString());

            _status_data = new Status_Data();
            _status_data.data_ss.Add(temperature);
            _status_data.data_ss.Add(humidity);
            string msg_test = JsonConvert.SerializeObject(_status_data);
            Debug.Log("The bytes: " + msg_test);
            client.Publish(topics[0], System.Text.Encoding.UTF8.GetBytes(msg_test), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
            SubscribeTopics();
        }

        protected override void SubscribeTopics()
        {
            foreach (string topic in topics)
            {
                if (topic != "")
                {
                    client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
                }
            }
        }

        protected override void UnsubscribeTopics()
        {
            foreach (string topic in topics)
            {
                if (topic != "")
                {
                    client.Unsubscribe(new string[] { topic });
                }
            }
        }

        protected override void OnConnectionFailed(string ErrorMessage)
        {
            Debug.Log("CONNECTION FAILED! " + ErrorMessage);
            GameObject g = GameObject.Find("Manager");
            g.GetComponent<Manager>().DisplayErrorMessage();
        }

        protected override void OnDisconnected()
        {
            Debug.Log("Disconnected.");
            GameObject g = GameObject.Find("Manager");
            g.GetComponent<Manager>().SwitchLayer();
        }

        protected override void OnConnectionLost()
        {
            Debug.Log("CONNECTION LOST!");
            Disconnect();
            GameObject g = GameObject.Find("Manager");
            g.GetComponent<Manager>().SwitchLayer();
        }

        public void SetEncrypted(bool isEncrypted)
        {
            this.isEncrypted = isEncrypted;
        }

        protected override void DecodeMessage(string topic, byte[] message)
        {
            string msg = System.Text.Encoding.UTF8.GetString(message);
            Debug.Log("Received: " + msg);
            //StoreMessage(msg);
            if (topic == topics[0])
                ProcessMessageStatus(msg);
        }

        private void ProcessMessageStatus(string msg)
        {
            _status_data = JsonConvert.DeserializeObject<Status_Data>(msg);
            GameObject.Find("Manager").GetComponent<Manager>().Update_Status(_status_data);

        }

        private void OnDestroy()
        {
            Disconnect();
        }

        private void OnValidate()
        {
            //if (autoTest)
            //{
            //    autoConnect = true;
            //}
        }

        public void PublishConfigLED()
        {
            if (GameObject.Find("Manager").GetComponent<Manager>().getLEDToggle())
            {
                _config_data = new Config_Data("LED", "ON");
            }
            else
            {
                _config_data = new Config_Data("LED", "OFF");
            }
            string msg_config = JsonConvert.SerializeObject(_config_data);
            client.Publish(topics[1], System.Text.Encoding.UTF8.GetBytes(msg_config), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
            Debug.Log("Published LED configurations.");
        }

        public void PublishConfigPump()
        {
            if (GameObject.Find("Manager").GetComponent<Manager>().getPumpToggle())
            {
                _config_data = new Config_Data("PUMP", "ON");
            }
            else
            {
                _config_data = new Config_Data("PUMP", "OFF");
            }
            string msg_config = JsonConvert.SerializeObject(_config_data);
            client.Publish(topics[2], System.Text.Encoding.UTF8.GetBytes(msg_config), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
            Debug.Log("Published Pump configurations.");
        }

        protected override void Start()
        {

            base.Start();
        }
    }
}
