using Unity.Netcode;
using UnityEngine;

public class ModelChange : NetworkBehaviour //karakterin üsütüne ata
{
  [SerializeField] private SkinnedMeshRenderer SkinnedMeshRenderer;

  [SerializeField] private Material[] materials;

  // private int SelectionMaterialsCount;

  private void Start()
  {
    SkinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
  }


  public override void OnNetworkSpawn()
  {
    if (IsClient)
    {
      sssssServerRPC(NetworkManager.Singleton.LocalClientId);
    }
  }

  [ServerRpc(RequireOwnership = false)]
  private void sssssServerRPC(ulong ownClientID)
  {
    for (int i = 0; i < NetManager.Instance._playerStatesList.Length; i++)
    {
      if (NetManager.Instance._playerStatesList[i].playerObject == null)
      {
        return;
      }

      SelectMaterialWithIDClientRPC(NetManager.Instance._playerStatesList[i].ClientId);
    }
  }


  [ClientRpc]
  private void SelectMaterialWithIDClientRPC(ulong clientID)
  {
    if (clientID == 0)
    {
      SkinnedMeshRenderer.material = materials[0];
    }
    else if (clientID == 1)
    {
      SkinnedMeshRenderer.material = materials[1];
    }
    else if (clientID == 2)
    {
      SkinnedMeshRenderer.material = materials[2];
    }
    else if (clientID == 3)
    {
      SkinnedMeshRenderer.material = materials[3];
    }
    else if (clientID == 4)
    {
      SkinnedMeshRenderer.material = materials[0];
    }
    else if (clientID >= 5)
    {
      SkinnedMeshRenderer.material = materials[1];
    }
else if (clientID == 6)
    {
      SkinnedMeshRenderer.material = materials[2];
    }
    else if (clientID == 7)
    {
      SkinnedMeshRenderer.material = materials[3];
    }
    else if (clientID == 8)
    {
      SkinnedMeshRenderer.material = materials[0];
    }
    else if (clientID == 9)
    {
      SkinnedMeshRenderer.material = materials[1];
    }
    else if (clientID == 10)
    {
      SkinnedMeshRenderer.material = materials[2];
    }
    else if (clientID == 11)
    {
      SkinnedMeshRenderer.material = materials[3];
    }
    else if (clientID == 12)
    {
      SkinnedMeshRenderer.material = materials[0];
    }
    else if (clientID == 13)
    {
      SkinnedMeshRenderer.material = materials[1];
    }
    else if (clientID == 14)
    {
      SkinnedMeshRenderer.material = materials[2];
    }
    else if (clientID == 15)
    {
      SkinnedMeshRenderer.material = materials[3];
    }
    else if (clientID == 16)
    {
      SkinnedMeshRenderer.material = materials[0];
    }
    else if (clientID == 17)
    {
      SkinnedMeshRenderer.material = materials[1];
    }
    else if (clientID == 18)
    {
      SkinnedMeshRenderer.material = materials[2];
    }
    else if (clientID == 19)
    {
      SkinnedMeshRenderer.material = materials[3];
    }
    else if (clientID == 20)
    {
      SkinnedMeshRenderer.material = materials[0];
    }
    else if (clientID == 21)
    {
      SkinnedMeshRenderer.material = materials[1];
    }
    else if (clientID == 22)
    {
      SkinnedMeshRenderer.material = materials[2];
    }
    else if (clientID == 23)
    {
      SkinnedMeshRenderer.material = materials[3];
    }
    else if (clientID == 24)
    {
      SkinnedMeshRenderer.material = materials[0];
    }
    else if (clientID == 25)
    {
      SkinnedMeshRenderer.material = materials[1];
    }
  }
}