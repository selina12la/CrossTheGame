using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int width = 15, height = 27;
    public int cellSize = 1;
    public Vector3 origin = Vector3.zero;
    public Vector2Int startCell = new(0,0), goalCell = new(7,26);

    public BoardGenerator boardGenerator;
    public ObstacleManager obstacleManager;

    void Start()
    {
        if (boardGenerator) boardGenerator.BuildBoard(this);
        if (obstacleManager) obstacleManager.BuildObstacles(this);
    }

    public Vector3 CellToWorld(Vector2Int c) => origin + new Vector3(c.x * cellSize + 0.5f * cellSize, 0f, c.y * cellSize + 0.5f * cellSize);
    public bool InBounds(Vector2Int c) => c.x >= 0 && c.x < width && c.y >= 0 && c.y < height;
}