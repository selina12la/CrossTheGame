using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Board Settings")]
    public int width = 8;
    public int height = 8;
    public Vector2Int startCell = new Vector2Int(0, 0);
    public Vector2Int goalCell = new Vector2Int(7, 7);
    public float cellSize = 1f;
    public Vector3 origin = Vector3.zero;

    [Header("Refs")]
    public BoardGenerator boardGenerator;
    public ObstacleManager obstacleManager;
    public PlayerControllerGrid player;

    private void Awake()
    {
        if (!boardGenerator) boardGenerator = GetComponentInChildren<BoardGenerator>();
        if (!obstacleManager) obstacleManager = GetComponentInChildren<ObstacleManager>();
        if (!player) player = FindAnyObjectByType<PlayerControllerGrid>();
    }

    private void Start()
    {
        boardGenerator.BuildBoard(this);
        obstacleManager.BuildObstacles(this);
        
        if (player)
        {
            player.Init(this, startCell);
        }
    }

    public bool InBounds(Vector2Int cell)
    {
        return cell.x >= 0 && cell.y >= 0 && cell.x < width && cell.y < height;
    }

    public Vector3 CellToWorld(Vector2Int cell)
    {
        return origin + new Vector3(cell.x * cellSize, 0f, cell.y * cellSize);
    }
}