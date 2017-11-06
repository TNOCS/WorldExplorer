using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;
using MapzenGo.Helpers;
using Assets.Scripts.Plugins;
using HoloToolkit.Unity;

namespace Assets.Scripts
{/*
    public class _SpeechManager : Singleton<SpeechManager> 
    {
        private AppState appState;
        private SessionManager sessionManager;
        public GameObject Hud;
        private AudioClip launchSound;
        public Dictionary<string, Action> Keywords = new Dictionary<string, Action>();
        public Dictionary<string, string> audioCommands;
        KeywordRecognizer keywordRecognizer = null;

        void Update()
        {

        }
        protected SpeechManager() { }

        public void Init()
        {
            Debug.Log("Initializing speech manager");
            audioCommands = new Dictionary<string, string>();
            appState = AppState.Instance;
            //launchSound = (AudioClip)Resources.Load("Sounds/MissileLaunch");
            //AudioClip[] audioClips = FindObjectsOfType(typeof(AudioClip)) as AudioClip[];
            //foreach (var clip in audioClips)
            //{
            //    if (clip.name != "MissileLaunch") continue;
            //    launchSound = clip;
            //}
            AddDefaultKeywords();
        }

        /// <summary>
        ///  start Listining only after all other scripts are done
        /// </summary>
        public void StartListining()
        {   // Tell the KeywordRecognizer about our keywords.

            keywordRecognizer = new KeywordRecognizer(Keywords.Keys.ToArray());
            // Register a callback for the KeywordRecognizer and start recognizing!
            keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
            keywordRecognizer.Start();
        }

        public void InitSessions()
        {
            if (sessionManager != null) return;
            sessionManager = SessionManager.Instance;
        }

        private void doNothing() { }

        private void AddDefaultKeywords()
        {
            audioCommands.Add("Hide Commands", " Hides the voice commands");
            appState.Speech.Keywords.Add("Hide Commands", () =>
            {
                Hud.SetActive(false);
            });
            audioCommands.Add("Show Commands", " Displays the voice commands");
            appState.Speech.Keywords.Add("Show Commands", () =>
            {
                Hud.SetActive(true);
            });
            //AddKeyword("Place", () => selectionHandler.releaseObj());
            AddTableCommands();
            AddMissileLaunchCommand();
            AddKeyword("Place", () => doNothing() );
            AddKeyword("Zoom in", () => SetZoomAndRange(1,0));
            AddKeyword("Zoom out", () => SetZoomAndRange(-1, 0));
            AddKeyword("Increase range", () => SetZoomAndRange(0, 1));
            AddKeyword("Decrease range", () => SetZoomAndRange(0, -1));
            var directions = new List<string> { "North", "South", "East", "West", "North East", "North West", "South East", "South West" };
            directions.ForEach(dir => AddKeyword("Move " + dir, () => Go(dir)));
        }

        private void AddMissileLaunchCommand()
        {
            var audioContainer = GameObject.Find("AudioContainer");
            var audioSource = audioContainer.GetComponent<AudioSource>();

            appState.Speech.Keywords.Add("Launch Missile", () =>
            {
                // var audioSource = GetComponent<AudioSource>();
                audioSource.Play();
            });
            appState.Speech.Keywords.Add("Launch Sharepoint", () =>
            {
                // var audioSource = GetComponent<AudioSource>();
                audioSource.Play();
            });
        }

        private void AddTableCommands()
        {
            var table = appState.Config.Table;
            audioCommands.Add("Center|Lower|Raise|Shrink|Expand table", " Manipulates the table");
            appState.Speech.Keywords.Add("Center table", () =>
            {
                Debug.Log("Center table");
                var pos = Camera.main.transform.position;
                appState.Terrain.transform.position = new Vector3(pos.x, table.Position, pos.z);
            });
            appState.Speech.Keywords.Add("Shrink table", () =>
            {
                table.Size -= 0.2F;
                var size = table.Size;
                Debug.Log(string.Format("Shrink table to {0}", size));
                appState.Table.transform.localScale = new Vector3(size, table.Position, size);
            });
            appState.Speech.Keywords.Add("Expand table", () =>
            {
                table.Size += 0.2F;
                var size = table.Size;
                Debug.Log(string.Format("Expand table to {0}", size));
                appState.Table.transform.localScale = new Vector3(size, table.Position, size);
            });
            appState.Speech.Keywords.Add("Lower table", () =>
            {
                table.Height -= 0.1F;
                Debug.Log("Lower table");
                appState.Terrain.transform.position = new Vector3(appState.Terrain.transform.position.x, table.Position, appState.Terrain.transform.position.z);
            });
            appState.Speech.Keywords.Add("Raise table", () =>
            {
                table.Height += 0.1F;
                Debug.Log("Raise table");
                appState.Terrain.transform.position = new Vector3(appState.Terrain.transform.position.x, table.Position, appState.Terrain.transform.position.z);
            });
        }

        private void SetZoomAndRange(int deltaZoom, int deltaRange)
        {
            InitSessions();
            Debug.Log("Setting new view");
            var view = appState.Config.ActiveView;
            view.Zoom += deltaZoom;
            view.Range += deltaRange;
            appState.ResetMap(view);
            sessionManager.UpdateView(view);
        }

        //TODO: Move outside speechmanager (thom)
        public void Go(string direction, int stepSize = 1)
        {
            InitSessions();
            Debug.Log(string.Format("Go {0}...", direction));
            var view = appState.Config.ActiveView;
            var metersPerTile = view.Resolution * stepSize;
            Debug.Log(string.Format("Moving {0} meters {1}...", metersPerTile, direction));
            var merc = GM.LatLonToMeters(new Vector2d(view.Lon, view.Lat));
            Vector2d delta;

            switch (direction.ToLowerInvariant())
            {
                case "north":
                    delta = new Vector2d(metersPerTile, 0);
                    break;
                case "south":
                    delta = new Vector2d(-metersPerTile, 0);
                    break;
                case "east":
                    delta = new Vector2d(0, metersPerTile);
                    break;
                case "west":
                    delta = new Vector2d(0, -metersPerTile);
                    break;
                case "northeast":
                    delta = new Vector2d(metersPerTile, metersPerTile);
                    break;
                case "southeast":
                    delta = new Vector2d(-metersPerTile, metersPerTile);
                    break;
                case "northwest":
                    delta = new Vector2d(metersPerTile, -metersPerTile);
                    break;
                case "southwest":
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

        /// <summary>
        /// Note that a keyword may be overwritten
        /// </summary>
        /// <param name="speech"></param>
        /// <param name="action"></param>
        public void AddKeyword(string speech, Action action)
        {
            //Debug.Log(speech);
            if (!Keywords.ContainsKey(speech) && keywordRecognizer != null && keywordRecognizer.IsRunning)
            {
                Debug.LogWarningFormat("You are trying to add the {0} keyword, but the speech manager is already running...", speech);
                //keywordRecognizer.Stop();
                //Keywords[speech] = action;
                //StartListining();
            }
            else
            {
                Keywords[speech] = action;
            }
        }

        /// <summary>
        /// Removing a keyword, after the speech manager has been initialized, will still trigger recognition.
        /// However, we will ignore its behaviour.
        /// </summary>
        /// <param name="speech"></param>
        public void RemoveKeyword(string speech)
        {
            if (Keywords.ContainsKey(speech)) Keywords[speech] = doNothing;
        }

    }*/
}
