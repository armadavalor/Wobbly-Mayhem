using System;
using System.Collections;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.SceneManagement;

public class GameMode : NetworkBehaviour
{
  public float remainingTime = 60f;
  public TextMeshProUGUI timeText;

  private bool workOneMoreTime = false;


  [SerializeField] private GameObject EndGameUI;
  [SerializeField] private AudioSource _endGameAudioSource;

  [SerializeField] private NetManager _netManager;
  public Action OnGameEnd;
  private void Update()
  {
    if (!IsServer) return;

    remainingTime -= Time.deltaTime;

    if (remainingTime <= 0f && !workOneMoreTime)
    {
      EndGameOnClientRpc();
      remainingTime = 0f; // Zaman� s�f�rlay�n
      UpdateClientRpc(remainingTime); // Zaman�n s�f�rland���n� istemcilere bildirin
      workOneMoreTime = true;
      WhoIsTheWinner();

      Time.timeScale = 0.2f;
      EndGameUI.SetActive(true);
      Cursor.lockState = CursorLockMode.None;
      _endGameAudioSource.Play();
      
      StartCoroutine(REstatGame());
    }
    else if (!workOneMoreTime)
      UpdateClientRpc(remainingTime);
  }

  [ClientRpc]
  private void UpdateClientRpc(float newTime)
  {
    if (timeText != null)
    {
      timeText.text = $"{Mathf.FloorToInt(newTime / 60)}:{(newTime % 60).ToString("00")}";

    }
  }

  [ClientRpc]
  private void EndGameOnClientRpc()
  {
    Debug.Log("Client: Oyun Bitti!");
  }

  [SerializeField] public TextMeshProUGUI WinnerText;
  private void WhoIsTheWinner()
  {
    var otherPlayer = _netManager._playerStatesList[0];

    var winnerPlayerID = _netManager._playerStatesList[0].ClientId;
    foreach (var player in _netManager._playerStatesList)
    {
      if (player.score > otherPlayer.score)
        winnerPlayerID = player.ClientId;

      otherPlayer = player;
    }

    foreach (var player in _netManager._playerStatesList)
    {
      if (player.ClientId == winnerPlayerID)
      {
        winner(player.ClientId);
        WinnerText.text = "Oyuncu " + player.ClientId.ToString();
      }
        
    }
  }

  private void winner(ulong clientID)
  {
    _netManager._playerStatesList[clientID].isThisWinner = true;
  }

  // [ContextMenu("ReStartGame")]
// [ClientRpc]
// public void ReStartGameClientRpc()
// {
//  // NetworkManager.Singleton.SceneManager.LoadScene("UI_MainMenu", LoadSceneMode.Single);
//   
//  currentHealth = 100f;
//  ("Wobbly Mayhem");
//  Cursor.lockState = CursorLockMode.None;
//  NetworkManager.Singleton.Shutdown();
// }
  #region Instance
  private static GameMode _instance;

  public static GameMode Instance
  {
    get
    {
      if (_instance == null)
      {
        _instance = FindObjectOfType<GameMode>();

        if (_instance == null)
        {
          GameObject singletonObject = new GameObject("SingletonExample");
          _instance = singletonObject.AddComponent<GameMode>();
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


  private IEnumerator REstatGame()
  {
    yield return new WaitForSeconds(5f);

    ReStartGameClientRpc();
  }
  
  [ContextMenu("ReStartGame__")]
  
  [ClientRpc]
  public void ReStartGameClientRpc()
  {
    OnGameEnd.Invoke();
    SetDefaultySo();
  }

  public void SetDefaultySo()
  {
    for (int i = 0; i < NetManager.Instance._playerStatesList.Length; i++)
    {

      SetDefaulty(i,
        0,
        false
        );


      if (NetManager.Instance._playerStatesList[i].playerObject == null)
        return;
    }
  }

  private void SetDefaulty(int i,
    int score,
    bool isThisWinner
   
  )
  {
    NetManager.Instance._playerStatesList[i].score = score;
    NetManager.Instance._playerStatesList[i].isThisWinner = isThisWinner;
  }

}