using UnityEngine;

public class BackgammonBoardGenerator : MonoBehaviour
{
    public Vector2 boardSize = new Vector2(15f, 11f);
    public float pointWidth = 0f;      // 0 = auto (boardSize.x/12)
    public float pointLength = 4f;
    public float baseThickness = 0.1f;
    public float yOffset = 0.002f;
    public Color topA = new(0.85f,0.85f,0.85f);
    public Color topB = new(0.2f,0.2f,0.2f);
    public Color botA = new(0.85f,0.85f,0.85f);
    public Color botB = new(0.2f,0.2f,0.2f);
    public Color baseColor = new(0.45f,0.30f,0.2f);

    void Start(){ Build(); }

    void Build()
    {
        for (int i = transform.childCount-1; i>=0; i--) DestroyImmediate(transform.GetChild(i).gameObject);
        var baseGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseGO.name="BoardBase";
        baseGO.transform.SetParent(transform,false);
        baseGO.transform.localScale = new Vector3(boardSize.x, baseThickness, boardSize.y);
        baseGO.transform.localPosition = new Vector3(boardSize.x*0.5f, -baseThickness*0.5f, boardSize.y*0.5f);
        var br = baseGO.GetComponent<Renderer>();
        br.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        br.sharedMaterial.color = baseColor;

        float w = (pointWidth<=0f)? boardSize.x/12f : pointWidth;
        float padX = (boardSize.x - 12f*w)*0.5f;
        float topZ = boardSize.y - 0.5f;
        float botZ = 0.5f;

        for (int i = 0; i < 24; i++)
        {
            bool top = i < 12;
            int idx = i % 12;
            float x = padX + idx*w + w*0.5f;
            var col = ((idx & 1)==0) ? (top?topA:botA) : (top?topB:botB);
            CreateTriangle(new Vector3(x, yOffset, top? topZ: botZ),
                           top? Vector3.back : Vector3.forward,
                           w, pointLength, col, (top?"Top":"Bot")+"_P"+idx);
        }
    }

    void CreateTriangle(Vector3 localCenter, Vector3 dir, float width, float length, Color col, string name)
    {
        var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        go.transform.SetParent(transform,false);
        go.transform.localPosition = localCenter;

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); mat.color = col;
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;

        float hw = width*0.5f;
        var m = new Mesh();
        m.vertices = new Vector3[] { new(-hw,0,0), new(hw,0,0), dir.normalized*length };
        m.triangles = new int[] { 0,1,2 };
        m.RecalculateNormals();
        go.GetComponent<MeshFilter>().sharedMesh = m;
    }
}
