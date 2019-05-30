using System.Collections.Generic;
using System.Text;
using System.Threading;
using Symbols;
using UnityEngine;

namespace Assets.Scripts.Classes
{
    public class Layer
    {
        public Layer()
        {
            Enabled = false;
            Height = 1.5F;
            UseTransparency = true;
        }

        public virtual void FromJson(JSONObject json)
        {
            LayerId = json.GetString("layerid");
            Enabled = json.GetBoolean("enabled");
            VoiceCommand = json.GetString("voiceCommand");
            UseTransparency = json.GetBoolean("useTransparency");
            Height = json.GetFloat("height");
            Group = json.GetString("group");

        }

        public bool _active { get; set; }
        public GameObject _object { get; set; }
       

        public string LayerId { get; set; }
        
        /// <summary>
        /// Voice command to turn the layer on/off
        /// </summary>
        public string VoiceCommand { get; set; }
        /// <summary>
        /// Source URL 
        /// </summary>

        public bool Enabled { get; set; }
        /// <summary>
        /// Some layers are transparent (default true), and the transparency needs to be set.
        /// </summary>
        public bool UseTransparency { get; set; }
        /// <summary>
        /// Rendering height of the layer
        /// </summary>
        public float Height { get; set; }



        /// <summary>
        /// Name of layer group, only one layer can be active in a group
        /// </summary>
        public string Group { get; set; }

    }

}
