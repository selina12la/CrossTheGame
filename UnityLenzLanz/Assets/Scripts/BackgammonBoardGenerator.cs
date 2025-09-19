using UnityEngine;

public class BackgammonBoardGenerator : MonoBehaviour
{
    public Vector2 boardSize = new(15f, 11f);
    public float pointWidth = 0f;      // 0 = auto (boardSize.x / 12)
    public float pointLength = 4f;
    public float baseThickness = 0.1f;
    public float yOffset = 0.003f;

    public Material topAMaterial;
    public Material topBMaterial;
    public Material botAMaterial;
    public Material botBMaterial;
    public Material baseMaterial;

    void Start()
    {
        if (Application.isPlaying) Build();
    }

    public void Build()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        
        var baseGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseGO.name = "BoardBase";
        baseGO.transform.SetParent(transform, false);
        baseGO.transform.localScale = new Vector3(boardSize.x, baseThickness, boardSize.y);
        baseGO.transform.localPosition = new Vector3(boardSize.x * 0.5f, -baseThickness * 0.5f, boardSize.y * 0.5f);
        var br = baseGO.GetComponent<Renderer>();
        if (baseMaterial) br.sharedMaterial = baseMaterial;

        
        float w    = (pointWidth <= 0f) ? boardSize.x / 12f : pointWidth;
        float padX = (boardSize.x - 12f * w) * 0.5f;
        float topZ = boardSize.y - 0.5f;
        float botZ = 0.5f;

        for (int i = 0; i < 24; i++)
        {
            bool top = i < 12;
            int idx  = i % 12;
            float x  = padX + idx * w + w * 0.5f;

            Material mat = top
                ? (((idx & 1) == 0) ? topAMaterial : topBMaterial)
                : (((idx & 1) == 0) ? botAMaterial : botBMaterial);

            CreateTriangle(
                new Vector3(x, yOffset, top ? topZ : botZ),
                top ? Vector3.back : Vector3.forward,
                w, pointLength, mat, (top ? "Top" : "Bot") + "_P" + idx
            );
        }
    }

    void CreateTriangle(Vector3 localCenter, Vector3 dir, float width, float length, Material mat, string name)
    {
        var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        go.transform.SetParent(transform, false);
        go.transform.localPosition = localCenter;

        if (mat) go.GetComponent<MeshRenderer>().sharedMaterial = mat;

        float hw  = width * 0.5f;
        Vector3 tip = dir.normalized * length;

        var m = new Mesh();
        m.vertices  = new Vector3[] { new(-hw, 0, 0), new(hw, 0, 0), tip };
        m.triangles = (dir.z > 0f) ? new int[] { 1, 0, 2 } : new int[] { 0, 1, 2 };
        m.RecalculateNormals();

        go.GetComponent<MeshFilter>().sharedMesh = m;
    }
}
