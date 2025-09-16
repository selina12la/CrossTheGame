using System;
using UnityEngine;

public class StoneHazard : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<FigureControl>();
        if (player != null)
        {
            GameSession.LoseLife();
            player.ResetToStart();
        }
    }
}