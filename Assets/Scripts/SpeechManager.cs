using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;
using MapzenGo.Helpers;
using Assets.Scripts.Plugins;

namespace Assets.Scripts
{
    public class SpeechManager : Singleton<SpeechManager>
    {
        private AppState appState;
        private SessionManager sessionManager;
        public Dictionary<string, Action> Keywords = new Dictionary<string, Action>();
        KeywordRecognizer keywordRecognizer = null;

        protected SpeechManager() { }

        public void Init()
        {
            Debug.Log("Initializing speech manager");
            AddDefaultKeywords();
            // Tell the KeywordRecognizer about our keywords.
            keywordRecognizer = new KeywordRecognizer(Keywords.Keys.ToArray());

            // Register a callback for the KeywordRecognizer and start recognizing!
            keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
            keywordRecognizer.Start();

            appState = AppState.Instance;
            sessionManager = SessionManager.Instance;
        }

        private void doNothing() { }

        private void AddDefaultKeywords()
        {
            AddKeyword("Place", () => doNothing() );
            AddKeyword("Zoom in", () => SetZoomAndRange(1,0));
            AddKeyword("Zoom out", () => SetZoomAndRange(-1, 0));
            AddKeyword("Increase range", () => SetZoomAndRange(0, 1));
            AddKeyword("Decrease range", () => SetZoomAndRange(0, -1));
            var directions = new List<string> { "North", "South", "East", "West", "North East", "North West", "South East", "South West" };
            directions.ForEach(dir => AddKeyword("Move " + dir, () => Go(dir)));
            //directions.ForEach(dir => AddKeyword("Go go " + dir, () => Go(dir, 2)));
        }

        private void SetZoomAndRange(int deltaZoom, int deltaRange)
        {
            Debug.Log("Setting new view");
            var view = appState.Config.ActiveView;
            view.Zoom += deltaZoom;
            view.Range += deltaRange;
            appState.ResetMap(view);
            sessionManager.UpdateView(view);
        }

        private void Go(string direction, int stepSize = 1)
        {
            var view = appState.Config.ActiveView;
            var metersPerTile = view.Resolution * stepSize;
            Debug.Log(string.Format("Moving {0} meters {1}...", metersPerTile, direction));
            var merc = GM.LatLonToMeters(new Vector2d(view.Lon, view.Lat));
            Vector2d delta;
            switch (direction.ToLowerInvariant())
            {
                case "north":
                    delta = new Vector2d(0, metersPerTile);
                    break;
                case "south":
                    delta = new Vector2d(0, -metersPerTile);
                    break;
                case "east":
                    delta = new Vector2d(metersPerTile, 0);
                    break;
                case "west":
                    delta = new Vector2d(-metersPerTile, 0);
                    break;
                case "north east":
                    delta = new Vector2d(metersPerTile, metersPerTile);
                    break;
                case "south east":
                    delta = new Vector2d(metersPerTile, -metersPerTile);
                    break;
                case "north west":
                    delta = new Vector2d(-metersPerTile, metersPerTile);
                    break;
                case "south west":
                    delta = new Vector2d(-metersPerTile, -metersPerTile);
                    break;
                default:
                    delta = new Vector2d();
                    break;
            }
            merc += delta;
            var ll = GM.MetersToLatLon(merc);
            view.Lat = (float)ll.x;
            view.Lon = (float)ll.y;
            appState.ResetMap(view);
            sessionManager.UpdateView(view);
        }

        private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
        {
            Debug.Log("Keyword recognized: " + args.text);
            Action keywordAction;
            if (Keywords.TryGetValue(args.text, out keywordAction))
            {
                keywordAction.Invoke();
            }
        }

        public void AddKeyword(string speech, Action action)
        {
            Keywords[speech] = action;
            //if (Keywords.ContainsKey(speech)) return;
            //Keywords.Add(speech, action);
        }

        public void RemoveKeyword(string speech)
        {
            if (Keywords.ContainsKey(speech)) Keywords[speech] = () => { return; };
        }

    }
}
