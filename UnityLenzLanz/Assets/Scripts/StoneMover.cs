using UnityEngine;

public class StoneMover : MonoBehaviour
{
    public float speed = 6f;
    public float xMin = -10f, xMax = 30f;

    void Update()
    {
        transform.position += Vector3.right * speed * Time.deltaTime;
        if (transform.position.x < xMin || transform.position.x > xMax)
            Destroy(gameObject);
    }
}