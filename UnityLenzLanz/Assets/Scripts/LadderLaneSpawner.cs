using UnityEngine;
using System.Collections.Generic;

public class LadderLaneSpawner : MonoBehaviour
{
    public GameObject ladderPrefab;
    public Transform surfaceRef;
    public string ladderLayerName = "Ladder";

    public float zPos = 32.5f;
    public float xMin = -5f, xMax = 20f;
    public bool leftToRight = true;
    public float speed = 5f;

    public float minLength = 1.8f;
    public float maxLength = 3.2f;
    public float minGap = 0.15f;
    public float maxGap = 0.45f;

    public int prewarmCount = 2;
    public int maxAlive = 6;
    public float startDelay = 0f;

    readonly List<GameObject> alive = new();
    float timer;

    void OnEnable()
    {
        if (!ladderPrefab || ladderPrefab.scene.IsValid()) { enabled = false; return; }
        if (!surfaceRef) { enabled = false; return; }

        timer = startDelay;

        float topY = GetTopY(surfaceRef);
        for (int i = 0; i < prewarmCount; i++)
        {
            float f = (i + 1f) / (prewarmCount + 1f);
            float x = Mathf.Lerp(leftToRight ? xMin : xMax, leftToRight ? xMax : xMin, f);
            SpawnAt(x, topY, Random.Range(minLength, maxLength));
        }
    }

    void Update()
    {
        for (int i = alive.Count - 1; i >= 0; i--) if (!alive[i]) alive.RemoveAt(i);

        timer -= Time.deltaTime;
        if (timer <= 0f && alive.Count < maxAlive)
        {
            float len = Random.Range(minLength, maxLength);
            float gap = Random.Range(minGap, maxGap);
            float nextDelay = (len + gap) / Mathf.Max(0.01f, speed);

            SpawnAt(leftToRight ? xMin : xMax, GetTopY(surfaceRef), len);
            timer = Mathf.Max(0.15f, nextDelay);
        }
    }

    void SpawnAt(float x, float topY, float length)
    {
        var go = Instantiate(ladderPrefab);
        SetLayerRecursive(go, ladderLayerName);
        go.transform.position = new Vector3(x, topY, zPos);

        var plat = go.GetComponent<LadderPlatform>();
        if (!plat) plat = go.AddComponent<LadderPlatform>();
        plat.size  = new Vector3(length, plat.size.y <= 0f ? 0.3f : plat.size.y, Mathf.Max(plat.size.z, 0.6f));
        plat.LaneZ = zPos;

        var col = go.GetComponentInChildren<BoxCollider>();
        if (!col) col = go.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.center    = new Vector3(0f, 0.15f, 0f);
        col.size      = new Vector3(Mathf.Max(1f, length), 0.3f, 0.8f);

        var rb = go.GetComponent<Rigidbody>();
        if (!rb) { rb = go.AddComponent<Rigidbody>(); rb.isKinematic = true; rb.interpolation = RigidbodyInterpolation.Interpolate; }

        var mover = go.GetComponent<LadderMover>();
        if (!mover) mover = go.AddComponent<LadderMover>();
        mover.speed = speed * (leftToRight ? 1f : -1f);
        mover.xMin  = Mathf.Min(xMin, xMax) - 2f;
        mover.xMax  = Mathf.Max(xMin, xMax) + 2f;

        alive.Add(go);
    }

    float GetTopY(Transform t)
    {
        float top = t.position.y;
        foreach (var r in t.GetComponentsInChildren<Renderer>()) top = Mathf.Max(top, r.bounds.max.y);
        foreach (var c in t.GetComponentsInChildren<Collider>()) top = Mathf.Max(top, c.bounds.max.y);
        return top;
    }

    static void SetLayerRecursive(GameObject go, string layerName)
    {
        int l = LayerMask.NameToLayer(layerName);
        if (l < 0) return;
        foreach (var tr in go.GetComponentsInChildren<Transform>(true))
            tr.gameObject.layer = l;
    }
}
