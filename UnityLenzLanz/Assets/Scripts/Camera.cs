using UnityEngine;

public class Camera : MonoBehaviour
{
   [Header("Targets")]
    public Transform target;
    public GameManager gm;              // zum Clampen am Board (optional)

    [Header("Framing")]
    public Vector3 offset = new Vector3(0f, 8f, -10f); // Höhe/Abstand
    public float tiltDeg = 55f;                        // Neigung nach unten
    public float fov = 35f;                            // enger Blick (Crossy-Feeling)
    public float lookAhead = 3f;                       // etwas vor den Player schauen

    [Header("Movement")]
    public float smooth = 8f;                          // je höher, desto snappier
    public bool forwardOnly = true;

    [Header("Clamping (seitlich)")]
    public float sidePaddingCells = 1.5f;              // wie viele Zellen links/rechts nicht zeigen

    float maxZ;                                        // für forwardOnly
    Vector3 vel;                                       // SmoothDamp

    void Start()
    {
        if (!target) return;

        maxZ = target.position.z;

        // Tilt & FOV einmalig setzen
        transform.rotation = Quaternion.Euler(tiltDeg, 0f, 0f);
        var cam = GetComponent<Camera>();
        if (cam) cam.fov = fov;
    }

    void LateUpdate()
    {
        if (!target) return;

        // Fokuspunkt (ein Stück vor dem Spieler)
        Vector3 focus = target.position + Vector3.forward * lookAhead;

        // Nur nach vorne scrollen
        if (forwardOnly)
        {
            if (target.position.z > maxZ) maxZ = target.position.z;
            focus.z = maxZ + lookAhead;
        }

        // Wunschposition = Fokus + Offset (hinter/über dem Player)
        Vector3 desired = focus + offset;

        // Seitlich innerhalb des Boards halten
        if (gm)
        {
            float minX = gm.origin.x + gm.cellSize * (0.5f + sidePaddingCells);
            float maxX = gm.origin.x + gm.cellSize * (gm.width - 0.5f - sidePaddingCells);
            desired.x = Mathf.Clamp(desired.x, minX, maxX);
        }

        // Smooth bewegen
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref vel, 1f / Mathf.Max(0.001f, smooth));

        // Blick leicht nach vorne ausrichten (ebenfalls smooth)
        Quaternion targetRot = Quaternion.LookRotation((target.position + Vector3.forward * lookAhead) - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 1f - Mathf.Exp(-smooth * Time.deltaTime));
    }
}