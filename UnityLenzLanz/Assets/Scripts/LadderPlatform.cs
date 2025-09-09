using UnityEngine;

[ExecuteAlways]
public class LadderPlatform : MonoBehaviour
{
    public Transform visual;             
    public Vector3 size = new Vector3(2.5f, 0.3f, 0.6f); 
    public Vector3 visualScaleMultiplier = Vector3.one;  

    BoxCollider box;

    void OnEnable()
    {
        if (!visual && transform.childCount > 0) visual = transform.GetChild(0);
        box = GetComponent<BoxCollider>();
        if (!box) box = gameObject.AddComponent<BoxCollider>();
        box.isTrigger = true;
        ApplySize();
    }

    void OnValidate() { if (enabled) ApplySize(); }

    void ApplySize()
    {
        if (!box) return;
        box.size = size;
        box.center = new Vector3(0f, size.y * 0.5f, 0f);
        if (visual)
        {
            // setze Visual so, dass es auf der Oberfläche „sitzt“
            visual.localPosition = new Vector3(0f, size.y * 0.5f, 0f);
            visual.localScale = Vector3.Scale(size, visualScaleMultiplier);
        }
    }
}