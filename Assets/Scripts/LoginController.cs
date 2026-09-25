using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.GraphicsBuffer;

public class LoginController : MonoBehaviour
{
    public TMP_InputField nameInput;
    public TMP_InputField serverInput;
    private int serverPort = 8888;

    private bool isLoginSuccess = false;

    public void OnBtnJoinClicked()
    {
        string playerName = nameInput.text.Trim();
        string serverIP = serverInput.text.Trim();

        PlayerPrefs.SetString("Username", playerName);
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("Chưa nhập tên kìa ông ơi!");
            return;
        }
        if (string.IsNullOrEmpty(serverIP))
        {
            serverIP = "127.0.0.1";
        }

        NetworkManager.Instance.ConnectToServer(serverIP, serverPort);
        NetworkManager.Instance.OnMessageReceived += HandleServerResponse;

        Invoke(nameof(SendLoginPacket), 0.4f);
    }

    private void SendLoginPacket()
    {
        string loginMessage = $"LOGIN:{nameInput.text}";
        NetworkManager.Instance.SendPacket(loginMessage);
    }

    private void HandleServerResponse(string message)
    {
        Debug.Log($"[Client Nhận] {message}");

        if (message.Contains("LOGIN_SUCCESS") || message.Contains("OK"))
        {
            isLoginSuccess = true;
        }
        else if (message.Contains("LOGIN_FAIL"))
        {
            Debug.LogWarning("Server từ chối tên đăng nhập này!");
        }
    }

    private void Update()
    {
        if (isLoginSuccess)
        {
            isLoginSuccess = false; 
            NetworkManager.Instance.OnMessageReceived -= HandleServerResponse;

            SceneManager.LoadScene("SelectSubject");
        }
    }
}