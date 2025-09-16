using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class FigureControl : MonoBehaviour
{
    [SerializeField] float stepSize = 1f;
    [SerializeField] float moveDuration = 0.14f;
    [SerializeField] float jumpHeight = 0.45f;

    [SerializeField] float repeatDelay = 0.22f;

    [SerializeField] float repeatRate = 0.08f;

    [SerializeField] LayerMask groundMask;
    [SerializeField] float groundSnap = 0.02f;
    [SerializeField] float groundRayHeight = 5f;

    [SerializeField] LayerMask obstacleMask;
    [SerializeField] float checkPadding = 0.05f;

    [SerializeField] LayerMask collectibleMask;
    [SerializeField] GameManager gameManager;

    [SerializeField] bool placeOnStart = true;
    [SerializeField] bool lockRotation = true;

    public bool IsMoving => isMoving;
    public void ResetToStart() => DoResetToStart();

    FigureInputAction ia;
    InputAction moveAction, resetAction;
    Collider selfCol;

    bool isMoving;
    Quaternion initialRot;
    Vector2 lastHeldDir;
    float holdTimer, repeatTimer;

    Vector3 startWorld;
    Vector2Int startCell;

    void Awake()
    {
        ia = new FigureInputAction();
        moveAction = ia.Figure.Move;
        resetAction = ia.Figure.Reset;

        selfCol = GetComponent<Collider>();
        initialRot = transform.rotation;

        if (!gameManager) gameManager = FindAnyObjectByType<GameManager>();
    }

    void OnEnable() => ia.Enable();
    void OnDisable() => ia.Disable();

    void Start()
    {
        if (gameManager && placeOnStart)
        {
            startCell = gameManager.startCell;
            startWorld = Grounded(gameManager.CellToWorld(startCell));
            transform.position = startWorld;
        }
        else
        {
            startWorld = Grounded(transform.position);
            transform.position = startWorld;
        }
    }

    void Update()
    {
        if (lockRotation) transform.rotation = initialRot;
        if (isMoving) return;

        Vector2 dir = moveAction.ReadValue<Vector2>();
        if (dir != Vector2.zero)
        {
            dir = SnapToCardinal(dir);
            if (dir != lastHeldDir)
            {
                lastHeldDir = dir;
                holdTimer = 0f;
                repeatTimer = 0f;
                TryStep(dir);
            }
            else
            {
                holdTimer += Time.deltaTime;
                if (holdTimer >= repeatDelay)
                {
                    repeatTimer += Time.deltaTime;
                    while (repeatTimer >= repeatRate)
                    {
                        TryStep(dir);
                        repeatTimer -= repeatRate;
                    }
                }
            }
        }
        else
        {
            lastHeldDir = Vector2.zero;
            holdTimer = repeatTimer = 0f;
        }

        if (resetAction.triggered) DoResetToStart();
    }

    void TryStep(Vector2 dir)
    {
        Vector3 worldDir = new Vector3(dir.x, 0f, dir.y);
        Vector3 start = transform.position;
        Vector3 end = start + worldDir * stepSize;

        if (!Physics.Raycast(end + Vector3.up * groundRayHeight, Vector3.down,
                out var hit, groundRayHeight * 2f, groundMask, QueryTriggerInteraction.Collide))
            return;

        end.y = hit.point.y + groundSnap;
        if (IsBlockedAt(end)) return;

        StartCoroutine(HopTo(start, end));
    }

    bool IsBlockedAt(Vector3 worldPos)
    {
        if (obstacleMask.value == 0) return false;
        Bounds b = selfCol.bounds;
        Vector3 center = new Vector3(worldPos.x, b.center.y, worldPos.z);
        Vector3 half = b.extents - Vector3.one * checkPadding;
        half.x = Mathf.Max(0.01f, half.x);
        half.y = Mathf.Max(0.01f, half.y);
        half.z = Mathf.Max(0.01f, half.z);

        var hits = Physics.OverlapBox(center, half, Quaternion.identity,
            obstacleMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++)
            if (hits[i] && hits[i] != selfCol)
                return true;
        return false;
    }

    IEnumerator HopTo(Vector3 start, Vector3 end)
    {
        isMoving = true;
        float t = 0f;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            float k = t / moveDuration;
            float h = Mathf.Sin(Mathf.PI * k) * jumpHeight;
            transform.position = Vector3.Lerp(start, end, k) + Vector3.up * h;
            yield return null;
        }

        transform.position = end;
        if (lockRotation) transform.rotation = initialRot;
        isMoving = false;

        CollectAt(end);
    }

    void CollectAt(Vector3 worldPos)
    {
        if (!selfCol || collectibleMask.value == 0) return;
        Bounds b = selfCol.bounds;
        Vector3 center = new Vector3(worldPos.x, b.center.y, worldPos.z);
        Vector3 half = b.extents;

        var hits = Physics.OverlapBox(center, half, Quaternion.identity,
            collectibleMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < hits.Length; i++)
        {
            if (!hits[i]) continue;
            var dc = hits[i].GetComponentInParent<DiceCollectible>();
            if (dc)
            {
                GameSession.AddScore(dc.value);
                Destroy(dc.gameObject);
            }
            else
            {
                GameSession.AddScore(1);
                Destroy(hits[i].gameObject);
            }
        }
    }

    void DoResetToStart()
    {
        StopAllCoroutines();
        isMoving = false;
        if (gameManager)
            transform.position = Grounded(gameManager.CellToWorld(startCell));
        else
            transform.position = startWorld;
    }

    Vector2 SnapToCardinal(Vector2 v)
    {
        if (Mathf.Abs(v.x) > Mathf.Abs(v.y)) return new Vector2(Mathf.Sign(v.x), 0f);
        else return new Vector2(0f, Mathf.Sign(v.y));
    }

    Vector3 Grounded(Vector3 xz)
    {
        Vector3 from = xz + Vector3.up * groundRayHeight;
        if (Physics.Raycast(from, Vector3.down, out var hit,
                groundRayHeight * 2f, groundMask, QueryTriggerInteraction.Collide))
            return new Vector3(xz.x, hit.point.y + groundSnap, xz.z);
        return xz;
    }
}