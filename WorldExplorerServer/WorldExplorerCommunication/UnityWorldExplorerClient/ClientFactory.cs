//using M2MqttWorldExplorerClient;
using StubWorldExplorerClient;
using System;
using System.Collections.Generic;
using System.Text;
using UnitySignalrClient;
using WorldExplorerClient.interfaces;
using static WorldExplorerClient.Logger;

namespace UnityWorldExplorerClient
{
    // This project wraps all WorldExporer plugings (MQTT, SignalR, Stub) to one project (naming conflicts in unity)

    public static class ClientFactory
    {
        public static IWorldExplorerClient CreateStubClient()
        {
            return new StubClient();
        }
        /*
        public static IWorldExplorerClient CreateMqttClient(string pHostName, int pPortNumber)
        {
            return new MqttClient(pHostName, pPortNumber);
        }
        */
        public static IWorldExplorerClient CreateSignalrClient(string pUrl, Action<IWorldClientLogging> pLogger)
        {
            return new WorldExplorerSignalrClient(pUrl, pLogger);
        }

    }
}
