using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class FigureControl : MonoBehaviour
{
    [SerializeField] float stepSize = 1f;
    [SerializeField] float moveDuration = 0.14f;
    [SerializeField] float jumpHeight = 0.45f;
    [SerializeField] float axisThreshold = 0.5f;
    [SerializeField] bool requireZeroBeforeNextStep = true;
    [SerializeField] LayerMask obstacleMask;
    [SerializeField] float checkPadding = 0.05f;
    [SerializeField] bool useGameManagerBounds = true;
    [SerializeField] bool placeOnStart = true;
    [SerializeField] bool lockRotation = true;
    [SerializeField] GameManager gameManager;

    FigureInputAction ia;
    InputAction moveAction;
    Collider selfCol;
    bool isMoving, inputConsumed;
    Vector2Int cellPos;
    Quaternion initialRot;

    void Awake()
    {
        ia = new FigureInputAction();
        ia.Enable();
        moveAction = ia.Figure.Move;
        selfCol = GetComponent<Collider>();
        if (!gameManager) gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager) stepSize = gameManager.cellSize;
        initialRot = transform.rotation;
    }

    void Start()
    {
        if (gameManager)
        {
            cellPos = placeOnStart ? gameManager.startCell : WorldToCell(transform.position);
            SnapToCell(cellPos);
        }
    }

    void Update()
    {
        if (lockRotation) transform.rotation = initialRot;
        if (isMoving) return;

        Vector2 input = moveAction.ReadValue<Vector2>();
        if (requireZeroBeforeNextStep)
        {
            if (Mathf.Abs(input.x) < 0.2f && Mathf.Abs(input.y) < 0.2f) inputConsumed = false;
            if (inputConsumed) return;
        }

        Vector2Int dir = Vector2Int.zero;
        if      (input.y >  axisThreshold) dir = Vector2Int.up;
        else if (input.y < -axisThreshold) dir = Vector2Int.down;
        else if (input.x < -axisThreshold) dir = Vector2Int.left;
        else if (input.x >  axisThreshold) dir = Vector2Int.right;
        if (dir == Vector2Int.zero) return;

        if (requireZeroBeforeNextStep) inputConsumed = true;
        TryStep(dir);
    }

    void TryStep(Vector2Int dir)
    {
        if (gameManager)
        {
            Vector2Int targetCell = cellPos + dir;
            if (useGameManagerBounds && !gameManager.InBounds(targetCell)) return;
            Vector3 end = gameManager.CellToWorld(targetCell) + Vector3.up * 0.5f;
            if (IsBlockedAt(end)) return;
            StartCoroutine(HopTo(targetCell, end));
        }
        else
        {
            Vector3 end = transform.position + new Vector3(dir.x, 0f, dir.y) * stepSize;
            if (IsBlockedAt(end)) return;
            StartCoroutine(HopToWorld(end));
        }
    }

    IEnumerator HopTo(Vector2Int targetCell, Vector3 end)
    {
        isMoving = true;
        Vector3 start = transform.position;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            float y = Mathf.Sin(t * Mathf.PI) * jumpHeight;
            Vector3 next = Vector3.Lerp(start, end, t) + Vector3.up * y;
            if (IsBlockedAt(next)) { Hit(); yield break; }
            transform.position = next;
            if (lockRotation) transform.rotation = initialRot;
            yield return null;
        }
        cellPos = targetCell;
        transform.position = end;
        isMoving = false;
    }

    IEnumerator HopToWorld(Vector3 end)
    {
        isMoving = true;
        Vector3 start = transform.position;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            float y = Mathf.Sin(t * Mathf.PI) * jumpHeight;
            Vector3 next = Vector3.Lerp(start, end, t) + Vector3.up * y;
            if (IsBlockedAt(next)) { Hit(); yield break; }
            transform.position = next;
            if (lockRotation) transform.rotation = initialRot;
            yield return null;
        }
        transform.position = end;
        isMoving = false;
    }

    bool IsBlockedAt(Vector3 worldPos)
    {
        Bounds b = selfCol.bounds;
        Vector3 center = new Vector3(worldPos.x, b.center.y, worldPos.z);
        Vector3 half = b.extents - Vector3.one * checkPadding;
        half.x = Mathf.Max(0.01f, half.x);
        half.y = Mathf.Max(0.01f, half.y);
        half.z = Mathf.Max(0.01f, half.z);
        var hits = Physics.OverlapBox(center, half, Quaternion.identity, obstacleMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++) if (hits[i] && hits[i] != selfCol) return true;
        return false;
    }

    public void Hit()
    {
        isMoving = false;
        if (gameManager)
        {
            cellPos = gameManager.startCell;
            SnapToCell(cellPos);
        }
        else
        {
            transform.position = Vector3.zero;
        }
        if (lockRotation) transform.rotation = initialRot;
    }

    void SnapToCell(Vector2Int cell) => transform.position = gameManager.CellToWorld(cell) + Vector3.up * 0.5f;

    Vector2Int WorldToCell(Vector3 worldPos)
    {
        Vector3 local = worldPos - gameManager.origin;
        float s = Mathf.Max(0.0001f, gameManager.cellSize);
        int x = Mathf.RoundToInt(local.x / s);
        int y = Mathf.RoundToInt(local.z / s);
        x = Mathf.Clamp(x, 0, gameManager.width - 1);
        y = Mathf.Clamp(y, 0, gameManager.height - 1);
        return new Vector2Int(x, y);
    }
}
