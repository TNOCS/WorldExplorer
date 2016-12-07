using Symbols;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Classes
{
    // A delegate type for hooking up change notifications.
    public delegate void SelectedFeatureChangedEventHandler(object sender, EventArgs e);

    public class User
    {
        private string id;
        private Color selectionColor;
        private DateTime lastUpdateReceived;
        public GameObject Cursor { get; set; }
        public Material UserMaterial { get; private set; }

        public User()
        {
            id = Guid.NewGuid().ToString();
            LastUpdateReceived = DateTime.UtcNow;
        }

        public User(string id)
        {
            this.id = id;
            LastUpdateReceived = DateTime.UtcNow;
        }

        public string Id { get { return id; } }

        public Color SelectionColor
        {
            get { return selectionColor; }
            set
            {
                if (selectionColor == value) return;
                selectionColor = value;
                UserMaterial = new Material(Shader.Find("HoloToolkit/Cursor"));
                UserMaterial.color = value;
            }
        }

        public Feature SelectedFeature { get; set; }

        /// <summary>
        /// Last update in UTC time
        /// </summary>
        public DateTime LastUpdateReceived
        {
            get { return lastUpdateReceived; }
            set { lastUpdateReceived = value; }
        }

        public string Name { get; set; }

        public override string ToString()
        {
            return string.Format(@"id: {0}, name: {6}, selectedFeatureId: {1}, selectionColor: r: {2}, g: {3}, b: {4}, a: {5}",
                    id, SelectedFeature.id, selectionColor.r, selectionColor.g, selectionColor.b, selectionColor.a, Name);
        }

        public string ToJSON()
        {
            if (SelectedFeature == null)
            {
                // Only send the ID
                if (Cursor == null)
                    return string.Format(@"{{ ""id"": ""{0}"", ""name"": ""{1}"", ""selectionColor"": {{ ""r"": {2}, ""g"": {3}, ""b"": {4}, ""a"": {5} }}, }}", id, Name, selectionColor.r, selectionColor.g, selectionColor.b, selectionColor.a);
                else
                    return string.Format(@"{{ ""id"": ""{0}"", ""name"": ""{1}"", ""selectionColor"": {{ ""r"": {2}, ""g"": {3}, ""b"": {4}, ""a"": {5} }}, ""cursor"":{{""xpos"":{6},""ypos"":{7},""zpos"":{8},""xrot"":{9},""yrot"":{10},""zrot"":{11} }}}}", id, Name, selectionColor.r, selectionColor.g, selectionColor.b, selectionColor.a, Cursor.transform.position.x, Cursor.transform.position.y, Cursor.transform.position.z, Cursor.transform.rotation.x, Cursor.transform.rotation.y, Cursor.transform.rotation.z);
            }
            else
            {
                if (Cursor == null)
                    return string.Format(@"{{ ""id"": ""{0}"", ""name"": ""{6}"", ""selectedFeature"": {1}, ""selectionColor"": {{ ""r"": {2}, ""g"": {3}, ""b"": {4}, ""a"": {5} }} }}", id, SelectedFeature.ToLimitedJSON(), selectionColor.r, selectionColor.g, selectionColor.b, selectionColor.a, Name);
                else
                    return string.Format(@"{{ ""id"": ""{0}"", ""name"": ""{6}"", ""selectedFeature"": {1}, ""selectionColor"": {{ ""r"": {2}, ""g"": {3}, ""b"": {4}, ""a"": {5} }}, ""cursor"":{{""xpos"":{7},""ypos"":{8},""zpos"":{9},""xrot"":{10},""yrot"":{11},""zrot"":{12} }} }}",
                id, SelectedFeature.ToLimitedJSON(), selectionColor.r, selectionColor.g, selectionColor.b, selectionColor.a, Name, Cursor.transform.position.x, Cursor.transform.position.y, Cursor.transform.position.z, Cursor.transform.rotation.x, Cursor.transform.rotation.y, Cursor.transform.rotation.z);
            }
        }

        public static User FromJSON(string json, List<GameObject> cursors)
        {
            var jsonObj = new JSONObject(json);
            var user = new User(jsonObj.GetString("id"));
            if (jsonObj.HasField("selectedFeature"))
            {

                user.SelectedFeature = new Feature();
                var sf = jsonObj["selectedFeature"];
                user.SelectedFeature.id = sf.GetString("id");

                user.SelectedFeature.SetLatLon(new Vector2d(sf.GetFloat("lon"), sf.GetFloat("lat")));

                if (jsonObj.HasField("selectionColor"))
                {
                    var a = jsonObj["selectionColor"].GetFloat("a", 1);
                    var r = jsonObj["selectionColor"].GetFloat("r", 1);
                    var g = jsonObj["selectionColor"].GetFloat("g", 1);
                    var b = jsonObj["selectionColor"].GetFloat("b", 1);
                    user.SelectionColor = new Color(r, g, b, a);
                }
                if (jsonObj.HasField("cursor"))
                {
                    var cur = jsonObj["cursor"];
                    var posx = cur.GetFloat("xpos", 1);
                    var posy = cur.GetFloat("ypos", 1);
                    var posz = cur.GetFloat("zpos", 1);
                    var rotx = cur.GetFloat("xrot", 1);
                    var roty = cur.GetFloat("yrot", 1);
                    var rotz = cur.GetFloat("zrot", 1);
                    if (user.Cursor == null && cursors !=null&& cursors.Count != 0)
                    {
                        user.Cursor = cursors.Find(i => i.name == user.id + "-Cursor");
                    }
                    if (user.Cursor != null)
                    {
                        user.Cursor.transform.position = new Vector3(posx, posy, posz);
                        user.Cursor.transform.rotation = Quaternion.Euler(rotx, roty, rotz);
                    }
                    


                }
            }
            return user;
        }
    }
}