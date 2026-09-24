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

        // 1. Kết nối và đăng ký lắng nghe phản hồi
        NetworkManager.Instance.ConnectToServer(serverIP, serverPort);
        NetworkManager.Instance.OnMessageReceived += HandleServerResponse;

        // 2. Gửi gói tin LOGIN
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
            // CHỈ BẬT CỜ Ở ĐÂY, KHÔNG GỌI LOADSCENE NỮA
            isLoginSuccess = true;
        }
        else if (message.Contains("LOGIN_FAIL"))
        {
            Debug.LogWarning("Server từ chối tên đăng nhập này!");
        }
    }

    // UNITY SẼ LIÊN TỤC KIỂM TRA HÀM NÀY TRÊN MAIN THREAD
    private void Update()
    {
        if (isLoginSuccess)
        {
            isLoginSuccess = false; // Tắt cờ ngay để không bị loop
            NetworkManager.Instance.OnMessageReceived -= HandleServerResponse;

            // Chuyển scene an toàn tuyệt đối
            SceneManager.LoadScene("SelectSubject");
        }
    }
}