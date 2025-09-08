using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    public GameObject tilePrefab;    
    public Material lightMat;
    public Material darkMat;

    public void BuildBoard(GameManager gm)
    {
        for (int i = transform.childCount - 1; i >= 0; i--) DestroyImmediate(transform.GetChild(i).gameObject);

        for (int y = 0; y < gm.height; y++)
        for (int x = 0; x < gm.width; x++)
        {
            var tile = Instantiate(tilePrefab, transform);
            tile.transform.position = gm.CellToWorld(new Vector2Int(x, y));
            tile.transform.localScale = new Vector3(gm.cellSize, tile.transform.localScale.y, gm.cellSize);

            var r = tile.GetComponent<Renderer>();
            r.sharedMaterial = (((x + y) & 1) == 0) ? lightMat : darkMat;

            if (x == gm.startCell.x && y == gm.startCell.y) r.sharedMaterial.color = Color.black;
            if (x == gm.goalCell.x  && y == gm.goalCell.y)  r.sharedMaterial.color = Color.white;
        }
    }
}