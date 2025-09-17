using UnityEngine;

public class LadderPlatform : MonoBehaviour
{
    public Transform visual;
    public Vector3 size = new Vector3(2.5f, 0.3f, 0.6f);
    public float LaneZ;

    void OnValidate()
    {
        if (visual)
        {
            visual.localScale = new Vector3(size.x, size.y, size.z);
        }
    }
}