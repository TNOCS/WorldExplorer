using System;
using UnityEngine;
using System.Text;
// using uPLibrary.Networking.M2Mqtt;
using System.Collections.Generic;
using Assets.Scripts.Classes;
using Symbols;
using uPLibrary.Networking.M2Mqtt;

namespace Assets.Scripts.Plugins
{
    /// <summary>
    /// The SessionManager is responsible for setting up a connection with the MQTT server, and managing view updates, room management, etc.
    /// It uses multiple topics in the Mqtt client, where each topic contains one object type (as JSON):
    /// [TOPIC_NAME]/presence/[ID]: Describes the session users, and for each User, their selection, selection color, cursor position etc.
    /// [TOPIC_NAME]/view: View class: Describes the active center location, and zoom level
    /// [TOPIC_NAME]/layers/[LAYER_NAME]: Each layer contains a dynamic (i.e. editable) GeoJSON layer. Note that we download the initial layer via the WWWclient, but updates (edits) are published here.
    /// </summary

    public class SessionManager : SingletonCustom<SessionManager>
    {
        const string NewSessionKeyword = "Join session ";
        protected readonly AppState appState = AppState.Instance;
        protected SelectionHandler selectionHandler;
        public readonly User me = new User();
        public List<string> userStrings = new List<string>();
        protected MqttClient client;
        protected string topic;
        protected string sessionName;
        /// <summary>
        /// Other users in the session
        /// </summary>
        public readonly List<User> users = new List<User>();
        protected readonly List<GameObject> cursors = new List<GameObject>();
        internal GameObject cursorPrefab;
        public Material UserMaterial;
        // Dictates if table position, rotation and scale is shared.
        public bool ShareTable = true;

        private void Awake()
        {
            selectionHandler = appState.selectionHandler;
        }

        private void OnApplicationQuit()
        {
            try
            {
                client.Disconnect();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
        protected SessionManager()
        {
        } // guarantee this will be always a singleton only - can't use the constructor!

        public void Init(GameObject cursor)
        {
            Debug.Log("Initializing SessionManager");
            if (selectionHandler == null) selectionHandler = SelectionHandler.Instance;
            selectionHandler.addUser(me);

            me.Name = appState.Config.UserName;
            me.SelectionColor = appState.Config.SelectionColor;
            me.Cursor = cursor;
            me.Cursor.name = me.Id + "-Cursor";
            me.Cursor.transform.Find("CursorOnHolograms").gameObject.GetComponent<Renderer>().material = me.UserMaterial;
            me.Cursor.transform.Find("CursorOnHolograms").GetComponent<MeshRenderer>().material = Resources.Load<Material>("ring_shadow");

            UserMaterial = new Material(Shader.Find("HoloToolkit/Cursor"));

            var mtd = gameObject.AddComponent<UnityMainThreadDispatcher>();
            InitMqtt();
            var sessions = new List<string> { "one", "two", "three" };
            //sessions.ForEach(session => appState.Speech.AddKeyword(NewSessionKeyword + session, () => JoinSession(session)));
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
                    // Debug.Log(string.Format("Received message on topic {0}: {1}", subtopic, msg));
                    switch (subtopic)
                    {
                        case "view":
                            SetView(msg);
                            break;
                        case "zoomdirection":
                            RescaleBoardObjects(msg);
                            break;
                        case "presence":
                            Debug.Log("Presence selected");
                            break;
                        case "newobject":
                            SetNewObject(msg);
                            break;
                        case "updateobject":
                            SetExistingObject(msg);
                            break;
                        case "deleteobject":
                            SetDeleteObject(msg);
                            break;
                        case "table":
                            if (ShareTable)
                            {
                                SetTable(msg);
                            }
                            break;
                    }
                    //GameObject _3dText = GameObject.Find("tbTemp");
                    //_3dText.GetComponent<TextMesh>().text = msg;
                });
            };
        }


        public void UpdateView(ViewState view, string zoomDirection = "none")
        {
            // Debug.Log("Sending view: " + view.ToLimitedJSON());
            SendJsonMessage("view", view.ToLimitedJSON(), false);

            var zoomDirectionString = string.Format(@"{{ ""direction"": ""{0}"" }}", zoomDirection);
            SendJsonMessage("zoomdirection", zoomDirectionString, false);
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
            UIManager.Instance.CurrentOverlayText.text = AppState.Instance.Config.ActiveView.Name.ToString();
        }


