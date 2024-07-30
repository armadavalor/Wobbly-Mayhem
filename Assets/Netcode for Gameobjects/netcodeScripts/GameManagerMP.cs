using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class GameManagerMP : MonoBehaviour
{
    
    
    [SerializeField] private NetworkManager networkManager;

    public void StartHost()
    {
        networkManager.StartHost();
        SceneManager.LoadScene("Test_RagDolls");
    }

    public void StartClient()
    {
        networkManager.StartClient();
        SceneManager.LoadScene("Test_RagDolls");
    }
}