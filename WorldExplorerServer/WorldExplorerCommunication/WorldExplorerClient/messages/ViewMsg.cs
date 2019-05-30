using eu.driver.model.edxl;
using eu.driver.model.worldexplorer;
using System;
using System.Collections.Generic;
using System.Text;

namespace WorldExplorerClient.messages
{
     
    public class ViewMsg : BaseMsg
    {
        public ViewMsg(EDXLDistribution pHeader, View pMsg) : base(pHeader)
        {
            Msg = pMsg;
        }

        public View Msg { get; private set; }

        public override Type MsgType => typeof(View);
    }
}
