using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Windows.Speech;

namespace Assets.Scripts
{

    public class SpeechManager
    {
        public Dictionary<string, System.Action> Keywords = new Dictionary<string, System.Action>();
        KeywordRecognizer keywordRecognizer = null;

        public void Init()
        {

            // Tell the KeywordRecognizer about our keywords.
            keywordRecognizer = new KeywordRecognizer(Keywords.Keys.ToArray());

            // Register a callback for the KeywordRecognizer and start recognizing!
            keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
            keywordRecognizer.Start();
        }

        private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
        {
            Action keywordAction;
            if (Keywords.TryGetValue(args.text, out keywordAction))
            {
                keywordAction.Invoke();
            }
        }


    }
}
