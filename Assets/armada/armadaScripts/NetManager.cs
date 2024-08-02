using System;
using Unity.Netcode;
using UnityEngine;

public class NetManager : NetworkBehaviour
{
  public PlayerStates[] _playerStatesList;

  [Serializable]
  public struct PlayerStates
  {
    public ulong      ClientId;
    public string     playerName;
    public GameObject playerObject;
    public int        score;
    public bool       isThisWinner;
  }

  
#region Instance
  private static NetManager _instance;

  public static NetManager Instance
  {
    get
    {
      if (_instance == null)
      {
        _instance = FindObjectOfType<NetManager>();

        if (_instance == null)
        {
          GameObject singletonObject = new GameObject("SingletonExample");
          _instance = singletonObject.AddComponent<NetManager>();
        }
      }

      return _instance;
    }
  }
  private void Awake()
  {
    if (_instance == null)
    {
      _instance = this;
      DontDestroyOnLoad(gameObject);
    }
    else
      Destroy(gameObject);
  }
#endregion

  public override void OnNetworkSpawn()
  {
    NetworkManager.Singleton.OnClientConnectedCallback += PlayerConnect;
  }

  private void Update()
  {
    if (!IsServer)
      return;


    if (_playerStatesList[0].playerObject == null)
      _playerStatesList[0].playerObject = findPlayer(0);
  }
private void PlayerConnect(ulong clientId)
  {
    if (IsOwner)
    {
      var playerName = NameInputHandler.PlayerName;
      var playerObject = findPlayer(clientId);
      _playerStatesList[clientId] = new PlayerStates
      {
        ClientId = clientId,
        playerName = playerName,
        playerObject = playerObject
      };

      Debug.Log($"Client ID: {clientId}, Name: {playerName}");
    }
  }

  private GameObject findPlayer(ulong clientId)
  {
    var players = FindObjectsByType<PlayerController2>(FindObjectsSortMode.None);
    foreach (var player in players)
    {
      if (player.GetComponent<NetworkObject>().OwnerClientId == clientId)
        return player.gameObject;
    }

    Debug.Log($"No player found for client ID: {clientId}");
    return null;
  }

  public string GetPlayerName(ulong clientId)
  {
    return _playerStatesList[clientId].playerName;
    // return playerStates.TryGetValue(clientId, out var state) ? state.playerName : "Unknown";
  }
  
 
}