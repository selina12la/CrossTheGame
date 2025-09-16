using UnityEngine;

public class DiceCollectible : MonoBehaviour
{
    public int value = 1;

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<FigureControl>() != null)
        {
            GameSession.AddScore(value);
            Destroy(gameObject);
        }
    }
}