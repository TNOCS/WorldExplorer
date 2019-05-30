using eu.driver.model.edxl;
using eu.driver.model.worldexplorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorldExplorerClient.interfaces;
using uPLibrary.Networking.M2Mqtt;
using WorldExplorerClient;
using WorldExplorerClient.messages;
using System.Net;

namespace M2MqttWorldExplorerClient
{
    public class MqttClient : IWorldExplorerClient
    {
        private Logger mLog = new Logger();

        private readonly uPLibrary.Networking.M2Mqtt.MqttClient mClient;
        private string mSessionName = null;

        public bool IsConnected => mClient.IsConnected;

        public string SenderId { get; set; }
      

        private string mTopic;

        public event EventHandler<PresenseMsg> OnPresense;
        public event EventHandler<NewObjectMsg> OnNewObject;
        public event EventHandler<UpdateObjectMsg> OnUpdateObject;
        public event EventHandler<DeleteObjectMsg> OnDeleteObject;
        public event EventHandler<ViewMsg> OnView;
        public event EventHandler<ZoomMsg> OnZoom;
        public event EventHandler<TableMsg> OnTable;


        public MqttClient(string pMqqtServer, int pPortnumber)
        {
            mClient = new uPLibrary.Networking.M2Mqtt.MqttClient(pMqqtServer, pPortnumber, false, null, null, MqttSslProtocols.None);
            
            // register to message received 
            mClient.MqttMsgPublishReceived += (sender, mqttMsg) =>
            {
                var msg = Encoding.UTF8.GetString(mqttMsg.Message);
                var subtopic = mqttMsg.Topic.Substring(mTopic.Length - 1);
                ProcessMessage(subtopic, msg);
            };
        }

        private void ProcessMessage(string topic, string pJsonMessage)
        {
            // Debug.Log(string.Format("Received message on topic {0}: {1}", subtopic, msg));
            switch (topic)
            {
                case TopicNames.MqttView:
                    View view = ConvertMessage.ConvertView(pJsonMessage);
                    ViewReceived(new ViewMsg(CreateKey(SenderId), view));                   
                    break;
                case TopicNames.MqttZoom:
                    Zoom zoom = ConvertMessage.ConvertZoom(pJsonMessage);
                    ZoomReceived(new ZoomMsg(CreateKey(SenderId), zoom));
                    break;
                case TopicNames.MqttPresense:
                    Presense presense = ConvertMessage.ConvertPresense(pJsonMessage);
                    PresenseReceived(new PresenseMsg(CreateKey(SenderId), presense));
                    break;
                case TopicNames.MqttNewObject:
                    NewObject newObject = ConvertMessage.ConvertNewObject(pJsonMessage);
                    NewObjectReceived(new NewObjectMsg(CreateKey(SenderId), newObject));
                    
                    break;
                case TopicNames.MqttUpdateObject:
                    UpdateObject updateObj = ConvertMessage.ConvertUpdateObject(pJsonMessage);
                    UpdateObjectReceived(new UpdateObjectMsg(CreateKey(SenderId), updateObj));
              
                    break;
                case TopicNames.MqttDeleteObject:
                    DeleteObject delObject = ConvertMessage.ConvertDeleteObject(pJsonMessage);
                    DeleteObjectReceived(new DeleteObjectMsg(CreateKey(SenderId), delObject));
           
                    break;
                case TopicNames.MqttTable:
                    Table table = ConvertMessage.ConvertTable(pJsonMessage);
                    TableReceived(new TableMsg(CreateKey(SenderId), table));
                    break;
            }
        }

        public async Task SendPresense(PresenseMsg pMessage)
        {
            await SendGeneric(pMessage, ConvertMessage.ToJson(pMessage.Msg), () => { PresenseReceived(pMessage); } );
        }
        public async Task SendNewObject(NewObjectMsg pMessage)
        {
            await SendGeneric(pMessage, ConvertMessage.ToJson(pMessage.Msg), () => { NewObjectReceived(pMessage); });
            
        }

