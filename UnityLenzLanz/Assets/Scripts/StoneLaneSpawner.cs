using UnityEngine;

public class StoneLaneSpawner : MonoBehaviour
{
  
    public GameObject stonePrefab;
    public Transform surfaceRef;        
    public float surfaceOffset = 0f;      
   
    public float zPos = 32f;
    public float xMin = -3f, xMax = 17f;
    public bool leftToRight = true;

    public float speed = 6f;

    public float spawnInterval = 1.2f;
    public float spawnJitter   = 0.4f;

    public string obstacleLayerName = "Obstacle";

    float timer;

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Spawn();
            float jitter = Random.Range(-spawnJitter, spawnJitter);
            timer = Mathf.Max(0.2f, spawnInterval + jitter);
        }
    }

    void Spawn()
    {
        if (!stonePrefab) return;

        float xStart = leftToRight ? xMin : xMax;
        GameObject go = Instantiate(stonePrefab);

        SetLayerRecursive(go, obstacleLayerName);

        var cols = go.GetComponentsInChildren<Collider>(true);
        if (cols != null && cols.Length > 0)
        {
            for (int i = 0; i < cols.Length; i++)
                cols[i].isTrigger = true;
        }
        else
        {
            var r = go.GetComponentInChildren<Renderer>();
            var bc = go.AddComponent<BoxCollider>();
            float h = r ? r.bounds.size.y : 1f;
            bc.center = new Vector3(0f, h * 0.5f, 0f);
            bc.size   = new Vector3(1f, h, 1f);
            bc.isTrigger = true;
        }

        float topY = GetSurfaceTopY(surfaceRef) + surfaceOffset;

        float halfH = 0.5f;
        {
            float best = 0f;
            var r = go.GetComponentInChildren<Renderer>();
            if (r) best = Mathf.Max(best, r.bounds.extents.y);
            var c = go.GetComponentInChildren<Collider>();
            if (c) best = Mathf.Max(best, c.bounds.extents.y);
            if (best > 0f) halfH = best;
        }

        go.transform.position = new Vector3(xStart, topY + halfH, zPos);

        var mover = go.GetComponent<StoneMover>();
        if (!mover) mover = go.AddComponent<StoneMover>();
        mover.speed = speed * (leftToRight ? 1f : -1f);
        mover.xMin  = Mathf.Min(xMin, xMax) - 2f;
        mover.xMax  = Mathf.Max(xMin, xMax) + 2f;

        var rb = go.GetComponentInChildren<Rigidbody>();
        if (!rb)
        {
            rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        if (!go.GetComponent<StoneHazard>())
            go.AddComponent<StoneHazard>();
    }

    static void SetLayerRecursive(GameObject root, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer < 0) return;
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = layer;
    }

    static float GetSurfaceTopY(Transform surface)
    {
        if (!surface) return 0f;
        float top = surface.position.y;
        var rends = surface.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < rends.Length; i++) top = Mathf.Max(top, rends[i].bounds.max.y);
        var cols = surface.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < cols.Length; i++) top = Mathf.Max(top, cols[i].bounds.max.y);
        return top;
    }
}