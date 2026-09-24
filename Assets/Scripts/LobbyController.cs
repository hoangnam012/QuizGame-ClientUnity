using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LobbyController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text txtJoinCode;
    public Transform playerListContainer;
    public GameObject playerRowPrefab;

    [Header("Panels & Buttons")]
    public GameObject panelLeaderboard;
    public GameObject panelQuestion;
    public GameObject btnPlay;
    public TMP_Text txtLeaderboardTimer;

    [Header("UI Đếm Ngược & Nút")]
    public Slider timerSlider;
    public TMP_Text txtTimer;
    public GameObject[] answerButtons = new GameObject[4];
    private Color[] originalButtonColors = new Color[4];

    [Header("UI Câu Hỏi")]
    public TMP_Text txtQuestion;
    public TMP_Text txtBtnA, txtBtnB, txtBtnC, txtBtnD;
    private int currentCorrectAnswer = -1;

    private string myUsername;
    private string currentRoomCode;

    // --- CỜ HIỆU QUẢN LÝ LUỒNG ---
    private bool isGameStarting = false;
    private bool hasNewData = false;
    private string[] pendingPlayers;

    private bool hasNewQuestion = false;
    private string pendingQuestionData;

    private bool hasNewLeaderboard = false;
    private string pendingLeaderboardData;

    // --- BIẾN QUẢN LÝ THỜI GIAN VÀ ĐÁP ÁN (Chỗ ông bị sót) ---
    private float currentTimer = 0f;
    private bool isTimerRunning = false;
    private bool hasAnswerResult = false;
    private int correctIndexToReveal = -1;

    void Start()
    {
        currentRoomCode = PlayerPrefs.GetString("CurrentRoomPIN", "000000");
        myUsername = PlayerPrefs.GetString("Username", "Unknown");

        if (txtJoinCode != null)
        {
            txtJoinCode.text = currentRoomCode;
        }

        NetworkManager.Instance.OnMessageReceived += HandleServerMessage;
        NetworkManager.Instance.SendPacket($"GET_LOBBY:{currentRoomCode}");

        // Lưu màu gốc của 4 nút
        for (int i = 0; i < 4; i++)
        {
            if (answerButtons[i] != null)
            {
                originalButtonColors[i] = answerButtons[i].GetComponent<Image>().color;
            }
        }
    }

    private void HandleServerMessage(string msg)
    {
        if (msg.StartsWith("LOBBY_UPDATE:"))
        {
            string namesData = msg.Substring(13);
            pendingPlayers = namesData.Split(',');
            hasNewData = true;
        }
        else if (msg.StartsWith("GAME_STARTED:"))
        {
            string roomToStart = msg.Split(':')[1].Trim();
            if (roomToStart == currentRoomCode)
            {
                Debug.Log("Game is starting for room: " + roomToStart);
                isGameStarting = true;
            }
        }
        else if (msg.StartsWith("QUESTION:"))
        {
            pendingQuestionData = msg.Substring(9);
            hasNewQuestion = true;
        }
        else if (msg.StartsWith("LEADERBOARD:"))
        {
            pendingLeaderboardData = msg.Substring(12);
            hasNewLeaderboard = true;
        }
        else if (msg.StartsWith("ANSWER_RESULT:"))
        {
            correctIndexToReveal = int.Parse(msg.Split(':')[1].Trim());
            hasAnswerResult = true;
        }
    }

    public void OnBtnPlayClicked()
    {
        NetworkManager.Instance.SendPacket($"START_GAME:{currentRoomCode}");
    }

    public void OnAnswerClicked(int answerIndex)
    {
        NetworkManager.Instance.SendPacket($"ANSWER:{currentRoomCode}:{myUsername}:{answerIndex}");
        Debug.Log("Đã nộp đáp án: " + answerIndex);

        // HIỆU ỨNG TỨC THÌ: VỪA BẤM LÀ HIỆN KẾT QUẢ NGAY!
        for (int i = 0; i < 4; i++)
        {
            if (answerButtons[i] != null)
            {
                // Khóa tất cả các nút lại để không bấm được nữa
                answerButtons[i].GetComponent<Button>().interactable = false;

                if (i + 1 == currentCorrectAnswer)
                {
                    // Nút đúng chuyển màu xanh rực
                    answerButtons[i].GetComponent<Image>().color = Color.green;
                }
                else
                {
                    // Các nút sai biến mất
                    answerButtons[i].SetActive(false);
                }
            }
        }
    }

    void Update()
    {
        // --- LOGIC ĐẾM NGƯỢC THỜI GIAN CHẠY ĐỘC LẬP ---
        if (isTimerRunning)
        {
            currentTimer -= Time.deltaTime;
            if (currentTimer <= 0)
            {
                currentTimer = 0;
                isTimerRunning = false;
            }

            if (timerSlider != null) timerSlider.value = currentTimer / 10f;
            if (txtTimer != null) txtTimer.text = Mathf.CeilToInt(currentTimer).ToString();

            if (txtLeaderboardTimer != null) txtLeaderboardTimer.text = Mathf.CeilToInt(currentTimer).ToString();
        }

        // --- KIỂM TRA CÁC SỰ KIỆN TỪ SERVER ---
        if (hasNewData)
        {
            hasNewData = false;
            UpdateLeaderboardUI(pendingPlayers, false);
        }
        else if (isGameStarting)
        {
            isGameStarting = false;
            if (btnPlay != null) btnPlay.SetActive(false);
        }
        else if (hasNewQuestion)
        {
            hasNewQuestion = false;

            if (panelLeaderboard != null) panelLeaderboard.SetActive(false);
            if (panelQuestion != null) panelQuestion.SetActive(true);

            currentTimer = 10f;
            isTimerRunning = true;

            for (int i = 0; i < 4; i++)
            {
                if (answerButtons[i] != null)
                {
                    answerButtons[i].SetActive(true);
                    answerButtons[i].GetComponent<Image>().color = originalButtonColors[i];
                    answerButtons[i].GetComponent<Button>().interactable = true; // Sáng nút lên
                }
            }

            string[] qData = pendingQuestionData.Split('|');
            // MỚI SỬA: Đảm bảo có đủ 6 mảnh (thêm mảnh đáp án đúng)
            if (qData.Length >= 6)
            {
                if (txtQuestion != null) txtQuestion.text = qData[0];
                if (txtBtnA != null) txtBtnA.text = qData[1];
                if (txtBtnB != null) txtBtnB.text = qData[2];
                if (txtBtnC != null) txtBtnC.text = qData[3];
                if (txtBtnD != null) txtBtnD.text = qData[4];

                // Lấy đáp án đúng cất vào túi
                currentCorrectAnswer = int.Parse(qData[5]);
            }
        }
        else if (hasAnswerResult)
        {
            hasAnswerResult = false;
            isTimerRunning = false; // Dừng đếm ngược

            // Tô xanh đáp án đúng, ẩn đáp án sai
            for (int i = 0; i < 4; i++)
            {
                if (answerButtons[i] != null)
                {
                    if (i + 1 == correctIndexToReveal)
                    {
                        answerButtons[i].GetComponent<Image>().color = Color.green;
                    }
                    else
                    {
                        answerButtons[i].SetActive(false);
                    }
                }
            }
        }
        else if (hasNewLeaderboard)
        {
            hasNewLeaderboard = false;

            if (panelQuestion != null) panelQuestion.SetActive(false);
            if (panelLeaderboard != null) panelLeaderboard.SetActive(true);

            currentTimer = 5f;
            isTimerRunning = true;

            string[] playersWithScores = pendingLeaderboardData.Split('|');
            UpdateLeaderboardUI(playersWithScores, true);
        }
    }

    private void UpdateLeaderboardUI(string[] data, bool hasScore)
    {
        foreach (Transform child in playerListContainer)
        {
            Destroy(child.gameObject);
        }

        float maxScore = 1f; 
        if (hasScore && data.Length > 0 && data[0].Contains(":"))
        {
            string[] top1Parts = data[0].Split(':');
            if (top1Parts.Length > 1)
            {
                float.TryParse(top1Parts[1], out maxScore);
                if (maxScore <= 0) maxScore = 1f; 
            }
        }

        for (int i = 0; i < data.Length; i++)
        {
            if (string.IsNullOrEmpty(data[i])) continue;

            GameObject newRow = Instantiate(playerRowPrefab, playerListContainer);

            string pName = data[i];
            string pScore = "0";
            float currentScore = 0f;

            if (hasScore && data[i].Contains(":"))
            {
                string[] parts = data[i].Split(':');
                pName = parts[0];
                pScore = parts[1];
                float.TryParse(pScore, out currentScore);
            }

            TMP_Text nameText = newRow.transform.Find("TxtName")?.GetComponent<TMP_Text>();
            if (nameText != null) nameText.text = pName;

            TMP_Text rankText = newRow.transform.Find("TxtRank")?.GetComponent<TMP_Text>();
            if (rankText != null) rankText.text = (i + 1).ToString();

            TMP_Text scoreText = newRow.transform.Find("TxtScore")?.GetComponent<TMP_Text>();
            if (scoreText != null) scoreText.text = pScore;

            Slider scoreSlider = newRow.transform.Find("ScoreSlider")?.GetComponent<Slider>();
            if (scoreSlider != null)
            {
                scoreSlider.value = currentScore / maxScore;
            }
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnMessageReceived -= HandleServerMessage;
        }
    }
}