        public async Task SendUpdateObject(UpdateObjectMsg pMessage)
        {
            await SendGeneric(pMessage, ConvertMessage.ToJson(pMessage.Msg), () => { UpdateObjectReceived(pMessage); });
            
        }
        public async Task SendDeleteObject(DeleteObjectMsg pMessage)
        {
            await SendGeneric(pMessage, ConvertMessage.ToJson(pMessage.Msg), () => { DeleteObjectReceived(pMessage); });
            
        }
        public async Task SendView(ViewMsg pMessage)
        {
            await SendGeneric(pMessage, ConvertMessage.ToJson(pMessage.Msg), () => { ViewReceived(pMessage); });
            
        }

        public async Task SendZoom(ZoomMsg pMessage)
        {
            await SendGeneric(pMessage, ConvertMessage.ToJson(pMessage.Msg), () => { ZoomReceived(pMessage); });
            
        }

        public async Task SendTable(TableMsg pMessage)
        {
            await SendGeneric(pMessage, ConvertMessage.ToJson(pMessage.Msg), () => { TableReceived(pMessage); });
            
        }

        private async Task SendGeneric<MsgType>(MsgType pJsonMessage, string pJsonMsg, Action pCallback) 
        {
             try
            {
                if (!TopicNames.topicNames.ContainsKey(typeof(MsgType))) throw new NotImplementedException();
                string topicName = TopicNames.topicNames[typeof(MsgType)];
                if (mClient.IsConnected)
                {
                    mClient.Publish(string.Format("{0}/{1}", mSessionName, topicName), Encoding.UTF8.GetBytes(pJsonMsg), uPLibrary.Networking.M2Mqtt.Messages.MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
                    //this client will also receive this own message
                }
                else
                {
                    // When not connected to mqtt, emulate the echo from the message from mqtt server
                    pCallback?.Invoke();

                }
            } catch(Exception ex)
            {
                
            } 
        }


        protected virtual void PresenseReceived(PresenseMsg pMessage)
        {
            OnPresense?.Invoke(this, pMessage);
        }

        protected virtual void NewObjectReceived(NewObjectMsg pMessage)
        {
            OnNewObject?.Invoke(this, pMessage);
        }

        protected virtual void UpdateObjectReceived(UpdateObjectMsg pMessage)
        {
            OnUpdateObject?.Invoke(this, pMessage);
        }

        protected virtual void DeleteObjectReceived(DeleteObjectMsg pMessage)
        {
            OnDeleteObject?.Invoke(this, pMessage);
        }

        protected virtual void ViewReceived(ViewMsg pMessage)
        {
            OnView?.Invoke(this, pMessage);
        }

        protected virtual void ZoomReceived(ZoomMsg pMessage)
        {
            OnZoom?.Invoke(this, pMessage);
        }

        protected virtual void TableReceived(TableMsg pMessage)
        {
            OnTable?.Invoke(this, pMessage);
        }

        public bool Connect(string pId)
        {
            try
            {
                mClient.Connect(pId);
            } catch(Exception ex)
            {
                //Log.LogException("Failed to connect to MQTT server", ex);
                return false;
            }
            return IsConnected;
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        private void Subscribe(string pTopicName)
        {
            mTopic = pTopicName;
            mClient.Subscribe(new[] { pTopicName }, new byte[] { uPLibrary.Networking.M2Mqtt.Messages.MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        }

        private void UnSubscribe(string pTopicName)
        {
            mTopic = null;
            mClient.Unsubscribe(new[] { pTopicName });
        }

        /// <summary>
        /// Method for creating the key to be used within this producer
        /// </summary>
        /// <returns>An EDXL-DE key containing all information for adapters to understand where this message originates from</returns>
        private EDXLDistribution CreateKey(string pSenderId)
        {
            return new EDXLDistribution()
            {
                senderID = pSenderId,
                distributionID = Guid.NewGuid().ToString(),
                distributionKind = DistributionKind.Unknown,
                distributionStatus = DistributionStatus.Unknown,
                dateTimeSent = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds,
                dateTimeExpires = (long)((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)) + new TimeSpan(0, 0, 10, 0, 0)).TotalMilliseconds,
            };
        }

        public void JoinSession(string pSessionName)
        {
            if (!IsConnected || string.IsNullOrEmpty(pSessionName)) return;
            
            if (!string.IsNullOrEmpty(mSessionName)) UnSubscribe(@"{pSessionName}/#");
            mSessionName = pSessionName;
            Subscribe($"{pSessionName}/#");
        }

        public void SetCallback(Action<IWorldClientLogging> pCallback)
        {
            mLog.SetCallback(pCallback);
        }
    }
}