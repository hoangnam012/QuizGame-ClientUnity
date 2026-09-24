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

    // 1. Gắn vào các nút Lớp 1, 2, 3, 4, 5
    public void SelectClass(string className)
    {
        selectedClass = className;
        Debug.Log("Đã chọn: " + selectedClass);
    }

    // 2. Gắn vào các nút Toán, Tiếng Việt, Tiếng Anh, Tư Duy
    public void SelectSubject(string subjectName)
    {
        selectedSubject = subjectName;
        Debug.Log("Đã chọn: " + selectedSubject);
    }

    // 3. Gắn vào nút "Tạo phòng"
    public void OnBtnCreateRoomClicked()
    {
        string roomCode = roomCodeInput != null ? roomCodeInput.text.Trim() : "";

        // Kiểm tra xem đã điền mã code và chọn đủ thông tin chưa
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

        // Đăng ký nhận phản hồi từ Server
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnMessageReceived -= HandleServerResponse;
            NetworkManager.Instance.OnMessageReceived += HandleServerResponse;

            // Gửi gói tin lên Server kèm luôn cái mã code tự nhập
            // Cú pháp: CREATE_ROOM:[Mã_Code]:[Lớp]:[Môn]
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

    // 4. Lắng nghe phản hồi từ Server
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

    // 5. Chuyển scene an toàn trên luồng chính
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

// Giữ cả 2 tên class để tương thích tuyệt đối mọi nơi trong Unity
public class RoomSetupController : RoomController
{
}