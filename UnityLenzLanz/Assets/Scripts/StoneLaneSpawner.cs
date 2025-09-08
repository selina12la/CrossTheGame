using UnityEngine;

public class StoneLaneSpawner : MonoBehaviour
{
    public GameObject stonePrefab;
    public Transform surfaceRef;
    public float surfaceOffset = 0f;

    public float zPos = 32f;
    public float xMin = -3f, xMax = 17f;
    public float speed = 6f;
    public float spawnInterval = 1.2f, spawnJitter = 0.4f;
    public bool leftToRight = true;

    float t;

    void Update()
    {
        t -= Time.deltaTime;
        if (t <= 0f)
        {
            Spawn();
            t = Mathf.Max(0.2f, spawnInterval + Random.Range(-spawnJitter, spawnJitter));
        }
    }

    void Spawn()
    {
        if (!stonePrefab) return;

        float x = leftToRight ? xMin : xMax;
        var go = Instantiate(stonePrefab);
        var r = go.GetComponentInChildren<Renderer>();
        var c = go.GetComponentInChildren<Collider>();

        float halfH = 0.5f;
        if (r) halfH = r.bounds.extents.y;
        else if (c) halfH = c.bounds.extents.y;

        float baseY = surfaceRef ? surfaceRef.position.y + surfaceOffset : 0f;
        go.transform.position = new Vector3(x, baseY + halfH, zPos);

        var mover = go.GetComponent<StoneMover>();
        if (!mover) mover = go.AddComponent<StoneMover>();
        mover.speed = speed * (leftToRight ? 1f : -1f);
        mover.xMin = xMin - 2f;
        mover.xMax = xMax + 2f;
    }
}