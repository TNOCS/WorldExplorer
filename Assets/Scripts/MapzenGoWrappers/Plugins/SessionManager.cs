using System;
using UnityEngine;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using System.Collections.Generic;

namespace Assets.Scripts.Plugins
{
    /// <summary>
    /// The SessionManager is responsible for setting up a connection with the MQTT server, and managing view updates, room management, etc.
    /// It uses multiple topics in the Mqtt client, where each topic contains one object type (as JSON):
    /// [TOPIC_NAME]/room: Room class: Describes the room members, and for each member, their selection, selection color etc.
    /// [TOPIC_NAME]/view: View class: Describes the active center location, and zoom level
    /// [TOPIC_NAME]/layers/[LAYER_NAME]: Each layer contains a dynamic (i.e. editable) GeoJSON layer. Note that we download the initial layer via the WWWclient, but updates (edits) are published here.
    /// </summary>
    public class SessionManager : Singleton<SessionManager>
    {
        const string NewSessionKeyword = "Join session ";
        AppState appState = AppState.Instance;
        MqttClient client;
        string topic;

        protected SessionManager()
        {
        } // guarantee this will be always a singleton only - can't use the constructor!

        public void Init()
        {
            Debug.Log("Initializing SessionManager");
            InitMqtt();
            var sessions = new List<string> { "one", "two", "three" };
            sessions.ForEach(session => appState.Speech.AddKeyword(NewSessionKeyword + session, () => JoinSession(session)));
            JoinSession(appState.Config.SessionName);
        }

        public void JoinSession(string name)
        {
            if (!client.IsConnected || string.IsNullOrEmpty(name)) return;
            if (!string.IsNullOrEmpty(topic)) client.Unsubscribe(new[] { topic });
            topic = (name + "/#").ToLower();
            Debug.Log("Client is joining session " + topic);
            client.Subscribe(new string[] { topic }, new byte[] { uPLibrary.Networking.M2Mqtt.Messages.MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        }

        protected void InitMqtt()
        {
            if (string.IsNullOrEmpty(appState.Config.MqttServer) || string.IsNullOrEmpty(appState.Config.MqttPort)) return;
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
            if (!client.IsConnected) return;

            Debug.Log(string.Format("Client is connected to MQTT server: {0}:{1}", appState.Config.MqttServer, appState.Config.MqttPort));
            // register to message received 
            client.MqttMsgPublishReceived += (sender, e) =>
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    var msg = Encoding.UTF8.GetString(e.Message);
                    Debug.Log("Received message: " + msg);
                    switch (e.Topic)
                    {
                        case "view":
                            SetView(msg);
                            break;
                        case "presence":
                            Debug.Log("Presence selected");
                            break;
                    }
                    //GameObject _3dText = GameObject.Find("tbTemp");
                    //_3dText.GetComponent<TextMesh>().text = msg;
                });
            };
        }

        protected void SetView(string msg)
        {
            var view = new JSONObject(msg);
            var iv = appState.Config.ActiveView;
            iv.Lat = view.GetFloat("Lat");
            iv.Lon = view.GetFloat("Lon");
            iv.Zoom = view.GetInt("Zoom");
            if (!appState.TileManager) return;
            appState.TileManager.Latitude = iv.Lat;
            appState.TileManager.Longitude = iv.Lon;
            appState.TileManager.Zoom = iv.Zoom;
            appState.ResetMap();
        }

    }
}
