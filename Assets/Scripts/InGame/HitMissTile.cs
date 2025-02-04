using UnityEngine;

public class HitMissTile : MonoBehaviour
{
    public enum TileState { Empty, Miss, Hit }
    public TileState currentState = TileState.Empty;

    public int x;
    public int y;

    public Material emptyMaterial;
    public Material hitMaterial;
    public Material missMaterial;
    public Material hoverMaterial;

    private Renderer tileRenderer;

    private PlayerNetwork playerNetwork;

    [SerializeField] private AudioClip invalidInput;

    void Start()
    {
        tileRenderer = GetComponent<Renderer>();
        UpdateVisuals();
        
        PlayerNetwork[] players = FindObjectsOfType<PlayerNetwork>();

        foreach (var player in players)
        {
            if (player.IsOwner)
            {
                playerNetwork = player;
            }
        }
    }
    void OnMouseEnter()
    {
        if (currentState != TileState.Hit && currentState != TileState.Miss)
        {
            tileRenderer.material = hoverMaterial;
        }
    }

    private void OnMouseExit()
    {
        if (currentState != TileState.Hit && currentState != TileState.Miss)
        {
            tileRenderer.material = emptyMaterial;
        }
    }

    void OnMouseDown()
    {
        Debug.Log("in" + x + " " + y);
        

        // if the player is in attack mode
        if (playerNetwork.playerController.playerStateMachine.currentState == playerNetwork.playerController.attackingState)
        {
            // if the tile was not already selected on a previous turn
            if (currentState != TileState.Hit && currentState != TileState.Miss)
            {
                playerNetwork.HitAttemptCall(this);
                return;
            }

        }

        if (playerNetwork.playerController.playerStateMachine.currentState ==
            playerNetwork.playerController.waitingForOpponentState)
        {
            //using the singleton audioManager to get a pool object to play a sound
            AudioManager.Instance.PlayOneShotSound(invalidInput);
        }

        
    }
    
    public void SetState(TileState newState)
    {
        currentState = newState;
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        switch (currentState)
        {
            case TileState.Empty:
                tileRenderer.material = emptyMaterial;
                break;
            case TileState.Hit:
                tileRenderer.material = hitMaterial;
                break;
            case TileState.Miss:
                tileRenderer.material = missMaterial;
                break;
        }
    }
}
