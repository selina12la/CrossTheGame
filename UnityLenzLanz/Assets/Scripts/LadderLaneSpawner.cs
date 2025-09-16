using System.Collections.Generic;
using UnityEngine;

public class LadderLaneSpawner : MonoBehaviour
{
    public GameObject ladderPrefab;     
    public Transform surfaceRef;        
    
    public string ladderLayerName = "Ladder";

    public float zPos = 27.7f;        
    public float xMin = -5f, xMax = 20f;
    public bool leftToRight = true;
    public float speed = 5f;

    public float minLength = 1.8f;
    public float maxLength = 3.2f;
    public float minGap    = 0.15f;
    public float maxGap    = 0.45f;

    public int   prewarmCount = 3;
    public int   maxAlive     = 6;
    public float startDelay   = 0f;

    readonly List<GameObject> alive = new();
    float timer;

    void OnEnable()
    {
        if (!ladderPrefab || ladderPrefab.scene.IsValid())
        { Debug.LogError($"{name}: ladderPrefab fehlt oder ist Scene-Objekt."); enabled = false; return; }
        if (!surfaceRef)
        { Debug.LogError($"{name}: surfaceRef fehlt."); enabled = false; return; }

        timer = startDelay;

        float topY = GetTopY(surfaceRef);
        for (int i = 0; i < prewarmCount; i++)
        {
            float f = (i + 1f) / (prewarmCount + 1f);
            float x = Mathf.Lerp(leftToRight ? xMin : xMax, leftToRight ? xMax : xMin, f);
            SpawnAt(x, topY, RandomLength());
        }
    }

    void Update()
    {
        CleanupDead();
        timer -= Time.deltaTime;
        if (timer <= 0f && alive.Count < maxAlive)
        {
            float len = RandomLength();
            float gap = Random.Range(minGap, maxGap);
            float nextDelay = (len + gap) / Mathf.Max(0.01f, speed);

            SpawnAt(leftToRight ? xMin : xMax, GetTopY(surfaceRef), len);
            timer = Mathf.Max(0.15f, nextDelay);
        }
    }

    void SpawnAt(float x, float topY, float length)
    {
        var go = Instantiate(ladderPrefab);
        go.layer = LayerMask.NameToLayer(ladderLayerName);
        go.transform.position = new Vector3(x, topY, zPos);

        var plat = go.GetComponent<LadderPlatform>();
        if (plat) plat.size = new Vector3(length, plat.size.y, Mathf.Max(plat.size.z, 1.0f)); // Tiefe ≥ 1.0

        var mover = go.GetComponent<LadderMover>() ?? go.AddComponent<LadderMover>();
        mover.speed = speed * (leftToRight ? 1f : -1f);
        mover.xMin  = Mathf.Min(xMin, xMax) - 2f;
        mover.xMax  = Mathf.Max(xMin, xMax) + 2f;

        alive.Add(go);
    }

    float RandomLength() => Random.Range(minLength, maxLength);

    void CleanupDead()
    {
        for (int i = alive.Count - 1; i >= 0; i--)
            if (!alive[i]) alive.RemoveAt(i);
    }

    float GetTopY(Transform t)
    {
        float top = t.position.y;
        foreach (var r in t.GetComponentsInChildren<Renderer>()) top = Mathf.Max(top, r.bounds.max.y);
        foreach (var c in t.GetComponentsInChildren<Collider>()) top = Mathf.Max(top, c.bounds.max.y);
        return top;
    }
}
