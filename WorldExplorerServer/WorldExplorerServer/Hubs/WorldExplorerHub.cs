using eu.driver.model.edxl;
using eu.driver.model.worldexplorer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WorldExplorerClient;
using WorldExplorerClient.messages;
using WorldExplorerServer.Models;
using WorldExplorerServer.Services;

namespace WorldExplorerServer.Hubs
{
    public class Helper
    {
        public PresenseMsg TheMsg { get; set; }
    }

    public class LogTraceWriter1 : ITraceWriter
    {
        ILogger<WorldExplorerHub> mLogger;
        public LogTraceWriter1(ILogger<WorldExplorerHub> pLogger)
        {
            mLogger = pLogger;

        }

        public TraceLevel LevelFilter => TraceLevel.Verbose;

       

        public void Trace(TraceLevel level, string message, Exception ex)
        {
            mLogger.LogInformation(message);
        }

  
    }


    public class WorldExplorerHub : Hub
    {
        private readonly ILogger mLogger;

        public WorldExplorerHub(
            IConfigurationService pConfig,
            ILogger<WorldExplorerHub> pLogger)
        {
            mLogger = pLogger;
            try
            {
                var json = JsonConvert.SerializeObject(new PresenseMsg(EDXLDistributionExtension.CreateHeader(),
                    new eu.driver.model.worldexplorer.Presense()),
                    new JsonSerializerSettings()
                    {
                        ContractResolver = new AvroJsonSerializer(),
                        TraceWriter = new LogTraceWriter1(pLogger)

                    });
                PresenseMsg account = JsonConvert.DeserializeObject<PresenseMsg>(json);
            }
            catch (Exception ex)
            {

            }
        }

        [HubMethodName("Presense")]
        public async Task SendPresense(PresenseMsg pMessage)
        {
           // await Clients.All.SendAsync("WorldExplorerClientLogMessage",
            //    new WorldExplorerClientLogMessage() { topic = "Presense", message = pMessage.Msg.ToString() });
           // await Clients.All.SendAsync("SendPresense", pMessage);
        }

        [HubMethodName("WorldExplorerClientLogMessage")]
        public async Task SendMessage(WorldExplorerClientLogMessage message)
        {
            await Clients.All.SendAsync("WorldExplorerClientLogMessage", message);
        }

        [HubMethodName("TEST")]
        public async Task SendMessage(PresenseMsg message)
        {
        }
            

        [HubMethodName("TestMsg")]
        public async Task TestMsg(string message)
        {
            await Clients.All.SendAsync("TestMsg", message);
        }

        public override async Task OnConnectedAsync()
        {
            mLogger.LogInformation($"SignalR client connected with id '{Context.ConnectionId}'.");
            await base.OnConnectedAsync();
            //SendPresense(new PresenseMsg(EDXLDistributionExtension.CreateHeader(),
              //  new eu.driver.model.worldexplorer.Presense()));
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            mLogger.LogInformation($"SignalR client with id '{Context.ConnectionId}' disconnected.");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
