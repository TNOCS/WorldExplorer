using eu.driver.model.worldexplorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M2MqttWorldExplorerClient
{
    public static class ConvertMessage
    {
        public static View ConvertView(string pMsg)
        {
            var json = new JSONObject(pMsg);
            return new View()
            {
                lat = json.GetFloat("lat"),
                lon = json.GetFloat("lon"),
                range = json.GetInt("range"),
                zoom = json.GetInt("zoom")
            };

        }

        public static DeleteObject ConvertDeleteObject(string pMsg)
        {
            var json = new JSONObject(pMsg);
            return new DeleteObject()
            {
                user = json.GetString("user"),
                Name = json.GetString("Name")
            };

        }

        public static Table ConvertTable(string pMsg)
        {
            var json = new JSONObject(pMsg);
            return new Table()
            {
                name = json.GetString("user"),
                xpos = json.GetFloat("posX"),
                ypos = json.GetFloat("posY"),
                zpos = json.GetFloat("posZ"),
                xscale = json.GetFloat("scaleX"),
                yscale = json.GetFloat("scaleY"),
                zscale = json.GetFloat("scaleZ"),
                xrot = json.GetFloat("rotX"),
                yrot = json.GetFloat("rotY"),
                zrot = json.GetFloat("rotZ")
            };

        }

        public static NewObject ConvertNewObject(string pJsonMessage)
        {
            var json = new JSONObject(pJsonMessage);
            return new NewObject()
            {
                Name = json.GetString("Name"),
                prefabname = json.GetString("prefabname"),
                lat = json.GetDouble("lat"),
                lon = json.GetDouble("lon"),
                scaleX = json.GetFloat("scaleX"),
                scaleY = json.GetFloat("scaleY"),
                scaleZ = json.GetFloat("scaleZ"),
                rotX = json.GetFloat("rotX"),
                rotY = json.GetFloat("rotY"),
                rotZ = json.GetFloat("rotZ"),
                centerPosX = json.GetFloat("centerPosX"),
                centerPosY = json.GetFloat("centerPosY"),
                centerPosZ = json.GetFloat("centerPosZ")
            };
        }

        public static UpdateObject ConvertUpdateObject(string pJsonMessage)
        {
            var json = new JSONObject(pJsonMessage);
            return new UpdateObject()
            {
                User = json.GetString("user"),
                Name = json.GetString("Name"),
                posX = json.GetFloat("posX"),
                posY = json.GetFloat("posY"),
                posZ = json.GetFloat("posZ"),
                lat = json.GetFloat("lat"),
                lon = json.GetFloat("lon"),
                scaleX = json.GetFloat("scaleX"),
                scaleY = json.GetFloat("scaleY"),
                scaleZ = json.GetFloat("scaleZ"),
                rotX = json.GetFloat("rotX"),
                rotY = json.GetFloat("rotY"),
                rotZ = json.GetFloat("rotZ")
            };

        }

        public static Presense ConvertPresense(string pJsonMessage)
        {
            var json = new JSONObject(pJsonMessage);
            return new Presense()
            {
               id = json.GetString("id"),
               //name = json.GetString("name")
               r = json.GetInt("r"),
               g = json.GetInt("g"),
               b = json.GetInt("b"),
               xpos = json.GetFloat("xpos"),
               ypos = json.GetFloat("ypos"),
               zpos = json.GetFloat("zpos"),
               xrot = json.GetFloat("xrot"),
               yrot = json.GetFloat("yrot"),
               zrot = json.GetFloat("zrot")

        };
        }

        public static Direction ConvertEnum(string pEnumString)
        {
            switch(pEnumString.ToUpper())
            {
                case "IN": return Direction.In;
                case "OUT": return Direction.Out;
            }
            return Direction.Out;
        }

        public static Zoom ConvertZoom(string pJsonMessage)
        {
            var json = new JSONObject(pJsonMessage);
            return new Zoom()
            {
                zoomdirection = ConvertEnum(json.GetString("direction"))
            };

        }

        public static string ToJson(Zoom pZoomObject)
        {
            string direction = "";
            switch (pZoomObject.zoomdirection)
            {
                case Direction.In:
                    direction = "In";
                    break;
                case Direction.Out:
                    direction = "Out";
                    break;

            }
            return string.Format(@"{{ ""direction"": ""{0}"" }}", direction);
        }

        public static string ToJson(DeleteObject pDelObject)
        {
            return string.Format(@"{{ ""Name"": ""{0}"", ""user"": ""{1}"" }}", pDelObject.Name, pDelObject.user);
        }

        public static string ToJson(UpdateObject pUpdateObject)
        {
             
            return string.Format(@"{{ ""Name"": ""{0}"", ""posX"": {1}, ""posY"": {2}, ""posZ"": {3}, ""lat"": {4}, ""lon"": {5}, ""scaleX"": {6}, ""scaleY"": {7}, ""scaleZ"": {8}, ""rotX"": {9}, ""rotY"": {10}, ""rotZ"": {11}, ""user"": ""{12}"" }}",
    pUpdateObject.Name, pUpdateObject.posX, pUpdateObject.posY, pUpdateObject.posZ,
    pUpdateObject.lat, pUpdateObject.lon, pUpdateObject.scaleX, pUpdateObject.scaleY, pUpdateObject.scaleZ,
    pUpdateObject.rotX, pUpdateObject.rotY, pUpdateObject.rotZ, pUpdateObject.User);
        }

        public static string ToJson(NewObject pNewObject)
        {
            return string.Format(@"{{ ""Name"": ""{0}"", ""prefabname"": ""{1}"", ""lat"": {2}, ""lon"": {3}, ""scaleX"": {4}, ""scaleY"": {5}, ""scaleZ"": {6}, ""rotX"": {7}, ""rotY"": {8}, ""rotZ"": {9}, ""centerPosX"": {10}, ""centerPosY"": {11}, ""centerPosZ"": {12} }}",
                pNewObject.Name,
                pNewObject.prefabname,
                pNewObject.lat,
                pNewObject.lon,
                pNewObject.scaleX,
                pNewObject.scaleY,
                pNewObject.scaleZ,
                pNewObject.rotX,
                pNewObject.rotY,
                pNewObject.rotZ,
                pNewObject.scaleX,
                pNewObject.scaleY,
                pNewObject.scaleZ,
                pNewObject.centerPosX,
                pNewObject.centerPosY,
                pNewObject.centerPosZ);
                
        }

        public static string ToJson(Table pTable)
        {
            return string.Format(@"{{ ""posX"": {0}, ""posY"": {1}, ""posZ"": {2}, ""rotX"": {3}, ""rotY"": {4}, ""rotZ"": {5}, ""scaleX"": {6}, ""scaleY"": {7}, ""scaleZ"": {8}, ""user"": ""{9}"" }}",
                    pTable.xpos, pTable.ypos, pTable.zpos,
                    pTable.xrot, pTable.yrot, pTable.zrot,
                    pTable.xscale, pTable.yscale, pTable.zscale,
                    pTable.name);
        }
        public static string ToJson(View pView)
        {
            return string.Format(@"{{ ""lat"": {0}, ""lon"": {1}, ""zoom"": {2}, ""range"": {3} }}",
                    pView.lat, pView.lon, pView.zoom, pView.range);
        }

        public static string ToJson(Presense pPresense)
        {
            return string.Format(@"{{ ""id"": ""{0}"", ""name"": ""{1}"", ""r"": {2}, ""g"": {3}, ""b"": {4}, ""xpos"": {5}, ""ypos"": {6}, ""zpos"": {7}, ""xrot"": {8}, ""yrot"": {9}, ""zrot"": {10} }}",
                    pPresense.id, pPresense.name, 
                    pPresense.r, pPresense.g, pPresense.b,
                    pPresense.xpos, pPresense.ypos, pPresense.zpos,
                    pPresense.xrot, pPresense.yrot, pPresense.zrot);
        }

    }
}
