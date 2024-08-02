using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Fatih.F_Scripts
{
    public class GoToMenu : MonoBehaviour
    {
        private void Start()
        {
            SceneManager.LoadScene("UI_MainMenu");
        }
    }
}