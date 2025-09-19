using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    public GameObject[] obstaclePrefabs;
    public int obstacleCount = 25;
    public Vector2Int minCell = new Vector2Int(0, 0);
    public Vector2Int maxCell = new Vector2Int(14, 14);
    public string obstacleLayerName = "Obstacle";
    public bool randomRotate90 = true;
    public float uniformScale = 0.8f;
    public float yEpsilon = 0.005f;

    public HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();

    public void BuildObstacles(GameManager gm)
    {
        foreach (Transform c in transform) DestroyImmediate(c.gameObject);
        occupied.Clear();
        occupied.Add(gm.startCell);
        occupied.Add(gm.goalCell);

        int placed = 0, tries = obstacleCount * 40;
        var rng = new System.Random();

        while (placed < obstacleCount && tries-- > 0)
        {
            int x = Random.Range(minCell.x, maxCell.x + 1);
            int y = Random.Range(minCell.y, maxCell.y + 1);
            var cell = new Vector2Int(x, y);
            if (occupied.Contains(cell)) continue;

            var prefab = ChoosePrefab();
            if (!prefab) continue;

            var go = Instantiate(prefab, transform);
            SetLayerRecursive(go, obstacleLayerName);

            Vector3 center = gm.CellToWorld(cell);
            float gy = SampleGroundY(center);

            go.transform.position = new Vector3(center.x, gy, center.z);
            ApplyUniformScale(go, uniformScale);

            if (randomRotate90)
            {
                int r = rng.Next(0, 4);
                go.transform.rotation = Quaternion.Euler(0f, r * 90f, 0f);
            }

            SnapBottomToGround(go, gy + yEpsilon);
            EnsureSolidCollider(go);

            go.name = $"Obstacle_{x}_{y}";
            occupied.Add(cell);
            placed++;
        }
    }

    public bool IsBlocked(Vector2Int cell) => occupied.Contains(cell);

    GameObject ChoosePrefab()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return null;
        int i = Random.Range(0, obstaclePrefabs.Length);
        return obstaclePrefabs[i];
    }

    float SampleGroundY(Vector3 xz)
    {
        if (Physics.Raycast(xz + Vector3.up * 20f, Vector3.down, out var hit, 100f, ~0, QueryTriggerInteraction.Ignore))
            return hit.point.y;
        return 0f;
    }

    void ApplyUniformScale(GameObject go, float s)
    {
        if (s <= 0f) return;
        go.transform.localScale = Vector3.one * s;
    }

    void SnapBottomToGround(GameObject go, float groundY)
    {
        Bounds b = GetWorldBounds(go);
        float delta = groundY - b.min.y;
        go.transform.position += Vector3.up * delta;
    }

    Bounds GetWorldBounds(GameObject go)
    {
        bool any = false;
        Bounds b = new Bounds(go.transform.position, Vector3.zero);
        var rends = go.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < rends.Length; i++)
        {
            if (!any) { b = rends[i].bounds; any = true; }
            else b.Encapsulate(rends[i].bounds);
        }
        if (!any)
        {
            var cols = go.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < cols.Length; i++)
            {
                if (!any) { b = cols[i].bounds; any = true; }
                else b.Encapsulate(cols[i].bounds);
            }
        }
        return b;
    }

    void EnsureSolidCollider(GameObject go)
    {
        var cols = go.GetComponentsInChildren<Collider>(true);
        if (cols == null || cols.Length == 0)
        {
            var bc = go.AddComponent<BoxCollider>();
            bc.isTrigger = false;
        }
        else
        {
            foreach (var c in cols) c.isTrigger = false;
        }
    }

    static void SetLayerRecursive(GameObject root, string layerName)
    {
        int l = LayerMask.NameToLayer(layerName);
        if (l < 0) return;
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = l;
    }
}
