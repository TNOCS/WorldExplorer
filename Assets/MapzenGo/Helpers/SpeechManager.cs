using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class SpeechManager : MonoBehaviour
{
    
    KeywordRecognizer keywordRecognizer = null;
    Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();

    // Use this for initialization
    void Start()
    {
        AppState appState = AppState.Instance;
        var world = GameObject.Find("World");
        if (world == null) throw new System.Exception("Cannot find GameObject world in SpeechManager!");
        keywords.Add("Move North", () => {
            appState.Center = new Vector3(appState.Center.x, appState.Center.y + 1, appState.Center.z);
        });
        keywords.Add("Move South", () => {
            appState.Center = new Vector3(appState.Center.x, appState.Center.y - 1, appState.Center.z);
        });
        keywords.Add("Move East", () => {
            appState.Center = new Vector3(appState.Center.x - 1, appState.Center.y, appState.Center.z);
        });
        keywords.Add("Move West", () => {
            appState.Center = new Vector3(appState.Center.x + 1, appState.Center.y, appState.Center.z);
        });
        keywords.Add("Move South East", () => {
            appState.Center = new Vector3(appState.Center.x - 1, appState.Center.y - 1, appState.Center.z);
        });
        keywords.Add("Move South West", () => {
            appState.Center = new Vector3(appState.Center.x + 1, appState.Center.y - 1, appState.Center.z);
        });
        keywords.Add("Move North East", () => {
            appState.Center = new Vector3(appState.Center.x - 1, appState.Center.y + 1, appState.Center.z);
        });
        keywords.Add("Move North West", () => {
            appState.Center = new Vector3(appState.Center.x + 1, appState.Center.y + 1, appState.Center.z);
        });
        keywords.Add("Zoom in", () => {
            appState.Center = new Vector3(appState.Center.x, appState.Center.y, appState.Center.z + 1);
        });
        keywords.Add("Zoom out", () => {
            appState.Center = new Vector3(appState.Center.x, appState.Center.y, appState.Center.z + 1);
        });
        keywords.Add("Clear cache", () => {
            var path = Application.temporaryCachePath + "/{0}/";
            for (var zoom = 0; zoom < 22; zoom++)
            {
                var cacheFolderPath = string.Format(path, zoom);
                if (Directory.Exists(cacheFolderPath)) Directory.Delete(cacheFolderPath);
            }
        });
        const float ScaleGrowShrinkFactor = 1.1F;
        keywords.Add("Grow map", () => {
            var s = ScaleGrowShrinkFactor * world.transform.localScale.x;
            world.transform.localScale = new Vector3(s, s, s);
        });
        keywords.Add("Shrink map", () => {
            var s = 1/ScaleGrowShrinkFactor * world.transform.localScale.x;
            world.transform.localScale = new Vector3(s, s, s);
        });
        keywords.Add("Reset all", () => {
        });
        keywords.Add("Increase range", () => {
        });
        keywords.Add("Decrease range", () => {
        });


        //keywords.Add("Reset world", () =>
        //{
        //    // Call the OnReset method on every descendant object.
        //    this.BroadcastMessage("OnReset");
        //});

        //keywords.Add("Drop Sphere", () =>
        //{
        //    //var focusObject = GazeGestureManager.Instance.FocusedObject;
        //    //if (focusObject != null)
        //    //{
        //    //    // Call the OnDrop method on just the focused object.
        //    //    focusObject.SendMessage("OnDrop");
        //    //}
        //});

        // Tell the KeywordRecognizer about our keywords.
        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());

        // Register a callback for the KeywordRecognizer and start recognizing!
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        keywordRecognizer.Start();
    }

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        System.Action keywordAction;
        if (keywords.TryGetValue(args.text, out keywordAction))
        {
            keywordAction.Invoke();
        }
    }
}