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
            remainingTime = 0f; // Zaman� s�f�rlay�n
            UpdateClientRpc(remainingTime); // Zaman�n s�f�rland���n� istemcilere bildirin
        }
        else
        {
            UpdateClientRpc(remainingTime);
        }
    }

    // Bu ClientRpc istemcilere zaman g�ncellemelerini iletmek i�in kullan�l�r
    [ClientRpc]
    private void UpdateClientRpc(float newTime)
    {
        // �stemcilerde zaman�n g�ncellenmesi
        if (timeText != null)
        {
            timeText.text = $"{Mathf.FloorToInt(newTime / 60)}:{(newTime % 60).ToString("00")}";
            Debug.Log("Client g�ncellendi: " + newTime);
        }
    }

    [ClientRpc]
    private void EndGameOnClientRpc()
    {
        // �stemcilerde oyun bitti�inde yap�lacak i�lemler
        Debug.Log("Client: Oyun Bitti!");

        // Burada ayr�ca, oyun bitimiyle ilgili di�er i�lemleri de yapabilirsiniz
        // �rne�in, oyun sahnesini de�i�tirmek, bir sonu� ekran� g�stermek, vb.
    }

}
