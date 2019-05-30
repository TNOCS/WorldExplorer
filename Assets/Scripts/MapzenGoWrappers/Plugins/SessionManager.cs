using Assets.Scripts.Classes;
using eu.driver.model.worldexplorer;
using Symbols;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityWorldExplorerClient;
using WorldExplorerClient;
using WorldExplorerClient.interfaces;
using WorldExplorerClient.messages;

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

        //protected IWorldExplorerClient client;
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

        private IWorldExplorerClient client;

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
            if (selectionHandler == null)
            {
                selectionHandler = SelectionHandler.Instance;
            }

            selectionHandler?.addUser(me);

            me.Name = appState.Config.UserName;
            me.SelectionColor = appState.Config.SelectionColor;
            me.Cursor = cursor;
            me.Cursor.name = me.Id + "-Cursor";
            me.Cursor.transform.Find("CursorOnHolograms").gameObject.GetComponent<Renderer>().material = me.UserMaterial;
            me.Cursor.transform.Find("CursorOnHolograms").GetComponent<MeshRenderer>().material = Resources.Load<Material>("ring_shadow");
            Debug.Log(String.Format("Assigned ID '{0}' to cursor", me.Cursor.name));
            UserMaterial = new Material(Shader.Find("MixedRealityToolkit/Cursor"));

            var mtd = gameObject.AddComponent<UnityMainThreadDispatcher>();
            InitMessageBus();
            //var sessions = new List<string> { "one", "two", "three" };
            //sessions.ForEach(session => appState.Speech.AddKeyword(NewSessionKeyword + session, () => JoinSession(session)));
            UpdatePresence();
        }

        protected void InitMessageBus()
        {
            try
            {
                appState.Config.MessageBus = AppConfig.MessageBusType.None;
                switch (appState.Config.MessageBus)
                {
                    case AppConfig.MessageBusType.Mqtt:
                        Debug.Log("Use MQTT as message bus");
                        if (string.IsNullOrEmpty(appState.Config.MqttServer) || string.IsNullOrEmpty(appState.Config.MqttPort))
                        {
                            Debug.LogError("No MQTT connection parameters not defined in config.");
                            return;
                        }
                        //client = ClientFactory.CreateMqttClient(appState.Config.MqttServer, Convert.ToInt32(appState.Config.MqttPort));
                        Debug.Log(string.Format("Using MQTT server: {0}:{1}", appState.Config.MqttServer, appState.Config.MqttPort));
                        break;
                    case AppConfig.MessageBusType.Kafka:
                        Debug.Log("Use KAFKA as message bus");
                        if (string.IsNullOrEmpty(appState.Config.KafkaBootstrapServer) ||
                            string.IsNullOrEmpty(appState.Config.KafkaSchemaRegistryServer))
                        {
                            Debug.LogError("No KAFKA connection parameters not defined in config.");
                            return;
                        }
                        client = ClientFactory.CreateStubClient(); // Kafka disabled for now
                        /*
                        client = new KafkaClient(null / * auto client id * /,
                            appState.Config.KafkaBootstrapServer, 
                            appState.Config.KafkaSchemaRegistryServer, 
                            "GROUP_WorldExplorer"); 
                */
                        break;
                    case AppConfig.MessageBusType.SignalR:
                        Debug.Log("Use SignalR as message bus");
                        if (string.IsNullOrEmpty(appState.BaseUrlConfigurationServer))
                        {
                            Debug.LogError("No SignalR connection parameters not defined in config.");
                            return;
                        }
                        var url = $"{appState.BaseUrlConfigurationServer}/WorldExplorerHub";
                        client = ClientFactory.CreateSignalrClient(url, logger => Debug.Log(logger.Message));
                        Debug.Log(string.Format("Using SignalR server: {0}", url));
                        break;
                    default:
                        // Create a dummy instead of null pointer checking for client
                        Debug.Log("Message bus disabled in config, use dummy client");
                        client = ClientFactory.CreateStubClient();
                        break;
                }

                // Subscribe to all message bus events
                client.OnView += SessionOnView; ;
                client.OnZoom += SessionOnZoom;
                client.OnNewObject += SessionOnNewObject;
                client.OnUpdateObject += SessionOnUpdateObject;
                client.OnDeleteObject += SessionOnDeleteObject;
                client.OnPresense += SessionOnPresense;
                client.OnTable += SessionOnTable;

                Debug.Log($"Connecting to message bus with client id {me.Id} and try to join session {appState.Config.SessionName}");
                client.Connect(me.Id);
                if (!client.IsConnected)
                {
                    Debug.LogError("Not connected to message bus");
                }

                client.JoinSession(appState.Config.SessionName);
                Heartbeat();

            }
            catch (Exception ex)
            {
                Debug.LogError("Error connecting to MQTT: " + ex.Message);
            }
            //if (!client.IsConnected) return;



        }

 
        private void SessionOnTable(object sender, TableMsg pMsg)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                SetTable(pMsg.Msg);
            });
        }


        private void SessionOnPresense(object sender, PresenseMsg pMsg)
        {
            // Invoke on GUI thread
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                eu.driver.model.worldexplorer.Presense presense = pMsg.Msg;
                if (presense.id == me.Id)
                {
                    return; // Do not update yourself
                        }
                        //Debug.Log("User: " + name);
                        var cursorId = CreateCursorId(presense.id);
                Debug.Log("Update presence for " + presense.id);
                var cursor = GameObject.Find(cursorId);
                if (cursor == null)
                {
                    cursor = Instantiate(cursorPrefab, new Vector3(0, 1, 0), transform.rotation);
                    cursor.name = cursorId;
                    Debug.Log("Create cursor: " + cursorId + " for remote user " + presense.id);
                    cursor.transform.Find("CursorOnHolograms").gameObject.GetComponent<Renderer>().material = UserMaterial;
                    var selectionColor = new Color(presense.r, presense.g, presense.b);
                    cursor.transform.GetChild(0).GetComponent<Renderer>().material.color = selectionColor;
                }

                if (cursor != null)
                {
                            // Update position of cursor
                            cursor.transform.position = new Vector3(presense.xpos, presense.ypos, presense.zpos);
                    cursor.transform.rotation = Quaternion.Euler(270, presense.yrot, presense.zrot);
                            //    Debug.Log("Setting " + cursor.name + " to " + cursor.transform.position);                
                        }
            });

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
            } */
        }

        private static string CreateCursorId(string pId)
        {
            return String.Format("{0}-Cursor", pId); ;
        }
        private void SessionOnDeleteObject(object sender, DeleteObjectMsg pMsg)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                DeleteObject delObject = pMsg.Msg;
                        // Only runs if the data received comes from another user.
                        if (delObject.user != me.id)
                {
                    var goInScene = GameObject.Find(delObject.Name);
                    DeleteOtherUsersObject(goInScene);
                }
            });
        }

        private void SessionOnUpdateObject(object sender, UpdateObjectMsg pMsg)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                UpdateObject updateObj = pMsg.Msg;

                if (updateObj.User != me.id)
                {


                    UpdateOtherUsersObject(updateObj);
                }

            });
        }

        private void SessionOnNewObject(object sender, NewObjectMsg pMsg)
        {

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                NewObject newObject = pMsg.Msg;
                SpawnOtherUsersObject(newObject);
            });
        }

        private void SessionOnZoom(object sender, ZoomMsg pMsg)
        {
            // Rescales the objects based on the given zoomdirection
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {

                Zoom zoom = pMsg.Msg;
                foreach (var spawnedObject in InventoryObjectInteraction.Instance.spawnedObjectsList)
                {
                            // Zoom out
                            if (zoom.zoomdirection == Direction.In)
                    {
                        spawnedObject.obj.transform.localScale = (spawnedObject.obj.transform.localScale * BoardInteraction.Instance.scaleFactor);
                    }
                            // Zoom in
                            if (zoom.zoomdirection == Direction.Out)
                    {
                        spawnedObject.obj.transform.localScale = (spawnedObject.obj.transform.localScale / BoardInteraction.Instance.scaleFactor);
                    }
                }
            });
        }

        private void SessionOnView(object sender, ViewMsg pMsg)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                View view = pMsg.Msg;
                var av = appState.Config.ActiveView;
                if (av.Equal(view.lat, view.lon, view.zoom, view.range))
                {
                    return;
                }

                av.SetView(view.lat, view.lon, view.zoom, view.range);
                if (!appState.TileManager)
                {
                    return;
                }

                appState.ResetMap(av);
                UIManager.Instance.CurrentOverlayText.text = AppState.Instance.Config.ActiveView.Name.ToString();
            });
        }




        public void UpdateView(ViewState view, Direction? zoomDirection)
        {
            client.SendView(new ViewMsg(EDXLDistributionExtension.CreateHeader(),
                new View() { lat = view.Lat, lon = view.Lon, range = view.Range, zoom = view.Zoom }));

            
            if (zoomDirection.HasValue)
            {
                client.SendZoom(new ZoomMsg(EDXLDistributionExtension.CreateHeader(),
                    new Zoom() { zoomdirection = zoomDirection.Value }));
            }
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
            {
                name = name.Substring(0, index);
            }

            // Scale, Rotation and CenterPosition have to be done seperately for X, Y and Z Due to Unity automatically removing all decimals after the first one is converting a Vector3/Quat to string.

            NewObject newObject = new NewObject()
            {
                Name = obj.obj.name,
                prefabname = name,
                lat = obj.lat,
                lon = obj.lon,
                scaleX = obj.localScale.x,
                scaleY = obj.localScale.y,
                scaleZ = obj.localScale.z,
                rotX = obj.rotation.x,
                rotY = obj.rotation.y,
                rotZ = obj.rotation.z,
                centerPosX = obj.centerPosition.x,
                centerPosY = obj.centerPosition.y,
                centerPosZ = obj.centerPosition.z

            };
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
            client.SendNewObject(new NewObjectMsg(EDXLDistributionExtension.CreateHeader(),
                newObject));


        }



        // Sends data of updated object to the other users.
        public void UpdateExistingObject(SpawnedObject obj)
        {

            UpdateObject updateObject = new UpdateObject()
            {
                Name = obj.obj.name,
                posX = obj.obj.transform.position.x,
                posY = obj.obj.transform.position.y,
                posZ = obj.obj.transform.position.z,
                lat = obj.lat,
                lon = obj.lon,
                scaleX = obj.obj.transform.localScale.x,
                scaleY = obj.obj.transform.localScale.y,
                scaleZ = obj.obj.transform.localScale.z,
                rotX = obj.obj.transform.eulerAngles.x,
                rotY = obj.obj.transform.eulerAngles.y,
                rotZ = obj.obj.transform.eulerAngles.z,
                User = me.id


            };
            client.SendUpdateObject(new UpdateObjectMsg(
                EDXLDistributionExtension.CreateHeader(),
                updateObject));
        }

        // Tells the other users what object should be deleted.
        public void UpdateDeletedObject(SpawnedObject obj)
        {
            DeleteObject delObject = new DeleteObject() { Name = obj.obj.name, user = me.id };
            client.SendDeleteObject(new DeleteObjectMsg(EDXLDistributionExtension.CreateHeader(),
                delObject));

        }



        // Applies the transformations done by other users.
        public void UpdateOtherUsersObject(UpdateObject pUpdateObject)
        {
            GameObject updatedObject = GameObject.Find(pUpdateObject.Name);

            updatedObject.transform.position = new Vector3(pUpdateObject.posX, pUpdateObject.posY, pUpdateObject.posZ);
            updatedObject.transform.localScale = new Vector3(pUpdateObject.scaleX, pUpdateObject.scaleY, pUpdateObject.scaleZ);
            updatedObject.transform.rotation = Quaternion.Euler(pUpdateObject.rotX, pUpdateObject.rotY, pUpdateObject.rotZ);

            foreach (SpawnedObject so in InventoryObjectInteraction.Instance.spawnedObjectsList)
            {
                if (so.obj == updatedObject)
                {
                    so.lat = pUpdateObject.lat;
                    so.lon = pUpdateObject.lon;
                }
            }

            Debug.Log("Object " + name + " has been updated");
        }

        // Recreates the object made by the other user.
        public void SpawnOtherUsersObject(NewObject pNewObject)
        {
            GameObject newObject;

            newObject = Instantiate(Resources.Load("Prefabs/Inventory/" + pNewObject.prefabname)) as GameObject;
            newObject.name = name;

            // Puts it under the same parent as objects created by the user of this instance.
            var newlySpawned = GameObject.Find("NewlySpawned");
            newObject.transform.SetParent(newlySpawned.transform, false);

            // Creates positional data from the seperate X Y Z values
            var objPosition = new Vector3(pNewObject.centerPosX, pNewObject.centerPosY, pNewObject.centerPosZ);
            var objRotation = Quaternion.Euler(pNewObject.rotX, pNewObject.rotY, pNewObject.rotZ);
            var objScale = new Vector3(pNewObject.scaleX, pNewObject.scaleY, pNewObject.scaleZ);

            // Sets the created positional data
            newObject.transform.position = objPosition;
            newObject.transform.localScale = objScale;
            newObject.transform.rotation = objRotation;

            // Creates a new SpawnedObject and adds it to the list.
            SpawnedObject spawnedObject = new SpawnedObject(newObject, newObject.transform.TransformDirection(newObject.transform.position), pNewObject.lat, pNewObject.lon, objScale, objRotation);
            Debug.Log("Adding to list: " + spawnedObject.obj);
            InventoryObjectInteraction.Instance.spawnedObjectsList.Add(spawnedObject);
            newObject.tag = "spawnobject";
        }

        // Deleted objects deleted by other user.
        public void DeleteOtherUsersObject(GameObject go)
        {
            if (go != null)
            {
                ObjectInteraction.Instance.Delete(go);
            }
        }




        #endregion

        #region Table management

        public void UpdateTable()
        {
            if (ShareTable)
            {
                var terrain = BoardInteraction.Instance.terrain.transform;
                var table = new eu.driver.model.worldexplorer.Table()
                {
                    xpos = terrain.position.x,
                    ypos = terrain.position.y,
                    zpos = terrain.position.z,
                    xrot = terrain.rotation.eulerAngles.x,
                    yrot = terrain.rotation.eulerAngles.y,
                    zrot = terrain.rotation.eulerAngles.z,
                    xscale = terrain.localScale.x,
                    yscale = terrain.localScale.y,
                    zscale = terrain.localScale.z,
                    name = me.id


                };
                client.SendTable(new TableMsg(EDXLDistributionExtension.CreateHeader(), table));

            }
        }

        public void SetTable(eu.driver.model.worldexplorer.Table pTable)
        {


            // Only runs if the data received comes from another user.
            if (pTable.name != me.id)
            {
                // Data is split up between X Y Z due to Unitys inability to format Vector3's to string without losing decimals.
                var tablePosition = new Vector3(pTable.xpos, pTable.ypos, pTable.zpos);
                var tableRotation = Quaternion.Euler(pTable.xrot, pTable.yrot, pTable.zrot);
                var tableScale = new Vector3(pTable.xscale, pTable.yscale, pTable.zscale);

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
            // Call method UpdatePresence every 5 seconds
            InvokeRepeating("UpdatePresence", 5, 1);
        }

        /// <summary>
        /// Update users in the session.
        /// </summary>
        /// <param name="json"></param>
        protected void UpdateUsersPresence(string json)
        {

        }

        /// <summary>
        /// A user in the session has changed its selection. Make it visible.
        /// </summary>
        /// <param name="selectedFeature"></param>
        /// <param name="user">If user does not exist, remove the current selection.</param>
        protected void UpdateUserSelection(Feature selectedFeature, User user = null)
        {
            var gameobj = GameObject.Find(selectedFeature.id);
            if (gameobj == null)
            {
                return;
            }

            GameObject selectedObject = gameobj.transform.parent.gameObject;
            SymbolTargetHandler handler = selectedObject.GetComponent<SymbolTargetHandler>();
            handler.OnSelect(user);
        }

        /// <summary>
        /// Update the presence status.
        /// </summary>
        protected void UpdatePresence()
        {
       
            PresenseMsg presense = new PresenseMsg(EDXLDistributionExtension.CreateHeader(),
            new Presense()
            {
                id = me.Id
               ,name = "TESTETETETETETTETETETETETETTET"
            }); 
            client.SendPresense(presense);
            // TODO SendJsonMessage(subtopic, me.ToJSON(), false);
            RemoveOldUsersFromSession();

        }

        /// <summary>
        /// Remove stale users from the session
        /// </summary>
        protected void RemoveOldUsersFromSession()
        {
            if (users.Count == 0)
            {
                return;
            }

            var now = DateTime.UtcNow;
            for (var i = users.Count - 1; i >= 0; i--)
            {
                var user = users[i];
                if (now - user.LastUpdateReceived > TimeSpan.FromSeconds(50))
                {
                    if (user.SelectedFeature != null && !string.IsNullOrEmpty(user.SelectedFeature.id))
                    {
                        UpdateUserSelection(user.SelectedFeature, user);
                    }

                    {
                        users.RemoveAt(i);
                        // TODO Not remove the cursor?!
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
            var subtopic = string.Format("layers/{0}", layer.LayerId);
            //SendJsonMessage(subtopic, layer.ToJSON(), false);
        }


    }
}
