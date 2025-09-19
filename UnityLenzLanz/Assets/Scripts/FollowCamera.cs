using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;
    public GameManager gm;

    public Vector3 offset = new Vector3(0f, 5f, -6f);
    public float tiltDeg = 55f;
    public float fov = 20f;
    public float lookAhead = 3f;

    public float smooth = 8f;
    public bool forwardOnly = true;

    public float sidePaddingCells = 1.5f;

    float _maxZ;
    Vector3 _vector3;
    Vector3 _latestTargetPosition;

    void Start()
    {
        if (!target) return;

        _maxZ = target.position.z;
        _latestTargetPosition = target.position;

        transform.rotation = Quaternion.Euler(tiltDeg, 0f, 0f);
        var cam = GetComponent<FollowCamera>();
        if (cam) cam.fov = fov;

        SnapToTarget();
    }

    void LateUpdate()
    {
        if (!target) return;

        if (target.position.z < _latestTargetPosition.z - 1.0f)
        {
            _maxZ = target.position.z;
            SnapToTarget();
        }

        _latestTargetPosition = target.position;

        Vector3 focus = target.position + Vector3.forward * lookAhead;

        if (forwardOnly)
        {
            if (target.position.z > _maxZ) _maxZ = target.position.z;
            focus.z = _maxZ + lookAhead;
        }

        Vector3 desired = focus + offset;

        if (gm)
        {
            float minX = gm.origin.x + gm.cellSize * (0.5f + sidePaddingCells);
            float maxX = gm.origin.x + gm.cellSize * (gm.width - 0.5f - sidePaddingCells);
            desired.x = Mathf.Clamp(desired.x, minX, maxX);
        }

        transform.position =
            Vector3.SmoothDamp(transform.position, desired, ref _vector3, 1f / Mathf.Max(0.001f, smooth));

        Quaternion targetRot =
            Quaternion.LookRotation((target.position + Vector3.forward * lookAhead) - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 1f - Mathf.Exp(-smooth * Time.deltaTime));
    }

    void SnapToTarget()
    {
        Vector3 focus = target.position + Vector3.forward * lookAhead;
        Vector3 desired = focus + offset;

        if (gm)
        {
            float minX = gm.origin.x + gm.cellSize * (0.5f + sidePaddingCells);
            float maxX = gm.origin.x + gm.cellSize * (gm.width - 0.5f - sidePaddingCells);
            desired.x = Mathf.Clamp(desired.x, minX, maxX);
        }

        transform.position = desired;
        transform.rotation = Quaternion.LookRotation((target.position + Vector3.forward * lookAhead) - transform.position, Vector3.up);
        _vector3 = Vector3.zero;
    }
}