using eu.driver.model.edxl;
using eu.driver.model.worldexplorer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WorldExplorerClient.messages
{
    public class PresenseMsg : BaseMsg
    {
        
        public PresenseMsg(EDXLDistribution pHeader, Presense pMsg) : base(pHeader)
        {
           
            Msg = pMsg;
        }


        [JsonProperty]
        public Presense Msg { get; private set; }

        [JsonIgnore]
        public override Type MsgType => typeof(Presense);
    }
}
