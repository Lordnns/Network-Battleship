using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlacingBoatState : GameState
{
    private float inputDelta;
    public override void Enter()
    {
        base.Enter();
        Debug.Log("im in placing boat");
        ui = Object.FindObjectOfType<InGameUIController>();
    }

    public override void Update()
    {
        base.Update();
        inputDelta = Input.mouseScrollDelta.y;
        if (inputDelta != 0)
        {
            ShipPlacementManager.Instance.UpdateSelectedType(inputDelta);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (ui.GetPauseMenuStatus())
                ui.SetPausePanelState(false);
            else
                ui.SetPausePanelState(true);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ShipPlacementManager.Instance.ToggleShipOrientation();
        }

        HandleClientDisconnect();

    }

    private void HandleClientDisconnect()
    {
        if (NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                if (NetworkManager.Singleton.ConnectedClientsList.Count != 2)
                {
                    NetworkManager.Singleton.Shutdown();
                    SceneManager.LoadScene(0);
                }
            }
        }
    }


    public PlacingBoatState(PlayerController playerController, StateMachine playerStateMachine, PlayerNetwork playerNetwork, InGameUIController ui) : base(playerController, playerStateMachine, playerNetwork, ui)
    {
    }
}
