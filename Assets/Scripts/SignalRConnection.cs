
using eu.driver.model.worldexplorer;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityWorldExplorerClient;
using WorldExplorerClient;
using WorldExplorerClient.interfaces;
using WorldExplorerClient.messages;
using Microsoft.Extensions.DependencyInjection;

public class SignalRConnection : MonoBehaviour
{
    public class ChatMessage
    {
        public string topic { get; set; }

        public string message { get; set; }

       
    }
    HubConnection connection;

    async void Start()
    {
        
        //var client = ClientFactory.CreateSignalrClient("localhost", x => Debug.Log(x.Message));
        /*
        var hubConnectionBuilder = new HubConnectionBuilder()
            
            .AddJsonProtocol(options => {
                options.PayloadSerializerSettings.TraceWriter = this;
                options.PayloadSerializerSettings.ContractResolver = new AvroJsonSerializer();
                options.PayloadSerializerSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All;
            })
            .WithUrl("http://127.0.0.1:8888/WorldExplorerHub");
        connection = hubConnectionBuilder.Build();
        connection.On<ChatMessage>("WorldExplorerClientLogMessage", msg =>
        {
            Debug.Log(msg.message);
        });
        try
        {
            await connection.StartAsync();
            var x = new PresenseMsg(EDXLDistributionExtension.CreateHeader(),
                new Presense() { name="kluiver" });
            
            await connection.InvokeAsync("TEST", x);
            await connection.InvokeAsync("WorldExplorerClientLogMessage", new ChatMessage() { message = "test message", topic = "Presense" });
        }
        catch (Exception ex)
        {

        }
        */
    }

    private void OnDestroy()
    {

    }
    
}
