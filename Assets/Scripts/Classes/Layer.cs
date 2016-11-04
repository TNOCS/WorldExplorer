using Assets.Scripts.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public void FromJson(JSONObject json)
        {
            Title = json.GetString("Title");
            Url = json.GetString("Url");
            Type = json.GetString("Type");
            Enabled = json.GetBoolean("Enabled");
            VoiceCommand = json.GetString("VoiceCommand");
            UseTransparency = json.GetBoolean("UseTransparency");
            Height = json.GetFloat("Height");
            Group = json.GetString("Group");
            IconUrl = json.GetString("IconUrl");
        }

        public string Title { get; set; }
        public string Type { get; set; }
        /// <summary>
        /// Voice command to turn the layer on/off
        /// </summary>
        public string VoiceCommand { get; set; }
        /// <summary>
        /// Source URL 
        /// </summary>
        public string Url { get; set; }
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
        /// Url of the image being used
        /// </summary>
        public string IconUrl { get; set; }

        /// <summary>
        /// Name of layer group, only one layer can be active in a group
        /// </summary>
        public string Group { get; set; }
    }

}
