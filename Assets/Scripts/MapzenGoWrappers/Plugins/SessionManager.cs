using System;
using UnityEngine;
using System.Text;
using uPLibrary.Networking.M2Mqtt;

namespace Assets.Scripts.Plugins
{
    /// <summary>
    /// The SessionManager is responsible for setting up a connection with the MQTT server, and managing view updates, room management, etc.
    /// </summary>
    public class SessionManager : Singleton<SessionManager>
    {
        AppState appState = AppState.Instance;
        MqttClient client;

        protected SessionManager()
        {
        } // guarantee this will be always a singleton only - can't use the constructor!

        public void Init()
        {
            Debug.Log("Initializing SessionManager");
            InitMqtt();
        }

        public void CreateSession(string name)
        {

        }

        protected void InitMqtt()
        {
#if (NETFX_CORE)
            client = new MqttClient(appState.Config.MqttServer, int.Parse(appState.Config.MqttPort), false);
#else
            client = new MqttClient(appState.Config.MqttServer, int.Parse(appState.Config.MqttPort));
#endif
            try
            {
                Debug.Log("Connecting to MQTT");
                client.Connect(Guid.NewGuid().ToString());
            }
            catch
            {
                Debug.LogError("Error connecting to MQTT");
            }
            if (client.IsConnected)
            {
                Debug.Log("Client is connected to MQTT");
                // register to message received 
                client.MqttMsgPublishReceived += (sender, e) =>
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        var msg = Encoding.UTF8.GetString(e.Message);
                        switch (e.Topic)
                        {
                            case "view":
                                SetView(msg);
                                break;
                        }
                        GameObject _3dText = GameObject.Find("tbTemp");
                        _3dText.GetComponent<TextMesh>().text = msg;
                    });
                };

                //// subscribe to the topic "/home/temperature" with QoS 2 
                client.Subscribe(new string[] { "view" }, new byte[] { uPLibrary.Networking.M2Mqtt.Messages.MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            }
        }

        protected void SetView(string msg)
        {
            var view = new JSONObject(msg);
            var iv = appState.Config.ActiveView;
            iv.Lat = view.GetFloat("Lat");
            iv.Lon = view.GetFloat("Lon");
            iv.Zoom = view.GetInt("Zoom");
            if (appState.TileManager)
            {
                appState.TileManager.Latitude = iv.Lat;
                appState.TileManager.Longitude = iv.Lon;
                appState.TileManager.Zoom = iv.Zoom;
                appState.ResetMap();
            }
        }

    }
}
