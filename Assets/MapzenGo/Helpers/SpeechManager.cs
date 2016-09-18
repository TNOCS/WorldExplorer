using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class SpeechManager : MonoBehaviour
{
    [SerializeField]
    private Transform _centerIndicator;

    KeywordRecognizer keywordRecognizer = null;
    Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();

    // Use this for initialization
    void Start()
    {
        var world = GameObject.Find("World");
        if (world == null) throw new System.Exception("Cannot find GameObject world in SpeechManager!");
        keywords.Add("Move North", () => {
            _centerIndicator.localPosition = new Vector3(_centerIndicator.localPosition.x, _centerIndicator.localPosition.y + 1, _centerIndicator.localPosition.z);
        });
        keywords.Add("Move South", () => {
            _centerIndicator.localPosition = new Vector3(_centerIndicator.localPosition.x, _centerIndicator.localPosition.y - 1, _centerIndicator.localPosition.z);
        });
        keywords.Add("Move East", () => {
            _centerIndicator.localPosition = new Vector3(_centerIndicator.localPosition.x - 1, _centerIndicator.localPosition.y, _centerIndicator.localPosition.z);
        });
        keywords.Add("Move West", () => {
            _centerIndicator.localPosition = new Vector3(_centerIndicator.localPosition.x + 1, _centerIndicator.localPosition.y, _centerIndicator.localPosition.z);
        });
        keywords.Add("Move South East", () => {
            _centerIndicator.localPosition = new Vector3(_centerIndicator.localPosition.x - 1, _centerIndicator.localPosition.y - 1, _centerIndicator.localPosition.z);
        });
        keywords.Add("Move South West", () => {
            _centerIndicator.localPosition = new Vector3(_centerIndicator.localPosition.x + 1, _centerIndicator.localPosition.y - 1, _centerIndicator.localPosition.z);
        });
        keywords.Add("Move North East", () => {
            _centerIndicator.localPosition = new Vector3(_centerIndicator.localPosition.x - 1, _centerIndicator.localPosition.y + 1, _centerIndicator.localPosition.z);
        });
        keywords.Add("Move North West", () => {
            _centerIndicator.localPosition = new Vector3(_centerIndicator.localPosition.x + 1, _centerIndicator.localPosition.y + 1, _centerIndicator.localPosition.z);
        });
        keywords.Add("Zoom in", () => {
            _centerIndicator.localPosition = new Vector3(_centerIndicator.localPosition.x, _centerIndicator.localPosition.y, _centerIndicator.localPosition.z + 1);
        });
        keywords.Add("Zoom out", () => {
            _centerIndicator.localPosition = new Vector3(_centerIndicator.localPosition.x, _centerIndicator.localPosition.y, _centerIndicator.localPosition.z + 1);
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