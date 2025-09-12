using UnityEngine;
using TMPro;

public class SnakesNumbers : MonoBehaviour
{
    [Header("Board")]
    public Transform surfaceRef;     // dein SnakesBoard-Transform (für Y-Höhe)
    public int width = 10;
    public int height = 10;
    public float cellSize = 1f;
    public Vector3 origin = Vector3.zero;

    [Header("Look")]
    public float yOffset = 0.02f;           // leicht über der Oberfläche
    public Vector2 cornerOffset = new(0.35f, 0.35f); // „oben rechts“ in der Zelle (in Zellen-Einheiten)
    public float fontSize = 0.25f;
    public Color color = Color.black;
    public bool serpentine = true;          // 1..10 links→rechts, 11..20 rechts→links, usw.
    public bool drawTopToBottom = false;    // klassisch: unten = 1 (false)

    [ContextMenu("Build Numbers")]
    public void BuildNumbers()
    {
        ClearChildren();

        float baseY = surfaceRef ? surfaceRef.position.y : 0f;
        int number = 1;

        for (int row = 0; row < height; row++)
        {
            int y = drawTopToBottom ? (height - 1 - row) : row;

            // x-Laufrichtung
            bool leftToRight = !serpentine || (row % 2 == 0);
            for (int colIter = 0; colIter < width; colIter++)
            {
                int x = leftToRight ? colIter : (width - 1 - colIter);

                // Weltposition Feldmitte
                Vector3 cellCenter = origin + new Vector3((x + 0.5f) * cellSize, 0f, (y + 0.5f) * cellSize);

                // „oben rechts“ relativ zur Feldmitte
                Vector3 corner = cellCenter + new Vector3((cornerOffset.x - 0.5f) * cellSize,
                                                          yOffset,
                                                          (cornerOffset.y - 0.5f) * cellSize);

                // 3D-TMP Text
                var go = new GameObject($"Num_{number}", typeof(TextMeshPro));
                go.transform.SetParent(transform, worldPositionStays: false);
                go.transform.position = new Vector3(corner.x, baseY + yOffset, corner.z);
                go.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // flach aufs Brett

                var tmp = go.GetComponent<TextMeshPro>();
                tmp.text = number.ToString();
                tmp.fontSize = fontSize;
                tmp.color = color;
                tmp.alignment = TextAlignmentOptions.TopRight;
                tmp.enableWordWrapping = false;

                number++;
            }
        }
    }

    [ContextMenu("Clear")]
    public void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (Application.isEditor) DestroyImmediate(transform.GetChild(i).gameObject);
            else Destroy(transform.GetChild(i).gameObject);
        }
    }
}
