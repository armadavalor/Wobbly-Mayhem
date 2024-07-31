using UnityEngine;
using Unity.Netcode;
using TMPro;

public class GameMode : NetworkBehaviour
{
    public float remainingTime = 60f;
    public TextMeshProUGUI timeText;

    private void Update()
    {
        if (!IsServer) return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            EndGameOnClientRpc();
            remainingTime = 0f; // Zamaný sýfýrlayýn
            UpdateClientRpc(remainingTime); // Zamanýn sýfýrlandýðýný istemcilere bildirin
        }
        else
        {
            UpdateClientRpc(remainingTime);
        }
    }

    // Bu ClientRpc istemcilere zaman güncellemelerini iletmek için kullanýlýr
    [ClientRpc]
    private void UpdateClientRpc(float newTime)
    {
        // Ýstemcilerde zamanýn güncellenmesi
        if (timeText != null)
        {
            timeText.text = $"{Mathf.FloorToInt(newTime / 60)}:{(newTime % 60).ToString("00")}";
            Debug.Log("Client güncellendi: " + newTime);
        }
    }

    [ClientRpc]
    private void EndGameOnClientRpc()
    {
        // Ýstemcilerde oyun bittiðinde yapýlacak iþlemler
        Debug.Log("Client: Oyun Bitti!");

        // Burada ayrýca, oyun bitimiyle ilgili diðer iþlemleri de yapabilirsiniz
        // Örneðin, oyun sahnesini deðiþtirmek, bir sonuç ekraný göstermek, vb.
    }

}
