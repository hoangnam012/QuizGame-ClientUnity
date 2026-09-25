using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[ExecuteAlways]
public class RoomUIController : MonoBehaviour
{
    [Header("Controller Reference")]
    public RoomController roomController;

    private TMP_FontAsset mainFont;

    private Sprite bgSprite;
    private Sprite blurredBgSprite;
    private Sprite glassPanelRound;
    private Sprite glassPill;
    private Sprite[] pastelPills = new Sprite[5];
    private Sprite candyBtnSprite;
    private Sprite backPillSprite;
    private Sprite keyIconSprite;
    private Sprite[] cleanIcons = new Sprite[4];
    private Sprite selectionGlowCardSprite;
    private Sprite selectionGlowPillSprite;
    private Sprite checkmarkBadgeSprite;

    private GameObject[] classBtnObjects = new GameObject[5];
    private GameObject[] classGlowObjects = new GameObject[5];
    private GameObject[] classCheckObjects = new GameObject[5];
    private CanvasGroup[] classCanvasGroups = new CanvasGroup[5];
    private Coroutine[] classCoroutines = new Coroutine[5];

    private GameObject[] subjectCardObjects = new GameObject[4];
    private GameObject[] subjectGlowObjects = new GameObject[4];
    private GameObject[] subjectCheckObjects = new GameObject[4];
    private CanvasGroup[] subjectCanvasGroups = new CanvasGroup[4];
    private Coroutine[] subjectCoroutines = new Coroutine[4];

    public string currentSelectedClass = "Lop1";
    public string currentSelectedSubject = "ToanHoc";
    private bool isInitialized = false;

    private void Awake()
    {
        if (!isInitialized)
        {
            InitializeUI();
            isInitialized = true;
        }
    }

    private void OnEnable()
    {
        if (!isInitialized)
        {
            InitializeUI();
            isInitialized = true;
        }
    }

    private void Start()
    {
        SelectClassUI(currentSelectedClass);
        SelectSubjectUI(currentSelectedSubject);
    }

    public void InitializeUI()
    {
        EnsureReferences();
        SetupCanvas();
        HideLegacyObjects();
        BuildUI();
    }

