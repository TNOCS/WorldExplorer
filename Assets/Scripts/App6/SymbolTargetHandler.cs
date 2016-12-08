using Assets.Scripts;
using Assets.Scripts.Plugins;
using MapzenGo.Helpers;
using UnityEngine;

namespace Symbols
{
    public class SymbolTargetHandler : MonoBehaviour
    {
        private readonly SessionManager sessionManager = SessionManager.Instance;
        private readonly AppState appState = AppState.Instance;

        [SerializeField]
        public GameObject gui;
        [SerializeField]
        private GameObject cursor;
        private bool selected = false;
        private Material selectedMat;
        private Material oldMat;
        private Renderer coneRender;
        public Feature Feature { get; set; }
        private bool OtherUserSelected = false;
        private GameObject otherCursor;
        // Use this for initialization
        void Start()
        {
            selectedMat = (Material)Resources.Load("Materials/cone-Color-J04", typeof(Material));
            selectedMat.color = AppState.Instance.Config.SelectionColor;
            coneRender = transform.FindChild("cone/Cone with Right Triangle/Component").gameObject.GetComponent<Renderer>();
            oldMat = coneRender.material;
            cursor = GameObject.Find(sessionManager.me.Id + "-Cursor");
        }

        /// <summary>
        /// OnSelect is called whenever a cone is selected or deselected.
        /// </summary>
        public void OnSelect()
        {
            selected = !selected;

            // If the user is in placing mode, display the spatial mapping mesh.
            if (selected)
            {
                SpeechManager.Instance.AddKeyword("Place", () =>
                {
                    selected = false;
                    coneRender.material = oldMat;
                    sessionManager.UpdateSelectedFeature(Feature, false);
                    gui.SetActive(false);
                });
                gui.SetActive(true);
                sessionManager.UpdateSelectedFeature(Feature, true);
                coneRender.material = selectedMat;
            }
            // If the user is not in placing mode, hide the spatial mapping mesh.
            else
            {
                SpeechManager.Instance.RemoveKeyword("Place");
                coneRender.material = oldMat;
                sessionManager.UpdateSelectedFeature(Feature, false);
                gui.SetActive(false);
            }
        }

        /// <summary>
        /// OnSelect is called whenever another cursor selects an item.
        /// </summary>
        /// <param name="selectedMat"></param>
        /// <param name="otherCursor"></param>
        public void OnSelect(Material selectedMat, GameObject otherCursor)
        {
            OtherUserSelected = !OtherUserSelected;

            // If the user is in placing mode, display the spatial mapping mesh.
            if (OtherUserSelected)
            {
                gui.SetActive(true);

                coneRender.material = selectedMat;
                this.otherCursor = otherCursor;
            }
            // If the user is not in placing mode, hide the spatial mapping mesh.
            else
            {
                coneRender.material = oldMat;
                gui.SetActive(false);
                transform.position = new Vector3(otherCursor.transform.position.x, otherCursor.transform.position.y, otherCursor.transform.position.z);

                //// Now, update the lat/lon of the feature
                //var sf = transform.parent.GetComponent<SymbolFactory>();
                //var v0 = new Vector2d(transform.localPosition.x, transform.localPosition.z) + sf.CenterInMercator;
                //// Debug.Log(string.Format("Meters x: {0}, y: {1}", v0.x, v0.y));
                //var v3 = GM.MetersToLatLon(v0);
                //Feature.SetLatLon(v3);
                otherCursor = null;
            }
        }

        public void Show()
        {
            if (selected) return;
            gui.SetActive(true);
        }

        public void Hide()
        {
            if (selected) return;
            gui.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 pos;
            if (OtherUserSelected)
            {
                pos = otherCursor.transform.position;
            }
            else if (selected)
            {
                // Move this object's parent object to where the raycast hit the Spatial Mapping mesh.
                pos = cursor.transform.position;
            } else
            {
                return;
            }

            transform.position = new Vector3(pos.x, pos.y, pos.z);
            // Now, update the lat/lon of the feature
            var sf = transform.parent.GetComponent<SymbolFactory>();
            var v0 = new Vector2d(transform.localPosition.x, transform.localPosition.z) + sf.CenterInMercator;
            // Debug.Log(string.Format("Meters x: {0}, y: {1}", v0.x, v0.y));
            var v3 = GM.MetersToLatLon(v0);
            Feature.SetLatLon(v3);
            // Debug.Log(string.Format("Latlng: {0}, {1}", v3.y, v3.x));
        }
    }
}
