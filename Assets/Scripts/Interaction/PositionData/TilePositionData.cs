using MapzenGo.Helpers;
using MapzenGo.Models;
using UnityEngine;

public class TilePositionData : MonoBehaviour
{

    // Only used for debugging purposes.

    // Vector positions of tiles.
    public Vector3 topLeft;
    public Vector3 bottomLeft;
    public Vector3 topRight;
    public Vector3 bottomRight;
    public Vector3 vectorCenter;
    public Vector2d meters;

    public double tileTmsX;
    public double tileTmsY;
    // String for visualization in Inspector (Vector2d can't be displayed)
    public string meterString;

    //Lat long center positions.
    public double lat;
    public double lon;
    void Update()
    {
        // Gets local points.
        var boundPoint1 = GetComponent<MeshCollider>().bounds.min;
        var boundPoint2 = GetComponent<MeshCollider>().bounds.max;

        // Gets world spaces.
        topLeft = transform.TransformPoint(new Vector3(boundPoint1.x, boundPoint1.y, boundPoint2.z));
        topRight = transform.TransformPoint(new Vector3(boundPoint2.x, boundPoint1.y, boundPoint2.z));
        bottomLeft = transform.TransformPoint(new Vector3(boundPoint1.x, boundPoint2.y, boundPoint1.z));
        bottomRight = transform.TransformPoint(new Vector3(boundPoint2.x, boundPoint1.y, boundPoint1.z));
        vectorCenter = transform.TransformPoint(GetComponent<MeshCollider>().bounds.center);

        meters = GM.LatLonToMeters(lat, lon);
        meterString = meters.ToString();

        tileTmsX = GetComponentInParent<Tile>().TileTms.x;
        tileTmsY = GetComponentInParent<Tile>().TileTms.y;
    }
}
