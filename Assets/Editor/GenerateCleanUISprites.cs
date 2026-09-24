#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class GenerateCleanUISprites
{
    private static readonly string UIPath = "Assets/Resources/UI";

    [InitializeOnLoadMethod]
    [MenuItem("Tools/Generate Clean UI Sprites")]
    public static void GenerateAll()
    {
        string fullDir = Path.Combine(Application.dataPath, "Resources", "UI");
        if (!Directory.Exists(fullDir))
        {
            Directory.CreateDirectory(fullDir);
        }

        // 1. Frosted Glass Panel (Rounded Rect, translucent fill, crisp white border)
        CreateGlassPanel(Path.Combine(fullDir, "glass_panel_round.png"), 256, 256, 36f, 2.5f,
            fillAlphaTop: 0.22f, fillAlphaBottom: 0.14f, borderAlpha: 0.85f);

        // 2. Frosted Glass Pill (for Top Bar & Input capsule)
        CreatePill(Path.Combine(fullDir, "glass_pill.png"), 256, 96, 44f, 2.2f,
            fillColorTop: new Color(1f, 1f, 1f, 0.20f),
            fillColorBottom: new Color(1f, 1f, 1f, 0.12f),
            borderColor: new Color(1f, 1f, 1f, 0.80f));

        // 3. Back Button Pill (Dark translucent glass)
        CreatePill(Path.Combine(fullDir, "back_pill.png"), 256, 96, 44f, 2.2f,
            fillColorTop: new Color(0.08f, 0.12f, 0.20f, 0.55f),
            fillColorBottom: new Color(0.05f, 0.08f, 0.15f, 0.65f),
            borderColor: new Color(1f, 1f, 1f, 0.85f));

        // 4. Pastel Class Pills (Rich candy colors with crisp white stroke & bevel)
        // Lớp 1: Lavender Blue
        CreatePill(Path.Combine(fullDir, "pastel_pill_1.png"), 256, 96, 44f, 2.2f,
            fillColorTop: new Color(0.62f, 0.70f, 1.0f, 0.98f),
            fillColorBottom: new Color(0.48f, 0.58f, 0.96f, 0.98f),
            borderColor: new Color(1f, 1f, 1f, 0.92f));

        // Lớp 2: Coral Pink
        CreatePill(Path.Combine(fullDir, "pastel_pill_2.png"), 256, 96, 44f, 2.2f,
            fillColorTop: new Color(1.0f, 0.58f, 0.58f, 0.98f),
            fillColorBottom: new Color(0.92f, 0.40f, 0.40f, 0.98f),
            borderColor: new Color(1f, 1f, 1f, 0.92f));

        // Lớp 3: Pastel Green
        CreatePill(Path.Combine(fullDir, "pastel_pill_3.png"), 256, 96, 44f, 2.2f,
            fillColorTop: new Color(0.55f, 0.86f, 0.60f, 0.98f),
            fillColorBottom: new Color(0.38f, 0.74f, 0.44f, 0.98f),
            borderColor: new Color(1f, 1f, 1f, 0.92f));

        // Lớp 4: Teal Mint
        CreatePill(Path.Combine(fullDir, "pastel_pill_4.png"), 256, 96, 44f, 2.2f,
            fillColorTop: new Color(0.44f, 0.86f, 0.80f, 0.98f),
            fillColorBottom: new Color(0.30f, 0.76f, 0.70f, 0.98f),
            borderColor: new Color(1f, 1f, 1f, 0.92f));

        // Lớp 5: Warm Amber
        CreatePill(Path.Combine(fullDir, "pastel_pill_5.png"), 256, 96, 44f, 2.2f,
            fillColorTop: new Color(1.0f, 0.78f, 0.38f, 0.98f),
            fillColorBottom: new Color(0.96f, 0.64f, 0.22f, 0.98f),
            borderColor: new Color(1f, 1f, 1f, 0.92f));

        // 5. Candy Gradient Button (Cyan -> Pink)
        CreateHorizontalGradientPill(Path.Combine(fullDir, "candy_button.png"), 256, 96, 44f, 2.5f,
            leftColor: new Color(0.58f, 0.90f, 1.0f, 1.0f),
            rightColor: new Color(1.0f, 0.66f, 0.82f, 1.0f),
            borderColor: new Color(1f, 1f, 1f, 0.95f));

        // 6. Crisp Key Icon
        CreateKeyIcon(Path.Combine(fullDir, "clean_key.png"), 128, 128);

        // 7. Blurred Background for Frosted Glass Panels
        string srcBg = Path.Combine(fullDir, "download_bg.jpg");
        if (!File.Exists(srcBg))
        {
            srcBg = Path.Combine(Application.dataPath, "UI", "download (3).jpg");
        }
        Debug.Log("[GenerateCleanUISprites] srcBg: " + srcBg + " exists: " + File.Exists(srcBg));
        if (File.Exists(srcBg))
        {
            GenerateBlurredBackground(srcBg, Path.Combine(fullDir, "download_bg_blurred.png"), 16);
            Debug.Log("[GenerateCleanUISprites] download_bg_blurred.png generated!");
        }

        // 8. Selection Glow Overlays
        CreateGlowBorder(Path.Combine(fullDir, "card_selection_glow.png"), 256, 256, 36f, 4.5f, 10f,
            borderColor: new Color(0.22f, 0.74f, 0.98f, 1f), // Bright Sky Blue #38BDF8
            glowColor: new Color(0.14f, 0.65f, 0.95f, 0.75f));

        CreateGlowBorder(Path.Combine(fullDir, "pill_selection_glow.png"), 256, 96, 44f, 4f, 8f,
            borderColor: new Color(1f, 1f, 1f, 1f), // Crisp White
            glowColor: new Color(0.22f, 0.74f, 0.98f, 0.85f));

        // 9. Checkmark Badge
        CreateCheckmarkBadge(Path.Combine(fullDir, "checkmark_badge.png"), 128,
            circleColorTop: new Color(0.13f, 0.77f, 0.36f, 1f),   // #22C55E Emerald
            circleColorBottom: new Color(0.09f, 0.63f, 0.28f, 1f), // #16A34A Darker Emerald
            checkColor: Color.white,
            borderColor: Color.white);

        AssetDatabase.Refresh();

        // Configure Sprite Importers with 9-slice borders
        ConfigureSprite("Assets/Resources/UI/glass_panel_round.png", new Vector4(45, 45, 45, 45));
        ConfigureSprite("Assets/Resources/UI/glass_pill.png", new Vector4(46, 46, 46, 46));
        ConfigureSprite("Assets/Resources/UI/back_pill.png", new Vector4(46, 46, 46, 46));
        ConfigureSprite("Assets/Resources/UI/pastel_pill_1.png", new Vector4(46, 46, 46, 46));
        ConfigureSprite("Assets/Resources/UI/pastel_pill_2.png", new Vector4(46, 46, 46, 46));
        ConfigureSprite("Assets/Resources/UI/pastel_pill_3.png", new Vector4(46, 46, 46, 46));
        ConfigureSprite("Assets/Resources/UI/pastel_pill_4.png", new Vector4(46, 46, 46, 46));
        ConfigureSprite("Assets/Resources/UI/pastel_pill_5.png", new Vector4(46, 46, 46, 46));
        ConfigureSprite("Assets/Resources/UI/candy_button.png", new Vector4(46, 46, 46, 46));
        ConfigureSprite("Assets/Resources/UI/clean_key.png", Vector4.zero);
        ConfigureSprite("Assets/Resources/UI/download_bg_blurred.png", Vector4.zero);
        ConfigureSprite("Assets/Resources/UI/card_selection_glow.png", new Vector4(48, 48, 48, 48));
        ConfigureSprite("Assets/Resources/UI/pill_selection_glow.png", new Vector4(52, 48, 52, 48));
        ConfigureSprite("Assets/Resources/UI/checkmark_badge.png", Vector4.zero);

        AssetDatabase.SaveAssets();
        Debug.Log("[GenerateCleanUISprites] All clean UI sprites generated & configured successfully!");
    }

    private static void ConfigureSprite(string assetPath, Vector4 border)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.spriteBorder = border;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
        }
    }

    private static void CreateGlassPanel(string path, int width, int height, float radius, float borderWidth,
        float fillAlphaTop, float fillAlphaBottom, float borderAlpha)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        float halfW = width * 0.5f;
        float halfH = height * 0.5f;

        for (int y = 0; y < height; y++)
        {
            float py = y - halfH + 0.5f;
            float tY = (float)y / (height - 1);
            float baseFillAlpha = Mathf.Lerp(fillAlphaBottom, fillAlphaTop, tY);

            for (int x = 0; x < width; x++)
            {
                float px = x - halfW + 0.5f;

                float qx = Mathf.Abs(px) - (halfW - radius);
                float qy = Mathf.Abs(py) - (halfH - radius);
                float dx = Mathf.Max(qx, 0f);
                float dy = Mathf.Max(qy, 0f);
                float dist = Mathf.Sqrt(dx * dx + dy * dy) - radius;

                if (dist > 0.5f)
                {
                    tex.SetPixel(x, y, Color.clear);
                }
                else
                {
                    float outerAlpha = Mathf.Clamp01(0.5f - dist);

                    // Border region
                    if (dist >= -borderWidth)
                    {
                        float borderFactor = Mathf.Clamp01((dist + borderWidth) / borderWidth);
                        float finalAlpha = Mathf.Lerp(baseFillAlpha, borderAlpha, borderFactor) * outerAlpha;
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, finalAlpha));
                    }
                    else
                    {
                        // Inner fill
                        // Subtle specular sheen near top border
                        float sheen = 0f;
                        if (py > (halfH - radius - 15f))
                        {
                            float sheenT = Mathf.Clamp01((py - (halfH - radius - 15f)) / 15f);
                            sheen = sheenT * 0.12f;
                        }
                        float fillA = Mathf.Clamp01(baseFillAlpha + sheen) * outerAlpha;
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, fillA));
                    }
                }
            }
        }

        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    private static void CreatePill(string path, int width, int height, float radius, float borderWidth,
        Color fillColorTop, Color fillColorBottom, Color borderColor)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        float halfW = width * 0.5f;
        float halfH = height * 0.5f;

        for (int y = 0; y < height; y++)
        {
            float py = y - halfH + 0.5f;
            float tY = (float)y / (height - 1);
            Color baseFill = Color.Lerp(fillColorBottom, fillColorTop, tY);

            for (int x = 0; x < width; x++)
            {
                float px = x - halfW + 0.5f;

                float qx = Mathf.Abs(px) - (halfW - radius);
                float qy = Mathf.Abs(py) - (halfH - radius);
                float dx = Mathf.Max(qx, 0f);
                float dy = Mathf.Max(qy, 0f);
                float dist = Mathf.Sqrt(dx * dx + dy * dy) - radius;

                if (dist > 0.5f)
                {
                    tex.SetPixel(x, y, Color.clear);
                }
                else
                {
                    float outerAlpha = Mathf.Clamp01(0.5f - dist);

                    if (dist >= -borderWidth)
                    {
                        float borderFactor = Mathf.Clamp01((dist + borderWidth) / borderWidth);
                        Color c = Color.Lerp(baseFill, borderColor, borderFactor);
                        c.a *= outerAlpha;
                        tex.SetPixel(x, y, c);
                    }
                    else
                    {
                        // Inner bevel/sheen
                        float sheen = 0f;
                        if (py > 0f)
                        {
                            sheen = (py / halfH) * 0.08f;
                        }
                        Color c = new Color(
                            Mathf.Clamp01(baseFill.r + sheen),
                            Mathf.Clamp01(baseFill.g + sheen),
                            Mathf.Clamp01(baseFill.b + sheen),
                            baseFill.a * outerAlpha
                        );
                        tex.SetPixel(x, y, c);
                    }
                }
            }
        }

        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    private static void CreateHorizontalGradientPill(string path, int width, int height, float radius, float borderWidth,
        Color leftColor, Color rightColor, Color borderColor)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        float halfW = width * 0.5f;
        float halfH = height * 0.5f;

        for (int y = 0; y < height; y++)
        {
            float py = y - halfH + 0.5f;
            float tY = (float)y / (height - 1);

            for (int x = 0; x < width; x++)
            {
                float px = x - halfW + 0.5f;
                float tX = (float)x / (width - 1);
                Color baseFill = Color.Lerp(leftColor, rightColor, tX);

                float qx = Mathf.Abs(px) - (halfW - radius);
                float qy = Mathf.Abs(py) - (halfH - radius);
                float dx = Mathf.Max(qx, 0f);
                float dy = Mathf.Max(qy, 0f);
                float dist = Mathf.Sqrt(dx * dx + dy * dy) - radius;

                if (dist > 0.5f)
                {
                    tex.SetPixel(x, y, Color.clear);
                }
                else
                {
                    float outerAlpha = Mathf.Clamp01(0.5f - dist);

                    if (dist >= -borderWidth)
                    {
                        float borderFactor = Mathf.Clamp01((dist + borderWidth) / borderWidth);
                        Color c = Color.Lerp(baseFill, borderColor, borderFactor);
                        c.a *= outerAlpha;
                        tex.SetPixel(x, y, c);
                    }
                    else
                    {
                        float sheen = 0f;
                        if (py > 0f) sheen = (py / halfH) * 0.10f;
                        Color c = new Color(
                            Mathf.Clamp01(baseFill.r + sheen),
                            Mathf.Clamp01(baseFill.g + sheen),
                            Mathf.Clamp01(baseFill.b + sheen),
                            baseFill.a * outerAlpha
                        );
                        tex.SetPixel(x, y, c);
                    }
                }
            }
        }

        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    private static void CreateKeyIcon(string path, int width, int height)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                tex.SetPixel(x, y, Color.clear);
            }
        }

        // Draw rotated key shape
        // Key head center (circle) at (78, 78), radius 22, hole radius 9
        // Stem from (78, 78) to (30, 30), thickness 8
        // Teeth at (42, 42) and (34, 34)
        Vector2 headCenter = new Vector2(80f, 80f);
        float headR = 24f;
        float holeR = 10f;

        Vector2 stemStart = new Vector2(75f, 75f);
        Vector2 stemEnd = new Vector2(28f, 28f);
        Vector2 stemDir = (stemEnd - stemStart).normalized;
        Vector2 stemNorm = new Vector2(-stemDir.y, stemDir.x);
        float stemHalfThick = 4.5f;

        Vector2 tooth1Base = stemStart + stemDir * 36f;
        Vector2 tooth1Tip = tooth1Base + stemNorm * 14f;

        Vector2 tooth2Base = stemStart + stemDir * 52f;
        Vector2 tooth2Tip = tooth2Base + stemNorm * 11f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 pt = new Vector2(x + 0.5f, y + 0.5f);

                // Head circle
                float dHead = Vector2.Distance(pt, headCenter) - headR;
                float dHole = holeR - Vector2.Distance(pt, headCenter);
                float dRing = Mathf.Max(dHead, dHole);

                // Stem segment SDF
                Vector2 pa = pt - stemStart;
                Vector2 ba = stemEnd - stemStart;
                float h = Mathf.Clamp01(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba));
                float dStem = (pa - ba * h).magnitude - stemHalfThick;

                // Tooth 1 segment SDF
                Vector2 t1a = pt - tooth1Base;
                Vector2 t1b = tooth1Tip - tooth1Base;
                float ht1 = Mathf.Clamp01(Vector2.Dot(t1a, t1b) / Vector2.Dot(t1b, t1b));
                float dTooth1 = (t1a - t1b * ht1).magnitude - 3.5f;

                // Tooth 2 segment SDF
                Vector2 t2a = pt - tooth2Base;
                Vector2 t2b = tooth2Tip - tooth2Base;
                float ht2 = Mathf.Clamp01(Vector2.Dot(t2a, t2b) / Vector2.Dot(t2b, t2b));
                float dTooth2 = (t2a - t2b * ht2).magnitude - 3.5f;

                float minDist = Mathf.Min(Mathf.Min(dRing, dStem), Mathf.Min(dTooth1, dTooth2));

                if (minDist <= 0.5f)
                {
                    float a = Mathf.Clamp01(0.5f - minDist);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
        }

        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    private static void GenerateBlurredBackground(string srcPath, string dstPath, int radius)
    {
        byte[] bytes = File.ReadAllBytes(srcPath);
        Texture2D src = new Texture2D(2, 2);
        src.LoadImage(bytes);

        int w = src.width;
        int h = src.height;
        Color[] pixels = src.GetPixels();
        Color[] temp = new Color[pixels.Length];

        // 3 passes of separable box blur = Gaussian blur
        for (int pass = 0; pass < 3; pass++)
        {
            BlurHorizontal(pixels, temp, w, h, radius);
            BlurVertical(temp, pixels, w, h, radius);
        }

        Texture2D dst = new Texture2D(w, h, TextureFormat.RGB24, false);
        dst.SetPixels(pixels);
        dst.Apply();

        File.WriteAllBytes(dstPath, dst.EncodeToPNG());
        Object.DestroyImmediate(src);
        Object.DestroyImmediate(dst);
    }

    private static void BlurHorizontal(Color[] src, Color[] dst, int w, int h, int r)
    {
        float inv = 1f / (r * 2 + 1);
        for (int y = 0; y < h; y++)
        {
            int rowStart = y * w;
            float rSum = 0, gSum = 0, bSum = 0;

            Color first = src[rowStart];
            rSum = first.r * (r + 1);
            gSum = first.g * (r + 1);
            bSum = first.b * (r + 1);

            for (int i = 1; i <= r; i++)
            {
                int px = Mathf.Min(i, w - 1);
                Color c = src[rowStart + px];
                rSum += c.r;
                gSum += c.g;
                bSum += c.b;
            }

            for (int x = 0; x < w; x++)
            {
                dst[rowStart + x] = new Color(rSum * inv, gSum * inv, bSum * inv, 1f);

                int left = Mathf.Max(x - r, 0);
                int right = Mathf.Min(x + r + 1, w - 1);

                Color cLeft = src[rowStart + left];
                Color cRight = src[rowStart + right];

                rSum += cRight.r - cLeft.r;
                gSum += cRight.g - cLeft.g;
                bSum += cRight.b - cLeft.b;
            }
        }
    }

    private static void BlurVertical(Color[] src, Color[] dst, int w, int h, int r)
    {
        float inv = 1f / (r * 2 + 1);
        for (int x = 0; x < w; x++)
        {
            float rSum = 0, gSum = 0, bSum = 0;

            Color first = src[x];
            rSum = first.r * (r + 1);
            gSum = first.g * (r + 1);
            bSum = first.b * (r + 1);

            for (int i = 1; i <= r; i++)
            {
                int py = Mathf.Min(i, h - 1);
                Color c = src[py * w + x];
                rSum += c.r;
                gSum += c.g;
                bSum += c.b;
            }

            for (int y = 0; y < h; y++)
            {
                dst[y * w + x] = new Color(rSum * inv, gSum * inv, bSum * inv, 1f);

                int top = Mathf.Max(y - r, 0);
                int btm = Mathf.Min(y + r + 1, h - 1);

                Color cTop = src[top * w + x];
                Color cBtm = src[btm * w + x];

                rSum += cBtm.r - cTop.r;
                gSum += cBtm.g - cTop.g;
                bSum += cBtm.b - cTop.b;
            }
        }
    }

    private static void CreateGlowBorder(string path, int width, int height, float radius, float borderWidth, float glowWidth,
        Color borderColor, Color glowColor)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        float halfW = width * 0.5f;
        float halfH = height * 0.5f;

        for (int y = 0; y < height; y++)
        {
            float py = y - halfH + 0.5f;
            for (int x = 0; x < width; x++)
            {
                float px = x - halfW + 0.5f;

                float qx = Mathf.Abs(px) - (halfW - radius - glowWidth);
                float qy = Mathf.Abs(py) - (halfH - radius - glowWidth);
                float dx = Mathf.Max(qx, 0f);
                float dy = Mathf.Max(qy, 0f);
                float dist = Mathf.Sqrt(dx * dx + dy * dy) - radius;

                if (dist < -borderWidth - 0.5f)
                {
                    tex.SetPixel(x, y, Color.clear);
                }
                else if (dist <= 0f)
                {
                    float innerEdge = Mathf.Clamp01(dist - (-borderWidth) + 0.5f);
                    float borderCenterT = 1f - Mathf.Abs((dist + borderWidth * 0.5f) / (borderWidth * 0.5f));
                    Color c = Color.Lerp(borderColor, Color.white, borderCenterT * 0.5f);
                    c.a = Mathf.Clamp01(c.a * innerEdge);
                    tex.SetPixel(x, y, c);
                }
                else if (dist <= glowWidth)
                {
                    float glowFactor = 1f - (dist / glowWidth);
                    glowFactor = glowFactor * glowFactor;
                    Color c = glowColor;
                    c.a *= glowFactor;
                    tex.SetPixel(x, y, c);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }

        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    private static void CreateCheckmarkBadge(string path, int size, Color circleColorTop, Color circleColorBottom,
        Color checkColor, Color borderColor)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float circleRadius = size * 0.42f;
        float borderWidth = 3.5f;
        float shadowWidth = 6f;

        Vector2 checkA = new Vector2(size * 0.28f, size * 0.50f);
        Vector2 checkB = new Vector2(size * 0.44f, size * 0.34f);
        Vector2 checkC = new Vector2(size * 0.74f, size * 0.68f);
        float checkHalfThick = size * 0.055f;

        for (int y = 0; y < size; y++)
        {
            float tY = (float)y / (size - 1);
            Color fillGrad = Color.Lerp(circleColorBottom, circleColorTop, tY);

            for (int x = 0; x < size; x++)
            {
                Vector2 pt = new Vector2(x + 0.5f, y + 0.5f);
                float distToCenter = Vector2.Distance(pt, center) - circleRadius;

                if (distToCenter > shadowWidth)
                {
                    tex.SetPixel(x, y, Color.clear);
                }
                else if (distToCenter > 0f)
                {
                    float shadowFactor = 1f - (distToCenter / shadowWidth);
                    float a = shadowFactor * shadowFactor * 0.45f;
                    tex.SetPixel(x, y, new Color(0f, 0f, 0f, a));
                }
                else
                {
                    float outerAlpha = Mathf.Clamp01(0.5f - distToCenter);
                    Color pixelColor;

                    if (distToCenter >= -borderWidth)
                    {
                        pixelColor = borderColor;
                    }
                    else
                    {
                        pixelColor = fillGrad;

                        float dCheck1 = DistToSegment(pt, checkA, checkB) - checkHalfThick;
                        float dCheck2 = DistToSegment(pt, checkB, checkC) - checkHalfThick;
                        float dCheck = Mathf.Min(dCheck1, dCheck2);

                        if (dCheck <= 0.5f)
                        {
                            float checkAlpha = Mathf.Clamp01(0.5f - dCheck);
                            pixelColor = Color.Lerp(pixelColor, checkColor, checkAlpha);
                        }
                    }

                    pixelColor.a *= outerAlpha;
                    tex.SetPixel(x, y, pixelColor);
                }
            }
        }

        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    private static float DistToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 pa = p - a;
        Vector2 ba = b - a;
        float h = Mathf.Clamp01(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba));
        return (pa - ba * h).magnitude;
    }
}
#endif
