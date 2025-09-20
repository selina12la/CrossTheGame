using UnityEngine;

public class AutoCollectOnLadder : MonoBehaviour
{
    public LayerMask ladderMask;
    public LayerMask collectibleMask;
    public float ladderCheckRadius = 0.35f;
    public float collectRadius = 0.75f;
    public float heightOffset = 0.2f;

    void Update()
    {
        var p = transform.position + Vector3.up * heightOffset;
        bool onLadder = Physics.CheckSphere(p, ladderCheckRadius, ladderMask, QueryTriggerInteraction.Collide);
        if (!onLadder) return;

        var hits = Physics.OverlapSphere(p, collectRadius, collectibleMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < hits.Length; i++)
        {
            var c = hits[i].GetComponentInParent<Collectible>() ?? hits[i].GetComponent<Collectible>();
            if (c) c.Collect();
        }
    }
}