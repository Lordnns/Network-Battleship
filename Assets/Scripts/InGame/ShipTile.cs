using UnityEngine;

public class ShipTile : MonoBehaviour
{
    public enum TileState { Empty, Ship, Hit, Miss, Hover }
    public TileState currentState = TileState.Empty;

    public int x { get; set; }
    public int y { get; set; }

    public Material emptyMaterial;
    public Material shipMaterial;
    public Material hitMaterial;
    public Material missMaterial;
    public Material hoverMaterial;
    
    [SerializeField] private ParticleSystem hitParticles;
    [SerializeField] private ParticleSystem missParticles;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private AudioClip waterSound;

    void Start()
    {
        UpdateVisuals();
    }

    public void StartExplosion()
    {
        Instantiate(hitParticles, transform.position, Quaternion.identity);
        
        //using the singleton audioManager to get a pool object to play a sound
        AudioManager.Instance.PlayOneShotSound(explosionSound);
    }

    public void StartMiss()
    {
        Instantiate(missParticles, transform.position, Quaternion.identity);
        //using the singleton audioManager to get a pool object to play a sound
        AudioManager.Instance.PlayOneShotSound(waterSound);
    }
    void OnMouseEnter()
    {
        Vector3 position = transform.position;
        ShipPlacementManager.Instance.IsInTile(true);
        ShipPlacementManager.Instance.UpdateShipPreview(position, false);
    }
    void OnMouseDown()
    {
        if (currentState is TileState.Empty or TileState.Hover)
        {
            bool success = ShipPlacementManager.Instance.PlaceShip(x, y);

            if (success)
            {
                Debug.Log($"Ship placed at {x},{y}.");
            }
            else
            {
                Debug.Log("Placement failed. Area occupied or out of bounds.");
            }
        }
    }
    void OnMouseExit()
    {
        ShipPlacementManager.Instance.TurnPreviewOff();
        ShipPlacementManager.Instance.IsInTile(false);
    }

    public void SetState(TileState newState)
    {
        currentState = newState;
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        Renderer tileRenderer = GetComponent<Renderer>(); // Get the Renderer component

        switch (currentState)
        {
            case TileState.Empty:
                tileRenderer.material = emptyMaterial;
                break;
            case TileState.Ship:
                tileRenderer.material = shipMaterial;
                break;
            case TileState.Hit:
                tileRenderer.material = hitMaterial;
                break;
            case TileState.Miss:
                tileRenderer.material = missMaterial;
                break;
            case TileState.Hover:
                tileRenderer.material = hoverMaterial;
                break;
        }
    }
    
}
