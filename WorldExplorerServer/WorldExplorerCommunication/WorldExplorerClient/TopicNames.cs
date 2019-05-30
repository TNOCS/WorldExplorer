using eu.driver.model.worldexplorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldExplorerClient
{
    public static class TopicNames
    {
        public const String Presense = "WorldExplorerPresense";
        public const String NewObject = "WorldExplorerNewObject";
        public const String UpdateObject = "WorldExplorerUpdateObject";
        public const String DeleteObject = "WorldExplorerDeleteObject";
        public const String Zoom = "WorldExplorerZoom";
        public const String View = "WorldExplorerView";
        public const String Table = "WorldExplorerTable";

        public const String MqttPresense = "presense";
        public const String MqttNewObject = "newobject";
        public const String MqttUpdateObject = "updateobject";
        public const String MqttDeleteObject = "deleteobject";
        public const String MqttZoom = "zoomdirection";
        public const String MqttView = "view";
        public const String MqttTable = "table";

        // Topic names used mqtt
        public static Dictionary<Type, string> topicNames = new Dictionary<Type, string>() {
             { typeof(View), MqttView},
             { typeof(Table), MqttTable },
             { typeof(NewObject), MqttNewObject },
             { typeof(UpdateObject), MqttUpdateObject },
             { typeof(DeleteObject), MqttDeleteObject },
             { typeof(Presense), MqttPresense },
             { typeof(Zoom), MqttZoom }
            };
    }
}
