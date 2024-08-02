using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetManager : NetworkBehaviour
{
    private Dictionary<ulong, PlayerStates> playerStates = new Dictionary<ulong, PlayerStates>();

    private List<GameObject> PlayersName;

    [Serializable]
    public struct PlayerStates
    {
        public ulong ClientId;
        public string playerName;
        public GameObject playerObject;
    }

    
    

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += PlayerConnect;
      
    }
    private void PlayerConnect(ulong clientId)
    {
        if (IsOwner)
        {
            var playerName = NameInputHandler.PlayerName;
            var playerObject = SomeFunction(clientId);
            playerStates[clientId] = new PlayerStates
            {
                ClientId = clientId,
                playerName = playerName,
                playerObject = playerObject
            };

            Debug.Log($"Client ID: {clientId}, Name: {playerName}");
        }
    }

    private GameObject SomeFunction(ulong clientId)
    {
        var players = FindObjectsByType<PlayerController2>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (player.GetComponent<NetworkObject>().NetworkObjectId == clientId)
            {
                return player.gameObject;
            }
        }
        Debug.Log($"No player found for client ID: {clientId}");
        return null;
    }

    public string GetPlayerName(ulong clientId)
    {
        return playerStates.TryGetValue(clientId, out var state) ? state.playerName : "Unknown";
    }

   
}
