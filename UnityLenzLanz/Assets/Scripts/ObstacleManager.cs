using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    [Header("Obstacle Setup")]
    public GameObject obstaclePrefab;
    [Range(0, 100)]
    public int obstacleCount = 12; // für 8x8 okay – passe an

    public HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();

    public void BuildObstacles(GameManager gm)
    {
        occupied.Clear();

        // Start & Ziel sperren
        occupied.Add(gm.startCell);
        occupied.Add(gm.goalCell);

        int maxTries = obstacleCount * 10;
        int placed = 0;
        System.Random rng = new System.Random();

        while (placed < obstacleCount && maxTries-- > 0)
        {
            int x = rng.Next(0, gm.width);
            int y = rng.Next(0, gm.height);
            var cell = new Vector2Int(x, y);

            if (occupied.Contains(cell)) continue;

            // Hindernis setzen
            var obs = Instantiate(obstaclePrefab, transform);
            obs.transform.position = gm.CellToWorld(cell) + Vector3.up * 0.5f; // Cube mit y=1 Höhe
            obs.name = $"Obstacle_{cell.x}_{cell.y}";

            occupied.Add(cell);
            placed++;
        }
    }

    public bool IsBlocked(Vector2Int cell) => occupied.Contains(cell);
}