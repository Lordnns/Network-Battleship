using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// This class is mostly managing the ship placement phase. its used as singleton but it is destroyed at scene change
/// </summary>
public class ShipPlacementManager : MonoBehaviour
{
    
    [Header("Ships")]
    public GameObject[] shipPreviewPrefabs;
    public GameObject[] shipPrefabs;
    private List<GameObject> shipPreviews = new List<GameObject>();
    bool isVertical = true;
    private int[] selectedShipLength = new int[5] {2,3,3,4,5};
    public int selectedShipType = 0; // mettre private 
    private Vector3 shipPreviewPosition;
    public List<GameObject> spawnedShips = new List<GameObject>();
    [SerializeField] private Material holoMat, placedHoloMat;
    
    [Header("tile")]
    public bool[,] occupied;
    private Vector3 lastTilePosition;
    private bool activeOnTile;
    public GridManager gridManager;

    private InGameUIController ui;

    [SerializeField] private AudioClip placedBoatSound;
    
    public static ShipPlacementManager Instance;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {

        ui = FindObjectOfType<InGameUIController>();
        
        PlayerNetwork[] p = FindObjectsOfType<PlayerNetwork>();
        foreach (var player in p)
        {
            if (player.IsOwner)
            {
                PlayerController pc = player.GetComponent<PlayerController>();
                {
                    pc.playerStateMachine.ChangeState(pc.placingBoatState);
                }
            }
            
        }

        activeOnTile = false;
        occupied = new bool[gridManager.width, gridManager.height];
        InitializeShipPreviews();
    }

    void InitializeShipPreviews()
    {
        foreach (var prefab in shipPreviewPrefabs)
        {
            var preview = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            preview.transform.parent = this.transform;
            preview.SetActive(false);
            shipPreviews.Add(preview);
        }
    }
    
    public bool PlaceShip(int startX, int startY)
    {
        foreach (var ship in spawnedShips)
        {
            if (ship.GetComponent<ShipScript>().GetShipType() == selectedShipType) return false;
        }

        for (int i = 0; i < selectedShipLength[selectedShipType]; i++)
        {
            int x = startX + (isVertical ? i : 0);
            int y = startY + (isVertical ? 0 : i);

            if (x >= gridManager.width || y >= gridManager.height || x < 0 || y < 0 || occupied[x, y])
                return false;
        }
        List<Vector2Int> occupiedTiles = new List<Vector2Int>();

        for (int i = 0; i < selectedShipLength[selectedShipType]; i++)
        {
            int x = startX + (isVertical ? i : 0);
            int y = startY + (isVertical ? 0 : i);
            occupied[x, y] = true;
            occupiedTiles.Add(new Vector2Int(x, y));
            gridManager.GetShipTileAt(x, y).SetState(ShipTile.TileState.Ship);
        }
        
        SpawnShip(occupiedTiles);
        
        //set mat to red

        foreach (var rendererChild in shipPreviews[selectedShipType].GetComponentsInChildren<Renderer>())
        {
            List<Material> shared = new List<Material>();
            foreach (var mat in rendererChild.materials)
            { 
                shared.Add(placedHoloMat);
            }

            rendererChild.sharedMaterials = shared.ToArray();
        }
        
        ui.OnBoatPlaced(VerifyIfPlayerIsReady());
        
        return true;
    }

    public void UpdateShipPreview(Vector3 newPosition, bool isRotateCall)
    {
        if (!activeOnTile)
            return;
        if (!isRotateCall) { lastTilePosition = newPosition; }
        // Calculate the midpoint of the ship based on its length and orientation
        float offsetX = isVertical ? (selectedShipLength[selectedShipType] - 1) * gridManager.spacing / 2f : 0;
        float offsetY = isVertical ? 0 : (selectedShipLength[selectedShipType] - 1) * gridManager.spacing / 2f;
        Vector3 shipPosition = new Vector3(lastTilePosition.x + offsetX, lastTilePosition.y + 0.8f, lastTilePosition.z + offsetY);
        shipPreviewPosition = shipPosition;


        foreach (var preview in shipPreviews)
        {
            preview.SetActive(false);
        }

        var selectedPreview = shipPreviews[selectedShipType];
        
        selectedPreview.transform.eulerAngles = isVertical ? Vector3.forward * 0 : Vector3.up * 90;
        selectedPreview.transform.position = shipPreviewPosition;
        selectedPreview.SetActive(true);
        
    }

    public void TurnPreviewOff()
    {
        foreach (var preview in shipPreviews)
        {
            preview.SetActive(false);
        }
    }

    public void ToggleShipOrientation()
    {
        isVertical = !isVertical;
        UpdateShipPreview(shipPreviewPosition , true);
    }

    public void ClearOccupiedTile(int x, int y)
    {
        if (x < 0 || x >= gridManager.width || y < 0 || y >= gridManager.height) return;

        occupied[x, y] = false;
        gridManager.GetShipTileAt(x, y).SetState(ShipTile.TileState.Empty);
    }
    
    private void SpawnShip(List<Vector2Int> occupiedTiles)
    {
        float offsetX = isVertical ? (selectedShipLength[selectedShipType] - 1) * gridManager.spacing / 2f : 0;
        float offsetY = isVertical ? 0 : (selectedShipLength[selectedShipType] - 1) * gridManager.spacing / 2f;
        Vector3 shipPosition = new Vector3(lastTilePosition.x + offsetX, lastTilePosition.y + 0.8f, lastTilePosition.z + offsetY);

        var spawnShipPrefab = shipPrefabs[selectedShipType];
        var spawnShip = Instantiate(spawnShipPrefab, Vector3.zero, Quaternion.identity);
        spawnShip.transform.eulerAngles = isVertical ? Vector3.forward * 0 : Vector3.up * 90;
        spawnShip.transform.position = shipPosition;
        spawnShip.GetComponent<ShipScript>().SetOccupiedTiles(occupiedTiles);
        spawnShip.GetComponent<ShipScript>().SetShipType(selectedShipType);

        Collider shipCollider = spawnShip.GetComponent<Collider>();
        if (shipCollider != null)
            shipCollider.enabled = true;
        spawnedShips.Add(spawnShip);
        
        //using the singleton audioManager to get a pool object to play a sound
        AudioManager.Instance.PlayOneShotSound(placedBoatSound);
        
    }
    public void RemoveShip(GameObject ship)
    {
        if (spawnedShips.Contains(ship))
            spawnedShips.Remove(ship);
        //set mat to blue
        foreach (var rendererChild in shipPreviews[ship.GetComponent<ShipScript>().GetShipType()].GetComponentsInChildren<Renderer>())
        {
            List<Material> shared = new List<Material>();
            foreach (var mat in rendererChild.materials)
            { 
                shared.Add(holoMat);
            }

            rendererChild.sharedMaterials = shared.ToArray();
        }
        ui.OnBoatPlaced(VerifyIfPlayerIsReady());
    }

    public void UpdateSelectedType(float delta)
    {
        if (delta < 0)
        {
            selectedShipType++;
            if (selectedShipType > shipPreviews.Count - 1)
            {
                selectedShipType = 0;
            }
            UpdateShipPreview(lastTilePosition, false);
        }
        else if (delta > 0) 
        {
            selectedShipType--;
            if (selectedShipType < 0)
            {
                selectedShipType = shipPreviews.Count - 1;
            }
            UpdateShipPreview(lastTilePosition, false);
        }
    }

    public void IsInTile(bool isInTile) { activeOnTile = isInTile; }

    public bool VerifyIfPlayerIsReady()
    {
        return spawnedShips.Count == 5;
    }
}

