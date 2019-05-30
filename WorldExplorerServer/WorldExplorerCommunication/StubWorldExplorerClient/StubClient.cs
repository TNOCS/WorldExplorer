using eu.driver.model.edxl;
using eu.driver.model.worldexplorer;
using System;
using System.Threading.Tasks;
using WorldExplorerClient;
using WorldExplorerClient.interfaces;
using WorldExplorerClient.messages;

namespace StubWorldExplorerClient
{
    public class StubClient : IWorldExplorerClient
    {
        private Logger mLog = new Logger();

        private string mSessionName = null;

        public bool IsConnected => mIsConnected;

        public string SenderId { get; set; }

        private bool mIsConnected = false;


        public event EventHandler<PresenseMsg> OnPresense;
        public event EventHandler<NewObjectMsg> OnNewObject;
        public event EventHandler<UpdateObjectMsg> OnUpdateObject;
        public event EventHandler<DeleteObjectMsg> OnDeleteObject;
        public event EventHandler<ViewMsg> OnView;
        public event EventHandler<ZoomMsg> OnZoom;
        public event EventHandler<TableMsg> OnTable;


        public StubClient()
        {
        }

        

        public async Task SendPresense(PresenseMsg pMessage)
        {
            
        }
        public async Task SendNewObject(NewObjectMsg pMessage)
        {
            
        }

        public async Task SendUpdateObject(UpdateObjectMsg pMessage)
        {
            
        }
        public async Task SendDeleteObject(DeleteObjectMsg pMessage)
        {
            
        }
        public async Task SendView(ViewMsg pMessage)
        {
            
        }

        public async Task SendZoom(ZoomMsg pMessage)
        {
            
        }

        public async Task SendTable(TableMsg pMessage)
        {
            
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
            mIsConnected = true;
            return true;
        }

        public void Disconnect()
        {
            mIsConnected = false;
        }
        public void JoinSession(string pSessionName)
        {
            mSessionName = pSessionName;
        }

        public void SetCallback(Action<IWorldClientLogging> pCallback)
        {
            mLog.SetCallback(pCallback);
        }
    }
}