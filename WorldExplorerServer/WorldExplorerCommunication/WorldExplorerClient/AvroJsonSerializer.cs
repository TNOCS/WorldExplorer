using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace WorldExplorerClient
{
    // Dont serialize AVRO schema's


    public class AvroJsonSerializer : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty prop = base.CreateProperty(member, memberSerialization);
            if (prop.PropertyType == typeof(Avro.Schema))
            {
                prop.Ignored = true;
            }
            return prop;
        }
    }
}
