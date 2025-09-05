using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    public GameObject tilePrefab;     // flacher Cube (scale.y ~ 0.1)
    public Material lightMat;
    public Material darkMat;

    public void BuildBoard(GameManager gm)
    {
        // Aufräumen (falls neu gebaut wird)
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        for (int y = 0; y < gm.height; y++)
        {
            for (int x = 0; x < gm.width; x++)
            {
                var tile = Instantiate(tilePrefab, transform);
                tile.transform.position = gm.CellToWorld(new Vector2Int(x, y));
                tile.transform.localScale = new Vector3(gm.cellSize, tile.transform.localScale.y, gm.cellSize);

                var rend = tile.GetComponent<Renderer>();
                if (rend)
                {
                    bool isLight = (x + y) % 2 == 0;
                    rend.sharedMaterial = isLight ? lightMat : darkMat;
                }
            }
        }
    }
}