using UnityEngine;
using System.Collections.Generic;

public class LadderLaneSpawner : MonoBehaviour
{
    [Header("Prefab & Surface")]
    public GameObject ladderPrefab;          // Root hat BoxCollider (Trigger), Child "Visual" enthält Renderers
    public Transform surfaceRef;
    public string ladderLayerName = "Ladder";

    [Header("Lane")]
    public float zPos = 0f;
    public float xMin = 0f, xMax = 15f;      // Reihenfolge egal
    public bool  leftToRight = true;
    public float speed = 5f;

    [Header("Spawn")]
    public float targetLength = 2.5f;        // gewünschte WORLD-Länge
    public float gap = 0.6f;                 // Abstand zwischen Leitern (World)
    public int   maxAlive = 8;
    public float startDelay = 0f;

    readonly List<GameObject> alive = new();
    float timer;
    float baseVisualLen = -1f;

    void OnEnable()
    {
        if (!ladderPrefab || ladderPrefab.scene.IsValid()) { enabled = false; return; }
        if (!surfaceRef) { enabled = false; return; }

        if (baseVisualLen <= 0f)
        {
            baseVisualLen = MeasureVisualLength(ladderPrefab.transform);
            if (baseVisualLen <= 0f) baseVisualLen = 1f;
        }

        timer = startDelay;
    }

    void Update()
    {
        for (int i = alive.Count - 1; i >= 0; i--) if (!alive[i]) alive.RemoveAt(i);
        if (alive.Count >= maxAlive) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            float lo = Mathf.Min(xMin, xMax);
            float hi = Mathf.Max(xMin, xMax);
            float laneWidth = Mathf.Max(0.01f, hi - lo);

            float len   = Mathf.Min(targetLength, laneWidth);
            float step  = len + Mathf.Max(0f, gap);
            float delay = step / Mathf.Max(0.01f, Mathf.Abs(speed));

            SpawnOne(len);
            timer = delay;
        }
    }

    void SpawnOne(float worldLen)
    {
        float lo = Mathf.Min(xMin, xMax);
        float hi = Mathf.Max(xMin, xMax);
        float xStart = leftToRight ? lo : hi;
        float yTop   = GetTopY(surfaceRef);

        // >>> unter diesem Spawner parenten
        var go = Instantiate(
            ladderPrefab,
            new Vector3(xStart, yTop, zPos),
            Quaternion.identity,
            transform
        );

        SetLayerRecursive(go, ladderLayerName);

        // Visual auf Ziel-Länge skalieren
        Transform visual = go.transform.Find("Visual");
        if (visual != null)
        {
            float scaleX = worldLen / baseVisualLen;
            var s = visual.localScale;
            visual.localScale = new Vector3(scaleX, s.y, s.z);
        }

        // Collider: zentriere auf Visual-Mitte und setze passende Größe
        var col = go.GetComponent<BoxCollider>(); if (!col) col = go.AddComponent<BoxCollider>();
        col.isTrigger = true;

        Bounds vb = (visual ? CombinedBoundsOf(visual) : CombinedBoundsOf(go.transform));
        // Center in lokale Koordinaten des Root umrechnen:
        Vector3 localCenter = go.transform.InverseTransformPoint(vb.center);
        col.center = new Vector3(localCenter.x, 0.15f, localCenter.z);
        // Breite = Visual-Länge, Tiefe konstant
        col.size   = new Vector3(Mathf.Max(0.1f, worldLen), 0.3f, 0.6f);

        // Rigidbody
        var rb = go.GetComponent<Rigidbody>();
        if (!rb) { rb = go.AddComponent<Rigidbody>(); rb.isKinematic = true; rb.interpolation = RigidbodyInterpolation.Interpolate; }

        // Mover – normalisierte Bounds
        var mover = go.GetComponent<LadderMover>() ?? go.AddComponent<LadderMover>();
        mover.speed = speed * (leftToRight ? 1f : -1f);
        mover.xMin  = lo - 2f;
        mover.xMax  = hi + 2f;

        alive.Add(go);
    }

    float MeasureVisualLength(Transform root)
    {
        Transform v = root.Find("Visual");
        Bounds b = v ? CombinedBoundsOf(v) : CombinedBoundsOf(root);
        return b.size.x;
    }

    Bounds CombinedBoundsOf(Transform t)
    {
        var rends = t.GetComponentsInChildren<Renderer>(true);
        if (rends.Length == 0) return new Bounds(t.position, Vector3.one * 0.01f);
        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
        return b;
    }

    float GetTopY(Transform tr)
    {
        float top = tr.position.y;
        foreach (var r in tr.GetComponentsInChildren<Renderer>()) top = Mathf.Max(top, r.bounds.max.y);
        foreach (var c in tr.GetComponentsInChildren<Collider>()) top = Mathf.Max(top, c.bounds.max.y);
        return top;
    }

    static void SetLayerRecursive(GameObject root, string layerName)
    {
        int l = LayerMask.NameToLayer(layerName);
        if (l < 0) return;
        foreach (var tr in root.GetComponentsInChildren<Transform>(true))
            tr.gameObject.layer = l;
    }
}
