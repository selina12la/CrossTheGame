using UnityEngine;

public class Camera : MonoBehaviour
{
    public Transform target;
    public GameManager gm;              

    public Vector3 offset = new Vector3(0f, 8f, -10f); 
    public float tiltDeg = 55f;                        
    public float fov = 35f;                            
    public float lookAhead = 3f;                      

    public float smooth = 8f;                        
    public bool forwardOnly = true;

    public float sidePaddingCells = 1.5f;              

    float _maxZ;                                        
    Vector3 _vector3;                                       

    void Start()
    {
        if (!target) return;

        _maxZ = target.position.z;
        
        transform.rotation = Quaternion.Euler(tiltDeg, 0f, 0f);
        var cam = GetComponent<Camera>();
        if (cam) cam.fov = fov;
    }

    void LateUpdate()
    {
        if (!target) return;

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
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref _vector3, 1f / Mathf.Max(0.001f, smooth));

        Quaternion targetRot = Quaternion.LookRotation((target.position + Vector3.forward * lookAhead) - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 1f - Mathf.Exp(-smooth * Time.deltaTime));
    }
}