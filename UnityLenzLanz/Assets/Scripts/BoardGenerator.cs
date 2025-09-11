using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    [Header("Scene template (disabled Cube)")]
    public GameObject tilePrefab;

    [Header("Materials")]
    public Material lightMat;
    public Material darkMat;

    [Header("Board size (fixed chess area)")]
    public int boardWidth  = 15;
    public int boardHeight = 15;

    [ContextMenu("Build From GameManager")]
    public void BuildFromGameManager()
    {
        var gm = FindAnyObjectByType<GameManager>();
        if (gm) BuildBoard(gm);
        else Debug.LogError("[BoardGenerator] Kein GameManager gefunden.");
    }

    public void BuildBoard(GameManager gm)
    {
        if (!tilePrefab) { Debug.LogError("[BoardGenerator] tilePrefab fehlt."); return; }


        for (int i = transform.childCount - 1; i >= 0; i--) DestroyImmediate(transform.GetChild(i).gameObject);

        
        float tileH = tilePrefab.transform.localScale.y;
        Vector3 downHalf = new Vector3(0f, -tileH * 0.5f, 0f);

        int w = Mathf.Min(boardWidth,  gm.width);
        int h = Mathf.Min(boardHeight, gm.height);

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            GameObject t = Instantiate(tilePrefab, transform);
            t.transform.position   = gm.CellToWorld(new Vector2Int(x, y)) + downHalf;
            t.transform.localScale = new Vector3(gm.cellSize, tileH, gm.cellSize);
            var r = t.GetComponent<Renderer>();
            if (r)
                r.sharedMaterial = (((x + y) & 1) == 0) ? lightMat : darkMat;
            t.SetActive(true);
        }
    }
}