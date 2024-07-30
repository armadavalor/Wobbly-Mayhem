using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NameInputHandler : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button joinButton;

    public static string PlayerName { get; private set; }

    private void Start()
    {
        joinButton.onClick.AddListener(OnJoinButtonClicked);
    }

    public void OnJoinButtonClicked()
    {
        if (!string.IsNullOrEmpty(nameInputField.text))
        {
            PlayerName = nameInputField.text;
        }
    }
}