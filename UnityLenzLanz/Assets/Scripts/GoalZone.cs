using UnityEngine;

public class GoalZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        var fc = other.GetComponentInParent<FigureControl>();
        if (fc) fc.ResetToStart();
    }
}