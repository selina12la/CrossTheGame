using UnityEngine;

public class SnakesLaddersBoardGenerator : MonoBehaviour
{
    public Vector2 boardSize = new(15f, 12f);
    public int cols = 10, rows = 10;
    public float baseThickness = 0.1f;
    public Color baseColor = new(0.25f,0.20f,0.15f);
    public Color a = new(0.9f,0.9f,0.9f), b = new(0.15f,0.15f,0.15f);
    public float yOffset = 0.003f;

    void Start(){ Build(); }

    void Build()
    {
        for (int i = transform.childCount-1; i>=0; i--) DestroyImmediate(transform.GetChild(i).gameObject);

        var baseGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseGO.name="BoardBase";
        baseGO.transform.SetParent(transform,false);
        baseGO.transform.localScale = new Vector3(boardSize.x, baseThickness, boardSize.y);
        baseGO.transform.localPosition = new Vector3(boardSize.x*0.5f, -baseThickness*0.5f, boardSize.y*0.5f);
        baseGO.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit")){color=baseColor};

        float cw = boardSize.x/cols, ch = boardSize.y/rows;
        for (int r=0; r<rows; r++)
        for (int c=0; c<cols; c++)
        {
            var tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tile.name = $"Cell_{c}_{r}";
            tile.transform.SetParent(transform,false);
            tile.transform.localScale = new Vector3(cw, 0.01f, ch);
            tile.transform.localPosition = new Vector3(cw*(c+0.5f), yOffset, ch*(r+0.5f));
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = ((c+r)&1)==0 ? a : b;
            tile.GetComponent<Renderer>().sharedMaterial = mat;
            DestroyImmediate(tile.GetComponent<Collider>()); // nur optisch
        }
    }
}