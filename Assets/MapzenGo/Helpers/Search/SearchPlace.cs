using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using MapzenGo.Models;
//using UniRx;
using UnityEngine;


namespace MapzenGo.Helpers.Search
{
    [ExecuteInEditMode]
    [AddComponentMenu("Mapzen/SearchPlace")]
    public class SearchPlace : MonoBehaviour
    {
        const string seachUrl = "https://search.mapzen.com/v1/autocomplete?text=";
        public string namePlace = "";
        public string namePlaceСache = "";
        public StructSearchData DataStructure;

        void OnEnable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
        }
        void Start()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;

#endif
        }

        public void SearchInMapzen()
        {
            if (namePlace != string.Empty && namePlaceСache != namePlace)
            {
                namePlaceСache = namePlace;
                Task.Factory.StartNew<string>(() =>
                {
                    WebClient wc = new WebClient();
                    return wc.DownloadString(seachUrl + namePlace);

                }).ContinueWith((t) =>
                {
                    if (t.IsFaulted)
                    {
                        // faulted with exception
                        Exception ex = t.Exception;
                        while (ex is AggregateException && ex.InnerException != null)
                            ex = ex.InnerException;
                        Debug.LogError(ex.Message);
                    }
                    else if (t.IsCanceled)
                    {

                    }
                    else
                    {
                        DataProcessing(t.Result);
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());


            }
        }

        public void SetupToTileManager(float Latitude, float Longitude)
        {
            TileManager tm = GetComponent<TileManager>();
            tm.Latitude = Latitude;
            tm.Longitude = Longitude;
        }

        public void DataProcessing(string success)
        {
            JSONObject obj = new JSONObject(success);
            DataStructure.dataChache = new List<SearchData>();
            foreach (JSONObject jsonObject in obj["features"].list)
            {
                DataStructure.dataChache.Add(new SearchData()
                {
                    coordinates = new Vector2(jsonObject["geometry"]["coordinates"][0].f, jsonObject["geometry"]["coordinates"][1].f),
                    label = jsonObject["properties"]["label"].str
                });
            }
        }
    }
}
