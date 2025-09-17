using UnityEngine;

[RequireComponent(typeof(FigureControl))]
public class OutOfBoundsReset : MonoBehaviour
{
    public bool useX = false;
    public float minX = -999f, maxX = 999f;

    public bool useZ = true;
    public float minZ = -999f, maxZ = 999f;

    public float margin = 0.05f;

    public bool callNextLevel = true;

    FigureControl fc;

    void Awake() { fc = GetComponent<FigureControl>(); }

    void Update()
    {
        Vector3 p = transform.position;
        bool outX = useX && (p.x < minX - margin || p.x > maxX + margin);
        bool outZ = useZ && (p.z < minZ - margin || p.z > maxZ + margin);

        if (outX || outZ)
        {
            if (callNextLevel && GameSession.I != null)
                GameSession.I.NextLevel();
            else
                fc.ResetToStart();
        }
    }
}