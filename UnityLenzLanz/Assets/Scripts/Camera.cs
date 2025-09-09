using UnityEngine;

public class Camera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 12f, -10f);
    public float smooth = 8f;
    public float lookAhead = 3f;
    public bool forwardOnly = true;

    float maxZ;

    void Start()
    {
        if (target) maxZ = target.position.z;
    }

    void LateUpdate()
    {
        if (!target) return;

        Vector3 focus = target.position + new Vector3(0f, 0f, lookAhead);

        if (forwardOnly)
        {
            if (target.position.z > maxZ) maxZ = target.position.z;
            focus.z = maxZ + lookAhead;
        }

        Vector3 desired = focus + offset;
        float k = 1f - Mathf.Exp(-smooth * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desired, k);

        transform.LookAt(target.position + new Vector3(0f, 0f, lookAhead));
    }
}