using UnityEngine;
using System.Collections.Generic;

public class CollectibleSpawner : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject dicePrefab;
    
    [Tooltip("Alle Ebenen, die Boden sind (Default/Board/SnakesBoard/Backgammon usw.).")]
    public LayerMask groundMask = ~0;
    [Tooltip("Diese Ebenen werden gemieden (z.B. Obstacle).")]
    public LayerMask avoidMask;
    [Tooltip("Leiter-Ebene, falls Collectibles im Fluss nur auf Leitern erlaubt sind.")]
    public LayerMask ladderMask;

    public int   count          = 12;
    public int   triesPerItem   = 40;
    public float minSeparation  = 1.0f;
    public float collectibleScale = 0.6f;
    public float rayHeight      = 50f;

    public bool allowInRivers   = false;
    public bool allowOnLadders  = true;
    public Vector2 riverA = new Vector2(26f, 31f);
    public Vector2 riverB = new Vector2(32f, 37f);

    public string collectibleLayerName = "Collectible";

    readonly HashSet<Vector2Int> usedCells = new();

    void Start()
    {
        if (!gameManager || !dicePrefab) return;
        SpawnAll();
    }

    void SpawnAll()
    {
        int collectibleLayer = LayerMask.NameToLayer(collectibleLayerName);
        int spawned = 0;

        while (spawned < count)
        {
            bool placed = false;

            for (int t = 0; t < triesPerItem && !placed; t++)
            {
                var cell = new Vector2Int(
                    Random.Range(0, gameManager.width),
                    Random.Range(0, gameManager.height)
                );
                if (usedCells.Contains(cell)) continue;

                Vector3 center = gameManager.CellToWorld(cell);

                if (!RaycastGround(center, out float gy)) continue;

                bool inRiver = InAnyRiver(center.z);
                if (inRiver && !allowInRivers)
                {
                    if (!(allowOnLadders && IsOnLayer(new Vector3(center.x, gy, center.z), ladderMask)))
                        continue;
                }

                if (IsBlocked(new Vector3(center.x, gy, center.z))) continue;

                GameObject go = Instantiate(dicePrefab);
                if (collectibleLayer >= 0) SetLayerRecursive(go, collectibleLayer);

                FitToGround(go.transform, new Vector3(center.x, gy, center.z), collectibleScale);
                usedCells.Add(cell);
                spawned++;
                placed = true;
            }

            if (!placed) break;
        }
    }

    bool RaycastGround(Vector3 xz, out float groundY)
    {
        groundY = 0f;
        if (Physics.Raycast(xz + Vector3.up * rayHeight, Vector3.down,
                            out var hit, rayHeight * 2f, groundMask, QueryTriggerInteraction.Ignore))
        {
            groundY = hit.point.y;
            return true;
        }
        return false;
    }

    bool InAnyRiver(float z)
        => (z >= riverA.x && z <= riverA.y) || (z >= riverB.x && z <= riverB.y);

    bool IsOnLayer(Vector3 pos, LayerMask mask)
    {
        var hits = Physics.OverlapBox(pos + Vector3.up * 0.4f,
                                      new Vector3(0.45f, 0.5f, 0.45f),
                                      Quaternion.identity, mask,
                                      QueryTriggerInteraction.Collide);
        return hits != null && hits.Length > 0;
    }

    bool IsBlocked(Vector3 pos)
    {
        var hits = Physics.OverlapBox(pos + Vector3.up * 0.5f,
                                      new Vector3(0.48f, 0.6f, 0.48f),
                                      Quaternion.identity, avoidMask,
                                      QueryTriggerInteraction.Ignore);
        return hits != null && hits.Length > 0;
    }

    void FitToGround(Transform t, Vector3 onGround, float scale)
    {
        t.localScale = Vector3.one * scale;

        float half = 0.5f;
        var r = t.GetComponentInChildren<Renderer>();
        if (r) half = r.bounds.extents.y;

        t.position = new Vector3(onGround.x, onGround.y + half, onGround.z);

        var col = t.GetComponentInChildren<Collider>();
        if (!col) col = t.gameObject.AddComponent<BoxCollider>();
        col.isTrigger = true;
    }

    static void SetLayerRecursive(GameObject root, int layer)
    {
        foreach (var tr in root.GetComponentsInChildren<Transform>(true))
            tr.gameObject.layer = layer;
    }
}
