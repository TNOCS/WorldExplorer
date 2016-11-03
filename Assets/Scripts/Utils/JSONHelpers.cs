using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Utils
{

    public static class JsonHelpers
    {
        public static double GetDouble(this JSONObject json, string key, double defaultValue = 0)
        {
            if (json && json.HasField(key) && json[key].IsNumber) return json[key].n;
            return defaultValue;
        }

        public static int GetInt(this JSONObject json, string key, int defaultValue = 0)
        {
            if (json && json.HasField(key) && json[key].IsNumber) return (int)json[key].n;
            return defaultValue;
        }

        public static float GetFloat(this JSONObject json, string key, float defaultValue = 0f)
        {
            if (json && json.HasField(key) && json[key].IsNumber) return (float)json[key].n;
            return defaultValue;
        }

        public static string GetString(this JSONObject json, string key, string defaultValue = "")
        {
            if (json && json.HasField(key) && json[key].IsString) return json[key].str;
            return defaultValue;
        }

        public static bool GetBoolean(this JSONObject json, string key, bool defaultValue = false)
        {
            if (json && json.HasField(key) && json[key].IsBool) return json[key].b;
            return defaultValue;
        }

    }
}
