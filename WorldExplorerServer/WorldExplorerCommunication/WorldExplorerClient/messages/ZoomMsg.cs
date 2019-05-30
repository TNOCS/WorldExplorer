using eu.driver.model.edxl;
using eu.driver.model.worldexplorer;
using System;
using System.Collections.Generic;
using System.Text;

namespace WorldExplorerClient.messages
{
     
    public class ZoomMsg : BaseMsg
    {
        public ZoomMsg(EDXLDistribution pHeader, Zoom pMsg) : base(pHeader)
        {
            Msg = pMsg;
        }

        public Zoom Msg { get; private set; }

        public override Type MsgType => typeof(Zoom);
    }
}
