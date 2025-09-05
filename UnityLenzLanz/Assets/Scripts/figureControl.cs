using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class figureControl : MonoBehaviour
{
    [SerializeField] float stepSize = 1f;           
    [SerializeField] float moveDuration = 0.14f;    
    [SerializeField] float jumpHeight = 0.45f;      
    [SerializeField] float repeatDelay = 0.22f;     
    [SerializeField] float repeatRate = 0.08f;      
    [SerializeField] LayerMask groundMask;
    [SerializeField] float groundSnap = 0.02f;     
 
    [SerializeField] bool faceMoveDirection = true; 
    [SerializeField] float rotateSpeed = 22f;       
    
    [SerializeField] Vector3 resetPosition = new Vector3(1f, 3f, 1f);


    private FigureInputAction inputAction;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction resetAction;


    bool isMoving;
    Vector3 gridForward = Vector3.forward; 
    float holdTimer;
    float repeatTimer;
    Vector2 lastHeldDir; 

    void Awake()
    {
        inputAction = new FigureInputAction();
        moveAction  = inputAction.Figure.Move;
        jumpAction  = inputAction.Figure.Jump;
        resetAction = inputAction.Figure.Reset;
    }

    void OnEnable()
    {
        inputAction.Enable();
        jumpAction.started  += OnJump;   
        resetAction.started += OnReset;
    }

    void OnDisable()
    {
        jumpAction.started  -= OnJump;
        resetAction.started -= OnReset;
        inputAction.Disable();
    }

    void Update()
    {
        HandleMovementInput(Time.deltaTime);
        
        if (faceMoveDirection && _targetFacing.HasValue)
        {
            var targetRot = Quaternion.LookRotation(_targetFacing.Value, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }
    }

  void HandleMovementInput(float dt)
    {
        var analog = moveAction.ReadValue<Vector2>();
        var digital = ToCardinal(analog, 0.35f); 

        if (digital == Vector2.zero)
        {
            holdTimer = 0f;
            repeatTimer = 0f;
            lastHeldDir = Vector2.zero;
            return;
        }

        bool firstPressNewDirection = lastHeldDir != digital;
        lastHeldDir = digital;

        if (!isMoving)
        {
            if (firstPressNewDirection)
            {
                TryStartMove(digital);
                holdTimer = 0f;
                repeatTimer = 0f;
                return;
            }

            // Auto-Repeat bei gehaltener Richtung
            holdTimer += dt;
            if (holdTimer >= repeatDelay)
            {
                repeatTimer += dt;
                if (repeatTimer >= repeatRate)
                {
                    if (TryStartMove(digital))
                        repeatTimer = 0f;
                }
            }
        }
    }

    // Richtung erzwingen: nur X oder Y (größerer Betrag gewinnt)
    static Vector2 ToCardinal(Vector2 inVec, float deadzone)
    {
        if (inVec.magnitude < deadzone) return Vector2.zero;

        if (Mathf.Abs(inVec.x) > Mathf.Abs(inVec.y))
            return new Vector2(Mathf.Sign(inVec.x), 0f);
        else
            return new Vector2(0f, Mathf.Sign(inVec.y));
    }

    // ————— Ein Hop starten ———————————————————————————————————————

    bool TryStartMove(Vector2 dir2D)
    {
        if (isMoving) return false;

        Vector3 dir;
        if (dir2D.x != 0) dir = (dir2D.x > 0) ? Vector3.right : Vector3.left;
        else              dir = (dir2D.y > 0) ? gridForward    : -gridForward;

        var target = SnapToGrid(transform.position) + dir * stepSize;

        // Optional: hier Kollision/Block prüfen (Raycast o.ä.). Einfaches Beispiel:
        if (Physics.CheckBox(target + Vector3.up * 0.5f, new Vector3(0.4f, 0.49f, 0.4f), Quaternion.identity, ~0, QueryTriggerInteraction.Ignore))
        {
            // Blockiert -> nicht bewegen
            return false;
        }

        if (faceMoveDirection) _targetFacing = dir;

        StartCoroutine(HopTo(target));
        return true;
    }

    Vector3 SnapToGrid(Vector3 p)
    {
        // X/Z auf Step runden, Y so lassen
        float x = Mathf.Round(p.x / stepSize) * stepSize;
        float z = Mathf.Round(p.z / stepSize) * stepSize;
        return new Vector3(x, p.y, z);
    }

    Vector3? _targetFacing;

    IEnumerator HopTo(Vector3 targetPos)
    {
        isMoving = true;
        Vector3 start = transform.position;
        float baseY = start.y;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            float tt = Mathf.Clamp01(t);

            // Parabel y(t) = 4h * t * (1 - t)
            float y = baseY + 4f * jumpHeight * tt * (1f - tt);

            // Linear Interp auf XZ, Parabel auf Y
            Vector3 pos = Vector3.Lerp(start, new Vector3(targetPos.x, baseY, targetPos.z), tt);
            pos.y = y;
            transform.position = pos;

            yield return null;
        }
        transform.position = new Vector3(targetPos.x, baseY, targetPos.z);

        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out var hit, 3f, groundMask, QueryTriggerInteraction.Ignore))
        {
            transform.position = hit.point + Vector3.up * groundSnap;
        }

        isMoving = false;
    }

    // ————— Optional: Jump / Reset ——————————————————————————————————

    void OnJump(InputAction.CallbackContext ctx)
    {
        // In Crossy Road gibt es keinen "freien" Sprung, deshalb hier leer
        // Du könntest hier z.B. ein "Schnell-vorwärts" oder eine Spezialaktion auslösen.
    }

    void OnReset(InputAction.CallbackContext ctx)
    {
        transform.position = resetPosition;
        _targetFacing = null;
        isMoving = false;
    }
}
