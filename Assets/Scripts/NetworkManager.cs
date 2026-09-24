using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;

    private TcpClient _client;
    private NetworkStream _stream;
    private CancellationTokenSource _cancelToken;
    private bool _isConnected = false;

    // Khai báo một sự kiện để truyền thông điệp từ mạng về UI
    public event Action<string> OnMessageReceived;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async void ConnectToServer(string ip, int port)
    {
        if (_isConnected) return;

        try
        {
            _client = new TcpClient();
            await _client.ConnectAsync(ip, port);
            _stream = _client.GetStream();
            _isConnected = true;
            _cancelToken = new CancellationTokenSource();

            Debug.Log($"[TCP] Đã kết nối tới Server {ip}:{port}");
            _ = Task.Run(() => ListenForMessages(_cancelToken.Token));
        }
        catch (Exception e)
        {
            Debug.LogError($"[TCP] Lỗi kết nối: {e.Message}");
        }
    }

    public void SendPacket(string message)
    {
        if (!_isConnected || _stream == null) return;

        try
        {
            // THÊM: Đảm bảo mọi tin nhắn Unity gửi lên Server cũng có \n để chống dính chùm chiều ngược lại
            if (!message.EndsWith("\n")) message += "\n";

            byte[] data = Encoding.UTF8.GetBytes(message);
            _stream.Write(data, 0, data.Length);
        }
        catch (Exception e)
        {
            Debug.LogError($"[TCP] Lỗi gửi tin: {e.Message}");
        }
    }

    private async Task ListenForMessages(CancellationToken token)
    {
        byte[] buffer = new byte[1024];

        while (_isConnected && !token.IsCancellationRequested)
        {
            try
            {
                int bytes = await _stream.ReadAsync(buffer, 0, buffer.Length, token);
                if (bytes > 0)
                {
                    // 1. Chuyển mẻ byte nhận được thành chuỗi thô
                    string incomingData = Encoding.UTF8.GetString(buffer, 0, bytes);

                    // 2. SỬA CỐT LÕI: Chẻ các gói tin dính chùm ra thành từng mảng riêng biệt dựa vào dấu \n
                    string[] packets = incomingData.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    // 3. Xử lý từng gói tin một
                    foreach (string packet in packets)
                    {
                        // Dọn dẹp sạch sẽ ký tự thừa của gói tin này
                        string cleanPacket = packet.Replace("\0", string.Empty).Trim();

                        if (!string.IsNullOrEmpty(cleanPacket))
                        {
                            Debug.Log($"[TCP CORE NHẬN] {cleanPacket}");
                            OnMessageReceived?.Invoke(cleanPacket);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[TCP Lỗi Ngầm] {ex.Message}");
                Disconnect();
            }
        }
    }

    public void Disconnect()
    {
        if (!_isConnected) return;
        _isConnected = false;
        _cancelToken?.Cancel();
        _stream?.Close();
        _client?.Close();
    }
    private void OnApplicationQuit()
    {
        // Tự động chạy khi ông bấm Stop Play hoặc bấm X tắt game
        Debug.Log("[TCP] Đang chủ động cúp máy trước khi thoát...");
        Disconnect();
    }

    private void OnDestroy()
    {
        // Phục vụ cho việc dọn dẹp nếu chuyển cảnh lỗi
        Disconnect();
    }
}