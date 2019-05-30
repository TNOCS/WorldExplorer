using eu.driver.model.edxl;
using eu.driver.model.worldexplorer;
using System;
using System.Collections.Generic;
using System.Text;

namespace WorldExplorerClient.messages
{
     
    public class DeleteObjectMsg : BaseMsg
    {
        public DeleteObjectMsg(EDXLDistribution pHeader, DeleteObject pMsg) : base(pHeader)
        {
            Msg = pMsg;
        }

        public DeleteObject Msg { get; private set; }

        public override Type MsgType => typeof(DeleteObject);
    }
}
