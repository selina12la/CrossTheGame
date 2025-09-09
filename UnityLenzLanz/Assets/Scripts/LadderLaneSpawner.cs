using UnityEngine;

public class LadderLaneSpawner : MonoBehaviour
{
    public GameObject ladderPrefab;
    public Transform surfaceRef;
    public string ladderLayerName = "Ladder";

    public float zPos = 30f;
    public float xMin = -3f, xMax = 18f;
    public float speed = 4f;
    public float spawnInterval = 1.2f, spawnJitter = 0.4f;
    public int prewarmCount = 2;
    public float startDelay = 0f;
    public bool leftToRight = true;

    float t;

    void Start()
    {
        t = startDelay <= 0f ? 0f : startDelay;
        float topY = GetSurfaceTopY(surfaceRef);
        float laneLen = Mathf.Abs(xMax - xMin);
        float travelTime = laneLen / Mathf.Max(0.01f, speed);
        int n = Mathf.Max(0, prewarmCount);
        for (int i = 0; i < n; i++)
        {
            float frac = (i + 1f) / (n + 1f);
            float x = Mathf.Lerp(leftToRight ? xMin : xMax, leftToRight ? xMax : xMin, frac);
            SpawnAt(x, topY);
        }
    }

    void Update()
    {
        t -= Time.deltaTime;
        if (t <= 0f)
        {
            float topY = GetSurfaceTopY(surfaceRef);
            SpawnAt(leftToRight ? xMin : xMax, topY);
            t = Mathf.Max(0.2f, spawnInterval + Random.Range(-spawnJitter, spawnJitter));
        }
    }

    void SpawnAt(float x, float topY)
    {
        if (!ladderPrefab) return;
        var go = Instantiate(ladderPrefab);
        go.layer = LayerMask.NameToLayer(ladderLayerName);

        var lp = go.GetComponent<LadderPlatform>();
        float rootY = topY;
        go.transform.position = new Vector3(x, rootY, zPos);

        var mover = go.GetComponent<LadderMover>();
        if (!mover) mover = go.AddComponent<LadderMover>();
        mover.speed = speed * (leftToRight ? 1f : -1f);
        mover.xMin = xMin - 2f;
        mover.xMax = xMax + 2f;
    }

    float GetSurfaceTopY(Transform surface)
    {
        if (!surface) return 0f;
        float top = surface.position.y;
        foreach (var r in surface.GetComponentsInChildren<Renderer>()) top = Mathf.Max(top, r.bounds.max.y);
        foreach (var c in surface.GetComponentsInChildren<Collider>()) top = Mathf.Max(top, c.bounds.max.y);
        return top;
    }
}
