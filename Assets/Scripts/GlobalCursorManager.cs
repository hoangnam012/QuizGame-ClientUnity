using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GlobalCursorManager : MonoBehaviour
{
    private static GlobalCursorManager s_instance;
    private static Texture2D s_handCursorTex;
    private static Sprite s_handCursorSprite;
    private static bool s_isHovering = false;

    private GameObject virtualCursorCanvas;
    private GameObject virtualCursorObj;
    private RectTransform virtualCursorRect;
    private Image virtualCursorImage;

    private float scanTimer = 0f;
    private static int s_hoverCount = 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInitialize()
    {
        if (s_instance != null) return;
        GameObject go = new GameObject("GlobalCursorManager");
        DontDestroyOnLoad(go);
        s_instance = go.AddComponent<GlobalCursorManager>();
    }

    void Awake()
    {
        if (s_instance == null)
        {
            s_instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (s_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        CreateVirtualCursorOverlay();
        SceneManager.sceneLoaded += OnSceneLoaded;
        AttachHoverToAllButtons();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        ResetCursor();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            ResetCursor();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        s_hoverCount = 0;
        ResetCursor();
        AttachHoverToAllButtons();
    }

    private void CreateVirtualCursorOverlay()
    {
        if (virtualCursorCanvas != null) return;

        virtualCursorCanvas = new GameObject("VirtualCursorCanvas");
        virtualCursorCanvas.transform.SetParent(this.transform);
        DontDestroyOnLoad(virtualCursorCanvas);

        Canvas canvas = virtualCursorCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767; // Luôn hiển thị trên cùng mọi UI khác

        CanvasGroup cg = virtualCursorCanvas.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable = false;

        virtualCursorObj = new GameObject("HandCursorPointer");
        virtualCursorObj.transform.SetParent(virtualCursorCanvas.transform, false);

        virtualCursorImage = virtualCursorObj.AddComponent<Image>();
        virtualCursorImage.raycastTarget = false;
        virtualCursorImage.sprite = GetOrCreateHandCursorSprite();

        virtualCursorRect = virtualCursorObj.GetComponent<RectTransform>();
        virtualCursorRect.sizeDelta = new Vector2(38f, 38f);
        // Fingertip hotspot: (8.5 / 32, 30.5 / 32)
        virtualCursorRect.pivot = new Vector2(0.265f, 0.953f);

        virtualCursorObj.SetActive(false);
    }

    void Update()
    {
        // Quét nút mới xuất hiện định kỳ
        scanTimer += Time.unscaledDeltaTime;
        if (scanTimer >= 0.8f)
        {
            scanTimer = 0f;
            AttachHoverToAllButtons();
        }

        Vector2 mousePos = GetMousePosition();

        // Kiểm tra raycast UI xem chuột có đang nằm trên nút bấm nào không
        bool isOverSelectable = false;
        if (EventSystem.current != null && mousePos != Vector2.zero)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = mousePos
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].gameObject == null) continue;
                Selectable sel = results[i].gameObject.GetComponentInParent<Selectable>();
                if (sel != null && sel.interactable && sel.isActiveAndEnabled)
                {
                    isOverSelectable = true;
                    break;
                }
            }
        }

        bool shouldShowHand = (s_hoverCount > 0) || isOverSelectable;

        if (shouldShowHand)
        {
            ShowHandCursor(mousePos);
        }
        else
        {
            ResetCursor();
        }
    }

    public static void NotifyHoverEnter()
    {
        s_hoverCount++;
        if (s_instance != null)
        {
            s_instance.ShowHandCursor(GetMousePosition());
        }
    }

    public static void NotifyHoverExit()
    {
        s_hoverCount = Mathf.Max(0, s_hoverCount - 1);
        if (s_hoverCount == 0 && s_instance != null)
        {
            s_instance.ResetCursor();
        }
    }

    public void ShowHandCursor(Vector2 mousePos)
    {
        s_isHovering = true;

        // Cập nhật vị trí Virtual Cursor Overlay
        if (virtualCursorObj != null && virtualCursorRect != null)
        {
            if (mousePos != Vector2.zero)
            {
                virtualCursorRect.position = new Vector3(mousePos.x, mousePos.y, 0f);
            }
            if (!virtualCursorObj.activeSelf)
            {
                virtualCursorObj.SetActive(true);
            }
        }

        // Ẩn con trỏ mặc định của OS để hiện bàn tay ảo mượt mà
        Cursor.visible = false;

        // Fallback: đồng thời gọi Cursor.SetCursor cho phần cứng nếu hỗ trợ
        Texture2D handTex = GetOrCreateHandCursorTexture();
        if (handTex != null)
        {
            Cursor.SetCursor(handTex, new Vector2(8f, 2f), CursorMode.ForceSoftware);
        }
    }

    public void ResetCursor()
    {
        if (!s_isHovering && (virtualCursorObj == null || !virtualCursorObj.activeSelf)) return;
        s_isHovering = false;

        if (virtualCursorObj != null)
        {
            virtualCursorObj.SetActive(false);
        }

        Cursor.visible = true;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public static void AttachHoverToAllButtons()
    {
        Selectable[] selectables = Resources.FindObjectsOfTypeAll<Selectable>();
        for (int i = 0; i < selectables.Length; i++)
        {
            Selectable sel = selectables[i];
            if (sel == null || sel.gameObject == null) continue;
            if (!sel.gameObject.scene.isLoaded) continue;

            if (sel.GetComponent<ButtonCursorHover>() == null)
            {
                sel.gameObject.AddComponent<ButtonCursorHover>();
            }
        }
    }

    private static Vector2 GetMousePosition()
    {
        try
        {
            if (UnityEngine.InputSystem.Mouse.current != null)
            {
                return UnityEngine.InputSystem.Mouse.current.position.ReadValue();
            }
        }
        catch { }
        return Vector2.zero;
    }

    public static Texture2D GetOrCreateHandCursorTexture()
    {
        if (s_handCursorTex != null) return s_handCursorTex;

        int width = 32, height = 32;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        string[] art = new string[]
        {
            "................................",
            ".......###......................",
            "......#WWW#.....................",
            "......#WWS#.....................",
            "......#WWS#.....................",
            "......#WWS#.....................",
            "......#WWS#...####..............",
            "......#WWS#..#WWWS#...####......",
            "......#WWS#.#WWWWWS#.#WWWS#.....",
            "......#WWS#.#WWSWWS#.#WWWS#..###",
            "......#WWS#.#WWSWWS#.#WWWS#.#WWS#",
            "......#WWS#.#WWSWWS#.#WWWS#.#WWS#",
            ".####.#WWS#.#WWSWWS#.#WWWS#.#WWS#",
            "#WWWW##WWS###WWSWW###WWSWW###WWS#",
            "#WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWS#",
            "#WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWS#",
            ".#WWWWWWWWWWWWWWWWWWWWWWWWWWWWWS#",
            "..#WWWWWWWWWWWWWWWWWWWWWWWWWWWWS#",
            "...#WWWWWWWWWWWWWWWWWWWWWWWWWWS#",
            "....#WWWWWWWWWWWWWWWWWWWWWWWWS#.",
            ".....#WWWWWWWWWWWWWWWWWWWWWWWS#.",
            "......#WWWWWWWWWWWWWWWWWWWWWS#..",
            "......#WWWWWWWWWWWWWWWWWWWWWS#..",
            ".......#WWWWWWWWWWWWWWWWWWWS#...",
            ".......#WWWWWWWWWWWWWWWWWWWS#...",
            "........#SSSSSSSSSSSSSSSSS#.....",
            ".........#################......",
            "................................",
            "................................",
            "................................",
            "................................",
            "................................"
        };

        Color[] pixels = new Color[width * height];
        Color border = new Color(0.12f, 0.15f, 0.22f, 1f);
        Color fill = Color.white;
        Color shade = new Color(0.85f, 0.90f, 0.96f, 1f);

        for (int y = 0; y < height; y++)
        {
            string row = art[height - 1 - y];
            for (int x = 0; x < width; x++)
            {
                char c = row[x];
                if (c == '#')
                {
                    pixels[y * width + x] = border;
                }
                else if (c == 'W')
                {
                    pixels[y * width + x] = fill;
                }
                else if (c == 'S')
                {
                    pixels[y * width + x] = shade;
                }
                else
                {
                    pixels[y * width + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        s_handCursorTex = tex;
        return s_handCursorTex;
    }

    public static Sprite GetOrCreateHandCursorSprite()
    {
        if (s_handCursorSprite != null) return s_handCursorSprite;
        Texture2D tex = GetOrCreateHandCursorTexture();
        s_handCursorSprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.265f, 0.953f), // Fingertip pivot
            32f
        );
        return s_handCursorSprite;
    }
}

public class ButtonCursorHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        Selectable sel = GetComponent<Selectable>();
        if (sel == null || sel.interactable)
        {
            GlobalCursorManager.NotifyHoverEnter();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GlobalCursorManager.NotifyHoverExit();
    }

    void OnDisable()
    {
        GlobalCursorManager.NotifyHoverExit();
    }
}
