using System;
using UnityEngine;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using System.Collections.Generic;
using Assets.Scripts.Classes;
using Symbols;

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
        protected readonly AppState appState = AppState.Instance;
        public readonly User me = new User();
        protected MqttClient client;
        protected string topic;
        protected string sessionName;
        /// <summary>
        /// Other users in the session
        /// </summary>
        protected readonly List<User> users = new List<User>();
        protected readonly List<GameObject> cursors = new List<GameObject>();
        internal GameObject cursorPrefab;
        private Dictionary<User, string> prevCommand;
        protected SessionManager()
        {
        } // guarantee this will be always a singleton only - can't use the constructor!

        public void Init(GameObject cursor)
        {
            Debug.Log("Initializing SessionManager");
            me.Name = appState.Config.UserName;
            me.SelectionColor = appState.Config.SelectionColor;
            me.Cursor = cursor;
            me.Cursor.name = me.Id + "-Cursor";
            me.Cursor.transform.FindChild("CursorOnHolograms").gameObject.GetComponent<Renderer>().material = me.UserMaterial;
            var mtd = gameObject.AddComponent<UnityMainThreadDispatcher>();
            InitMqtt();
            var sessions = new List<string> { "one", "two", "three" };
            sessions.ForEach(session => appState.Speech.AddKeyword(NewSessionKeyword + session, () => JoinSession(session)));
            JoinSession(appState.Config.SessionName);
        }

        public void JoinSession(string name)
        {
            if (!client.IsConnected || string.IsNullOrEmpty(name)) return;
            sessionName = name;
            if (!string.IsNullOrEmpty(topic)) client.Unsubscribe(new[] { topic });
            topic = (name + "/#").ToLower();
            Debug.Log("Client is joining session " + topic);
            client.Subscribe(new string[] { topic }, new byte[] { uPLibrary.Networking.M2Mqtt.Messages.MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            Heartbeat();
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
                client.Connect(me.Id);
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
                    var subtopic = e.Topic.Substring(topic.Length - 1);

                    if (subtopic.StartsWith("presence/"))
                    {
                        UpdateUsersPresence(msg);
                        return;
                    }
                    Debug.Log(string.Format("Received message on topic {0}: {1}", subtopic, msg));
                    switch (subtopic)
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
            var av = appState.Config.ActiveView;
            var view = new JSONObject(msg);
            var lat = view.GetFloat("lat");
            var lon = view.GetFloat("lon");
            var zoom = view.GetInt("zoom");
            var range = view.GetInt("range", av.Range);
            if (av.Equal(lat, lon, zoom, range)) return;
            av.SetView(lat, lon, zoom, range);
            if (!appState.TileManager) return;
            appState.ResetMap(av);
        }

        public void UpdateView(ViewState view)
        {
            SendJsonMessage("view", string.Format("{ lat: {0}, lon: {1}, zoom: {2}, range: {3} }", view.Lat, view.Lon, view.Zoom, view.Range));
        }

        #region Room management

        protected void Heartbeat()
        {
            InvokeRepeating("UpdatePresence", 5, 4);
        }

        /// <summary>
        /// Update users in the session.
        /// </summary>
        /// <param name="json"></param>
        protected void UpdateUsersPresence(string json)
        {
            var user = User.FromJSON(json, cursors);
            if (user.Id == me.Id) return; // Do not update yourself

            var found = false;
            User existingUser = null;
            int i = -1;
            while (i < users.Count - 1 && !found)
            {
                i++;
                existingUser = users[i];
                if (user.Id != existingUser.Id) continue;
                found = true;


            }
            if (!found)
            {

                var cursor = cursors.Find(u => u.name == user.Id + "-Cursor");
                if (cursor == null)
                {
                    user.Cursor = Instantiate(cursorPrefab, new Vector3(0, 1, 0), transform.rotation);
                    user.Cursor.name = user.Id + "-Cursor";
                    user.Cursor.transform.FindChild("CursorOnHolograms").gameObject.GetComponent<Renderer>().material = user.UserMaterial;
                    cursors.Add(user.Cursor);
                }
                else
                    user.Cursor = cursor;
                users.Add(user);
                if (user.SelectedFeature != null)
                    UpdateUserSelection(existingUser.SelectedFeature, user);

            }
            else
            {
                if (user.Cursor == null)
                    user.Cursor = users[i].Cursor;
                if (user.SelectedFeature != null && existingUser.SelectedFeature != null)// && user.SelectedFeature.id != existingUser.SelectedFeature.id)
                    UpdateUserSelection(existingUser.SelectedFeature, user);
                users[i] = user;

            }
        }

        /// <summary>
        /// A user in the session has changed its selection. Make it visible.
        /// </summary>
        /// <param name="selectedFeature"></param>
        /// <param name="user">If user does not exist, remove the current selection.</param>
        protected void UpdateUserSelection(Feature selectedFeature, User user = null)
        {
            var gameobj = GameObject.Find(selectedFeature.id);
            if (gameobj == null) return;
            GameObject selectedObject = gameobj.transform.parent.gameObject;
            SymbolTargetHandler handler = selectedObject.GetComponent<SymbolTargetHandler>();
            handler.OnSelect(user.UserMaterial, user.Cursor);
        }

        /// <summary>
        /// Update the presence status.
        /// </summary>
        protected void UpdatePresence()
        {
            var subtopic = string.Format("presence/{0}", me.Id);
            SendJsonMessage(subtopic, me.ToJSON(), false);
            RemoveOldUsersFromSession();
        }

        /// <summary>
        /// Remove stale users from the session
        /// </summary>
        protected void RemoveOldUsersFromSession()
        {
            var now = DateTime.UtcNow;
            if (users.Count == 0) return;
            for (var i = users.Count - 1; i >= 0; i--)
            {
                var user = users[i];
                if (now - user.LastUpdateReceived > TimeSpan.FromSeconds(25))
                {
                    if (user.SelectedFeature != null && !string.IsNullOrEmpty(user.SelectedFeature.id)) UpdateUserSelection(user.SelectedFeature);
                    users.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Set or unset the selected feature.
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="isSelected"></param>
        public void UpdateSelectedFeature(Feature feature, bool isSelected)
        {
            if (isSelected)
            {
                me.SelectedFeature = feature;
                UpdatePresence();
            }
            else
            {
                UpdatePresence();
                me.SelectedFeature = null;
            }
        }

        #endregion Room management

        public void UpdateLayer(Layer layer)
        {
            var subtopic = string.Format("layers/{0}", layer.Title);
            SendJsonMessage(subtopic, layer.ToJSON());
        }

        /// <summary>
        /// Send a JSON message as UTF8 bytes to a subtopic.
        /// </summary>
        /// <param name="subtopic"></param>
        /// <param name="json"></param>
        /// <param name="retain">Retain the message</param>
        protected void SendJsonMessage(string subtopic, string json, bool retain = true)
        {
            Debug.Log(string.Format("Sending JSON message to topic {0}/{1}: {2}", sessionName, subtopic, json));
            client.Publish(string.Format("{0}/{1}", sessionName, subtopic), Encoding.UTF8.GetBytes(json), uPLibrary.Networking.M2Mqtt.Messages.MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, retain);
        }
    }
}
