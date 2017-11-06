using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManagerThom : Singleton<CursorManagerThom>
{
    public GameObject Cursor;
    public GameObject CursorOnHolograms;
    public GameObject CursorVisual;
    public GameObject InteractionPossibleIcon;
    public GameObject PlacementPossibleIcon;
    private GameObject NavigateTutorial;
    private GameObject CursorIcon;

    public void InitCursor()
    {
        Cursor = GameObject.Find("Cursor");
        CursorIcon = GameObject.Find("CursorIcon");
        CursorOnHolograms = GameObject.Find("CursorOnHolograms");
        InteractionPossibleIcon = GameObject.Find("InteractionPossibleIcon");
        PlacementPossibleIcon = GameObject.Find("PlacementPossibleIcon");
        NavigateTutorial = GameObject.Find("NavigateTutorial");

        // The GameObject that shows current manipulating mode.
        CursorVisual = Cursor.transform.GetChild(0).gameObject;
        CursorIcon.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("CursorIcons/Sprites/MoveBtn");

        NavigateTutorial.SetActive(false);
    }

    private void Update()
    {
        CursorVisualHandler();
    }

    public void SetCursorIcon(string currentMode)
    {
        CursorVisual.SetActive(true);
        CursorIcon.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("CursorIcons/Sprites/" + currentMode);

        if (currentMode == "RotateBtn" || currentMode == "ScaleBtn")
        {
            StartCoroutine(ShowTutorial());
        }
    }

    private IEnumerator ShowTutorial()
    {
        NavigateTutorial.SetActive(true);
        yield return new WaitForSeconds(6);
        NavigateTutorial.SetActive(false);
    }

    private void InteractionPossible()
    {
        CursorIcon.GetComponent<SpriteRenderer>().color = Color.green;
        InteractionPossibleIcon.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("CursorIcons/Sprites/InteractionPossible");
        InteractionPossibleIcon.GetComponent<SpriteRenderer>().color = Color.green;
    }

    private void InteractionNotPossible()
    {
        CursorIcon.GetComponent<SpriteRenderer>().color = Color.red;
        InteractionPossibleIcon.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("CursorIcons/Sprites/InteractionNotPossible");
        InteractionPossibleIcon.GetComponent<SpriteRenderer>().color = Color.red;
    }

    private void CursorVisualHandler()
    {
        if (RaycastManager.Instance.Raycast().collider != null)
        {
            // Disables cursor whenever not aimed at board or other relevant objects.
            var rayCastFocus = RaycastManager.Instance.Raycast().collider.gameObject;
            var currentMode = UIManager.Instance.currentMode;
            if (rayCastFocus.tag == "uistatic" || rayCastFocus.tag == "uibutton" || rayCastFocus.tag == "inventoryobject" || rayCastFocus.name == "FrontSide")
            {
                Instance.CursorVisual.SetActive(false);

                // Except if ui button is currentmode
                if (rayCastFocus.name == currentMode)
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

            // Sets icons green if it is an interactible object.
            if (rayCastFocus.tag == "spawnobject" )
            {
                if (currentMode != "ZoomInBtn" && currentMode != "ZoomOutBtn" && currentMode != "CenterBtn")
                {
                    InteractionPossible();
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

            // If minimum zoom level is already reached...
            if (AppState.Instance.Config.ActiveView.Zoom == BoardInteraction.Instance.maxZoomLevel)
            {
                if (currentMode == "ZoomInBtn")
                {
                    InteractionNotPossible();
                }
            }

            // If maximum zoom level is already reached...
            if (AppState.Instance.Config.ActiveView.Zoom == BoardInteraction.Instance.minZoomLevel)
            {
                if (currentMode == "ZoomOutBtn")
                {
                    InteractionNotPossible();
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
        }
        // If Raycast is not hitting anything.
        else
        {
            CursorVisual.SetActive(false);
        }
    }
    }
