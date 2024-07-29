using UnityEngine;
using TMPro;
using Unity.Collections;
using Unity.Netcode;

public class PlayerSettings : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;

    private NetworkVariable<FixedString128Bytes> networkPlayerName =
        new NetworkVariable<FixedString128Bytes>(
            "", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            networkPlayerName.Value = NameInputHandler.PlayerName;
        }

        networkPlayerName.OnValueChanged += OnNameChanged;
        playerNameText.text = networkPlayerName.Value.ToString();
    }

    private void OnNameChanged(FixedString128Bytes oldName, FixedString128Bytes newName)
    {
        playerNameText.text = newName.ToString();
    }
}