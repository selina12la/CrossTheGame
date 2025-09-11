using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    public GameObject obstaclePrefab;                 
    public int obstacleCount = 25;
    public Vector2Int minCell = new Vector2Int(0, 0);
    public Vector2Int maxCell = new Vector2Int(14, 14);

    public HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();

    public void BuildObstacles(GameManager gm)
    {
        foreach (Transform c in transform) if (c != obstaclePrefab?.transform) DestroyImmediate(c.gameObject);
        occupied.Clear();
        occupied.Add(gm.startCell);
        occupied.Add(gm.goalCell);

        int placed = 0, tries = obstacleCount * 20;
        var rng = new System.Random();
        while (placed < obstacleCount && tries-- > 0)
        {
            int x = rng.Next(minCell.x, maxCell.x + 1);
            int y = rng.Next(minCell.y, maxCell.y + 1);
            var cell = new Vector2Int(x, y);
            if (occupied.Contains(cell)) continue;

            var go = Instantiate(obstaclePrefab, transform);
            go.transform.position = gm.CellToWorld(cell) + Vector3.up * 0.5f; 
            go.name = $"Obstacle_{x}_{y}";
            go.SetActive(true);

            occupied.Add(cell);
            placed++;
        }
    }

    public bool IsBlocked(Vector2Int cell) => occupied.Contains(cell);
}