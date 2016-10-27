using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Scripts.App6
{
    public class Symbol : MonoBehaviour
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

                transform.position = _target.position;// new 
                                                      //  transform.position = Camera.main.WorldToScreenPoint(_target.position);//old
                transform.localPosition = new Vector3(_target.localPosition.x, _target.localPosition.y + 10, _target.localPosition.x);
                // RJ Turn the Pois according to the camera
                transform.parent = _target.transform;
                transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward, Camera.main.transform.up);
            }
        }
    }
}
