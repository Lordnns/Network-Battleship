/// <summary>
/// This is only a place holder state when we open the game
/// </summary>
public class MainMenuState : GameState
{
    public MainMenuState(PlayerController playerController, StateMachine playerStateMachine, PlayerNetwork playerNetwork, InGameUIController ui) : base(playerController, playerStateMachine, playerNetwork, ui)
    {
    }
}
