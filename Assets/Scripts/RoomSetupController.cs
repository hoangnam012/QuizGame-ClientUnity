using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class RoomController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField roomCodeInput;

    [Header("Selections")]
    public string selectedClass = "";
    public string selectedSubject = "";

    private bool isRoomCreated = false;

    public void SelectClass(string className)
    {
        selectedClass = className;
        Debug.Log("Đã chọn: " + selectedClass);
    }

    public void SelectSubject(string subjectName)
    {
        selectedSubject = subjectName;
        Debug.Log("Đã chọn: " + selectedSubject);
    }

    public void OnBtnCreateRoomClicked()
    {
        string roomCode = roomCodeInput != null ? roomCodeInput.text.Trim() : "";
        if (string.IsNullOrEmpty(roomCode))
        {
            Debug.LogWarning("Ê, chưa nhập mã code phòng kìa!");
            return;
        }
        if (string.IsNullOrEmpty(selectedClass) || string.IsNullOrEmpty(selectedSubject))
        {
            Debug.LogWarning("Vui lòng chọn đủ Lớp và Môn học!");
            return;
        }

        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnMessageReceived -= HandleServerResponse;
            NetworkManager.Instance.OnMessageReceived += HandleServerResponse;

            string packet = $"CREATE_ROOM:{roomCode}:{selectedClass}:{selectedSubject}";
            NetworkManager.Instance.SendPacket(packet);
            Debug.Log($"[Gửi Server] {packet}");
        }
        else
        {
            Debug.LogError("NetworkManager.Instance is null!");
        }
    }
    
    public void OnBtnJoinRoomClicked()
    {
        string roomCode = roomCodeInput != null ? roomCodeInput.text.Trim() : "";

        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnMessageReceived -= HandleServerResponse;
            NetworkManager.Instance.OnMessageReceived += HandleServerResponse;

            string packet = $"JOIN_ROOM:{roomCode}"; 
            NetworkManager.Instance.SendPacket(packet);
            Debug.Log($"[Gửi Server] {packet}");
        }
    }

    private void HandleServerResponse(string message)
    {
        Debug.Log($"[RoomController Nhận] {message}");
        if (message.Contains("CREATE_SUCCESS") || message.Contains("JOIN_SUCCESS"))
        {
            Debug.Log("Vào phòng thành công, chuẩn bị chuyển Scene!");
            isRoomCreated = true;
        }
        else if (message.Contains("ROOM_EXISTS"))
        {
            Debug.LogWarning("Mã code này đã có người xài!");
            if (NetworkManager.Instance != null)
                NetworkManager.Instance.OnMessageReceived -= HandleServerResponse;
        }
        else if (message.Contains("ROOM_NOT_FOUND"))
        {
            Debug.LogWarning("Phòng không tồn tại, kiểm tra lại mã code!");
            if (NetworkManager.Instance != null)
                NetworkManager.Instance.OnMessageReceived -= HandleServerResponse;
        }
    }

    private void Update()
    {
        if (isRoomCreated)
        {
            isRoomCreated = false;
            if (NetworkManager.Instance != null)
                NetworkManager.Instance.OnMessageReceived -= HandleServerResponse;

            if (roomCodeInput != null)
            {
                PlayerPrefs.SetString("CurrentRoomPIN", roomCodeInput.text.Trim());
            }

            SceneManager.LoadScene("MainScene");
        }
    }
}

public class RoomSetupController : RoomController
{
}