    private Sprite LoadSprite(string resName, Vector4 border)
    {
        Sprite s = Resources.Load<Sprite>("UI/" + resName);
        if (s != null) return s;

        string[] possiblePaths = new string[]
        {
            Path.Combine(Application.dataPath, "Resources", "UI", resName + ".png"),
            Path.Combine(Application.dataPath, "Resources", "UI", resName + ".jpg"),
            Path.Combine(Application.dataPath, "UI", resName + ".png"),
            Path.Combine(Application.dataPath, "UI", resName + ".jpg"),
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
            {
                try
                {
                    byte[] bytes = File.ReadAllBytes(path);
                    Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (tex.LoadImage(bytes))
                    {
                        tex.wrapMode = TextureWrapMode.Clamp;
                        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("LoadSprite error: " + e.Message);
                }
            }
        }
        return null;
    }

    private void EnsureReferences()
    {
        if (roomController == null)
        {
            roomController = FindAnyObjectByType<RoomController>();
            if (roomController == null)
            {
                var rcObj = GameObject.Find("RoomSetup");
                if (rcObj != null)
                {
                    roomController = rcObj.GetComponent<RoomController>();
                    if (roomController == null) roomController = rcObj.AddComponent<RoomController>();
                }
            }
        }

        // Font
        var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        if (fonts != null && fonts.Length > 0)
        {
            foreach (var f in fonts)
            {
                if (f.name.Contains("LiberationSans") || f.name.Contains("SDF"))
                {
                    mainFont = f;
                    break;
                }
            }
            if (mainFont == null) mainFont = fonts[0];
        }

        bgSprite = LoadSprite("download_bg", Vector4.zero);
        if (bgSprite == null) bgSprite = LoadSprite("download (3)", Vector4.zero);

        blurredBgSprite = LoadSprite("download_bg_blurred", Vector4.zero);

        glassPanelRound = LoadSprite("glass_panel_round", new Vector4(45, 45, 45, 45));
        glassPill = LoadSprite("glass_pill", new Vector4(46, 46, 46, 46));

        for (int i = 0; i < 5; i++)
        {
            pastelPills[i] = LoadSprite($"pastel_pill_{i + 1}", new Vector4(46, 46, 46, 46));
        }

        candyBtnSprite = LoadSprite("candy_button", new Vector4(46, 46, 46, 46));
        backPillSprite = LoadSprite("back_pill", new Vector4(46, 46, 46, 46));
        keyIconSprite = LoadSprite("clean_key", Vector4.zero);

        cleanIcons[0] = LoadSprite("clean_math", Vector4.zero);
        cleanIcons[1] = LoadSprite("clean_viet", Vector4.zero);
        cleanIcons[2] = LoadSprite("clean_eng", Vector4.zero);
        cleanIcons[3] = LoadSprite("clean_logic", Vector4.zero);

        selectionGlowCardSprite = LoadSprite("card_selection_glow", new Vector4(48, 48, 48, 48));
        selectionGlowPillSprite = LoadSprite("pill_selection_glow", new Vector4(52, 48, 52, 48));
        checkmarkBadgeSprite = LoadSprite("checkmark_badge", Vector4.zero);
    }

    private GameObject CreateFrostedGlassVisual(string name, RectTransform parent, Sprite shapeSprite, float whiteTint = 0.22f)
    {
        GameObject glassObj = new GameObject(name, typeof(RectTransform));
        glassObj.transform.SetParent(parent, false);
        RectTransform gr = glassObj.GetComponent<RectTransform>();
        gr.anchorMin = Vector2.zero;
        gr.anchorMax = Vector2.one;
        gr.offsetMin = Vector2.zero;
        gr.offsetMax = Vector2.zero;

        Image maskImg = glassObj.AddComponent<Image>();
        maskImg.sprite = shapeSprite;
        maskImg.type = Image.Type.Sliced;
        maskImg.raycastTarget = false; 
        Mask mask = glassObj.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        if (blurredBgSprite != null)
        {
            GameObject blurObj = new GameObject("BlurredBackground", typeof(RectTransform), typeof(Image), typeof(FrostedGlassAligner));
            blurObj.transform.SetParent(glassObj.transform, false);
            Image blurImg = blurObj.GetComponent<Image>();
            blurImg.sprite = blurredBgSprite;
            blurImg.preserveAspect = false;
            blurImg.raycastTarget = false;
            blurObj.GetComponent<FrostedGlassAligner>().Align();
        }

        GameObject tintObj = new GameObject("GlassTint", typeof(RectTransform), typeof(Image));
        tintObj.transform.SetParent(glassObj.transform, false);
        RectTransform tintRect = tintObj.GetComponent<RectTransform>();
        tintRect.anchorMin = Vector2.zero;
        tintRect.anchorMax = Vector2.one;
        tintRect.offsetMin = Vector2.zero;
        tintRect.offsetMax = Vector2.zero;
        Image tintImg = tintObj.GetComponent<Image>();
        tintImg.color = new Color(1f, 1f, 1f, whiteTint);
        tintImg.raycastTarget = false;

        GameObject borderObj = new GameObject("GlassBorder", typeof(RectTransform), typeof(Image));
        borderObj.transform.SetParent(glassObj.transform, false);
        RectTransform borderRect = borderObj.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = Vector2.zero;
        borderRect.offsetMax = Vector2.zero;
        Image borderImg = borderObj.GetComponent<Image>();
        borderImg.sprite = shapeSprite;
        borderImg.type = Image.Type.Sliced;
        borderImg.color = Color.white;
        borderImg.raycastTarget = false;

        return glassObj;
    }

    private void SetupCanvas()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = FindAnyObjectByType<Canvas>();

        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }
    }

    private void HideLegacyObjects()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        foreach (Transform child in canvas.transform)
        {
            if (child.name != "NewRoomUIRoot")
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    private void BuildUI()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        Transform existingRoot = canvas.transform.Find("NewRoomUIRoot");
        if (existingRoot != null)
        {
            if (Application.isPlaying)
            {
                existingRoot.name = "NewRoomUIRoot_Old";
                Destroy(existingRoot.gameObject);
            }
            else
            {
                DestroyImmediate(existingRoot.gameObject);
            }
        }

        GameObject rootObj = new GameObject("NewRoomUIRoot", typeof(RectTransform));
        rootObj.transform.SetParent(canvas.transform, false);
        RectTransform rootRect = rootObj.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;
        rootObj.transform.SetAsLastSibling();

        GameObject bgObj = new GameObject("BackgroundImage", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(rootRect, false);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImg = bgObj.GetComponent<Image>();
        bgImg.sprite = bgSprite;
        bgImg.color = Color.white;
        bgImg.preserveAspect = false;
        bgImg.raycastTarget = false; 

        // 2. Back Button (← Quay về)
        BuildBackButton(rootRect);

        // 3. Top Code Bar (Nhập mã code)
        BuildTopCodeBar(rootRect);

        // 4. Class Section (Chọn lớp học)
        BuildClassSection(rootRect);

        // 5. Subject Section (Chọn môn học)
        BuildSubjectSection(rootRect);

        // 6. Create Room Button (Tạo phòng)
        BuildCreateRoomButton(rootRect);
    }

    private void BuildBackButton(RectTransform parent)
    {
        GameObject btnObj = new GameObject("BtnBack", typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent, false);
        RectTransform r = btnObj.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0f, 1f);
        r.anchorMax = new Vector2(0f, 1f);
        r.pivot = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = new Vector2(150f, -65f);
        r.sizeDelta = new Vector2(180f, 54f);

        Image img = btnObj.GetComponent<Image>();
        img.sprite = backPillSprite;
        img.type = Image.Type.Sliced;
        img.raycastTarget = true;

        Button btn = btnObj.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("SelectSubject");
        });

        // Text
        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform tr = textObj.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero;
        tr.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        if (mainFont != null) tmp.font = mainFont;
        tmp.text = "← Quay về";
        tmp.fontSize = 24;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
    }

    private void BuildTopCodeBar(RectTransform parent)
    {
        // Container
        GameObject bar = new GameObject("TopCodeBar", typeof(RectTransform));
        bar.transform.SetParent(parent, false);
        RectTransform br = bar.GetComponent<RectTransform>();
        br.anchorMin = new Vector2(0.5f, 1f);
        br.anchorMax = new Vector2(0.5f, 1f);
        br.pivot = new Vector2(0.5f, 0.5f);
        br.anchoredPosition = new Vector2(0f, -88f);
        br.sizeDelta = new Vector2(1160f, 92f);

        // Visual Frosted Glass Background (Non-blocking)
        CreateFrostedGlassVisual("GlassBackground", br, glassPanelRound, 0.20f);

        // Left Label: Nhập mã code
        GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObj.transform.SetParent(bar.transform, false);
        RectTransform lr = labelObj.GetComponent<RectTransform>();
        lr.anchorMin = new Vector2(0f, 0.5f);
        lr.anchorMax = new Vector2(0f, 0.5f);
        lr.pivot = new Vector2(0f, 0.5f);
        lr.anchoredPosition = new Vector2(50f, 0f);
        lr.sizeDelta = new Vector2(300f, 60f);

        TextMeshProUGUI lbl = labelObj.GetComponent<TextMeshProUGUI>();
        if (mainFont != null) lbl.font = mainFont;
        lbl.text = "Nhập mã code";
        lbl.fontSize = 34;
        lbl.fontStyle = FontStyles.Bold;
        lbl.color = Color.black;
        lbl.alignment = TextAlignmentOptions.MidlineLeft;
        lbl.raycastTarget = false;

        // Right Input Capsule (Interactive Target)
        GameObject inputCapsule = new GameObject("InputCapsule", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
        inputCapsule.transform.SetParent(bar.transform, false);
        RectTransform ir = inputCapsule.GetComponent<RectTransform>();
        ir.anchorMin = new Vector2(1f, 0.5f);
        ir.anchorMax = new Vector2(1f, 0.5f);
        ir.pivot = new Vector2(1f, 0.5f);
        ir.anchoredPosition = new Vector2(-45f, 0f);
        ir.sizeDelta = new Vector2(670f, 64f);

        Image capImg = inputCapsule.GetComponent<Image>();
        capImg.sprite = glassPill;
        capImg.type = Image.Type.Sliced;
        capImg.color = new Color(1f, 1f, 1f, 0.40f);
        capImg.raycastTarget = true;

        TMP_InputField inputField = inputCapsule.GetComponent<TMP_InputField>();
        inputField.targetGraphic = capImg;

        // Key Icon
        GameObject keyObj = new GameObject("KeyIcon", typeof(RectTransform), typeof(Image));
        keyObj.transform.SetParent(inputCapsule.transform, false);
        RectTransform kr = keyObj.GetComponent<RectTransform>();
        kr.anchorMin = new Vector2(1f, 0.5f);
        kr.anchorMax = new Vector2(1f, 0.5f);
        kr.pivot = new Vector2(1f, 0.5f);
        kr.anchoredPosition = new Vector2(-22f, 0f);
        kr.sizeDelta = new Vector2(32f, 32f);

        Image kImg = keyObj.GetComponent<Image>();
        kImg.sprite = keyIconSprite;
        kImg.color = Color.white;
        kImg.preserveAspect = true;
        kImg.raycastTarget = false;

        // Text Area
        GameObject textArea = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
        textArea.transform.SetParent(inputCapsule.transform, false);
        RectTransform tar = textArea.GetComponent<RectTransform>();
        tar.anchorMin = Vector2.zero;
        tar.anchorMax = Vector2.one;
        tar.offsetMin = new Vector2(25f, 0f);
        tar.offsetMax = new Vector2(-70f, 0f);

        // Placeholder
        GameObject phObj = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
        phObj.transform.SetParent(textArea.transform, false);
        RectTransform phr = phObj.GetComponent<RectTransform>();
        phr.anchorMin = Vector2.zero;
        phr.anchorMax = Vector2.one;
        phr.offsetMin = Vector2.zero;
        phr.offsetMax = Vector2.zero;

        TextMeshProUGUI phText = phObj.GetComponent<TextMeshProUGUI>();
        if (mainFont != null) phText.font = mainFont;
        phText.text = "Nhập mã của bạn";
        phText.fontSize = 26;
        phText.fontStyle = FontStyles.Normal;
        phText.color = Color.black;
        phText.alignment = TextAlignmentOptions.MidlineLeft;
        phText.raycastTarget = false;

        // Main Text
        GameObject mainTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        mainTextObj.transform.SetParent(textArea.transform, false);
        RectTransform mtr = mainTextObj.GetComponent<RectTransform>();
        mtr.anchorMin = Vector2.zero;
        mtr.anchorMax = Vector2.one;
        mtr.offsetMin = Vector2.zero;
        mtr.offsetMax = Vector2.zero;

        TextMeshProUGUI mainText = mainTextObj.GetComponent<TextMeshProUGUI>();
        if (mainFont != null) mainText.font = mainFont;
        mainText.fontSize = 26;
        mainText.fontStyle = FontStyles.Bold;
        mainText.color = Color.black;
        mainText.alignment = TextAlignmentOptions.MidlineLeft;
        mainText.raycastTarget = false;

        inputField.textViewport = tar;
        inputField.textComponent = mainText;
        inputField.placeholder = phText;
        inputField.fontAsset = mainFont;

        if (roomController != null)
        {
            roomController.roomCodeInput = inputField;
        }
    }

    private void BuildClassSection(RectTransform parent)
    {
        // Container
        GameObject classBox = new GameObject("ClassSectionBox", typeof(RectTransform));
        classBox.transform.SetParent(parent, false);
        RectTransform cbr = classBox.GetComponent<RectTransform>();
        cbr.anchorMin = new Vector2(0.5f, 1f);
        cbr.anchorMax = new Vector2(0.5f, 1f);
        cbr.pivot = new Vector2(0.5f, 0.5f);
        cbr.anchoredPosition = new Vector2(0f, -220f);
        cbr.sizeDelta = new Vector2(1160f, 138f);

        // Visual Frosted Glass Background (Non-blocking)
        CreateFrostedGlassVisual("GlassBackground", cbr, glassPanelRound, 0.20f);

        // Title: Chọn lớp học
        GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(classBox.transform, false);
        RectTransform tr = titleObj.GetComponent<RectTransform>();
        tr.anchorMin = new Vector2(0.5f, 1f);
        tr.anchorMax = new Vector2(0.5f, 1f);
        tr.pivot = new Vector2(0.5f, 1f);
        tr.anchoredPosition = new Vector2(0f, -14f);
        tr.sizeDelta = new Vector2(400f, 35f);

        TextMeshProUGUI title = titleObj.GetComponent<TextMeshProUGUI>();
        if (mainFont != null) title.font = mainFont;
        title.text = "Chọn lớp học";
        title.fontSize = 26;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;
        title.color = Color.black;
        title.raycastTarget = false;

        // 5 Pastel Class Pills
        float startX = -420f;
        float gap = 210f;

        for (int i = 0; i < 5; i++)
        {
            int index = i + 1;
            string classKey = $"Lop{index}";
            string classLabel = $"Lớp {index}";

            GameObject pillObj = new GameObject($"BtnClass_{index}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(CanvasGroup));
            pillObj.transform.SetParent(classBox.transform, false);
            RectTransform pr = pillObj.GetComponent<RectTransform>();
            pr.anchorMin = new Vector2(0.5f, 0f);
            pr.anchorMax = new Vector2(0.5f, 0f);
            pr.pivot = new Vector2(0.5f, 0.5f);
            pr.anchoredPosition = new Vector2(startX + (i * gap), 44f);
            pr.sizeDelta = new Vector2(184f, 56f);

            Image pImg = pillObj.GetComponent<Image>();
            pImg.sprite = pastelPills[i];
            pImg.type = Image.Type.Sliced;
            pImg.raycastTarget = true;

            CanvasGroup cg = pillObj.GetComponent<CanvasGroup>();
            classCanvasGroups[i] = cg;

            // Selection Glow Overlay
            if (selectionGlowPillSprite != null)
            {
                GameObject glowObj = new GameObject("SelectionGlow", typeof(RectTransform), typeof(Image));
                glowObj.transform.SetParent(pillObj.transform, false);
                RectTransform gr = glowObj.GetComponent<RectTransform>();
                gr.anchorMin = Vector2.zero;
                gr.anchorMax = Vector2.one;
                gr.offsetMin = new Vector2(-6f, -6f);
                gr.offsetMax = new Vector2(6f, 6f);
                Image gi = glowObj.GetComponent<Image>();
                gi.sprite = selectionGlowPillSprite;
                gi.type = Image.Type.Sliced;
                gi.raycastTarget = false;
                glowObj.SetActive(false);
                classGlowObjects[i] = glowObj;
            }

            // Text
            GameObject txtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtObj.transform.SetParent(pillObj.transform, false);
            RectTransform txtR = txtObj.GetComponent<RectTransform>();
            txtR.anchorMin = Vector2.zero;
            txtR.anchorMax = Vector2.one;
            txtR.offsetMin = Vector2.zero;
            txtR.offsetMax = Vector2.zero;

            TextMeshProUGUI txt = txtObj.GetComponent<TextMeshProUGUI>();
            if (mainFont != null) txt.font = mainFont;
            txt.text = classLabel;
            txt.fontSize = 24;
            txt.fontStyle = FontStyles.Bold;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = Color.white;
            txt.raycastTarget = false;

            // Checkmark Badge at top-right
            if (checkmarkBadgeSprite != null)
            {
                GameObject checkObj = new GameObject("CheckBadge", typeof(RectTransform), typeof(Image));
                checkObj.transform.SetParent(pillObj.transform, false);
                RectTransform chr = checkObj.GetComponent<RectTransform>();
                chr.anchorMin = new Vector2(1f, 1f);
                chr.anchorMax = new Vector2(1f, 1f);
                chr.pivot = new Vector2(0.5f, 0.5f);
                chr.anchoredPosition = new Vector2(-6f, -6f);
                chr.sizeDelta = new Vector2(26f, 26f);
                Image ci = checkObj.GetComponent<Image>();
                ci.sprite = checkmarkBadgeSprite;
                ci.preserveAspect = true;
                ci.raycastTarget = false;
                checkObj.SetActive(false);
                classCheckObjects[i] = checkObj;
            }

            Button btn = pillObj.GetComponent<Button>();
            btn.targetGraphic = pImg;
            btn.onClick.AddListener(() =>
            {
                SelectClassUI(classKey);
            });

            classBtnObjects[i] = pillObj;
        }
    }

    private void BuildSubjectSection(RectTransform parent)
    {
        // 1. Title: Chọn môn học
        GameObject titleObj = new GameObject("SubjectTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(parent, false);
        RectTransform tr = titleObj.GetComponent<RectTransform>();
        tr.anchorMin = new Vector2(0.5f, 1f);
        tr.anchorMax = new Vector2(0.5f, 1f);
        tr.pivot = new Vector2(0.5f, 0.5f);
        tr.anchoredPosition = new Vector2(0f, -325f);
        tr.sizeDelta = new Vector2(400f, 45f);

        TextMeshProUGUI title = titleObj.GetComponent<TextMeshProUGUI>();
        if (mainFont != null) title.font = mainFont;
        title.text = "Chọn môn học";
        title.fontSize = 32;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;
        title.color = Color.black;
        title.raycastTarget = false;

        // 2. 4 Cards (2x2) with Frosted Glass Panels
        string[] subjectKeys = new string[] { "ToanHoc", "TiengViet", "TiengAnh", "TuDuy" };
        string[] subjectLabels = new string[] { "Toán học", "Tiếng Việt", "Tiếng anh", "Tư Duy" };
        Vector2[] cardPositions = new Vector2[]
        {
            new Vector2(-165f, -450f),
            new Vector2(165f, -450f),
            new Vector2(-165f, -675f),
            new Vector2(165f, -675f)
        };

        for (int i = 0; i < 4; i++)
        {
            int index = i;
            string sKey = subjectKeys[index];
            string sLabel = subjectLabels[index];
            Vector2 pos = cardPositions[index];

            // Card Container
            GameObject card = new GameObject($"SubjectCard_{sKey}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(CanvasGroup));
            card.transform.SetParent(parent, false);
            RectTransform cr = card.GetComponent<RectTransform>();
            cr.anchorMin = new Vector2(0.5f, 1f);
            cr.anchorMax = new Vector2(0.5f, 1f);
            cr.pivot = new Vector2(0.5f, 0.5f);
            cr.anchoredPosition = pos;
            cr.sizeDelta = new Vector2(290f, 205f);

            CanvasGroup cg = card.GetComponent<CanvasGroup>();
            subjectCanvasGroups[index] = cg;

            // Transparent hit graphic for button
            Image cardHitImg = card.GetComponent<Image>();
            cardHitImg.color = Color.clear;
            cardHitImg.raycastTarget = true;

            Button btn = card.GetComponent<Button>();
            btn.targetGraphic = cardHitImg;
            btn.onClick.AddListener(() =>
            {
                SelectSubjectUI(sKey);
            });

            // Frosted Glass Background
            CreateFrostedGlassVisual("GlassBackground", cr, glassPanelRound, 0.35f);

            // Selection Glow Overlay
            if (selectionGlowCardSprite != null)
            {
                GameObject glowObj = new GameObject("SelectionGlow", typeof(RectTransform), typeof(Image));
                glowObj.transform.SetParent(card.transform, false);
                RectTransform gr = glowObj.GetComponent<RectTransform>();
                gr.anchorMin = Vector2.zero;
                gr.anchorMax = Vector2.one;
                gr.offsetMin = new Vector2(-8f, -8f);
                gr.offsetMax = new Vector2(8f, 8f);
                Image gi = glowObj.GetComponent<Image>();
                gi.sprite = selectionGlowCardSprite;
                gi.type = Image.Type.Sliced;
                gi.raycastTarget = false;
                glowObj.SetActive(false);
                subjectGlowObjects[index] = glowObj;
            }

            // Clean 3D Icon
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(card.transform, false);
            RectTransform ir = iconObj.GetComponent<RectTransform>();
            ir.anchorMin = new Vector2(0.5f, 1f);
            ir.anchorMax = new Vector2(0.5f, 1f);
            ir.pivot = new Vector2(0.5f, 1f);
            ir.anchoredPosition = new Vector2(0f, -12f);
            ir.sizeDelta = new Vector2(160f, 125f);

            Image iconImg = iconObj.GetComponent<Image>();
            iconImg.sprite = cleanIcons[index];
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            // Label
            GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObj.transform.SetParent(card.transform, false);
            RectTransform lr = labelObj.GetComponent<RectTransform>();
            lr.anchorMin = new Vector2(0.5f, 0f);
            lr.anchorMax = new Vector2(0.5f, 0f);
            lr.pivot = new Vector2(0.5f, 0f);
            lr.anchoredPosition = new Vector2(0f, 18f);
            lr.sizeDelta = new Vector2(260f, 36f);

            TextMeshProUGUI lbl = labelObj.GetComponent<TextMeshProUGUI>();
            if (mainFont != null) lbl.font = mainFont;
            lbl.text = sLabel;
            lbl.fontSize = 26;
            lbl.fontStyle = FontStyles.Bold;
            lbl.color = new Color(0.12f, 0.16f, 0.23f, 1f); // Dark Navy #1E293B
            lbl.alignment = TextAlignmentOptions.Center;
            lbl.raycastTarget = false;

            // Checkmark Badge at top-right
            if (checkmarkBadgeSprite != null)
            {
                GameObject checkObj = new GameObject("CheckBadge", typeof(RectTransform), typeof(Image));
                checkObj.transform.SetParent(card.transform, false);
                RectTransform chr = checkObj.GetComponent<RectTransform>();
                chr.anchorMin = new Vector2(1f, 1f);
                chr.anchorMax = new Vector2(1f, 1f);
                chr.pivot = new Vector2(0.5f, 0.5f);
                chr.anchoredPosition = new Vector2(-12f, -12f);
                chr.sizeDelta = new Vector2(34f, 34f);
                Image ci = checkObj.GetComponent<Image>();
                ci.sprite = checkmarkBadgeSprite;
                ci.preserveAspect = true;
                ci.raycastTarget = false;
                checkObj.SetActive(false);
                subjectCheckObjects[index] = checkObj;
            }

            subjectCardObjects[index] = card;
        }
    }

    private void BuildCreateRoomButton(RectTransform parent)
    {
        GameObject btnObj = new GameObject("BtnCreateRoom", typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent, false);
        RectTransform r = btnObj.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0.5f, 0f);
        r.anchorMax = new Vector2(0.5f, 0f);
        r.pivot = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = new Vector2(0f, 68f);
        r.sizeDelta = new Vector2(320f, 74f);

        Image img = btnObj.GetComponent<Image>();
        img.sprite = candyBtnSprite;
        img.type = Image.Type.Sliced;
        img.raycastTarget = true;

        Button btn = btnObj.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() =>
        {
            if (roomController != null)
            {
                roomController.OnBtnCreateRoomClicked();
            }
        });

        // Text
        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform tr = textObj.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero;
        tr.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        if (mainFont != null) tmp.font = mainFont;
        tmp.text = "Tạo phòng";
        tmp.fontSize = 28;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.12f, 0.16f, 0.23f, 1f);
        tmp.raycastTarget = false;
    }

    public void SelectClassUI(string classKey)
    {
        currentSelectedClass = classKey;
        if (roomController != null)
        {
            roomController.SelectClass(classKey);
        }

        for (int i = 0; i < 5; i++)
        {
            if (classBtnObjects[i] == null) continue;
            string key = $"Lop{i + 1}";
            bool isSelected = (key == classKey);

            if (classCoroutines[i] != null)
            {
                StopCoroutine(classCoroutines[i]);
                classCoroutines[i] = null;
            }

            if (Application.isPlaying)
            {
                classCoroutines[i] = StartCoroutine(AnimateItemSelection(
                    classBtnObjects[i].transform,
                    classCanvasGroups[i],
                    classGlowObjects[i],
                    classCheckObjects[i],
                    isSelected,
                    targetSelectedScale: 1.15f,
                    punch: isSelected
                ));
            }
            else
            {
                classBtnObjects[i].transform.localScale = isSelected ? new Vector3(1.15f, 1.15f, 1f) : Vector3.one;
                if (classCanvasGroups[i] != null) classCanvasGroups[i].alpha = isSelected ? 1f : 0.72f;
                if (classGlowObjects[i] != null) classGlowObjects[i].SetActive(isSelected);
                if (classCheckObjects[i] != null) classCheckObjects[i].SetActive(isSelected);
            }
        }
    }

    public void SelectSubjectUI(string subjectKey)
    {
        currentSelectedSubject = subjectKey;
        if (roomController != null)
        {
            roomController.SelectSubject(subjectKey);
        }

        string[] subjectKeys = new string[] { "ToanHoc", "TiengViet", "TiengAnh", "TuDuy" };
        for (int i = 0; i < 4; i++)
        {
            if (subjectCardObjects[i] == null) continue;
            bool isSelected = (subjectKeys[i] == subjectKey);

            if (subjectCoroutines[i] != null)
            {
                StopCoroutine(subjectCoroutines[i]);
                subjectCoroutines[i] = null;
            }

            if (Application.isPlaying)
            {
                subjectCoroutines[i] = StartCoroutine(AnimateItemSelection(
                    subjectCardObjects[i].transform,
                    subjectCanvasGroups[i],
                    subjectGlowObjects[i],
                    subjectCheckObjects[i],
                    isSelected,
                    targetSelectedScale: 1.08f,
                    punch: isSelected
                ));
            }
            else
            {
                subjectCardObjects[i].transform.localScale = isSelected ? new Vector3(1.08f, 1.08f, 1f) : Vector3.one;
                if (subjectCanvasGroups[i] != null) subjectCanvasGroups[i].alpha = isSelected ? 1f : 0.75f;
                if (subjectGlowObjects[i] != null) subjectGlowObjects[i].SetActive(isSelected);
                if (subjectCheckObjects[i] != null) subjectCheckObjects[i].SetActive(isSelected);
            }
        }
    }

    private IEnumerator AnimateItemSelection(Transform target, CanvasGroup cg, GameObject glow, GameObject check,
        bool isSelected, float targetSelectedScale, bool punch)
    {
        if (target == null) yield break;

        if (glow != null) glow.SetActive(isSelected);
        if (check != null) check.SetActive(isSelected);

        Vector3 startScale = target.localScale;
        float startAlpha = cg != null ? cg.alpha : 1f;
        float targetAlpha = isSelected ? 1f : 0.72f;

        if (isSelected && punch)
        {
            // 1. Quick punch out (phồng to ra nhanh)
            Vector3 peakScale = new Vector3(targetSelectedScale * 1.12f, targetSelectedScale * 1.12f, 1f);
            float punchDur = 0.09f;
            float elapsed = 0f;
            while (elapsed < punchDur)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / punchDur);
                t = 1f - (1f - t) * (1f - t);
                target.localScale = Vector3.Lerp(startScale, peakScale, t);
                if (cg != null) cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            // 2. Smooth bounce back to targetSelectedScale (nảy nhẹ về kích thước chuẩn)
            Vector3 finalScale = new Vector3(targetSelectedScale, targetSelectedScale, 1f);
            float settleDur = 0.12f;
            elapsed = 0f;
            while (elapsed < settleDur)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / settleDur);
                t = Mathf.SmoothStep(0f, 1f, t);
                target.localScale = Vector3.Lerp(peakScale, finalScale, t);
                if (cg != null) cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }
            target.localScale = finalScale;
            if (cg != null) cg.alpha = targetAlpha;
        }
        else
        {
            Vector3 finalScale = isSelected ? new Vector3(targetSelectedScale, targetSelectedScale, 1f) : Vector3.one;
            float dur = 0.15f;
            float elapsed = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                t = Mathf.SmoothStep(0f, 1f, t);
                target.localScale = Vector3.Lerp(startScale, finalScale, t);
                if (cg != null) cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }
            target.localScale = finalScale;
            if (cg != null) cg.alpha = targetAlpha;
        }
    }
}
