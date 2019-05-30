using eu.driver.model.edxl;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using WorldExplorerClient.interfaces;

namespace WorldExplorerClient.messages
{
    // Because of JSON it not possible to use generic types (without including type in JSON)
    public abstract class BaseMsg : IMessage
    {
        protected BaseMsg(EDXLDistribution pHeader)
        {
            Header = pHeader;
            
        }

        [JsonProperty]
        public EDXLDistribution Header { get; private set; }

        [JsonIgnore]
        public abstract Type MsgType { get;  }
    }
}
