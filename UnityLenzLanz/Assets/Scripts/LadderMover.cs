using UnityEngine;

public class LadderMover : MonoBehaviour
{
    public float speed = 4f;
    public float xMin = -5f;
    public float xMax = 20f;

    void Update()
    {
        var p = transform.position;
        p.x += speed * Time.deltaTime;
        transform.position = p;

        if (speed > 0 && p.x > xMax + 1f) Destroy(gameObject);
        if (speed < 0 && p.x < xMin - 1f) Destroy(gameObject);
    }
}