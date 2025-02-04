using UnityEngine;
/// <summary>
/// Base class for our game state
/// </summary>
public class GameState : State

{
    protected PlayerController playerController;
    protected StateMachine playerStateMachine;
    protected PlayerNetwork playerNetwork;
    protected InGameUIController ui;
    
    // put the all data needed when creating the state
    protected GameState(PlayerController playerController, StateMachine playerStateMachine, PlayerNetwork playerNetwork, InGameUIController ui)
    {
        this.playerController = playerController;
        this.playerStateMachine = playerStateMachine;
        this.playerNetwork = playerNetwork;
        this.ui = ui;
    }
    private float startTime;

    public override void Enter()
    {
        base.Enter();
        startTime = Time.time;
    }
}
