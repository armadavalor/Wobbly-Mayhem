using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnOutOfBounds : MonoBehaviour
{
   private void OnTriggerEnter(Collider other)
   {
      if (other.CompareTag("Player"))
      {
         Debug.Log("oyuncu dusuyo");
         other.GetComponent<PlayerController2>().DieClientRpc(respawn:true);
      }
   }
}
