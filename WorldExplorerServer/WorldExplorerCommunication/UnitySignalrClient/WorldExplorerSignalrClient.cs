using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using WorldExplorerClient;
using WorldExplorerClient.interfaces;
using WorldExplorerClient.messages;
using static WorldExplorerClient.Logger;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using System.Diagnostics;

namespace UnitySignalrClient
{
    public class WorldExplorerSignalrClient : IWorldExplorerClient, ITraceWriter
    {
        private Logger mLog = new Logger();
        private HubConnection mConnection;

        private string mSessionName = null;

        public bool IsConnected => mIsConnected;

        public string SenderId { get; set; }

        private bool mIsConnected = false;

        // ITraceWriter
        public TraceLevel LevelFilter => TraceLevel.Verbose;

        public void Trace(TraceLevel level, string message, Exception ex)
        {
            mLog.LogMessage(message);
        }

        public event EventHandler<PresenseMsg> OnPresense;
        public event EventHandler<NewObjectMsg> OnNewObject;
        public event EventHandler<UpdateObjectMsg> OnUpdateObject;
        public event EventHandler<DeleteObjectMsg> OnDeleteObject;
        public event EventHandler<ViewMsg> OnView;
        public event EventHandler<ZoomMsg> OnZoom;
        public event EventHandler<TableMsg> OnTable;
       

        public WorldExplorerSignalrClient(string pUrl, Action<IWorldClientLogging> pLogger)
        {
            SetCallback(pLogger);
            mLog.LogMessage($"Constructor WorldExplorerSignalrClient");
            
            InitializeSignalr(pUrl);
        }

        private void InitializeSignalr(string pUrl)
        {
            mLog.LogMessage($"Initialize signalr hub {pUrl}");
            mConnection = new HubConnectionBuilder() /*
                .ConfigureLogging(logging =>
                {
                    // Register your providers

                    // Set the default log level to Information, but to Debug for SignalR-related loggers.
                    logging.SetMinimumLevel(LogLevel.Information);
                    logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
                    logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);
                }) */
                .AddJsonProtocol(options => {
                    options.PayloadSerializerSettings.TraceWriter = this;
                    options.PayloadSerializerSettings.ContractResolver = new AvroJsonSerializer();

                })
                .WithUrl(pUrl)
                .Build();
            var connect = mConnection != null;
            mLog.LogMessage($"mConnection {connect}");
            mConnection.Closed += async (error) =>
            {
                mLog.LogMessage($"Lost signalr connection, reconnect");
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await mConnection.StartAsync();
            };

     

            mConnection.On<PresenseMsg>("Presense", (msg) => OnPresense(this, msg));
            mConnection.On<NewObjectMsg>("NewObject", (msg) => OnNewObject(this, msg));
            mConnection.On<UpdateObjectMsg>("UpdateObject", (msg) => OnUpdateObject(this, msg));
            mConnection.On<DeleteObjectMsg>("DeleteObject", (msg) => OnDeleteObject(this, msg));
            mConnection.On<ViewMsg>("View", (msg) => OnView(this, msg));
            mConnection.On<ZoomMsg>("Zoom", (msg) => OnZoom(this, msg));
            mConnection.On<TableMsg>("Table", (msg) => OnTable(this, msg));

            mLog.LogMessage($"Finished");
           
        }



        public async Task SendPresense(PresenseMsg pMessage)
        {
            await mConnection.InvokeAsync("Presense", pMessage);
        }
        public async Task SendNewObject(NewObjectMsg pMessage)
        {
            await mConnection.InvokeAsync("NewObject", pMessage);
        }

        public async Task SendUpdateObject(UpdateObjectMsg pMessage)
        {
            await mConnection.InvokeAsync("UpdateObject", pMessage);
        }
        public async Task SendDeleteObject(DeleteObjectMsg pMessage)
        {
            await mConnection.InvokeAsync("DeleteObject", pMessage);
        }
        public async Task SendView(ViewMsg pMessage)
        {
            await mConnection.InvokeAsync("View", pMessage);
        }

        public async Task SendZoom(ZoomMsg pMessage)
        {
            await mConnection.InvokeAsync("Zoom", pMessage);
        }

        public async Task SendTable(TableMsg pMessage)
        {
            await mConnection.InvokeAsync("Table", pMessage);
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
                mLog.LogMessage("Start connecting");
                var task = Task.Run(async () => await ConnectSync());
                task.Wait();
                mIsConnected = task.Result; //.GetAwaiter().GetResult(); ;
                mLog.LogMessage($"Signalr connected: {mIsConnected}");
                return mIsConnected;
            } catch (Exception ex)
            {
                mLog.LogMessage("Failed to connect to signalr:" + ex.Message);
                return false;
            }
        }

        public async Task<bool> ConnectSync()
        {
            try
            {
                if (mConnection == null)
                {
                    mLog.LogMessage($"Signalr mConnection is null");
                    return false;
                }
                await mConnection.StartAsync();
                mLog.LogMessage($"Connected to signalr hub");
            }
            catch (Exception ex)
            {
                mLog.LogMessage("Failed to connect to signalr:" + ex.Message);
                return false;
            }
            return true;
        }

        public void Disconnect()
        {
            mConnection.StopAsync();
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