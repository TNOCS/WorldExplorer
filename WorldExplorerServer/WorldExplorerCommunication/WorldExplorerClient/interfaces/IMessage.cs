using eu.driver.model.edxl;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WorldExplorerClient.interfaces
{
    interface IMessage
    {
        EDXLDistribution Header { get;  }
        [JsonIgnore]
        Type MsgType { get;}
    }
}
