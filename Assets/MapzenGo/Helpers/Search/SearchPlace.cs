using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
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

        public async void SearchInMapzen()
        {
            if (namePlace != string.Empty && namePlaceСache != namePlace)
            {
                namePlaceСache = namePlace;
                var data = await NetworkUtil.DownloadStringAsync(CancellationToken.None, seachUrl + namePlace);
                
                DataProcessing(data);

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
