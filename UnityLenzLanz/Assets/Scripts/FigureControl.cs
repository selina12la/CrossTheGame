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
    [SerializeField] float repeatRate  = 0.08f;

    [SerializeField] LayerMask groundMask;
    [SerializeField] float groundSnap = 0.02f;
    [SerializeField] float groundRayHeight = 5f;

    [SerializeField] LayerMask obstacleMask;
    [SerializeField] LayerMask collectibleMask;

    [SerializeField] GameManager gameManager;
    [SerializeField] bool placeOnStart = true;

    public bool IsMoving => isMoving;
    public void ResetToStart() => DoResetToStart();

    FigureInputAction ia;
    InputAction moveAction, resetAction;
    Collider selfCol;

    bool isMoving;
    Vector2 lastHeldDir;
    float holdTimer, repeatTimer;

    Vector3 startWorld;
    Vector2Int startCell;

    float worldXMin, worldXMax, worldZMin, worldZMax, resetMargin;

    void Awake()
    {
        ia = new FigureInputAction();
        moveAction  = ia.Figure.Move;
        resetAction = ia.Figure.Reset;
        selfCol = GetComponent<Collider>();
        if (!gameManager) gameManager = FindAnyObjectByType<GameManager>();
    }

    void OnEnable()  => ia.Enable();
    void OnDisable() => ia.Disable();

    void Start()
    {
        if (gameManager)
        {
            float s = Mathf.Max(0.0001f, gameManager.cellSize);
            Vector3 o = gameManager.origin;
            worldXMin = o.x + 0.5f * s;
            worldXMax = o.x + (gameManager.width  - 0.5f) * s;
            worldZMin = o.z - 0.5f * s;
            worldZMax = o.z + (gameManager.height - 0.5f) * s;
            resetMargin = 0.05f * s;

            if (placeOnStart)
            {
                startCell  = gameManager.startCell;
                startWorld = Grounded(gameManager.CellToWorld(startCell));
                transform.position = startWorld;
            }
            else
            {
                startWorld = Grounded(transform.position);
                transform.position = startWorld;
            }
        }
        else
        {
            startWorld = Grounded(transform.position);
            transform.position = startWorld;
            worldXMin = worldXMax = worldZMin = worldZMax = float.NaN;
            resetMargin = 0.1f;
        }
    }

    void Update()
    {
        if (!float.IsNaN(worldXMin))
        {
            Vector3 p = transform.position;
            bool off =
                p.x < worldXMin - resetMargin ||
                p.x > worldXMax + resetMargin ||
                p.z < worldZMin - resetMargin ||
                p.z > worldZMax - resetMargin;     
            if (off)
            {
                GameSession.LoseLife();
                DoResetToStart();
                return;
            }
        }

        if (isMoving) return;

        Vector2 dir = moveAction.ReadValue<Vector2>();
        if (dir != Vector2.zero)
        {
            dir = Mathf.Abs(dir.x) > Mathf.Abs(dir.y)
                ? new Vector2(Mathf.Sign(dir.x), 0f)
                : new Vector2(0f, Mathf.Sign(dir.y));

            if (dir != lastHeldDir)
            {
                lastHeldDir = dir;
                holdTimer = 0f; repeatTimer = 0f;
                Step(dir);
            }
            else
            {
                holdTimer += Time.deltaTime;
                if (holdTimer >= repeatDelay)
                {
                    repeatTimer += Time.deltaTime;
                    while (repeatTimer >= repeatRate)
                    {
                        Step(dir);
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

    void DoResetToStart()
    {
        StopAllCoroutines();
        isMoving = false;

        if (gameManager)
        {
            Vector3 t = Grounded(gameManager.CellToWorld(startCell));
            transform.position = t;
        }
        else
        {
            transform.position = startWorld;
        }
    }

    void Step(Vector2 dir)
    {
        Vector3 start = transform.position;
        Vector3 end = start + new Vector3(dir.x, 0f, dir.y) * stepSize;

        if (!float.IsNaN(worldXMin))
        {
            if (end.x < worldXMin || end.x > worldXMax || end.z < worldZMin || end.z > worldZMax)
            {
                GameSession.LoseLife();
                DoResetToStart();
                return;
            }
        }

        if (Physics.CheckBox(end, Vector3.one * 0.45f, Quaternion.identity, obstacleMask, QueryTriggerInteraction.Ignore))
            return;

        if (Physics.Raycast(end + Vector3.up * groundRayHeight, Vector3.down, out var hit, groundRayHeight * 2f, groundMask, QueryTriggerInteraction.Collide))
        {
            end.y = hit.point.y + groundSnap;
            StartCoroutine(HopTo(start, end));
        }
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
        isMoving = false;
        CollectAt(end);
    }

    void CollectAt(Vector3 worldPos)
    {
        if (!selfCol) selfCol = GetComponent<Collider>();
        Bounds b = selfCol.bounds;
        Vector3 center = new Vector3(worldPos.x, b.center.y, worldPos.z);
        Vector3 half = b.extents;
        var hits = Physics.OverlapBox(center, half, Quaternion.identity, collectibleMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < hits.Length; i++)
        {
            if (!hits[i]) continue;
            var dc = hits[i].GetComponentInParent<DiceCollectible>();
            if (dc) { GameSession.AddScore(dc.value); Destroy(dc.gameObject); }
            else    { GameSession.AddScore(1);        Destroy(hits[i].gameObject); }
        }
    }

    Vector3 Grounded(Vector3 xz)
    {
        Vector3 from = xz + Vector3.up * groundRayHeight;
        if (Physics.Raycast(from, Vector3.down, out var hit, groundRayHeight * 2f, groundMask, QueryTriggerInteraction.Collide))
            return new Vector3(xz.x, hit.point.y + groundSnap, xz.z);
        return xz;
    }
}
