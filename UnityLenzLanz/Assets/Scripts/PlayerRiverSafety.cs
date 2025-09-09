using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerRiverSafety : MonoBehaviour
{
    public Vector2 zRange = new(28f, 40f);
    public LayerMask ladderMask;
    public float checkPadding = 0.05f;
    public float graceEnter = 0.25f;
    public float graceExit = 0.1f;

    FigureControl fc;
    Collider self;
    Transform currentLadder;
    float inZoneSince = -999f;
    float offLadderSince = -999f;
    bool wasInZone;

    void Awake(){ fc = GetComponent<FigureControl>(); self = GetComponent<Collider>(); }

    void LateUpdate()
    {
        bool inZone = transform.position.z >= zRange.x && transform.position.z <= zRange.y;

        if (inZone && !wasInZone) inZoneSince = Time.time;
        wasInZone = inZone;

        if (!inZone)
        {
            if (currentLadder){ transform.SetParent(null); currentLadder = null; }
            return;
        }

        Bounds b = self.bounds;
        Vector3 center = new Vector3(transform.position.x, b.center.y, transform.position.z);
        Vector3 half = b.extents - Vector3.one * checkPadding;
        var hits = Physics.OverlapBox(center, half, Quaternion.identity, ladderMask, QueryTriggerInteraction.Collide);

        bool onLadder = hits.Length > 0;
        if (onLadder)
        {
            if (currentLadder != hits[0].transform)
            {
                currentLadder = hits[0].transform;
                transform.SetParent(currentLadder, true);
            }
            offLadderSince = -999f;
        }
        else
        {
            if (currentLadder){ transform.SetParent(null); currentLadder = null; }
            if (offLadderSince < 0f) offLadderSince = Time.time;
        }

        if (Time.time - inZoneSince < graceEnter) return;
        if (!onLadder && Time.time - offLadderSince > graceExit)
        {
            fc.SendMessage("Hit", SendMessageOptions.DontRequireReceiver);
            inZoneSince = -999f;
            offLadderSince = -999f;
        }
    }
}