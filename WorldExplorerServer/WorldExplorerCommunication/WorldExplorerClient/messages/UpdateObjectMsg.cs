using eu.driver.model.edxl;
using eu.driver.model.worldexplorer;
using System;
using System.Collections.Generic;
using System.Text;

namespace WorldExplorerClient.messages
{

    public class UpdateObjectMsg : BaseMsg
    {
        public UpdateObjectMsg(EDXLDistribution pHeader, UpdateObject pMsg) : base(pHeader)
        {
            Msg = pMsg;
        }

        public UpdateObject Msg { get; private set; }

        public override Type MsgType => typeof(UpdateObject);
    }
}
