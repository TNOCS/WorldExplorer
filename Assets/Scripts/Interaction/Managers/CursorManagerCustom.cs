using Assets.Scripts;
using Assets.Scripts.Plugins;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using MapzenGo.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class CursorManagerCustom : Singleton<CursorManagerCustom>
{
    /// <summary>
    /// Handles the users own cursor, including feedback through icons and color.
    /// </summary>

    public GameObject Cursor;
    public GameObject CursorOnHolograms;
    public GameObject CursorVisual;
    public GameObject InteractionPossibleIcon;
    public GameObject PlacementPossibleIcon;
    private GameObject NavigateTutorial;
    private GameObject CursorIcon;

    private bool IsCursorInitialized = false;

    public void InitCursor()
    {
        Cursor = GameObject.Find("/Cursor");
        if (Cursor != null)
        {
            if (Cursor.GetComponent<ObjectCursor>().isActiveAndEnabled)
                Debug.Log("The ObjectCursor component must be disabled (will be enabled by this script)!");
        }


        CursorIcon = GameObject.Find("/Cursor/CursorVisualCube/CursorIcon");
        CursorOnHolograms = GameObject.Find("/Cursor/CursorOnHolograms");
        CursorVisual = GameObject.Find("/Cursor/CursorVisualCube"); ;
        InteractionPossibleIcon = GameObject.Find("/Cursor/CursorVisualCube/CursorIcon/InteractionPossibleIcon");
        PlacementPossibleIcon = GameObject.Find("/Cursor/CursorVisualCube/CursorIcon/PlacementPossibleIcon");
        NavigateTutorial = GameObject.Find("NavigateTutorial");
        Assert.IsTrue(Cursor != null &&
                     CursorIcon != null &&
                     CursorOnHolograms != null &&
                     InteractionPossibleIcon != null &&
                     PlacementPossibleIcon != null &&
                     CursorVisual != null &&
                     NavigateTutorial != null, "Failed to initialize cursor objects");
        // The GameObject that shows current manipulating mode.

        if (CursorIcon != null) CursorIcon.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("CursorIcons/Sprites/MoveBtn");
        if (NavigateTutorial != null) NavigateTutorial.SetActive(false);

        if (Cursor != null)
        {
            // The ObjectCursor makes children of Cursor GameObject inactive based on state
            // Therefore the GameObject.find  doesn't work (code above) if the script is enabled at startup
            Cursor.GetComponent<HoloToolkit.Unity.InputModule.ObjectCursor>().enabled = true;
        }
        IsCursorInitialized = true;
    }

    private void Update()
    {
        if (!IsCursorInitialized) return;
        CursorVisualHandler();

        // Disables tutorial when user switches out of rotate or scale mode before the animation has ended.
        if (UIManager.Instance.currentMode != "RotateBtn" && UIManager.Instance.currentMode != "ScaleBtn")
        {
            NavigateTutorial.SetActive(false);
        }
    }

    public void SetCursorIcon(string currentMode)
    {
        if (!IsCursorInitialized) return;
        CursorVisual.SetActive(true);
        CursorIcon.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("CursorIcons/Sprites/" + currentMode);

        if (currentMode == "RotateBtn" || currentMode == "ScaleBtn")
        {
            StartCoroutine(ShowTutorial());
        }
    }

    private IEnumerator ShowTutorial()
    {
        if (!IsCursorInitialized) yield return null;
        NavigateTutorial.SetActive(true);
        yield return new WaitForSeconds(8);
        NavigateTutorial.SetActive(false);
    }

    private void InteractionPossible()
    {
        if (!IsCursorInitialized) return;
        CursorIcon.GetComponent<SpriteRenderer>().color = Color.green;
        InteractionPossibleIcon.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("CursorIcons/Sprites/InteractionPossible");
        InteractionPossibleIcon.GetComponent<SpriteRenderer>().color = Color.green;
    }

    private void InteractionNotPossible()
    {
        if (!IsCursorInitialized) return;
        CursorIcon.GetComponent<SpriteRenderer>().color = Color.red;
        InteractionPossibleIcon.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("CursorIcons/Sprites/InteractionNotPossible");
        InteractionPossibleIcon.GetComponent<SpriteRenderer>().color = Color.red;
    }

    private void CursorVisualHandler()
    {
        if (!IsCursorInitialized) return;
        if (RaycastManager.Instance.Raycast().collider != null)
        {
            // Disables cursor whenever not aimed at board or other relevant objects.
            var rayCastFocus = RaycastManager.Instance.Raycast().collider.gameObject;
            var currentMode = UIManager.Instance.currentMode;
            if (rayCastFocus.tag == "uistatic" || rayCastFocus.tag == "uibutton" || rayCastFocus.tag == "inventoryobject" || rayCastFocus.name == "FrontSide")
            {
                if (rayCastFocus.name != currentMode)
                {
                    Instance.CursorVisual.SetActive(false);
                }
                // Except if raycast target is the current mode, to increase visual feedback.
                else
                {
                    CursorVisual.SetActive(true);
                }
            }
            else
            {
                // Enables it when aimed at board.
                CursorVisual.SetActive(true);
            }

            // Zoom in&out and center buttons are always green when aimed at the board.
            if (currentMode == "ZoomInBtn" || currentMode == "ZoomOutBtn" || currentMode == "CenterBtn")
            {
                if (rayCastFocus.tag == "board")
                {
                    InteractionPossible();
                }
                else
                {
                    InteractionNotPossible();
                }
            }

            // Sets icons green if it is an interactible object
            if (rayCastFocus.tag == "spawnobject")
            {
                if (currentMode != "ZoomInBtn" && currentMode != "ZoomOutBtn" && currentMode != "CenterBtn")
                {
                    // Exception: when current mode is "Scale", but the object is unscalable (like a jeep).
                    if (currentMode == "ScaleBtn" && rayCastFocus.gameObject.GetComponent<PrefabObjectData>().scaleable == false)
                    {
                        InteractionNotPossible();
                    }
                    else
                    {
                        InteractionPossible();
                    }
                }
                else
                {
                    InteractionNotPossible();
                }

            }
            // Otherwise turn them red.
            else
            {
                if (currentMode != "ZoomInBtn" && currentMode != "ZoomOutBtn" && currentMode != "CenterBtn")
                {
                    InteractionNotPossible();
                }
            }

            // Semi dirty fix to stop a bug that displays the wrong colors.
            if (currentMode == "ZoomInBtn" || currentMode == "ZoomOutBtn" || currentMode == "CenterBtn")
            {
                InteractionPossible();
            }

            // If minimum zoom level is already reached, the user can not zoom out more.
            if (AppState.Instance.Config != null && BoardInteraction.Instance != null)
            {
                if (AppState.Instance.Config.ActiveView.Zoom == BoardInteraction.Instance.maxZoomLevel)
                {
                    if (currentMode == "ZoomInBtn")
                    {
                        InteractionNotPossible();
                    }
                }
            }

            // If maximum zoom level is already reached, the user can not zoom in more.
            if (AppState.Instance.Config != null && BoardInteraction.Instance != null)
            {
                if (AppState.Instance.Config.ActiveView.Zoom == BoardInteraction.Instance.minZoomLevel)
                {
                    if (currentMode == "ZoomOutBtn")
                    {
                        InteractionNotPossible();
                    }
                }
            }


            // If the user is placing an object, turn the icons green when it can be placed.
            if (currentMode == "Placing")
            {
                CursorIcon.GetComponent<SpriteRenderer>().sprite = null;
                InteractionPossibleIcon.SetActive(false);
                PlacementPossibleIcon.SetActive(true);
                if (rayCastFocus.tag == "board")
                {
                    PlacementPossibleIcon.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("CursorIcons/Sprites/InteractionPossible");
                    PlacementPossibleIcon.GetComponent<SpriteRenderer>().color = Color.green;
                }
                // Otherwise turn them red
                else
                {
                    PlacementPossibleIcon.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("CursorIcons/Sprites/InteractionNotPossible");
                    PlacementPossibleIcon.GetComponent<SpriteRenderer>().color = Color.red;
                }
            }
            else
            {
                InteractionPossibleIcon.SetActive(true);
                PlacementPossibleIcon.SetActive(false);
            }

            // If the user is in the Info mode, the cursor should turn green when it hits building.
            if (currentMode == "InfoBtn")
            {
                if (rayCastFocus.name.Contains("Buildings"))
                {
                    InteractionPossible();
                }
                else
                {
                    InteractionNotPossible();
                }
            }

        }
        // If Raycast is not hitting anything.
        else
        {
            CursorVisual.SetActive(false);
        }
    }

    public Vector2d Cursor2LatLon()
    {
        if (Cursor == null)
        {
            Debug.Log("Cursor is null, setting");
            Cursor = GameObject.Find("Cursor");
        }

        // Offset to account for a repositioned world.
        AppState.Instance.worldOffset = AppState.Instance.Terrain.transform.position;

        var cursorPosition = (Cursor.transform.position - AppState.Instance.worldOffset);
        float newScale;
        newScale = SessionManager.Instance.me.Scale * GameObject.Find("terrain").transform.localScale.x;
        var v0 = new Vector2d(cursorPosition.x / newScale, cursorPosition.z / newScale) + SessionManager.Instance.me.CenterInMercator;
        //var v0 = new Vector2d(Cursor.transform.position.x / Scale, Cursor.transform.position.z / Scale) + CenterInMercator;
        return GM.MetersToLatLon(v0);
    }
}
