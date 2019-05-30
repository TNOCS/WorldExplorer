
using eu.driver.model.edxl;
using eu.driver.model.worldexplorer;
using System;
using System.Threading.Tasks;
using WorldExplorerClient.messages;

namespace WorldExplorerClient.interfaces
{
    public interface IWorldClientLogging
    {
        string Message { get; }
    }
    public interface IWorldExplorerClient
    {
        bool Connect(string pClientId);
        void Disconnect();

        void JoinSession(string pSessionName);

        Task SendPresense(PresenseMsg pMessage);
        Task SendNewObject(NewObjectMsg pMessage);
        Task SendUpdateObject(UpdateObjectMsg pMessage);
        Task SendDeleteObject(DeleteObjectMsg pMessage);
        Task SendView(ViewMsg pMessage);
        Task SendZoom(ZoomMsg message);
        Task SendTable(TableMsg message);

        bool IsConnected { get; }


        event EventHandler<PresenseMsg> OnPresense;
        event EventHandler<NewObjectMsg> OnNewObject;
        event EventHandler<UpdateObjectMsg> OnUpdateObject;
        event EventHandler<DeleteObjectMsg> OnDeleteObject;
        event EventHandler<ViewMsg> OnView;
        event EventHandler<ZoomMsg> OnZoom;
        event EventHandler<TableMsg> OnTable;

        void SetCallback(Action<IWorldClientLogging> pCallback);
    }
}
