using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject shipTilePrefab;
    public GameObject hitMissTilePrefab;
    public int width = 10;
    public int height = 10;
    public float spacing = 1.1f;
    private ShipTile[,] shipTiles;
    private HitMissTile[,] hitMissTiles;
    public Vector3 tileScale;
    Vector3 myPosition;

    void Start()
    {
        shipTiles = new ShipTile[width, height];
        hitMissTiles = new HitMissTile[width, height];
        myPosition = this.transform.position;
        GenerateShipGrid();
        GenerateHitMissGrid();
    }

    void GenerateShipGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
               
                GameObject newTile = Instantiate(shipTilePrefab, new Vector3((myPosition.x + (x * spacing * tileScale.x)), (myPosition.y + 0), (myPosition.z + (y * spacing * tileScale.z))), Quaternion.identity);
                newTile.transform.parent = this.transform;

                newTile.name = $"ShipTile {x} {y}";

                newTile.transform.localScale = tileScale;

                ShipTile tileComponent = newTile.GetComponent<ShipTile>();
                if (tileComponent != null)
                {
                    tileComponent.x = x;
                    tileComponent.y = y;
                    shipTiles[x, y] = tileComponent;
                }

            }
        }
    }
    void GenerateHitMissGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {

                GameObject newTile = Instantiate(hitMissTilePrefab, new Vector3((myPosition.x + (11 * tileScale.x) + (x * spacing * tileScale.x)), (myPosition.y + 0), (myPosition.z + (y * spacing * tileScale.z))), Quaternion.identity);
                newTile.transform.parent = this.transform;

                newTile.name = $"HitMissTile {x} {y}";

                newTile.transform.localScale = tileScale;

                HitMissTile tileComponent = newTile.GetComponent<HitMissTile>();
                if (tileComponent != null)
                {
                    tileComponent.x = x;
                    tileComponent.y = y;
                    hitMissTiles[x, y] = tileComponent;
                }

            }
        }
    }
    public HitMissTile GetHitMissTileAt(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return hitMissTiles[x, y];
        }
        return null;
    }
    public ShipTile GetShipTileAt(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return shipTiles[x, y];
        }
        return null;
    }
}