        #region Object Management

        // Sends all data of the new object to the other users.
        // Also handles new positions of objects.
        public void UpdateNewObject(SpawnedObject obj)
        {
            // Gets original prefab name (i.e. "jeep" instead of "jeep(Clone)(2)")
            var name = obj.obj.name;
            int index = name.IndexOf("(");
            if (index > 0)
                name = name.Substring(0, index);

            // Scale, Rotation and CenterPosition have to be done seperately for X, Y and Z Due to Unity automatically removing all decimals after the first one is converting a Vector3/Quat to string.
            var objString = string.Format(@"{{ ""Name"": ""{0}"", ""prefabname"": ""{1}"", ""lat"": {2}, ""lon"": {3}, ""scaleX"": {4}, ""scaleY"": {5}, ""scaleZ"": {6}, ""rotX"": {7}, ""rotY"": {8}, ""rotZ"": {9}, ""centerPosX"": {10}, ""centerPosY"": {11}, ""centerPosZ"": {12} }}",
                obj.obj.name, name, obj.lat, obj.lon, obj.localScale.x, obj.localScale.y, obj.localScale.z, obj.rotation.x, obj.rotation.y, obj.rotation.z, obj.centerPosition.x, obj.centerPosition.y, obj.centerPosition.z);

            // Destroy the new object and it's reference in the list. The object immediatly gets recreated in the SetNewObject function.
            // A different way would be to check if it works better to check if the data received in SetNewObject is from the user itself (like with the table).
            // However, this way updated positional data is also handled correctly in this function.
            for (int i = 0; i < InventoryObjectInteraction.Instance.spawnedObjectsList.Count; i++)
            {
                if (obj == InventoryObjectInteraction.Instance.spawnedObjectsList[i])
                {
                    InventoryObjectInteraction.Instance.spawnedObjectsList.Remove(obj);
                    Destroy(obj.obj);
                }
            }

            SendJsonMessage("newobject", objString, false);

        }

        // Recreates the object made by the other user.
        public void SetNewObject(string msg)
        {
            var newObject = new JSONObject(msg);
            var name = newObject.GetString("Name");
            var prefabName = newObject.GetString("prefabname");
            var lat = newObject.GetDouble("lat");
            var lon = newObject.GetDouble("lon");
            var scaleX = newObject.GetFloat("scaleX");
            var scaleY = newObject.GetFloat("scaleY");
            var scaleZ = newObject.GetFloat("scaleZ");
            var rotX = newObject.GetFloat("rotX");
            var rotY = newObject.GetFloat("rotY");
            var rotZ = newObject.GetFloat("rotZ");
            var centerPosX = newObject.GetFloat("centerPosX");
            var centerPosY = newObject.GetFloat("centerPosY");
            var centerPosZ = newObject.GetFloat("centerPosZ");

            SpawnOtherUsersObject(name, prefabName, lat, lon, scaleX, scaleY, scaleZ, rotX, rotY, rotZ, centerPosX, centerPosY, centerPosZ);
        }

        // Sends data of updated object to the other users.
        public void UpdateExistingObject(SpawnedObject obj)
        {
            var objString = string.Format(@"{{ ""Name"": ""{0}"", ""posX"": {1}, ""posY"": {2}, ""posZ"": {3}, ""lat"": {4}, ""lon"": {5}, ""scaleX"": {6}, ""scaleY"": {7}, ""scaleZ"": {8}, ""rotX"": {9}, ""rotY"": {10}, ""rotZ"": {11}, ""user"": ""{12}"" }}",
                obj.obj.name, obj.obj.transform.position.x, obj.obj.transform.position.y, obj.obj.transform.position.z, obj.lat, obj.lon, obj.obj.transform.localScale.x, obj.obj.transform.localScale.y, obj.obj.transform.localScale.z, obj.obj.transform.eulerAngles.x, obj.obj.transform.eulerAngles.y, obj.obj.transform.eulerAngles.z, me.id);

            SendJsonMessage("updateobject", objString, false);
        }

