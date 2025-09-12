using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerRiverSafety : MonoBehaviour
{
    public Vector2 zRange = new(26f, 31f);
    public LayerMask ladderMask;
    public float checkPadding = 0.05f;
    public float graceEnter = 0.25f;
    public float graceExit = 0.5f;

    FigureControl fc;
    Collider self;
    Transform currentLadder;
    float inZoneSince = -999f;
    float offLadderSince = -999f;
    bool wasInZone;

    void Awake()
    {
        fc = GetComponent<FigureControl>();
        self = GetComponent<Collider>();
    }

    void LateUpdate()
    {
        if (fc != null && fc.IsMoving) return;

        bool inZone = transform.position.z >= zRange.x && transform.position.z <= zRange.y;

        if (inZone && !wasInZone) inZoneSince = Time.time;
        wasInZone = inZone;

        if (!inZone)
        {
            if (currentLadder) { transform.SetParent(null, true); currentLadder = null; }
            return;
        }

        Bounds b = self.bounds;
        Vector3 center = new Vector3(transform.position.x, b.center.y, transform.position.z);
        Vector3 half = b.extents - Vector3.one * checkPadding;
        half.x = Mathf.Max(0.01f, half.x);
        half.y = Mathf.Max(0.01f, half.y);
        half.z = Mathf.Max(0.01f, half.z);

        var hits = Physics.OverlapBox(center, half, Quaternion.identity, ladderMask, QueryTriggerInteraction.Collide);
        bool onLadder = hits != null && hits.Length > 0;

        if (onLadder)
        {
            Transform hitLadder = hits[0].transform;
            if (currentLadder != hitLadder)
            {
                currentLadder = hitLadder;
                transform.SetParent(currentLadder, true);
            }
            offLadderSince = -999f;
        }
        else
        {
            if (currentLadder) { transform.SetParent(null, true); currentLadder = null; }
            if (offLadderSince < 0f) offLadderSince = Time.time;
        }

        if (Time.time - inZoneSince < graceEnter) return;
        if (!onLadder && Time.time - offLadderSince > graceExit)
        {
            fc?.ResetToStart();
            inZoneSince = -999f;
            offLadderSince = -999f;
        }
    }
}
