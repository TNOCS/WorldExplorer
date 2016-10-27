using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MapzenGo.Helpers;
using MapzenGo.Models.Enums;
using UnityEngine;

namespace MapzenGo.Models
{
    public class Poi : MonoBehaviour
    {
        private Transform _target;
        public string Id;
        public string Type;
        public string Kind;
        public string Name;
        public int SortKey;

        public void Stick(Transform t)
        {
            _target = t;
        }

        public void Update()
        {
            if (_target == null)
                Destroy(gameObject);
            else
            {
                transform.position = _target.position;
                transform.localPosition = new Vector3(0, _target.localPosition.y + 30, 10);
                // RJ Turn the Pois according to the camera
                transform.parent = _target.transform;
                transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward, Camera.main.transform.up);
            }
        }
    }
}