        // Sets the received updated data of the relevant object
        public void SetExistingObject(string msg)
        {
            var updatedObject = new JSONObject(msg);
            var user = updatedObject.GetString("user");
            if (user != me.id)
            {
                var name = updatedObject.GetString("Name");
                var posX = updatedObject.GetFloat("posX");
                var posY = updatedObject.GetFloat("posY");
                var posZ = updatedObject.GetFloat("posZ");
                var lat = updatedObject.GetFloat("lat");
                var lon = updatedObject.GetFloat("lon");
                var scaleX = updatedObject.GetFloat("scaleX");
                var scaleY = updatedObject.GetFloat("scaleY");
                var scaleZ = updatedObject.GetFloat("scaleZ");
                var rotX = updatedObject.GetFloat("rotX");
                var rotY = updatedObject.GetFloat("rotY");
                var rotZ = updatedObject.GetFloat("rotZ");

                UpdateOtherUsersObject(name, posX, posY, posZ, lat, lon, scaleX, scaleY, scaleZ, rotX, rotY, rotZ);
            }
        }

        // Tells the other users what object should be deleted.
        public void UpdateDeletedObject(SpawnedObject obj)
        {
            var objString = string.Format(@"{{ ""Name"": ""{0}"", ""user"": ""{1}"" }}", obj.obj.name, me.id);
            SendJsonMessage("deleteobject", objString, false);
        }

        // Deletes objects deleted by other users.
        public void SetDeleteObject(string msg)
        {
            var goMessage = new JSONObject(msg);
            var user = goMessage.GetString("user");

            // Only runs if the data received comes from another user.
            if (user != me.id)
            {
                var goName = goMessage.GetString("Name");
                var goInScene = GameObject.Find(goName);
                DeleteOtherUsersObject(goInScene);
            }
        }

        // Applies the transformations done by other users.
        public void UpdateOtherUsersObject(string name, float posX, float posY, float posZ, float lat, float lon, float scaleX, float scaleY, float scaleZ, float rotX, float rotY, float rotZ)
        {
            GameObject updatedObject = GameObject.Find(name);

            updatedObject.transform.position = new Vector3(posX, posY, posZ);
            updatedObject.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
            updatedObject.transform.rotation = Quaternion.Euler(rotX, rotY, rotZ);

            foreach (SpawnedObject so in InventoryObjectInteraction.Instance.spawnedObjectsList)
            {
                if (so.obj == updatedObject)
                {
                    so.lat = lat;
                    so.lon = lon;
                }
            }

            Debug.Log("Object " + name + " has been updated");
        }

        // Recreates the object made by the other user.
        public void SpawnOtherUsersObject(string name, string prefabName, double lat, double lon, float scaleX, float scaleY, float scaleZ, float rotX, float rotY, float rotZ, float centerPosX, float centerPosY, float centerPosZ)
        {
            GameObject newObject;

            newObject = Instantiate(Resources.Load("Prefabs/Inventory/" + prefabName)) as GameObject;
            newObject.name = name;

            // Puts it under the same parent as objects created by the user of this instance.
            var newlySpawned = GameObject.Find("NewlySpawned");
            newObject.transform.SetParent(newlySpawned.transform, false);

            // Creates positional data from the seperate X Y Z values
            var objPosition = new Vector3(centerPosX, centerPosY, centerPosZ);
            var objRotation = Quaternion.Euler(rotX, rotY, rotZ);
            var objScale = new Vector3(scaleX, scaleY, scaleZ);

            // Sets the created positional data
            newObject.transform.position = objPosition;
            newObject.transform.localScale = objScale;
            newObject.transform.rotation = objRotation;

            // Creates a new SpawnedObject and adds it to the list.
            SpawnedObject spawnedObject = new SpawnedObject(newObject, newObject.transform.TransformDirection(newObject.transform.position), lat, lon, objScale, objRotation);
            Debug.Log("Adding to list: " + spawnedObject.obj);
            InventoryObjectInteraction.Instance.spawnedObjectsList.Add(spawnedObject);
            newObject.tag = "spawnobject";
        }

