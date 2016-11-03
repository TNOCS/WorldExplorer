using Assets.Scripts.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Classes
{
    public class Table
    {
        public void FromJson(JSONObject json)
        {
            TableSize = json.GetFloat("TableSize");
            TableHeight = json.GetFloat("TableHeight");
        }

        public float TableSize { get; set; }
        public float TableHeight { get; set; }

    }
}
