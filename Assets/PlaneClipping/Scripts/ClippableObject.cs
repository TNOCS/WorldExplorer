using UnityEngine;
using System.Collections;
using System.Linq;

[ExecuteInEditMode]
public class ClippableObject : MonoBehaviour {
    public void OnEnable() {
        //let's just create a new material instance.
        GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Custom/StandardClippable")) {
            hideFlags = HideFlags.HideAndDontSave
        };

		plane1Position = tableMidPoint + new Vector3 (Mathf.Sin(Mathf.PI * 0 * 0.125f) * radius, 0, Mathf.Sin(Mathf.PI * 4 * 0.125f) * radius);
		plane2Position = tableMidPoint + new Vector3 (Mathf.Sin(Mathf.PI * 1 * 0.125f) * radius, 0, Mathf.Sin(Mathf.PI * 5 * 0.125f) * radius);
		plane3Position = tableMidPoint + new Vector3 (Mathf.Sin(Mathf.PI * 2 * 0.125f) * radius, 0, Mathf.Sin(Mathf.PI * 6 * 0.125f) * radius);
		plane4Position = tableMidPoint + new Vector3 (Mathf.Sin(Mathf.PI * 3 * 0.125f) * radius, 0, Mathf.Sin(Mathf.PI * 7 * 0.125f) * radius);
		plane5Position = tableMidPoint + new Vector3 (Mathf.Sin(Mathf.PI * 4 * 0.125f) * radius, 0, Mathf.Sin(Mathf.PI * 8 * 0.125f) * radius);
		plane6Position = tableMidPoint + new Vector3 (Mathf.Sin(Mathf.PI * 5 * 0.125f) * radius, 0, Mathf.Sin(Mathf.PI * 9 * 0.125f) * radius);
		plane7Position = tableMidPoint + new Vector3 (Mathf.Sin(Mathf.PI * 6 * 0.125f) * radius, 0, Mathf.Sin(Mathf.PI * 10 * 0.125f) * radius);
		plane8Position = tableMidPoint + new Vector3 (Mathf.Sin(Mathf.PI * 7 * 0.125f) * radius, 0, Mathf.Sin(Mathf.PI * 11 * 0.125f) * radius);
		plane9Position = tableMidPoint + new Vector3 (Mathf.Sin(Mathf.PI * 8 * 0.125f) * radius, 0, Mathf.Sin(Mathf.PI * 12 * 0.125f) * radius);
		plane10Position = tableMidPoint + new Vector3 (Mathf.Sin(Mathf.PI * 9 * 0.125f) * radius, 0, Mathf.Sin(Mathf.PI * 13 * 0.125f) * radius);
		plane11Position = tableMidPoint + new Vector3 (Mathf.Sin(Mathf.PI * 10 * 0.125f) * radius, 0, Mathf.Sin(Mathf.PI * 14 * 0.125f) * radius);
		plane12Position = tableMidPoint + new Vector3 (Mathf.Sin(Mathf.PI * 11 * 0.125f) * radius, 0, Mathf.Sin(Mathf.PI * 15 * 0.125f) * radius);
		plane13Position = tableMidPoint + new Vector3 (Mathf.Sin(Mathf.PI * 12 * 0.125f) * radius, 0, Mathf.Sin(Mathf.PI * 0 * 0.125f) * radius);
		plane14Position = tableMidPoint + new Vector3 (Mathf.Sin(Mathf.PI * 13 * 0.125f) * radius, 0, Mathf.Sin(Mathf.PI * 1 * 0.125f) * radius);
		plane15Position = tableMidPoint + new Vector3 (Mathf.Sin(Mathf.PI * 14 * 0.125f) * radius, 0, Mathf.Sin(Mathf.PI * 2 * 0.125f) * radius);
		plane16Position = tableMidPoint + new Vector3 (Mathf.Sin(Mathf.PI * 15 * 0.125f) * radius, 0, Mathf.Sin(Mathf.PI * 3 * 0.125f) * radius);
    }

    public void Start() { }

    //only 3 clip planes for now, will need to modify the shader for more.
    [Range(0, 3)]
    public int clipPlanes = 0;

    //preview size for the planes. Shown when the object is selected.
    public float planePreviewSize = 5.0f;

    //Positions and rotations for the planes. The rotations will be converted into normals to be used by the shaders.
	private float radius = 1.6f;
	private Vector3 tableMidPoint = new Vector3(0, 0.75f, 0);

	public Vector3 plane1Position = Vector3.zero; 
    public Vector3 plane1Rotation = new Vector3(-90, 0, 360 / 16 * 0);

    public Vector3 plane2Position = Vector3.zero;
	public Vector3 plane2Rotation = new Vector3(-90, 0, 360 / 16 * 1);

    public Vector3 plane3Position = Vector3.zero;
	public Vector3 plane3Rotation = new Vector3(-90, 0, 360 / 16 * 2);

	public Vector3 plane4Position = Vector3.zero;
	public Vector3 plane4Rotation = new Vector3(-90, 0, 360 / 16 * 3);

	public Vector3 plane5Position = Vector3.zero;
	public Vector3 plane5Rotation = new Vector3(-90, 0, 360 / 16 * 4);

	public Vector3 plane6Position = Vector3.zero;
	public Vector3 plane6Rotation = new Vector3(-90, 0, 360 / 16 * 5);

	public Vector3 plane7Position = Vector3.zero;
	public Vector3 plane7Rotation = new Vector3(-90, 0, 360 / 16 * 6);

	public Vector3 plane8Position = Vector3.zero;
	public Vector3 plane8Rotation = new Vector3(-90, 0, 360 / 16 * 7);

	public Vector3 plane9Position = Vector3.zero;
	public Vector3 plane9Rotation = new Vector3(-90, 0, 360 / 16 * 8);

	public Vector3 plane10Position = Vector3.zero;
	public Vector3 plane10Rotation = new Vector3(-90, 0, 360 / 16 * 9);

	public Vector3 plane11Position = Vector3.zero;
	public Vector3 plane11Rotation = new Vector3(-90, 0, 360 / 16 * 10);

	public Vector3 plane12Position = Vector3.zero;
	public Vector3 plane12Rotation = new Vector3(-90, 0, 360 / 16 * 11);

	public Vector3 plane13Position = Vector3.zero;
	public Vector3 plane13Rotation = new Vector3(-90, 0, 360 / 16 * 12);

	public Vector3 plane14Position = Vector3.zero;
	public Vector3 plane14Rotation = new Vector3(-90, 0, 360 / 16 * 13);

	public Vector3 plane15Position = Vector3.zero;
	public Vector3 plane15Rotation = new Vector3(-90, 0, 360 / 16 * 14);

	public Vector3 plane16Position = Vector3.zero;
	public Vector3 plane16Rotation = new Vector3(-90, 0, 360 / 16 * 15);

    //Only used for previewing a plane. Draws diagonals and edges of a limited flat plane.
    private void DrawPlane(Vector3 position, Vector3 euler) {
        var forward = Quaternion.Euler(euler) * Vector3.forward;
        var left = Quaternion.Euler(euler) * Vector3.left;

        var forwardLeft = position + forward * planePreviewSize * 0.5f + left * planePreviewSize * 0.5f;
        var forwardRight = forwardLeft - left * planePreviewSize;
        var backRight = forwardRight - forward * planePreviewSize;
        var backLeft = forwardLeft - forward * planePreviewSize;

        Gizmos.DrawLine(position, forwardLeft);
        Gizmos.DrawLine(position, forwardRight);
        Gizmos.DrawLine(position, backRight);
        Gizmos.DrawLine(position, backLeft);

        Gizmos.DrawLine(forwardLeft, forwardRight);
        Gizmos.DrawLine(forwardRight, backRight);
        Gizmos.DrawLine(backRight, backLeft);
        Gizmos.DrawLine(backLeft, forwardLeft);
    }

    private void OnDrawGizmosSelected() {
        if (clipPlanes >= 1) {
            DrawPlane(plane1Position, plane1Rotation);
        }
        if (clipPlanes >= 2) {
            DrawPlane(plane2Position, plane2Rotation);
        }
        if (clipPlanes >= 3) {
            DrawPlane(plane3Position, plane3Rotation);
			DrawPlane(plane4Position, plane4Rotation);
			DrawPlane(plane5Position, plane5Rotation);
			DrawPlane(plane6Position, plane6Rotation);
			DrawPlane(plane7Position, plane7Rotation);
			DrawPlane(plane8Position, plane8Rotation);
			DrawPlane(plane9Position, plane9Rotation);
			DrawPlane(plane10Position, plane10Rotation);
			DrawPlane(plane11Position, plane11Rotation);
			DrawPlane(plane12Position, plane12Rotation);
			DrawPlane(plane13Position, plane13Rotation);
			DrawPlane(plane14Position, plane14Rotation);
			DrawPlane(plane15Position, plane15Rotation);
			DrawPlane(plane16Position, plane16Rotation);
		}

    }

    //Ideally the planes do not need to be updated every frame, but we'll just keep the logic here for simplicity purposes.
    public void Update()
    {
        var sharedMaterial = GetComponent<MeshRenderer>().sharedMaterial;

        //Only should enable one keyword. If you want to enable any one of them, you actually need to disable the others. 
        //This may be a bug...
        switch (clipPlanes) {
            case 0:
                sharedMaterial.DisableKeyword("CLIP_ONE");
                sharedMaterial.DisableKeyword("CLIP_TWO");
                sharedMaterial.DisableKeyword("CLIP_THREE");
                break;
            case 1:
                sharedMaterial.EnableKeyword("CLIP_ONE");
                sharedMaterial.DisableKeyword("CLIP_TWO");
                sharedMaterial.DisableKeyword("CLIP_THREE");
                break;
            case 2:
                sharedMaterial.DisableKeyword("CLIP_ONE");
                sharedMaterial.EnableKeyword("CLIP_TWO");
                sharedMaterial.DisableKeyword("CLIP_THREE");
                break;
            case 3:
                sharedMaterial.DisableKeyword("CLIP_ONE");
                sharedMaterial.DisableKeyword("CLIP_TWO");
                sharedMaterial.EnableKeyword("CLIP_THREE");
                break;
        }

        //pass the planes to the shader if necessary.
        if (clipPlanes >= 1)
        {
            sharedMaterial.SetVector("_planePos", plane1Position);
            //plane normal vector is the rotated 'up' vector.
            sharedMaterial.SetVector("_planeNorm", Quaternion.Euler(plane1Rotation) * Vector3.up);
        }

        if (clipPlanes >= 2)
        {
            sharedMaterial.SetVector("_planePos2", plane2Position);
            sharedMaterial.SetVector("_planeNorm2", Quaternion.Euler(plane2Rotation) * Vector3.up);
        }

        if (clipPlanes >= 3)
        {
            sharedMaterial.SetVector("_planePos3", plane3Position);
            sharedMaterial.SetVector("_planeNorm3", Quaternion.Euler(plane3Rotation) * Vector3.up);

			sharedMaterial.SetVector("_planePos4", plane4Position);
			sharedMaterial.SetVector("_planeNorm4", Quaternion.Euler(plane4Rotation) * Vector3.up);

			sharedMaterial.SetVector("_planePos5", plane5Position);
			sharedMaterial.SetVector("_planeNorm5", Quaternion.Euler(plane5Rotation) * Vector3.up);

			sharedMaterial.SetVector("_planePos6", plane6Position);
			sharedMaterial.SetVector("_planeNorm6", Quaternion.Euler(plane6Rotation) * Vector3.up);

			sharedMaterial.SetVector("_planePos7", plane7Position);
			sharedMaterial.SetVector("_planeNorm7", Quaternion.Euler(plane7Rotation) * Vector3.up);

			sharedMaterial.SetVector("_planePos8", plane8Position);
			sharedMaterial.SetVector("_planeNorm8", Quaternion.Euler(plane8Rotation) * Vector3.up);

			sharedMaterial.SetVector("_planePos9", plane9Position);
			sharedMaterial.SetVector("_planeNorm9", Quaternion.Euler(plane9Rotation) * Vector3.up);

			sharedMaterial.SetVector("_planePos10", plane10Position);
			sharedMaterial.SetVector("_planeNorm10", Quaternion.Euler(plane10Rotation) * Vector3.up);

			sharedMaterial.SetVector("_planePos11", plane11Position);
			sharedMaterial.SetVector("_planeNorm11", Quaternion.Euler(plane11Rotation) * Vector3.up);

			sharedMaterial.SetVector("_planePos12", plane12Position);
			sharedMaterial.SetVector("_planeNorm12", Quaternion.Euler(plane12Rotation) * Vector3.up);

			sharedMaterial.SetVector("_planePos13", plane13Position);
			sharedMaterial.SetVector("_planeNorm13", Quaternion.Euler(plane13Rotation) * Vector3.up);

			sharedMaterial.SetVector("_planePos14", plane14Position);
			sharedMaterial.SetVector("_planeNorm14", Quaternion.Euler(plane14Rotation) * Vector3.up);

			sharedMaterial.SetVector("_planePos15", plane15Position);
			sharedMaterial.SetVector("_planeNorm15", Quaternion.Euler(plane15Rotation) * Vector3.up);

			sharedMaterial.SetVector("_planePos16", plane16Position);
			sharedMaterial.SetVector("_planeNorm16", Quaternion.Euler(plane16Rotation) * Vector3.up);
		}
    }
}