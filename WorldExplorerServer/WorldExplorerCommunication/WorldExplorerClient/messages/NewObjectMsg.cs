using eu.driver.model.edxl;
using eu.driver.model.worldexplorer;
using System;
using System.Collections.Generic;
using System.Text;

namespace WorldExplorerClient.messages
{
   
    public class NewObjectMsg : BaseMsg
    {
        public NewObjectMsg(EDXLDistribution pHeader, NewObject pMsg) : base(pHeader)
        {
            Msg = pMsg;
        }

        public NewObject Msg { get; private set; }

        public override Type MsgType => typeof(NewObject);
    }
}
