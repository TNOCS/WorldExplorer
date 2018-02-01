using UnityEngine;

public class RaycastManager : SingletonCustom<RaycastManager>
{
    /// <summary>
    /// Handles the raycast from the users point of view, including the emphasizing of gaze targets.
    /// </summary>
    
    [SerializeField]
    private float uiScalefactor = 1.25f;
    private bool enlarged = false;
    private bool animationPlaying = false;
    private Quaternion originalRotation;
    private GameObject rotatingObject;
    public GameObject enlargedObject;
    public AudioClip gazeFeedback;
    public AudioSource audioSource;

    private void Start()
    {
        var audioContainer = GameObject.Find("AudioContainer");
        audioSource = audioContainer.GetComponents<AudioSource>()[0];
        gazeFeedback = Resources.Load<AudioClip>("Audio/lowclick");
    }

    public void Update()
    {
        // Enlarges elements being gazed at.
        EmphasizeGazeTargets();
    }

    private void EmphasizeGazeTargets()
    {
        if (Raycast().collider != null)
        {
            var rayCastFocus = Raycast().collider.gameObject;

            // Enlarges elements that are being gazed and plays a low key sounds.
            if (rayCastFocus.tag == "inventoryobject" || rayCastFocus.tag == "uibutton")
            {
                // Enlarges.
                if (!enlarged)
                {
                    rayCastFocus.transform.localScale = rayCastFocus.transform.localScale * uiScalefactor;
                    audioSource.PlayOneShot(gazeFeedback, 0.1f);
                    enlargedObject = rayCastFocus;
                    enlarged = true;
                }

                // 'Unlarges'.
                if (enlarged && rayCastFocus != enlargedObject)
                {
                    enlargedObject.transform.localScale = enlargedObject.transform.localScale / uiScalefactor;
                    enlargedObject = null;
                    enlarged = false;
                }
            }
            else
            {
                // Scales elements back down.
                if (enlargedObject != null)
                {
                    enlargedObject.transform.localScale = enlargedObject.transform.localScale / uiScalefactor;
                    enlargedObject = null;
                    enlarged = false;
                }
            }
        }
        else
        {
            // Stop rotating objects.
            if (rotatingObject != null)
            {
                rotatingObject.transform.rotation = originalRotation;
                rotatingObject = null;
            }

            // Scales elements back down.
            if (enlargedObject != null)
            {
                enlargedObject.transform.localScale = enlargedObject.transform.localScale / uiScalefactor;
                enlargedObject = null;
                enlarged = false;
            }
        }
    }

    public RaycastHit Raycast()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 20.0f, Physics.DefaultRaycastLayers))
        {
            return hit;
        }
        return hit;
    }
}

