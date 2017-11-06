using MapzenGo.Helpers;
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

        public Vector2d CenterInMercator { get; set; }

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
        public float Scale { get; internal set; }

        public override string ToString()
        {
            return string.Format(@"id: {0}, name: {6}, selectedFeatureId: {1}, selectionColor: r: {2}, g: {3}, b: {4}, a: {5}",
                    id, SelectedFeature.id, selectionColor.r, selectionColor.g, selectionColor.b, selectionColor.a, Name);
        }

        public string CursorLocationToJSON()
        {
          //  if (Cursor == null){
          //      Debug.Log("Cursor is null, setting");
          //      Cursor = GameObject.Find("Cursor");
          //  }

            if (CenterInMercator == null) return string.Empty;
            var v0 = new Vector2d(Cursor.transform.position.x / Scale, Cursor.transform.position.z / Scale) + CenterInMercator;
            var v1 = GM.MetersToLatLon(v0);
            //Debug.Log(string.Format(@"""loc"":{{""lat"":{0},""lon"":{1}}}", v1.y, v1.x));
            return string.Format(@"""loc"":{{""lat"":{0},""lon"":{1}}}", v1.y, v1.x);
        }

        public Vector2d CursorLocationToVector2d()
        {
            if (Cursor == null)
            {
                Debug.Log("Cursor is null, setting");
                Cursor = GameObject.Find("Cursor");
            }
            
            // Offset to account for a repositioned world.
            AppState.Instance.worldOffset = AppState.Instance.Terrain.transform.position;
            var cursorPosition = (Cursor.transform.position - AppState.Instance.worldOffset);
            var v0 = new Vector2d(cursorPosition.x / Scale, cursorPosition.z / Scale) + CenterInMercator;
            //var v0 = new Vector2d(Cursor.transform.position.x / Scale, Cursor.transform.position.z / Scale) + CenterInMercator;
            var v1 = GM.MetersToLatLon(v0);
            //Debug.Log(string.Format(@"""loc"":{{""lat"":{0},""lon"":{1}}}", v1.y, v1.x));
            return new Vector2d(v1.y, v1.x);
        }

        public string ToJSON()
        {
            if (SelectedFeature == null)
            {
                // Only send the ID
                if (Cursor == null)
                {
                    return string.Format(@"{{ ""id"": ""{0}"", ""name"": ""{1}"", ""selectionColor"": {{ ""r"": {2}, ""g"": {3}, ""b"": {4}, ""a"": {5} }}, }}", 
                        id, Name, selectionColor.r, selectionColor.g, selectionColor.b, selectionColor.a);
                }
                else
                {
                    return string.Format(@"{{ ""id"": ""{0}"", ""name"": ""{1}"", ""selectionColor"": {{ ""r"": {2}, ""g"": {3}, ""b"": {4}, ""a"": {5} }}, ""cursor"":{{""xpos"":{6},""ypos"":{7},""zpos"":{8},""xrot"":{9},""yrot"":{10},""zrot"":{11} }}, {12}}}",
                        id, Name, selectionColor.r, selectionColor.g, selectionColor.b, selectionColor.a, Cursor.transform.position.x, Cursor.transform.position.y, Cursor.transform.position.z, Cursor.transform.rotation.x, Cursor.transform.rotation.y, Cursor.transform.rotation.z, CursorLocationToJSON());
                }
            }
            else
            {
                Func<float, float> nearZero = x => Math.Abs(x) < 0.0001 ? 0F : x;
                if (Cursor == null)
                {
                    return string.Format(@"{{ ""id"": ""{0}"", ""name"": ""{6}"", ""selectedFeature"": {1}, ""selectionColor"": {{ ""r"": {2}, ""g"": {3}, ""b"": {4}, ""a"": {5} }} }}", id, SelectedFeature.ToLimitedJSON(), selectionColor.r, selectionColor.g, selectionColor.b, selectionColor.a, Name);
                }
                else
                {
                    return string.Format(@"{{ ""id"": ""{0}"", ""name"": ""{6}"", ""selectedFeature"": {1}, ""selectionColor"": {{ ""r"": {2}, ""g"": {3}, ""b"": {4}, ""a"": {5} }}, ""cursor"":{{""xpos"":{7},""ypos"":{8},""zpos"":{9},""xrot"":{10},""yrot"":{11},""zrot"":{12} }}, {13}}}",
                        id, SelectedFeature.ToLimitedJSON(), selectionColor.r, selectionColor.g, selectionColor.b, selectionColor.a, Name,
                        nearZero(Cursor.transform.position.x), nearZero(Cursor.transform.position.y), nearZero(Cursor.transform.position.z),
                        nearZero(Cursor.transform.rotation.x), nearZero(Cursor.transform.rotation.y), nearZero(Cursor.transform.rotation.z),
                        CursorLocationToJSON());
                }
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
            }
            if (jsonObj.HasField("selectionColor"))
            {
                var sc = jsonObj["selectionColor"];
                var a = sc.GetFloat("a", 1);
                var r = sc.GetFloat("r", 1);
                var g = sc.GetFloat("g", 1);
                var b = sc.GetFloat("b", 1);
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
                if (user.Cursor == null && cursors != null && cursors.Count != 0)
                {
                    var name = user.id + "-Cursor";
                    user.Cursor = cursors.Find(i => i.name == name);
                }
                if (user.Cursor != null)
                {
                    user.Cursor.transform.position = new Vector3(posx, posy, posz);
                    user.Cursor.transform.rotation = Quaternion.Euler(rotx, roty, rotz);
                }
            }

            return user;
        }
    }
}