using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

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

    // --- BIẾN QUẢN LÝ THỜI GIAN VÀ ĐÁP ÁN ---
    private float currentTimer = 0f;
    private bool isTimerRunning = false;
    private bool hasAnswerResult = false;
    private int correctIndexToReveal = -1;
    private int mySelectedAnswerIndex = -1;

    // Cache procedural sprites tươi sáng kẹo ngọt cho thiếu nhi
    private static Sprite s_questionCardSprite;
    private static Sprite s_buttonNormalSprite;
    private static Sprite s_buttonCorrectSprite;
    private static Sprite s_buttonWrongSprite;
    private static Sprite s_badgeSprite;
    private static Sprite s_timerBadgeSprite;
    private static Sprite s_timerBgSprite;
    private static Sprite s_timerFillSprite;
    private static Sprite s_doodlePillSprite;
    private static Sprite s_doodleCircleSprite;

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

        // Khởi tạo giao diện câu hỏi hoạt hình siêu tươi sáng, rõ màu 100%
        SetupKidQuizUI();

        // Lưu màu gốc của 4 nút
        for (int i = 0; i < 4; i++)
        {
            if (answerButtons[i] != null)
            {
                Image btnImg = answerButtons[i].GetComponent<Image>();
                if (btnImg != null) originalButtonColors[i] = btnImg.color;
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

    // ==========================================
    // XỬ LÝ CHỌN ĐÁP ÁN: MÀU RÕ 100%, KHÔNG BỊ MỜ HAY TRONG SUỐT
    // - ĐÚNG: Nút chuyển sang XANH LÁ ĐẬM TƯƠI 100% RÕ NÉT
    // - SAI: Nút vừa bấm chuyển sang ĐỎ TƯƠI 100% RÕ NÉT, nút đúng sáng XANH LÁ
    // - Cả 4 nút đều sáng rõ 100%, hoàn toàn không bị mờ đục!
    // ==========================================
    public void OnAnswerClicked(int answerIndex)
    {
        mySelectedAnswerIndex = answerIndex;
        NetworkManager.Instance.SendPacket($"ANSWER:{currentRoomCode}:{myUsername}:{answerIndex}");
        Debug.Log("Đã nộp đáp án: " + answerIndex);

        int chosenIdx = answerIndex - 1;

        for (int i = 0; i < 4; i++)
        {
            if (answerButtons[i] != null)
            {
                Button btnComp = answerButtons[i].GetComponent<Button>();
                if (btnComp != null)
                {
                    btnComp.transition = Selectable.Transition.None; // Tắt ColorTint để Unity không làm mờ nút!
                    btnComp.interactable = false;
                }

                Image btnImg = answerButtons[i].GetComponent<Image>();
                CanvasGroup cg = GetOrAddCanvasGroup(answerButtons[i]);
                cg.alpha = 1.0f; // 100% ĐẬM MÀU RÕ RÀNG!

                if (i + 1 == currentCorrectAnswer)
                {
                    // Nút ĐÚNG: Xanh lá neon tươi 100% rõ màu
                    if (btnImg != null && s_buttonCorrectSprite != null)
                    {
                        btnImg.sprite = s_buttonCorrectSprite;
                        btnImg.color = Color.white;
                    }
                }
                else if (i == chosenIdx)
                {
                    // Nút người chơi bấm bị SAI: Đỏ tươi 100% rõ màu
                    if (btnImg != null && s_buttonWrongSprite != null)
                    {
                        btnImg.sprite = s_buttonWrongSprite;
                        btnImg.color = Color.white;
                    }
                }
                else
                {
                    // Các nút còn lại: vẫn sáng rõ 100%, không bị mờ đục
                    if (btnImg != null && s_buttonNormalSprite != null)
                    {
                        btnImg.sprite = s_buttonNormalSprite;
                        btnImg.color = new Color(0.94f, 0.94f, 0.94f, 1.0f);
                    }
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
            mySelectedAnswerIndex = -1;

            // Reset toàn bộ 4 nút về trạng thái bình thường (Khối hồng kẹo ngọt siêu sáng rõ 100%)
            for (int i = 0; i < 4; i++)
            {
                if (answerButtons[i] != null)
                {
                    answerButtons[i].SetActive(true);
                    Button btnComp = answerButtons[i].GetComponent<Button>();
                    if (btnComp != null)
                    {
                        btnComp.transition = Selectable.Transition.None;
                        btnComp.interactable = true;
                    }

                    Image btnImg = answerButtons[i].GetComponent<Image>();
                    if (btnImg != null && s_buttonNormalSprite != null)
                    {
                        btnImg.sprite = s_buttonNormalSprite;
                        btnImg.color = Color.white;
                    }

                    CanvasGroup cg = GetOrAddCanvasGroup(answerButtons[i]);
                    cg.alpha = 1.0f;
                }
            }

            string[] qData = pendingQuestionData.Split('|');
            if (qData.Length >= 6)
            {
                if (txtQuestion != null) txtQuestion.text = qData[0];
                if (txtBtnA != null) txtBtnA.text = FormatOptionText("A.", qData[1]);
                if (txtBtnB != null) txtBtnB.text = FormatOptionText("B.", qData[2]);
                if (txtBtnC != null) txtBtnC.text = FormatOptionText("C.", qData[3]);
                if (txtBtnD != null) txtBtnD.text = FormatOptionText("D.", qData[4]);

                currentCorrectAnswer = int.Parse(qData[5]);
            }
        }
        else if (hasAnswerResult)
        {
            hasAnswerResult = false;
            isTimerRunning = false;

            for (int i = 0; i < 4; i++)
            {
                if (answerButtons[i] != null)
                {
                    Button btnComp = answerButtons[i].GetComponent<Button>();
                    if (btnComp != null)
                    {
                        btnComp.transition = Selectable.Transition.None;
                        btnComp.interactable = false;
                    }

                    Image btnImg = answerButtons[i].GetComponent<Image>();
                    CanvasGroup cg = GetOrAddCanvasGroup(answerButtons[i]);
                    cg.alpha = 1.0f; // 100% RÕ MÀU!

                    if (i + 1 == correctIndexToReveal)
                    {
                        if (btnImg != null && s_buttonCorrectSprite != null)
                        {
                            btnImg.sprite = s_buttonCorrectSprite;
                            btnImg.color = Color.white;
                        }
                    }
                    else if (i == mySelectedAnswerIndex - 1)
                    {
                        if (btnImg != null && s_buttonWrongSprite != null)
                        {
                            btnImg.sprite = s_buttonWrongSprite;
                            btnImg.color = Color.white;
                        }
                    }
                    else
                    {
                        if (btnImg != null && s_buttonNormalSprite != null)
                        {
                            btnImg.sprite = s_buttonNormalSprite;
                            btnImg.color = new Color(0.94f, 0.94f, 0.94f, 1.0f);
                        }
                    }
                }
            }
        }
        else if (hasNewLeaderboard)
        {
            hasNewLeaderboard = false;

            if (panelQuestion != null) panelQuestion.SetActive(false);
            if (panelLeaderboard != null) panelLeaderboard.SetActive(true);

            string[] playersWithScores = pendingLeaderboardData.Split(',');
            UpdateLeaderboardUI(playersWithScores, true);
        }
    }

    private string FormatOptionText(string prefix, string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "";
        raw = raw.Trim();
        if (raw.StartsWith("a.", System.StringComparison.OrdinalIgnoreCase) ||
            raw.StartsWith("b.", System.StringComparison.OrdinalIgnoreCase) ||
            raw.StartsWith("c.", System.StringComparison.OrdinalIgnoreCase) ||
            raw.StartsWith("d.", System.StringComparison.OrdinalIgnoreCase))
        {
            return raw;
        }
        return $"{prefix}  {raw}";
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject go)
    {
        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        return cg;
    }

    // ==========================================
    // THIẾT KẾ GIAO DIỆN HOẠT HÌNH SIÊU TƯƠI SÁNG, 100% RÕ MÀU
    // ==========================================
    private void SetupKidQuizUI()
    {
        if (panelQuestion == null) return;

        // 0. Đảm bảo CanvasScaler co giãn theo màn hình (Reference: 1920x1080)
        CanvasScaler scaler = panelQuestion.GetComponentInParent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        // 1. Tạo các Procedural Sprite 100% ĐẬM ĐẶC, RỰC RỠ, KHÔNG BỊ TRONG SUỐT HAY MỜ
        if (s_questionCardSprite == null)
        {
            // Thẻ câu hỏi: Vàng chanh tươi sáng (#FFF733) -> Vàng cam đậm (#FFA000)
            s_questionCardSprite = CreateBright3DCapsuleSprite(
                512, 128,
                new Color(1.0f, 0.97f, 0.20f, 1f),
                new Color(1.0f, 0.63f, 0.00f, 1f),
                Color.white,
                9,
                true
            );
        }

        if (s_buttonNormalSprite == null)
        {
            // Nút đáp án thường: Hồng kẹo ngọt (#FFA0DC) -> Hồng cánh sen đậm (#FF1E88)
            s_buttonNormalSprite = CreateBright3DCapsuleSprite(
                512, 128,
                new Color(1.0f, 0.63f, 0.86f, 1f),
                new Color(1.0f, 0.12f, 0.53f, 1f),
                Color.white,
                8,
                true
            );
        }

        if (s_buttonCorrectSprite == null)
        {
            // Nút đáp án đúng: Xanh lá neon rực rỡ (#00FF66) -> Xanh ngọc tươi đậm (#00C853) - 100% RÕ MÀU
            s_buttonCorrectSprite = CreateBright3DCapsuleSprite(
                512, 128,
                new Color(0.00f, 1.00f, 0.40f, 1f),
                new Color(0.00f, 0.78f, 0.32f, 1f),
                Color.white,
                8,
                true
            );
        }

        if (s_buttonWrongSprite == null)
        {
            // Nút đáp án sai: Đỏ tươi rực (#FF2A3A) -> Đỏ thắm đậm (#D50000) - 100% RÕ MÀU
            s_buttonWrongSprite = CreateBright3DCapsuleSprite(
                512, 128,
                new Color(1.0f, 0.16f, 0.23f, 1f),
                new Color(0.83f, 0.00f, 0.00f, 1f),
                Color.white,
                8,
                true
            );
        }

        if (s_badgeSprite == null)
        {
            s_badgeSprite = CreateBright3DCircleSprite(128, Color.white, new Color(0.92f, 0.95f, 0.98f, 1f), 3);
        }

        if (s_timerBadgeSprite == null)
        {
            s_timerBadgeSprite = CreateBright3DCircleSprite(128, new Color(1f, 0.90f, 0.20f, 1f), Color.white, 4);
        }

        if (s_timerBgSprite == null)
        {
            s_timerBgSprite = CreateBright3DCapsuleSprite(512, 64, new Color(1f, 1f, 1f, 0.35f), new Color(1f, 1f, 1f, 0.25f), Color.white, 0, false);
        }

        if (s_timerFillSprite == null)
        {
            s_timerFillSprite = CreateBright3DCapsuleSprite(512, 64, Color.white, new Color(1f, 0.92f, 0.35f, 1f), Color.white, 0, false);
        }

        if (s_doodlePillSprite == null)
        {
            s_doodlePillSprite = CreateBright3DCapsuleSprite(256, 64, Color.white, Color.white, Color.white, 0, false);
        }

        if (s_doodleCircleSprite == null)
        {
            s_doodleCircleSprite = CreateBright3DCircleSprite(64, Color.white, Color.white, 0);
        }

        // 2. Màu nền xanh da trời SIÊU TƯƠI SÁNG
        Image panelImg = panelQuestion.GetComponent<Image>();
        if (panelImg != null)
        {
            panelImg.enabled = true;
            panelImg.color = new Color(0.35f, 0.82f, 0.93f, 1f);
        }

        // 3. Tạo các icon toán học và họa tiết trang trí 4 góc (Cộng, Trừ, Nhân, Chia)
        SetupBackgroundDoodles();

        // 4. Khung dây nối trắng (Connector Lines)
        Transform existingFrame = panelQuestion.transform.Find("KidConnectorFrame");
        GameObject frameGo = existingFrame != null ? existingFrame.gameObject : new GameObject("KidConnectorFrame");
        frameGo.transform.SetParent(panelQuestion.transform, false);
        frameGo.transform.SetSiblingIndex(1);

        for (int i = frameGo.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(frameGo.transform.GetChild(i).gameObject);
        }

        // Khung dây bên trái
        CreateLine(frameGo.transform, "L_Spine", new Vector2(-710f, 65f), new Vector2(8f, 360f));
        CreateLine(frameGo.transform, "L_TopBranch", new Vector2(-695f, 240f), new Vector2(35f, 8f));
        CreateLine(frameGo.transform, "L_BranchA", new Vector2(-695f, 50f), new Vector2(35f, 8f));
        CreateLine(frameGo.transform, "L_BranchC", new Vector2(-695f, -110f), new Vector2(35f, 8f));

        // Khung dây bên phải
        CreateLine(frameGo.transform, "R_Spine", new Vector2(710f, 65f), new Vector2(8f, 360f));
        CreateLine(frameGo.transform, "R_TopBranch", new Vector2(695f, 240f), new Vector2(35f, 8f));
        CreateLine(frameGo.transform, "R_BranchB", new Vector2(695f, 50f), new Vector2(35f, 8f));
        CreateLine(frameGo.transform, "R_BranchD", new Vector2(695f, -110f), new Vector2(35f, 8f));

        // 5. Khối câu hỏi lớn bo tròn (Question Card Background)
        Transform existingBg = panelQuestion.transform.Find("KidQuestionCardBg");
        GameObject questionCardBg = existingBg != null ? existingBg.gameObject : new GameObject("KidQuestionCardBg");
        questionCardBg.transform.SetParent(panelQuestion.transform, false);

        RectTransform rtBg = questionCardBg.GetComponent<RectTransform>();
        if (rtBg == null) rtBg = questionCardBg.AddComponent<RectTransform>();
        rtBg.anchorMin = new Vector2(0.5f, 0.5f);
        rtBg.anchorMax = new Vector2(0.5f, 0.5f);
        rtBg.pivot = new Vector2(0.5f, 0.5f);
        rtBg.anchoredPosition = new Vector2(0f, 240f);
        rtBg.sizeDelta = new Vector2(1380f, 200f);
        rtBg.localScale = Vector3.one;

        Image imgBg = questionCardBg.GetComponent<Image>();
        if (imgBg == null) imgBg = questionCardBg.AddComponent<Image>();
        imgBg.sprite = s_questionCardSprite;
        imgBg.type = Image.Type.Simple;
        imgBg.color = Color.white;
        imgBg.raycastTarget = false;

        Shadow qShadow = questionCardBg.GetComponent<Shadow>();
        if (qShadow == null) qShadow = questionCardBg.AddComponent<Shadow>();
        qShadow.effectColor = new Color(0.15f, 0.65f, 0.80f, 0.25f);
        qShadow.effectDistance = new Vector2(0f, -6f);

        // 6. Chữ câu hỏi TxtQuestion
        if (txtQuestion != null)
        {
            RectTransform rtQ = txtQuestion.GetComponent<RectTransform>();
            rtQ.anchorMin = new Vector2(0.5f, 0.5f);
            rtQ.anchorMax = new Vector2(0.5f, 0.5f);
            rtQ.pivot = new Vector2(0.5f, 0.5f);
            rtQ.anchoredPosition = new Vector2(0f, 240f);
            rtQ.sizeDelta = new Vector2(1260f, 170f);
            rtQ.localScale = Vector3.one;

            txtQuestion.color = new Color(0.12f, 0.12f, 0.16f, 1f);
            txtQuestion.fontStyle = FontStyles.Bold;
            txtQuestion.fontSize = 44f;
            txtQuestion.alignment = TextAlignmentOptions.Center;
            txtQuestion.enableWordWrapping = true;
            txtQuestion.margin = new Vector4(40f, 10f, 40f, 10f);

            questionCardBg.transform.SetSiblingIndex(txtQuestion.transform.GetSiblingIndex());
            txtQuestion.transform.SetSiblingIndex(questionCardBg.transform.GetSiblingIndex() + 1);
        }

        // 7. Huy hiệu vòng tròn dấu chấm hỏi "?" ở đỉnh câu hỏi
        Transform existingBadge = panelQuestion.transform.Find("KidQuestionBadge");
        GameObject badgeGo = existingBadge != null ? existingBadge.gameObject : new GameObject("KidQuestionBadge");
        badgeGo.transform.SetParent(panelQuestion.transform, false);

        RectTransform rtBadge = badgeGo.GetComponent<RectTransform>();
        if (rtBadge == null) rtBadge = badgeGo.AddComponent<RectTransform>();
        rtBadge.anchorMin = new Vector2(0.5f, 0.5f);
        rtBadge.anchorMax = new Vector2(0.5f, 0.5f);
        rtBadge.pivot = new Vector2(0.5f, 0.5f);
        rtBadge.anchoredPosition = new Vector2(0f, 340f);
        rtBadge.sizeDelta = new Vector2(92f, 92f);
        rtBadge.localScale = Vector3.one;

        Image imgBadge = badgeGo.GetComponent<Image>();
        if (imgBadge == null) imgBadge = badgeGo.AddComponent<Image>();
        imgBadge.sprite = s_badgeSprite;
        imgBadge.color = Color.white;
        imgBadge.raycastTarget = false;

        Shadow bShadow = badgeGo.GetComponent<Shadow>();
        if (bShadow == null) bShadow = badgeGo.AddComponent<Shadow>();
        bShadow.effectColor = new Color(0.15f, 0.65f, 0.80f, 0.20f);
        bShadow.effectDistance = new Vector2(0f, -4f);

        Transform existingBadgeTxt = badgeGo.transform.Find("TxtQuestionMark");
        GameObject badgeTxtGo = existingBadgeTxt != null ? existingBadgeTxt.gameObject : new GameObject("TxtQuestionMark");
        badgeTxtGo.transform.SetParent(badgeGo.transform, false);

        RectTransform rtBadgeTxt = badgeTxtGo.GetComponent<RectTransform>();
        if (rtBadgeTxt == null) rtBadgeTxt = badgeTxtGo.AddComponent<RectTransform>();
        rtBadgeTxt.anchorMin = Vector2.zero;
        rtBadgeTxt.anchorMax = Vector2.one;
        rtBadgeTxt.offsetMin = Vector2.zero;
        rtBadgeTxt.offsetMax = Vector2.zero;
        rtBadgeTxt.localScale = Vector3.one;

        TextMeshProUGUI tmpMark = badgeTxtGo.GetComponent<TextMeshProUGUI>();
        if (tmpMark == null) tmpMark = badgeTxtGo.AddComponent<TextMeshProUGUI>();
        tmpMark.text = "?";
        tmpMark.fontStyle = FontStyles.Bold;
        tmpMark.fontSize = 58f;
        tmpMark.color = new Color(0.12f, 0.12f, 0.16f, 1f);
        tmpMark.alignment = TextAlignmentOptions.Center;
        tmpMark.raycastTarget = false;

        badgeGo.transform.SetSiblingIndex(panelQuestion.transform.childCount - 1);

        // 8. Căn 4 nút đáp án thành LƯỚI 2x2 lớn với màu sắc 100% rõ nét
        Vector2[] btnPositions = new Vector2[]
        {
            new Vector2(-360f, 50f),   // Nút A: Hàng trên, bên trái
            new Vector2(360f, 50f),    // Nút B: Hàng trên, bên phải
            new Vector2(-360f, -110f), // Nút C: Hàng dưới, bên trái
            new Vector2(360f, -110f)   // Nút D: Hàng dưới, bên phải
        };

        TMP_Text[] btnTexts = new TMP_Text[] { txtBtnA, txtBtnB, txtBtnC, txtBtnD };

        for (int i = 0; i < 4; i++)
        {
            if (answerButtons[i] != null)
            {
                RectTransform rtBtn = answerButtons[i].GetComponent<RectTransform>();
                rtBtn.anchorMin = new Vector2(0.5f, 0.5f);
                rtBtn.anchorMax = new Vector2(0.5f, 0.5f);
                rtBtn.pivot = new Vector2(0.5f, 0.5f);
                rtBtn.anchoredPosition = btnPositions[i];
                rtBtn.sizeDelta = new Vector2(660f, 120f);
                rtBtn.localScale = Vector3.one;

                // Tắt transition tự động của Button để ngăn chặn Unity làm mờ xỉn nút
                Button btnComp = answerButtons[i].GetComponent<Button>();
                if (btnComp != null)
                {
                    btnComp.transition = Selectable.Transition.None;
                    ColorBlock cb = btnComp.colors;
                    cb.normalColor = Color.white;
                    cb.disabledColor = Color.white; // Không làm mờ khi disabled
                    btnComp.colors = cb;
                }

                Image img = answerButtons[i].GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = s_buttonNormalSprite;
                    img.type = Image.Type.Simple;
                    img.color = Color.white;
                    originalButtonColors[i] = Color.white;
                }

                Shadow btnShadow = answerButtons[i].GetComponent<Shadow>();
                if (btnShadow == null) btnShadow = answerButtons[i].AddComponent<Shadow>();
                btnShadow.effectColor = new Color(0.15f, 0.65f, 0.80f, 0.22f);
                btnShadow.effectDistance = new Vector2(0f, -5f);

                CanvasGroup cg = GetOrAddCanvasGroup(answerButtons[i]);
                cg.alpha = 1.0f; // Luôn giữ 100% rõ nét

                if (btnTexts[i] != null)
                {
                    RectTransform txtRt = btnTexts[i].GetComponent<RectTransform>();
                    txtRt.anchorMin = new Vector2(0f, 0f);
                    txtRt.anchorMax = new Vector2(1f, 1f);
                    txtRt.pivot = new Vector2(0.5f, 0.5f);
                    txtRt.offsetMin = new Vector2(55f, 0f);
                    txtRt.offsetMax = new Vector2(-40f, 0f);
                    txtRt.anchoredPosition = Vector2.zero;
                    txtRt.localScale = Vector3.one;

                    btnTexts[i].color = new Color(0.12f, 0.12f, 0.16f, 1f);
                    btnTexts[i].fontStyle = FontStyles.Bold;
                    btnTexts[i].fontSize = 38f;
                    btnTexts[i].alignment = TextAlignmentOptions.MidlineLeft;
                    btnTexts[i].margin = Vector4.zero;
                }
            }
        }

        // 9. Căn thanh thời gian (Timer Slider) và số đếm (TxtTimer) hiện đại bên dưới
        if (timerSlider != null)
        {
            RectTransform rtSlider = timerSlider.GetComponent<RectTransform>();
            rtSlider.anchorMin = new Vector2(0.5f, 0.5f);
            rtSlider.anchorMax = new Vector2(0.5f, 0.5f);
            rtSlider.pivot = new Vector2(0.5f, 0.5f);
            rtSlider.anchoredPosition = new Vector2(40f, -250f);
            rtSlider.sizeDelta = new Vector2(900f, 26f);
            rtSlider.localScale = Vector3.one;

            Transform bgTransform = timerSlider.transform.Find("Background");
            if (bgTransform != null)
            {
                Image bgImg = bgTransform.GetComponent<Image>();
                if (bgImg != null)
                {
                    bgImg.sprite = s_timerBgSprite;
                    bgImg.color = Color.white;
                }
            }

            Transform fillTransform = timerSlider.transform.Find("Fill Area/Fill");
            if (fillTransform != null)
            {
                Image fillImg = fillTransform.GetComponent<Image>();
                if (fillImg != null)
                {
                    fillImg.sprite = s_timerFillSprite;
                    fillImg.color = Color.white;
                }
            }
        }

        // Tạo vòng tròn đồng hồ nổi bật cho số đếm giây txtTimer
        Transform existingTimerBadge = panelQuestion.transform.Find("KidTimerBadge");
        GameObject timerBadgeGo = existingTimerBadge != null ? existingTimerBadge.gameObject : new GameObject("KidTimerBadge");
        timerBadgeGo.transform.SetParent(panelQuestion.transform, false);

        RectTransform rtTBadge = timerBadgeGo.GetComponent<RectTransform>();
        if (rtTBadge == null) rtTBadge = timerBadgeGo.AddComponent<RectTransform>();
        rtTBadge.anchorMin = new Vector2(0.5f, 0.5f);
        rtTBadge.anchorMax = new Vector2(0.5f, 0.5f);
        rtTBadge.pivot = new Vector2(0.5f, 0.5f);
        rtTBadge.anchoredPosition = new Vector2(-465f, -250f);
        rtTBadge.sizeDelta = new Vector2(68f, 68f);
        rtTBadge.localScale = Vector3.one;

        Image imgTBadge = timerBadgeGo.GetComponent<Image>();
        if (imgTBadge == null) imgTBadge = timerBadgeGo.AddComponent<Image>();
        imgTBadge.sprite = s_timerBadgeSprite;
        imgTBadge.color = Color.white;
        imgTBadge.raycastTarget = false;

        Shadow tShadow = timerBadgeGo.GetComponent<Shadow>();
        if (tShadow == null) tShadow = timerBadgeGo.AddComponent<Shadow>();
        tShadow.effectColor = new Color(0.15f, 0.65f, 0.80f, 0.20f);
        tShadow.effectDistance = new Vector2(0f, -4f);

        if (txtTimer != null)
        {
            RectTransform rtTimer = txtTimer.GetComponent<RectTransform>();
            rtTimer.anchorMin = new Vector2(0.5f, 0.5f);
            rtTimer.anchorMax = new Vector2(0.5f, 0.5f);
            rtTimer.pivot = new Vector2(0.5f, 0.5f);
            rtTimer.anchoredPosition = new Vector2(-465f, -250f);
            rtTimer.sizeDelta = new Vector2(68f, 68f);
            rtTimer.localScale = Vector3.one;

            txtTimer.color = new Color(0.12f, 0.12f, 0.16f, 1f);
            txtTimer.fontStyle = FontStyles.Bold;
            txtTimer.fontSize = 34f;
            txtTimer.alignment = TextAlignmentOptions.Center;

            txtTimer.transform.SetSiblingIndex(timerBadgeGo.transform.GetSiblingIndex() + 1);
        }

        // 10. Ẩn các GameObject rác / sticker cũ trong Panel_Question để không che khuất UI
        HashSet<string> keepNames = new HashSet<string>()
        {
            "TxtQuestion", "KidQuestionCardBg", "KidQuestionBadge", "KidConnectorFrame",
            "TxtBtnA", "TxtBtnB", "TxtBtnC", "TxtBtnD",
            "Slider", "KidTimerBadge", "KidBackgroundDoodles"
        };

        for (int i = 0; i < panelQuestion.transform.childCount; i++)
        {
            Transform child = panelQuestion.transform.GetChild(i);
            if (keepNames.Contains(child.name)) continue;
            if (txtTimer != null && child == txtTimer.transform) continue;
            if (timerSlider != null && child == timerSlider.transform) continue;

            child.gameObject.SetActive(false);
        }
    }

    // ==========================================
    // TẠO HỌA TIẾT CỘNG TRỪ NHÂN CHIA VÀ DOODLE HOẠT HÌNH 4 GÓC MÀN HÌNH
    // ==========================================
    private void SetupBackgroundDoodles()
    {
        Transform existingDoodles = panelQuestion.transform.Find("KidBackgroundDoodles");
        GameObject doodlesGo = existingDoodles != null ? existingDoodles.gameObject : new GameObject("KidBackgroundDoodles");
        doodlesGo.transform.SetParent(panelQuestion.transform, false);
        doodlesGo.transform.SetSiblingIndex(0);

        RectTransform rtDoodles = doodlesGo.GetComponent<RectTransform>();
        if (rtDoodles == null) rtDoodles = doodlesGo.AddComponent<RectTransform>();
        rtDoodles.anchorMin = Vector2.zero;
        rtDoodles.anchorMax = Vector2.one;
        rtDoodles.offsetMin = Vector2.zero;
        rtDoodles.offsetMax = Vector2.zero;
        rtDoodles.localScale = Vector3.one;

        for (int i = doodlesGo.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(doodlesGo.transform.GetChild(i).gameObject);
        }

        Color doodleColor = new Color(1f, 1f, 1f, 0.42f);
        Color doodleSoft = new Color(1f, 1f, 1f, 0.30f);

        // --- GÓC TRÊN TRÁI (TOP-LEFT): Họa tiết Dấu Trừ (−), Thanh bo tròn và Vòng tròn ---
        Vector2 tl = new Vector2(0f, 1f);
        CreateDoodleShape(doodlesGo.transform, "TL_MinusBar1", s_doodlePillSprite, tl, new Vector2(130f, -80f), new Vector2(150f, 28f), doodleColor, 0f);
        CreateDoodleShape(doodlesGo.transform, "TL_Circle1", s_doodleCircleSprite, tl, new Vector2(235f, -80f), new Vector2(28f, 28f), doodleColor, 0f);
        CreateDoodleShape(doodlesGo.transform, "TL_MinusBar2", s_doodlePillSprite, tl, new Vector2(105f, -125f), new Vector2(100f, 28f), doodleColor, 0f);
        CreateDoodleShape(doodlesGo.transform, "TL_Circle2", s_doodleCircleSprite, tl, new Vector2(180f, -125f), new Vector2(24f, 24f), doodleColor, 0f);
        CreateDoodleShape(doodlesGo.transform, "TL_Dot1", s_doodleCircleSprite, tl, new Vector2(65f, -170f), new Vector2(18f, 18f), doodleSoft, 0f);
        CreateDoodleText(doodlesGo.transform, "TL_Arc", "C", tl, new Vector2(150f, -180f), 65f, doodleColor, 20f);

        // --- GÓC TRÊN PHẢI (TOP-RIGHT): Dấu Cộng (+), Dấu Chia (/), Vòng cung (C) ---
        Vector2 tr = new Vector2(1f, 1f);
        CreateDoodleText(doodlesGo.transform, "TR_Plus", "+", tr, new Vector2(-120f, -95f), 95f, doodleColor, 0f);
        CreateDoodleText(doodlesGo.transform, "TR_Arc", "C", tr, new Vector2(-225f, -80f), 85f, doodleColor, -15f);
        CreateDoodleText(doodlesGo.transform, "TR_Slash", "/", tr, new Vector2(-85f, -190f), 80f, doodleColor, 35f);
        CreateDoodleShape(doodlesGo.transform, "TR_Dot1", s_doodleCircleSprite, tr, new Vector2(-170f, -175f), new Vector2(26f, 26f), doodleColor, 0f);
        CreateDoodleShape(doodlesGo.transform, "TR_Dot2", s_doodleCircleSprite, tr, new Vector2(-125f, -240f), new Vector2(18f, 18f), doodleSoft, 0f);

        // --- GÓC DƯỚI TRÁI (BOTTOM-LEFT): Dấu Nhân (×), Dấu Cộng (+), Vòng cung (C) ---
        Vector2 bl = new Vector2(0f, 0f);
        CreateDoodleText(doodlesGo.transform, "BL_Multiply", "×", bl, new Vector2(110f, 175f), 95f, doodleColor, 0f);
        CreateDoodleText(doodlesGo.transform, "BL_Arc", "C", bl, new Vector2(135f, 90f), 90f, doodleColor, -25f);
        CreateDoodleText(doodlesGo.transform, "BL_Plus", "+", bl, new Vector2(200f, 170f), 55f, doodleColor, 0f);
        CreateDoodleShape(doodlesGo.transform, "BL_Dot1", s_doodleCircleSprite, bl, new Vector2(70f, 100f), new Vector2(26f, 26f), doodleColor, 0f);
        CreateDoodleShape(doodlesGo.transform, "BL_Dot2", s_doodleCircleSprite, bl, new Vector2(215f, 90f), new Vector2(20f, 20f), doodleSoft, 0f);

        // --- GÓC DƯỚI PHẢI (BOTTOM-RIGHT): Dấu Chia (÷), Dấu Bằng (=), Vòng tròn ---
        Vector2 br = new Vector2(1f, 0f);
        CreateDoodleText(doodlesGo.transform, "BR_Divide", "÷", br, new Vector2(-115f, 175f), 100f, doodleColor, 0f);
        CreateDoodleShape(doodlesGo.transform, "BR_Equal1", s_doodlePillSprite, br, new Vector2(-120f, 100f), new Vector2(90f, 22f), doodleColor, 0f);
        CreateDoodleShape(doodlesGo.transform, "BR_Equal2", s_doodlePillSprite, br, new Vector2(-120f, 70f), new Vector2(90f, 22f), doodleColor, 0f);
        CreateDoodleText(doodlesGo.transform, "BR_Arc", "C", br, new Vector2(-210f, 140f), 75f, doodleColor, 35f);
        CreateDoodleShape(doodlesGo.transform, "BR_Dot1", s_doodleCircleSprite, br, new Vector2(-65f, 80f), new Vector2(22f, 22f), doodleColor, 0f);
        CreateDoodleShape(doodlesGo.transform, "BR_Dot2", s_doodleCircleSprite, br, new Vector2(-205f, 70f), new Vector2(20f, 20f), doodleSoft, 0f);
    }

    private static void CreateDoodleText(Transform parent, string name, string text, Vector2 anchor, Vector2 pos, float size, Color color, float rotation = 0f)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(size * 1.6f, size * 1.6f);
        rt.localEulerAngles = new Vector3(0, 0, rotation);
        rt.localScale = Vector3.one;

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
    }

    private static void CreateDoodleShape(Transform parent, string name, Sprite sprite, Vector2 anchor, Vector2 pos, Vector2 size, Color color, float rotation = 0f)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        rt.localEulerAngles = new Vector3(0, 0, rotation);
        rt.localScale = Vector3.one;

        Image img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
    }

    private static void CreateLine(Transform parent, string name, Vector2 pos, Vector2 size)
    {
        GameObject line = new GameObject(name);
        line.transform.SetParent(parent, false);
        RectTransform rt = line.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        rt.localScale = Vector3.one;

        Image img = line.AddComponent<Image>();
        img.color = Color.white;
        img.raycastTarget = false;
    }

    // ==========================================
    // THUẬT TOÁN TẠO KHỐI 3D 100% ĐẬM ĐẶC RÕ MÀU (KHÔNG BỊ MỜ HAY XỈN TỐI)
    // ==========================================
    private static Sprite CreateBright3DCapsuleSprite(int width, int height, Color lightColor, Color saturatedColor, Color borderColor, int borderWidth, bool hasGlossyBubble)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        float r = height / 2f;
        Color[] pixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            float ny = (float)y / (height - 1); // 0 ở đáy, 1 ở đỉnh

            for (int x = 0; x < width; x++)
            {
                float nx = (float)x / (width - 1); // 0 ở trái, 1 ở phải

                float cx = Mathf.Clamp(x + 0.5f, r, width - r);
                float cy = r;
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));

                float outerAlpha = Mathf.Clamp01(r - dist + 0.5f);
                if (outerAlpha <= 0f)
                {
                    pixels[y * width + x] = Color.clear;
                    continue;
                }

                float borderDist = r - dist;
                Color c;

                if (borderDist < borderWidth)
                {
                    // Viền trắng tinh khôi dày sắc nét
                    c = borderColor;
                }
                else
                {
                    // Chuyển màu mượt mà: Bên trái sáng pastel rực rỡ, bên phải là màu tươi nguyên bản (100% rực rỡ, không bị xỉn đen)
                    float diagT = (1.0f - nx) * 0.55f + ny * 0.45f;
                    diagT = Mathf.Clamp01(diagT);

                    Color fill = Color.Lerp(saturatedColor, lightColor, diagT);

                    // Vệt sáng lấp lánh (Top shine) dọc theo đường cong mép trên
                    if (ny > 0.55f)
                    {
                        float topShine = (ny - 0.55f) / 0.45f;
                        fill = Color.Lerp(fill, Color.white, topShine * 0.48f);
                    }

                    // Đốm sáng tròn bóng kính 3D (Glossy Bubble Specular) ở góc trên-trái như viên kẹo thạch 3D
                    if (hasGlossyBubble)
                    {
                        Vector2 bubbleCenter = new Vector2(r * 1.05f, r * 1.25f);
                        float bubbleRadius = r * 0.72f;
                        float bDist = Vector2.Distance(new Vector2(x, y), bubbleCenter) / bubbleRadius;
                        if (bDist < 1.0f)
                        {
                            float bShine = Mathf.SmoothStep(1.0f, 0.0f, bDist) * 0.65f;
                            fill = Color.Lerp(fill, Color.white, bShine);
                        }
                    }

                    float innerEdge = Mathf.Clamp01(borderDist - borderWidth + 0.5f);
                    c = Color.Lerp(borderColor, fill, innerEdge);
                }

                c.a *= outerAlpha;
                pixels[y * width + x] = c;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
    }

    private static Sprite CreateBright3DCircleSprite(int size, Color fillColor, Color borderColor, int borderWidth)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        float r = size / 2f;
        Vector2 center = new Vector2(r, r);
        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            float ny = (float)y / (size - 1);

            for (int x = 0; x < size; x++)
            {
                float nx = (float)x / (size - 1);
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                float outerAlpha = Mathf.Clamp01(r - dist + 0.5f);
                if (outerAlpha <= 0f)
                {
                    pixels[y * size + x] = Color.clear;
                    continue;
                }

                float borderDist = r - dist;
                Color c;
                if (borderDist < borderWidth)
                {
                    c = borderColor;
                }
                else
                {
                    float diagT = (1.0f - nx) * 0.5f + ny * 0.5f;
                    Color sphereFill = Color.Lerp(fillColor * 0.94f, fillColor, diagT);

                    float innerEdge = Mathf.Clamp01(borderDist - borderWidth + 0.5f);
                    c = Color.Lerp(borderColor, sphereFill, innerEdge);
                }
                c.a *= outerAlpha;
                pixels[y * size + x] = c;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    private void UpdateLeaderboardUI(string[] data, bool hasScore)
    {
        foreach (Transform child in playerListContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < data.Length; i++)
        {
            if (string.IsNullOrEmpty(data[i])) continue;

            GameObject newRow = Instantiate(playerRowPrefab, playerListContainer);

            string pName = data[i];
            string pScore = "0";

            if (hasScore && data[i].Contains(":"))
            {
                string[] parts = data[i].Split(':');
                pName = parts[0];
                pScore = parts[1];
            }

            TMP_Text nameText = newRow.transform.Find("TxtName")?.GetComponent<TMP_Text>();
            if (nameText != null) nameText.text = pName;

            TMP_Text rankText = newRow.transform.Find("TxtRank")?.GetComponent<TMP_Text>();
            if (rankText != null) rankText.text = (i + 1).ToString();

            TMP_Text scoreText = newRow.transform.Find("TxtScore")?.GetComponent<TMP_Text>();
            if (scoreText != null) scoreText.text = pScore;
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