        // Deleted objects deleted by other user.
        public void DeleteOtherUsersObject(GameObject go)
        {
            ObjectInteraction.Instance.Delete(go);
        }

        // Rescales the objects based on the given zoomdirection
        public void RescaleBoardObjects(string msg)
        {
            var zoomDirectionMessage = new JSONObject(msg);
            var zoomDirection = zoomDirectionMessage.GetString("direction");

            foreach (var spawnedObject in InventoryObjectInteraction.Instance.spawnedObjectsList)
            {
                // Zoom out
                if (zoomDirection == "in")
                {
                    spawnedObject.obj.transform.localScale = (spawnedObject.obj.transform.localScale * BoardInteraction.Instance.scaleFactor);
                }
                // Zoom in
                if (zoomDirection == "out")
                {
                    spawnedObject.obj.transform.localScale = (spawnedObject.obj.transform.localScale / BoardInteraction.Instance.scaleFactor);
                }
            }
        }


        #endregion

        #region Table management

        public void UpdateTable()
        {
            if (ShareTable)
            {
                var terrain = BoardInteraction.Instance.terrain.transform;
                var tableData = string.Format(@"{{ ""posX"": {0}, ""posY"": {1}, ""posZ"": {2}, ""rotX"": {3}, ""rotY"": {4}, ""rotZ"": {5}, ""scaleX"": {6}, ""scaleY"": {7}, ""scaleZ"": {8}, ""user"": ""{9}"" }}",
                    terrain.position.x, terrain.position.y, terrain.position.z, terrain.rotation.eulerAngles.x, terrain.rotation.eulerAngles.y, terrain.rotation.eulerAngles.z, terrain.localScale.x, terrain.localScale.y, terrain.localScale.z, me.id);
                SendJsonMessage("table", tableData, false);
            }            
        }

        public void SetTable(string msg)
        {
            var tableData = new JSONObject(msg);
            var user = tableData.GetString("user");

            // Only runs if the data received comes from another user.
            if (user != me.id)
            {
                // Data is split up between X Y Z due to Unitys inability to format Vector3's to string without losing decimals.
                var tablePositionX = tableData.GetFloat("posX");
                var tablePositionY = tableData.GetFloat("posY");
                var tablePositionZ = tableData.GetFloat("posZ");
                var tablePosition = new Vector3(tablePositionX, tablePositionY, tablePositionZ);

                var tableRotationX = tableData.GetFloat("rotX");
                var tableRotationY = tableData.GetFloat("rotY");
                var tableRotationZ = tableData.GetFloat("rotZ");
                var tableRotation = Quaternion.Euler(tableRotationX, tableRotationY, tableRotationZ);

                var tableScaleX = tableData.GetFloat("scaleX");
                var tableScaleY = tableData.GetFloat("scaleY");
                var tableScaleZ = tableData.GetFloat("scaleZ");
                var tableScale = new Vector3(tableScaleX, tableScaleY, tableScaleZ);

                Debug.Log("Setting rotation to " + tableRotation);
                BoardInteraction.Instance.terrain.transform.position = tablePosition;
                BoardInteraction.Instance.terrain.transform.rotation = tableRotation;
                BoardInteraction.Instance.terrain.transform.localScale = tableScale;
            }
        }

        #endregion

        #region Room management

        protected void Heartbeat()
        {
            InvokeRepeating("UpdatePresence", 5, 1);
        }

