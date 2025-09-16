using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerControllerGrid : MonoBehaviour
{
    public float moveDuration = 0.12f;
    public float hopHeight = 0.35f;

    private GameManager _gm;
    private ObstacleManager _obstacles;
    private Vector2Int _cellPos;
    private bool _isMoving = false;

    public void Init(GameManager manager, Vector2Int startCell)
    {
        _gm = manager;
        _obstacles = _gm.obstacleManager;
        _cellPos = startCell;
        transform.position = _gm.CellToWorld(_cellPos) + Vector3.up * 0.5f;
    }

    private void Update()
    {
        if (_gm == null || _isMoving) return;

        Vector2Int dir = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) dir = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) dir = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) dir = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) dir = Vector2Int.right;

        if (dir != Vector2Int.zero)
        {
            TryStep(dir);
        }
    }

    private void TryStep(Vector2Int dir)
    {
        Vector2Int target = _cellPos + dir;

        if (!_gm.InBounds(target)) return;

        if (_obstacles != null && _obstacles.IsBlocked(target)) return;

        StartCoroutine(HopTo(target));
    }

    private IEnumerator HopTo(Vector2Int targetCell)
    {
        _isMoving = true;

        Vector3 start = transform.position;
        Vector3 end = _gm.CellToWorld(targetCell) + Vector3.up * 0.5f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            float yOffset = Mathf.Sin(t * Mathf.PI) * hopHeight;

            transform.position = Vector3.Lerp(start, end, t) + Vector3.up * yOffset;
            yield return null;
        }

        _cellPos = targetCell;
        transform.position = end;
        _isMoving = false;
        
        if (_cellPos == _gm.goalCell)
        {
            Debug.Log("Ziel erreicht! 🎉");
        }
    }
}
