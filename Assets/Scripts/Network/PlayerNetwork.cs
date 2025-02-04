using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerNetwork : NetworkBehaviour
{


    public PlayerController playerController;

    #region State Variables

    public bool hostShipReady;
    public bool clientShipReady;

    private int opponentShipSunk = 0;
    
    public NetworkVariable<bool> hasThrownDice = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);
    
    #endregion

    #region Boat/Grid Variables

    private NetworkList<int> sipGridNetworkList;

    //list of boat positions index. the last digit in the vector is the type of boat
    private NetworkList<Vector3> boatPositions;
    
    private int[,] shipGrid = new int[10, 10];
    private int[,] hitMissGrid = new int[10, 10];

    private readonly List<char> letterList = new List<char>() { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'F' };

    #endregion
    
    public NetworkVariable<int> diceValue = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);
    
    
    public void Awake()
    {
        sipGridNetworkList = new NetworkList<int>(new List<int>(), NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);
        boatPositions = new NetworkList<Vector3>(new List<Vector3>(), NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        diceValue.OnValueChanged += OnDiceThrown;

        letterList.Reverse();
    }

    public override void OnDestroy()
    {
        sipGridNetworkList?.Dispose();
        boatPositions?.Dispose();
        SceneManager.LoadScene(0);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            InitializeGrid();
            playerController = GetComponent<PlayerController>();

        }
    }

    //function called by the on value change delegate of the diceValue network variable 
    private void OnDiceThrown(int i, int z)
    {
        DiceController d = FindObjectOfType<DiceController>();
        if (d == null) return;
        
        d.UpdateText();
    }

    #region RPC

        [ServerRpc(RequireOwnership = false)]
    public void ResetRemotePlayerDiceServerRpc()
    {
        PlayerNetwork remotePlayerNetwork = GetOpponent();
        remotePlayerNetwork.ResetRemotePlayerDiceClientRpc();
    }

    //remove the dice panel for the receiver
    [ClientRpc]
    void ResetRemotePlayerDiceClientRpc()
    {
        if (IsOwner)
        {
            hasThrownDice.Value = false;
            DiceController remoteDiceController = FindObjectOfType<DiceController>();
            remoteDiceController.EnableDiceButton();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetRemotePlayerStateServerRPC(int local, int remote)
    {
        PlayerNetwork remotePlayerNetwork = GetOpponent();
        remotePlayerNetwork.SetRemotePlayerStateClientRpc(local, remote);
        
    }

    //inform receiver that both players threw their dices. receiver must change its state accordingly  
    [ClientRpc]
    void SetRemotePlayerStateClientRpc(int local, int remote)
    {
        if (IsOwner)
        {
            playerController.rollingDiceState.CheckForWinner(remote, local);
        }
    }
    
    
    [ServerRpc(RequireOwnership = false)]
    private void UpdateReadyStateServerRpc(bool state, ulong senderClientId)
    {
        bool isSenderHost = senderClientId == NetworkManager.Singleton.LocalClientId;

        if (isSenderHost)
            hostShipReady = state;
        else
            clientShipReady = state;

        PlayerNetwork opponent = GetOpponentPlayerNetwork(senderClientId);
        if (opponent != null)
        {
            opponent.hostShipReady = hostShipReady;
            opponent.clientShipReady = clientShipReady;
            
            opponent.UpdateReadyStateClientRpc(clientShipReady, hostShipReady);
        }

        PlayerNetwork playerNetwork = GetSenderPlayerNetwork(senderClientId);

        if (playerNetwork != null)
        {
            playerNetwork.UpdateReadyStateClientRpc(clientShipReady, hostShipReady);
        }
    }

    [ClientRpc]
    private void UpdateReadyStateClientRpc(bool _clientShipReady, bool _hostShipReady)
    {
        hostShipReady = _hostShipReady;
        clientShipReady = _clientShipReady;
        VerifyShipPlacementState();
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SetShipTileStateServerRPC(int x, int y, ShipTile.TileState state)
    {
        PlayerNetwork remotePlayerNetwork = GetOpponent();
        remotePlayerNetwork.SetShipTileStateClientRPC(x, y, state);
    }

    [ClientRpc]
    private void SetShipTileStateClientRPC(int x, int y, ShipTile.TileState state)
    {
        if (IsOwner)
        {
            ShipTile[] tiles = FindObjectsOfType<ShipTile>();

            foreach (var tile in tiles)
            {
                if (tile.x == x && tile.y == y)
                {
                    if (state == ShipTile.TileState.Hit)
                    {
                        tile.StartExplosion();
                    }
                    else if (state == ShipTile.TileState.Miss)
                    {
                        tile.StartMiss();
                    }

                    tile.SetState(state);

                    InGameUIController ui = FindObjectOfType<InGameUIController>();
                    ui.SetEnemyHitInfo("Enemy chose to hit : " + letterList[tile.y] + ", " + (tile.x + 1));
                }
            }
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SetShipLooserStateServerRPC()
    {
        PlayerNetwork remotePlayerNetwork = GetOpponent();
        remotePlayerNetwork.SetShipLooserStateClientRPC();
    }

    //inform the receiver that he lost the game
    [ClientRpc]
    private void SetShipLooserStateClientRPC()
    {
        if (IsOwner)
        {
            playerController.playerStateMachine.ChangeState(playerController.loosingState);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SetRemotePlayerAttackStateServerRPC()
    {
        PlayerNetwork remotePlayerNetwork = GetOpponent();
        remotePlayerNetwork.SetRemotePlayerAttackStateClientRpc();
        
    }

    //inform the receiver that he can now attack
    [ClientRpc]
    void SetRemotePlayerAttackStateClientRpc()
    {
        if (IsOwner)
        {
            playerController.playerStateMachine.ChangeState(playerController.attackingState);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void NotifyRemotePlayerOfSunkenShipServerRPC(int boatType)
    {
        PlayerNetwork remotePlayerNetwork = GetOpponent();
        remotePlayerNetwork.NotifyRemotePlayerOfSunkenShipClientRPC(boatType);
        
    }

    //inform the receiver that he can now attack
    [ClientRpc]
    void NotifyRemotePlayerOfSunkenShipClientRPC(int boatType)
    {
        if (IsOwner)
        {
            foreach (var ship in ShipPlacementManager.Instance.spawnedShips)
            {
                if (ship.GetComponent<ShipScript>().GetShipType() == boatType)
                {
                    ShipPlacementManager.Instance.spawnedShips.Remove(ship);
                    Destroy(ship);
                    break;
                }
            }
        }
    }

    #endregion
    
    
    public void OnPlayerShipReady()
    {
        UpdateReadyStateServerRpc(true, NetworkManager.Singleton.LocalClientId);
        bool[,] temp = ShipPlacementManager.Instance.occupied;
        for (int i = 0; i < temp.GetLength(1); i++)
        {
            for (int j = 0; j < temp.GetLength(0); j++)
            {
                shipGrid[i,j] = temp[i,j]?1:0;
            }
        }
    }
    
    //flatten the array
    private void To1dArray(int[,] input)
    {

        for (int i = 0; i <= input.GetUpperBound(0); i++)
        {
            for (int z = 0; z <= input.GetUpperBound(1); z++)
            {
                sipGridNetworkList.Add(input[i, z]);
            }
        }
    }
    
    //confirm that all players placed their boat and store the data
    private void VerifyShipPlacementState()
    {
        if (hostShipReady && clientShipReady)
        {
            if (playerController != null)
            {
                playerController.playerStateMachine.ChangeState(playerController.rollingDiceState);
                To1dArray(shipGrid);
                
                //get all boat indexes
                ShipScript[] ships = FindObjectsOfType<ShipScript>();

                foreach (var ship in ships)
                {
                    foreach (var tile in ship.occupiedTiles)
                    {
                        var info = new Vector3(tile.x, tile.y, ship.GetShipType());
                        boatPositions.Add(info);
                    }
                }

            }
        }
    }

    public PlayerNetwork GetOpponent()
    {
        PlayerNetwork[] players = FindObjectsOfType<PlayerNetwork>();

        foreach (var player in players)
        {
            if (player != this)
            {
                return player;
            }
        }

        return null;
    }

    public void HitAttemptCall(HitMissTile tile)
    {
        var tilePosition = GetOpponent().sipGridNetworkList[tile.y + (tile.x * 10)];

        if (tilePosition == 1)
        {
            Debug.Log("opponent hit");
            tile.SetState(HitMissTile.TileState.Hit);
            hitMissGrid[tile.x, tile.y] = 2;
            
            //send rpc to opponent
            SetShipTileStateServerRPC(tile.x, tile.y, ShipTile.TileState.Hit);
        }

        if (tilePosition == 0)
        {
            Debug.Log("miss");
            tile.SetState(HitMissTile.TileState.Miss);
            
            //send rpc to opponent
            SetShipTileStateServerRPC(tile.x, tile.y, ShipTile.TileState.Miss);
        }
        
        //check if the player sunk a ship
        if (CheckForSunkenShip(tile))
        {
            //check for the win
            if (opponentShipSunk == 5)
            {
                playerController.playerStateMachine.ChangeState(playerController.winningState);

                //set remote losing state;
                SetShipLooserStateServerRPC();
                return;
            }
            
        }
        
        //set local player to waiting after attacking
        playerController.playerStateMachine.ChangeState(playerController.waitingForOpponentState);
        
        //set remote player to attacking
        SetRemotePlayerAttackStateServerRPC();
        
    }

    private bool CheckForSunkenShip(HitMissTile tile)
    {
        float type = -1;

        foreach (var infos in GetOpponent().boatPositions)
        {
            //TODO check if the cast doest not break anything
            if ((int)infos.x == tile.x && (int)infos.y == tile.y)
            {
                //get the hitboat type
                type = infos.z;
            }
        }

        
        if ((int)type != -1)
        {
            //get boat index for all tiles
            List<GameObject> boatTiles = new List<GameObject>();
            foreach (var boatPosition in GetOpponent().boatPositions)
            {
                if ((int)boatPosition.z == (int)type)
                {
                    if (hitMissGrid[(int)boatPosition.x, (int)boatPosition.y] == 2)
                    {
                        GameObject tileObj =
                            GameObject.Find($"HitMissTile {(int)boatPosition.x} {(int)boatPosition.y}");

                        if (tileObj != null)
                        {
                            boatTiles.Add(tileObj);
                        }

                    }
                    else
                    {
                        return false;
                    }

                }
            }

            opponentShipSunk += 1;
            Debug.Log("boat of type : " + (int)type + " was sunk");

            //modify outline color to signify a sunken ship
            foreach (var tileObj in boatTiles)
            {
                Outline[] outlines = tileObj.GetComponentsInChildren<Outline>();

                foreach (var outline in outlines)
                {
                    outline.OutlineColor = Color.red;
                    outline.OutlineWidth = 4;
                }
                
            }
            
            NotifyRemotePlayerOfSunkenShipServerRPC((int)type);

            return true;
        }

        return false;
    }

    public PlayerNetwork GetOpponentPlayerNetwork(ulong senderClientId)
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            if (client.Key != senderClientId)
            {
                return client.Value.PlayerObject.GetComponent<PlayerNetwork>();
            }
        }
        return null;
    }
    
    public PlayerNetwork GetSenderPlayerNetwork(ulong senderClientId)
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            if (client.Key == senderClientId)
            {
                return client.Value.PlayerObject.GetComponent<PlayerNetwork>();
            }
        }
        return null;
    }

    
    void InitializeGrid()
    {
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                shipGrid[x, y] = 0;
                hitMissGrid[x, y] = 0;
            }
        }
    }
}