        /// <summary>
        /// Update users in the session.
        /// </summary>
        /// <param name="json"></param>
        protected void UpdateUsersPresence(string json)
        {
            var data = new JSONObject(json);
            var id = data.GetString("id");
            var name = data.GetString("name");

            if (id == me.Id) return; // Do not update yourself
            //Debug.Log("User: " + name);
            userStrings.Add(name);
            Debug.Log("Update presence for " + id);
            var cursor = GameObject.Find(id + "-Cursor");
            if (cursor == null)
            {                  
                cursor = Instantiate(cursorPrefab, new Vector3(0, 1, 0), transform.rotation);
                cursor.name = id + "-Cursor";
                Debug.Log("Instantiated cursor: " + cursor.name);
                cursor.transform.Find("CursorOnHolograms").gameObject.GetComponent<Renderer>().material = UserMaterial;
                var r = data.GetInt("r");
                var g = data.GetInt("g");
                var b = data.GetInt("b");
                //Debug.Log(r + " " + g + " " + b);
                var selectionColor = new Color(r, g, b);
                cursor.transform.GetChild(0).GetComponent<Renderer>().material.color = selectionColor;
            }

            if (cursor != null)
            {
                var xpos = data.GetFloat("xpos");
                var ypos = data.GetFloat("ypos");
                var zpos = data.GetFloat("zpos");

                var xrot = data.GetFloat("xrot");
                var yrot = data.GetFloat("yrot");
                var zrot = data.GetFloat("zrot");                

                cursor.transform.position = new Vector3(xpos, ypos, zpos);
                cursor.transform.rotation = Quaternion.Euler(270, yrot, zrot);
            //    Debug.Log("Setting " + cursor.name + " to " + cursor.transform.position);                
            }


            //======
            /*
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
                      //user.Cursor = GameObject.Find(user.Id + "-Cursor");                    
                      user.Cursor = Instantiate(cursorPrefab, new Vector3(0, 1, 0), transform.rotation);
                      user.Cursor.name = user.Id + "-Cursor";
                    Debug.Log("Other cursors cursor: " + user.Cursor);
                      user.Cursor.transform.Find("CursorOnHolograms").gameObject.GetComponent<Renderer>().material = user.UserMaterial;
                      cursors.Add(user.Cursor);
                }
                else
                    user.Cursor = cursor;
                users.Add(user);
                selectionHandler.addUser(user);
                if (user.SelectedFeature != null)
                    UpdateUserSelection(user.SelectedFeature, user);
            }
            else
            {
                if (user.Cursor == null)
                    user.Cursor = users[i].Cursor;
                if (user.SelectedFeature != null && existingUser.SelectedFeature != null)
                    UpdateUserSelection(existingUser.SelectedFeature, user);
                users[i] = user;
            }*/
        }

        /// <summary>
        /// A user in the session has changed its selection. Make it visible.
        /// </summary>
        /// <param name="selectedFeature"></param>
        /// <param name="user">If user does not exist, remove the current selection.</param>
        protected void UpdateUserSelection(Feature selectedFeature, User user = null)
        {
         //  var gameobj = GameObject.Find(selectedFeature.id);
         //  if (gameobj == null) return;
         //  GameObject selectedObject = gameobj.transform.parent.gameObject;
         //  SymbolTargetHandler handler = selectedObject.GetComponent<SymbolTargetHandler>();
         //  handler.OnSelect(user);
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
            if (users.Count == 0) return;
            var now = DateTime.UtcNow;
            for (var i = users.Count - 1; i >= 0; i--)
            {
                var user = users[i];
                if (now - user.LastUpdateReceived > TimeSpan.FromSeconds(50))
                {
                    if (user.SelectedFeature != null && !string.IsNullOrEmpty(user.SelectedFeature.id)) UpdateUserSelection(user.SelectedFeature, user);
                    {
                        users.RemoveAt(i);
                    }
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
          //if (isSelected)
          //{
          //    me.SelectedFeature = feature;
          //    UpdatePresence();
          //}
          //else
          //{
          //    UpdatePresence();
          //    me.SelectedFeature = null;
          // }
        }

        #endregion Room management

        public void UpdateLayer(Layer layer)
        {
            var subtopic = string.Format("layers/{0}", layer.Title);
            SendJsonMessage(subtopic, layer.ToJSON(), false);
        }

        /// <summary>
        /// Send a JSON message as UTF8 bytes to a subtopic.
        /// </summary>
        /// <param name="subtopic"></param>
        /// <param name="json"></param>
        /// <param name="retain">Retain the message</param>
        protected void SendJsonMessage(string subtopic, string json, bool retain = true)
        {
            //Debug.Log(string.Format("Sending JSON message to topic {0}/{1}: {2}", sessionName, subtopic, json));
            client.Publish(string.Format("{0}/{1}", sessionName, subtopic), Encoding.UTF8.GetBytes(json), uPLibrary.Networking.M2Mqtt.Messages.MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, retain);
        }
    }
}
