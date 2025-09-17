using UnityEngine;

public class PlayerLadderRide : MonoBehaviour
{
    [SerializeField] LayerMask ladderMask;
    [SerializeField] float attachDistance = 0.15f;
    [SerializeField] float detachDelay    = 0.08f;
    [SerializeField] float laneSnap       = 0.25f;

    Collider selfCol;
    Transform ridingParent;
    Collider  ridingCol;
    float detachT;

    void Awake() => selfCol = GetComponent<Collider>();

    void LateUpdate()
    {
        float footY = selfCol ? selfCol.bounds.min.y + 0.05f : transform.position.y - 0.45f;
        Vector3 origin = new Vector3(transform.position.x, footY + 0.05f, transform.position.z);

        bool rayHit = Physics.Raycast(
            origin, Vector3.down, out var hit, 0.5f, ladderMask, QueryTriggerInteraction.Collide
        );

        bool canAttach = false;
        Collider hitCol = null;

        if (rayHit && hit.distance <= attachDistance)
        {
            hitCol = hit.collider;
            var lp = hitCol.GetComponentInParent<LadderPlatform>();
            if (lp != null && Mathf.Abs(lp.LaneZ - transform.position.z) <= laneSnap)
                canAttach = true;
        }

        if (canAttach)
        {
            detachT = 0f;

            if (ridingCol != hitCol)
            {
                ridingCol    = hitCol;
                ridingParent = hitCol.transform;
                transform.SetParent(ridingParent, true);
            }
        }
        else if (ridingParent)
        {
            detachT += Time.deltaTime;
            if (detachT >= detachDelay)
            {
                transform.SetParent(null, true);
                ridingParent = null;
                ridingCol    = null;
            }
        }
    }
}