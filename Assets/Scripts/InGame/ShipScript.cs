using System.Collections.Generic;
using UnityEngine;

public class ShipScript : MonoBehaviour
{
    public List<Vector2Int> occupiedTiles = new List<Vector2Int>();
    private int shipType;
    private List<Collider> tilesColliders = new List<Collider>();
    
    
    public void SetShipType(int type) { shipType = type;}
    public int GetShipType() { return shipType; }

    public void SetOccupiedTiles(List<Vector2Int> tiles)
    {
        occupiedTiles = tiles;
    }

    public void RemoveShip()
    {
        foreach (var tile in occupiedTiles)
        {
            ShipPlacementManager.Instance.ClearOccupiedTile(tile.x, tile.y);
            
        }
        
        ShipPlacementManager.Instance.RemoveShip(gameObject);

        Destroy(gameObject);
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButton(1))
        {
            RemoveShip();
        }
    }

    private void OnDisable()
    {
        if(tilesColliders.Count <= 0) return; 
        foreach (var tile in tilesColliders)
        {
            tile.GetComponent<ShipTile>().SetState(ShipTile.TileState.Empty);
        }
        
        tilesColliders.Clear();
    }

    //this is to get a preview of where the ship is going on the grid
    private void OnTriggerEnter(Collider other)
    {
        ShipTile tile = other.GetComponent<ShipTile>();

        if (tile != null)
        {
            tilesColliders.Add(other);
            tile.SetState(ShipTile.TileState.Hover);
        }
    }
    
    //remove the preview
    private void OnTriggerExit(Collider other)
    {
        ShipTile tile = other.GetComponent<ShipTile>();

        if (tile != null)
        {
            tilesColliders.Remove(other);
            tile.SetState(ShipTile.TileState.Empty);
        }
    }
}
