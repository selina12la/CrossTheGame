using UnityEngine;

public class Collectible : MonoBehaviour
{
    public int value = 1;

    public void Collect()
    {
        GameSession.AddScore(value);
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerMarker>()) Collect();
    }
}