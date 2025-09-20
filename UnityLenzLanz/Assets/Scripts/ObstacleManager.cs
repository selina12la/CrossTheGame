using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    [Header("Prefabs & Menge")]
    public List<GameObject> obstaclePrefabs = new(); // 4 Schach-FBX/Prefabs
    public int obstacleCount = 25;

    [Header("Zellbereich (inklusive)")]
    public Vector2Int minCell = new Vector2Int(0, 0);
    public Vector2Int maxCell = new Vector2Int(14, 14); // passe an dein Board an

    [Header("Board & Platzierung")]
    public Transform boardRef;                 // -> ChessBoard (Transform)
    public LayerMask groundMask = ~0;          // für Raycast/Rückfall
    public string obstacleLayerName = "Obstacle";
    public bool randomRotate90 = true;
    [Range(0.1f, 2f)] public float uniformScale = 0.75f;
    public float yEpsilon = 0.005f;

    [Header("Game")]
    public GameManager gameManager;            // kann leer bleiben
    public bool autoBuildOnStart = true;

    // Belegte Zellen (für externe Abfragen)
    readonly HashSet<Vector2Int> occupied = new();
    public bool IsBlocked(Vector2Int cell) => occupied.Contains(cell);

    void Start()
    {
        if (autoBuildOnStart) BuildObstacles();
    }

    public void BuildObstacles()               // ohne Argument – sucht GM selbst
    {
        if (!gameManager) gameManager = FindAnyObjectByType<GameManager>();
        if (!gameManager) { Debug.LogWarning("[ObstacleManager] Kein GameManager gefunden."); return; }
        BuildObstacles(gameManager);
    }

    public void BuildObstacles(GameManager gm) // mit GameManager
    {
        // Alte Instanzen entfernen
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        occupied.Clear();

        // Defaults, falls Min/Max nicht sinnvoll konfiguriert
        int xmin = Mathf.Clamp(minCell.x, 0, gm.width - 1);
        int ymin = Mathf.Clamp(minCell.y, 0, gm.height - 1);
        int xmax = Mathf.Clamp(maxCell.x, 0, gm.width - 1);
        int ymax = Mathf.Clamp(maxCell.y, 0, gm.height - 1);

        // Oberkante des Boards (flach angenommen)
        float topY = GetSurfaceTopY(boardRef);

        int placed = 0;
        int guard = obstacleCount * 40;
        var rng = new System.Random();

        while (placed < obstacleCount && guard-- > 0)
        {
            var c = new Vector2Int(
                rng.Next(xmin, xmax + 1),
                rng.Next(ymin, ymax + 1)
            );

            if (!gm.InBounds(c)) continue;
            if (occupied.Contains(c)) continue;                   // 1 Figur pro Zelle

            // Weltposition der Zellmitte
            Vector3 pos = gm.CellToWorld(c);
            float y = topY;                                       // Boardhöhe
            if (float.IsNaN(y))                                   // Fallback auf Raycast
            {
                if (RaycastGround(pos, out float gy)) y = gy;
                else continue;
            }
            pos.y = y + yEpsilon;

            // Prefab wählen
            var prefab = PickPrefab();
            if (!prefab) continue;

            // Instanz
            var go = Instantiate(prefab, pos, Quaternion.identity, transform);
            go.name = $"Obstacle_{c.x}_{c.y}";
            SetLayerRecursive(go, obstacleLayerName);

            if (randomRotate90)
            {
                int r = rng.Next(0, 4) * 90;
                go.transform.rotation = Quaternion.Euler(0f, r, 0f);
            }

            go.transform.localScale = Vector3.one * uniformScale;

            // Collider sicherstellen (nicht Trigger)
            EnsureSolidCollider(go);

            occupied.Add(c);
            placed++;
        }
    }

    GameObject PickPrefab()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Count == 0) return null;
        // Zufällig, aber nur aus gültigen (nicht-Szene) Prefabs
        for (int tries = 0; tries < 10; tries++)
        {
            var p = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Count)];
            if (p && !p.scene.IsValid()) return p;
        }
        return obstaclePrefabs[0];
    }

    float GetSurfaceTopY(Transform t)
    {
        if (!t) return float.NaN;
        float top = t.position.y;
        var rends = t.GetComponentsInChildren<Renderer>(true);
        foreach (var r in rends) top = Mathf.Max(top, r.bounds.max.y);
        var cols = t.GetComponentsInChildren<Collider>(true);
        foreach (var c in cols) top = Mathf.Max(top, c.bounds.max.y);
        return top;
    }

    bool RaycastGround(Vector3 xz, out float groundY)
    {
        Vector3 from = xz + Vector3.up * 50f;
        if (Physics.Raycast(from, Vector3.down, out var hit, 100f, groundMask, QueryTriggerInteraction.Ignore))
        {
            groundY = hit.point.y;
            return true;
        }
        groundY = 0f;
        return false;
    }

    void EnsureSolidCollider(GameObject root)
    {
        var col = root.GetComponentInChildren<Collider>();
        if (!col)
        {
            var rr = root.GetComponentInChildren<Renderer>();
            var bc = root.AddComponent<BoxCollider>();
            if (rr)
            {
                // grob an den sichtbaren Bounds orientieren
                var b = rr.bounds;
                bc.center = root.transform.InverseTransformPoint(b.center);
                bc.size = b.size;
            }
            col = bc;
        }
        col.isTrigger = false;
    }

    void SetLayerRecursive(GameObject root, string layerName)
    {
        int l = LayerMask.NameToLayer(layerName);
        if (l < 0) return;
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = l;
    }
}
