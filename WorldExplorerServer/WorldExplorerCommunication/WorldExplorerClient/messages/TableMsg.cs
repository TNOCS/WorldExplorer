using eu.driver.model.edxl;
using eu.driver.model.worldexplorer;
using System;
using System.Collections.Generic;
using System.Text;

namespace WorldExplorerClient.messages
{
     
    public class TableMsg : BaseMsg
    {
        public TableMsg(EDXLDistribution pHeader, Table pMsg) : base(pHeader)
        {
            Msg = pMsg;
        }

        public Table Msg { get; private set; }

        public override Type MsgType => typeof(Table);
    }
}
