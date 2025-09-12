using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class FigureControl : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] float stepSize = 1f;
    [SerializeField] float moveDuration = 0.14f;
    [SerializeField] float jumpHeight = 0.45f;
    [SerializeField] float axisThreshold = 0.5f;
    [SerializeField] bool requireZeroBeforeNextStep = true;

    [Header("Layers")]
    [SerializeField] LayerMask obstacleMask;
    [SerializeField] LayerMask ladderMask;
    [SerializeField] LayerMask groundMask;

    [Header("Checks")]
    [SerializeField] float groundRayHeight = 10f;
    [SerializeField] float checkPadding = 0.05f;
    [SerializeField] bool useGameManagerBounds = true;
    [SerializeField] Vector2[] riverRanges = { new Vector2(26f, 31f), new Vector2(32f, 37f) };

    [Header("Misc")]
    [SerializeField] bool placeOnStart = true;
    [SerializeField] bool lockRotation = true;
    [SerializeField] GameManager gameManager;

    public bool IsMoving { get; private set; }

    FigureInputAction ia;
    InputAction moveAction;
    Collider selfCol;
    bool isMoving, inputConsumed;
    Vector2Int cellPos;
    Quaternion initialRot;

    void Awake()
    {
        ia = new FigureInputAction(); ia.Enable();
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
        else
        {
            float gy = GroundYAt(transform.position);
            transform.position = new Vector3(transform.position.x, gy, transform.position.z);
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
        Vector3 end; Vector2Int targetCell;

        if (gameManager)
        {
            targetCell = cellPos + dir;
            if (useGameManagerBounds && !gameManager.InBounds(targetCell)) return;
            Vector3 c = gameManager.CellToWorld(targetCell);
            float gy = GroundYAt(c);
            end = new Vector3(c.x, gy, c.z);
        }
        else
        {
            Vector3 flat = transform.position + new Vector3(dir.x, 0f, dir.y) * stepSize;
            float gy = GroundYAt(flat);
            end = new Vector3(flat.x, gy, flat.z);
            targetCell = WorldToCell(end);
        }

        if (IsBlockedAt(end)) return;

        StartCoroutine(HopTo(targetCell, end));
    }

    IEnumerator HopTo(Vector2Int targetCell, Vector3 end)
    {
        isMoving = true; IsMoving = true;
        transform.SetParent(null, true);

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

        if (InRiver(end) && !OnLadderAt(end)) { Hit(); yield break; }

        cellPos = targetCell;
        transform.position = end;
        isMoving = false; IsMoving = false;
    }

    IEnumerator HopToWorld(Vector3 end)
    {
        isMoving = true; IsMoving = true;
        transform.SetParent(null, true);

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

        if (InRiver(end) && !OnLadderAt(end)) { Hit(); yield break; }

        transform.position = end;
        isMoving = false; IsMoving = false;
    }

    bool IsBlockedAt(Vector3 worldPos)
    {
        Bounds b = selfCol.bounds;
        Vector3 half = b.extents - Vector3.one * checkPadding;
        half.x = Mathf.Max(0.01f, half.x);
        half.y = Mathf.Max(0.01f, half.y);
        half.z = Mathf.Max(0.01f, half.z);

        float hh = b.extents.y;
        Vector3 center = new Vector3(worldPos.x, worldPos.y + hh, worldPos.z);

        var hits = Physics.OverlapBox(center, half, Quaternion.identity, obstacleMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++) if (hits[i] && hits[i] != selfCol) return true;
        return false;
    }

    bool InRiver(Vector3 p)
    {
        for (int i = 0; i < riverRanges.Length; i++)
            if (p.z >= riverRanges[i].x && p.z <= riverRanges[i].y) return true;
        return false;
    }

    bool OnLadderAt(Vector3 worldPos)
    {
        float checkHeight = 0.35f;                   // etwas über deiner Ladder-Höhe (0.3)
        float centerY = worldPos.y + checkHeight * 0.5f;

        Bounds b = selfCol.bounds;
        Vector3 half = b.extents - Vector3.one * checkPadding;
        half.x = Mathf.Max(0.05f, half.x);
        half.y = checkHeight * 0.5f;                 // flacher Boden-Check
        half.z = Mathf.Max(0.05f, half.z);

        Vector3 center = new Vector3(worldPos.x, centerY, worldPos.z);
        var hits = Physics.OverlapBox(center, half, Quaternion.identity, ladderMask, QueryTriggerInteraction.Collide);
        return hits != null && hits.Length > 0;
    }

    public void ResetToStart()
    {
        isMoving = false; IsMoving = false;
        if (gameManager) { cellPos = gameManager.startCell; SnapToCell(cellPos); }
        else             { float gy = GroundYAt(Vector3.zero); transform.position = new Vector3(0f, gy, 0f); }
        if (lockRotation) transform.rotation = initialRot;
    }

    public void Hit()
    {
        GameSession.LoseLife();     
        isMoving = false;
        ResetToStart();
    }

    void SnapToCell(Vector2Int cell)
    {
        Vector3 c = gameManager.CellToWorld(cell);
        float gy = GroundYAt(c);
        transform.position = new Vector3(c.x, gy, c.z);
    }

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

    float GroundYAt(Vector3 xz)
    {
        Vector3 from = new Vector3(xz.x, groundRayHeight, xz.z);
        var hits = Physics.RaycastAll(from, Vector3.down, groundRayHeight * 2f, groundMask, QueryTriggerInteraction.Ignore);
        float best = float.NegativeInfinity;
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider == selfCol) continue;
            if (hits[i].point.y > best) best = hits[i].point.y;
        }
        return (best == float.NegativeInfinity) ? 0f : best;
    }
}
