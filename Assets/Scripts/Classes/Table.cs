namespace Assets.Scripts.Classes
{
    public class Table
    {
        public void FromJson(JSONObject json)
        {
            Size = json.GetFloat("size", 1.5F);
            Height = json.GetFloat("height", 1.3F);
            Thickness = json.GetFloat("thickness", 0.1F);
        }

        /// <summary>
        /// Size, i.e. width and length of the square table
        /// </summary>
        public float Size { get; set; }

        /// <summary>
        /// Distance from the floor of the table top
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// Thickness of the table top
        /// </summary>
        public float Thickness { get; set; }

        /// <summary>
        /// Returns the position of the table's centre point based on the desired table height and thickness.
        /// </summary>
        public float Position
        {
            get
            {
                return Height - Thickness / 2;
            }
        }
    }
}
