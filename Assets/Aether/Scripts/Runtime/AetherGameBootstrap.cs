using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using System.Collections;
using Aether.Gameplay.Menus;

/// <summary>Builds the first playable graybox slice; production art and verified RTL fonts are separate milestones.</summary>
[DisallowMultipleComponent]
public sealed class AetherGameBootstrap : MonoBehaviour
{
    private static readonly Color Ink = new Color(0.025f, 0.027f, 0.035f);
    private static readonly Color Gold = new Color(0.86f, 0.67f, 0.29f);
    private static readonly Color Muted = new Color(1f, 1f, 1f, 0.64f);
    private Canvas canvas;
    private RectTransform safeAreaRoot;
    private int lastScreenWidth;
    private int lastScreenHeight;
    private Rect lastSafeArea;
    private bool hasSafeArea;
    private Font font;
    private Font rtlFont;
    private GameObject worldRoot;
    private static Sprite cachedWhiteSprite;
    private static Sprite cachedGlintSprite;
    private static Sprite joystickBaseSprite;
    private static Sprite joystickKnobSprite;
    private static Sprite jumpControlSprite;
    private static Sprite pauseControlSprite;
    private bool warningVisible;
    private bool animateMenuUi;
    private bool menuEntrancePlayed;
    private float nextMenuRevealDelay;
    private static bool menuMotionEnabled = true;
    private static float uiTextScale = 1f;

    private void Awake()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        menuMotionEnabled = PlayerPrefs.GetInt("Aether.MenuMotionEnabled", 1) != 0;
        uiTextScale = Mathf.Clamp(PlayerPrefs.GetFloat("Aether.UiTextScale", 1f), 1f, 1.12f);
        ApplyMenuPreferences();
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        rtlFont = Resources.Load<Font>("Fonts/Vazirmatn-Regular");
        EnsureCamera();
        BuildCanvas();
        ShowStudioIntro();
    }

    private void EnsureCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            var cameraObject = new GameObject("Aether Camera");
            cameraObject.tag = "MainCamera";
            cam = cameraObject.AddComponent<Camera>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 5.5f;
        cam.backgroundColor = Ink;
        cam.transform.position = new Vector3(0f, 1.5f, -10f);
    }

    private void BuildCanvas()
    {
        var canvasObject = new GameObject("Aether UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var safeAreaObject = new GameObject("Safe Area Content", typeof(RectTransform));
        safeAreaRoot = safeAreaObject.GetComponent<RectTransform>();
        safeAreaRoot.SetParent(canvas.transform, false);
        ApplySafeArea();

        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        bool needsDefaultActions = eventSystem == null;
        if (eventSystem == null)
        {
            var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
            eventSystem = eventSystemObject.GetComponent<EventSystem>();
        }

        var inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (inputModule == null)
        {
            inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            needsDefaultActions = true;
        }

        // A newly created Input System UI module does not automatically guarantee
        // populated pointer/click action references. Initialize only new modules so
        // preconfigured EventSystems retain their chosen bindings.
        if (needsDefaultActions) inputModule.AssignDefaultActions();
    }


    private void LateUpdate()
    {
        Rect currentSafeArea = Screen.safeArea;
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight || !hasSafeArea || currentSafeArea != lastSafeArea)
            ApplySafeArea();
    }

    private void ApplySafeArea()
    {
        if (safeAreaRoot == null || Screen.width <= 0 || Screen.height <= 0) return;
        Rect safe = Screen.safeArea;
        safeAreaRoot.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
        safeAreaRoot.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
        safeAreaRoot.offsetMin = Vector2.zero;
        safeAreaRoot.offsetMax = Vector2.zero;
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        lastSafeArea = safe;
        hasSafeArea = true;
    }

    private void ShowStudioIntro()
    {
        ClearUI();
        Panel("StudioIntroBackdrop", Ink, Vector2.zero, Vector2.one);
        Sprite logoSprite = Resources.Load<Sprite>("Brand/VarellonLogo");
        if (logoSprite != null)
        {
            var logoObject = new GameObject("Varellon Logo", typeof(RectTransform), typeof(Image));
            logoObject.transform.SetParent(canvas.transform, false);
            var logoRect = logoObject.GetComponent<RectTransform>();
            logoRect.anchorMin = new Vector2(0.5f, 0.5f);
            logoRect.anchorMax = new Vector2(0.5f, 0.5f);
            logoRect.sizeDelta = new Vector2(340f, 600f);
            var logoImage = logoObject.GetComponent<Image>();
            logoImage.sprite = logoSprite;
            logoImage.preserveAspect = true;
            logoImage.color = menuMotionEnabled ? new Color(1f, 1f, 1f, 0f) : Color.white;
            logoImage.raycastTarget = false;
            logoRect.localScale = menuMotionEnabled ? Vector3.one * 0.94f : Vector3.one;
            if (menuMotionEnabled) StartCoroutine(AnimateLogoReveal(logoRect, logoImage));

            var glintObject = new GameObject("Single Logo Glint", typeof(RectTransform), typeof(Image));
            glintObject.transform.SetParent(canvas.transform, false);
            var glintRect = glintObject.GetComponent<RectTransform>();
            glintRect.anchorMin = new Vector2(0.5f, 0.5f);
            glintRect.anchorMax = new Vector2(0.5f, 0.5f);
            glintRect.sizeDelta = new Vector2(44f, 500f);
            glintRect.localRotation = Quaternion.Euler(0f, 0f, 18f);
            var glintImage = glintObject.GetComponent<Image>();
            glintImage.color = new Color(Gold.r, Gold.g, Gold.b, 0f);
            glintImage.raycastTarget = false;
            if (menuMotionEnabled)
            {
                glintImage.sprite = SoftGlintSprite();
                StartCoroutine(PlayLogoGlint(glintRect, glintImage));
            }
        }
        else
        {
            Label("VARELLON", 52, Gold, new Vector2(0.5f, 0.5f), new Vector2(0.7f, 0.16f), TextAnchor.MiddleCenter);
        }
        StartCoroutine(ShowWarningAfterIntro());
    }

    private IEnumerator AnimateLogoReveal(RectTransform logo, Image image)
    {
        yield return new WaitForSecondsRealtime(0.08f);
        const float duration = 0.95f;
        float elapsed = 0f;
        while (elapsed < duration && logo != null && image != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            float scale;
            if (t < 0.82f)
            {
                float settleT = t / 0.82f;
                float settleEase = 1f - Mathf.Pow(1f - settleT, 3f);
                scale = Mathf.Lerp(0.94f, 1.018f, settleEase);
            }
            else
            {
                float settleT = (t - 0.82f) / 0.18f;
                settleT = settleT * settleT * (3f - 2f * settleT);
                scale = Mathf.Lerp(1.018f, 1f, settleT);
            }
            logo.localScale = Vector3.one * scale;
            image.color = new Color(1f, 1f, 1f, eased);
            yield return null;
        }
        if (logo != null) logo.localScale = Vector3.one;
        if (image != null) image.color = Color.white;
    }

    private static Sprite SoftGlintSprite()
    {
        if (cachedGlintSprite != null) return cachedGlintSprite;
        const int width = 64;
        const int height = 256;
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
        texture.name = "Aether Soft Gold Glint";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        for (int y = 0; y < height; y++)
        {
            float v = (float)y / (height - 1);
            float edge = Mathf.Clamp01(Mathf.Min(v, 1f - v) * 8f);
            float verticalFade = edge * edge * (3f - 2f * edge);
            for (int x = 0; x < width; x++)
            {
                float u = ((x / (float)(width - 1)) - 0.5f) * 2f;
                float core = Mathf.Exp(-u * u * 24f);
                float halo = Mathf.Exp(-u * u * 3.2f) * 0.13f;
                float alpha = Mathf.Clamp01(core + halo) * verticalFade * 0.72f;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        texture.Apply(false, true);
        cachedGlintSprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        cachedGlintSprite.name = "Aether Soft Gold Glint";
        return cachedGlintSprite;
    }

    private IEnumerator PlayLogoGlint(RectTransform glint, Image image)
    {
        yield return new WaitForSecondsRealtime(0.78f);
        const float duration = 1.15f;
        float elapsed = 0f;
        while (elapsed < duration && glint != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            glint.anchoredPosition = new Vector2(Mathf.Lerp(-260f, 260f, t), 0f);
            float intensity = Mathf.Sin(t * Mathf.PI);
            image.color = new Color(Gold.r, Gold.g, Gold.b, intensity * 0.58f);
            yield return null;
        }
        if (image != null) image.color = new Color(Gold.r, Gold.g, Gold.b, 0f);
    }

    private IEnumerator ShowWarningAfterIntro()
    {
        yield return new WaitForSecondsRealtime(menuMotionEnabled ? 2.1f : 0.65f);
        ShowWarning();
    }

    private IEnumerator DismissWarningAfterDelay()
    {
        yield return new WaitForSecondsRealtime(7f);
        if (warningVisible) ShowMenu();
    }

    private void ShowWarning()
    {
        warningVisible = true;
        ClearUI();
        Panel("WarningPanel", Ink, Vector2.zero, Vector2.one);
        Label("هشدار", 54, Gold, new Vector2(0.5f, 0.83f), new Vector2(0.8f, 0.14f), TextAnchor.MiddleCenter);
        Label("این بازی هنوز در حال توسعه است؛ بنابراین ممکن است بخش‌ها، کنترل‌ها و روند پیشرفت در به‌روزرسانی‌های آینده تغییر کنند.\nذخیره‌های نسخه‌های آزمایشی ممکن است با نسخه‌های بعدی سازگار نباشند؛ برای پیشرفت مهم خود نسخهٔ پشتیبان نگه دارید.\nممکن است با خطا، افت عملکرد یا رفتارهای پیش‌بینی‌نشده روبه‌رو شوید. در صورت بروز مشکل، بازی را ببندید و دوباره اجرا کنید.\nبا ادامه، وارد نسخهٔ آزمایشی می‌شوید و می‌پذیرید که این نسخه هنوز محصول نهایی نیست.", 25, Color.white,
            new Vector2(0.5f, 0.48f), new Vector2(0.76f, 0.48f), TextAnchor.MiddleCenter);
        StopCoroutine(nameof(DismissWarningAfterDelay));
        StartCoroutine(DismissWarningAfterDelay());
    }

    private void ShowMenu()
    {
        warningVisible = false;
        animateMenuUi = false;
        ClearUI();
        Panel("MenuBackdrop", Ink, Vector2.zero, Vector2.one);
        BuildMenuAtmosphere();

        animateMenuUi = !menuEntrancePlayed;
        nextMenuRevealDelay = 0.02f;
        Label("V A R E L L O N", 20, Muted, new Vector2(0.22f, 0.875f),
            new Vector2(0.34f, 0.055f), TextAnchor.MiddleLeft);
        Label("AETHER", 102, Color.white, new Vector2(0.235f, 0.695f),
            new Vector2(0.43f, 0.19f), TextAnchor.MiddleLeft);
        Label("سفری تازه از دل طبیعت ناشناخته", 25, Color.white,
            new Vector2(0.245f, 0.555f), new Vector2(0.46f, 0.075f), TextAnchor.MiddleRight);
        Button primaryMenuButton = MakeButton("شروع بازی", new Vector2(0.22f, 0.405f),
            new Vector2(0.30f, 0.095f), StartGame, true);
        MakeButton("کنترل‌ها", new Vector2(0.125f, 0.275f),
            new Vector2(0.18f, 0.075f), ShowControls, false);
        MakeButton("دربارهٔ بازی", new Vector2(0.36f, 0.275f),
            new Vector2(0.22f, 0.075f), ShowAbout, false);
        MakeButton("تنظیمات", new Vector2(0.22f, 0.16f),
            new Vector2(0.30f, 0.072f), ShowSettings, false);
        Label("نمونهٔ اولیه  •  منطقهٔ آزمایشی", 14,
            new Color(1f, 1f, 1f, 0.43f), new Vector2(0.205f, 0.065f),
            new Vector2(0.36f, 0.035f), TextAnchor.MiddleRight);
        animateMenuUi = false;
        menuEntrancePlayed = true;
        StartCoroutine(FocusButtonWhenVisible(primaryMenuButton));
    }

    private IEnumerator FocusButtonWhenVisible(Button target)
    {
        if (target == null) yield break;
        AetherUIReveal reveal = target.GetComponent<AetherUIReveal>();
        while (target != null && reveal != null && !reveal.IsComplete) yield return null;
        if (target != null && target.isActiveAndEnabled && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(target.gameObject);
    }

    private void BuildMenuAtmosphere()
    {
        // A nested Canvas isolates the frequently refreshed atmosphere mesh from
        // the mostly static title, labels, and buttons on the main UI Canvas.
        var go = new GameObject("Menu Atmosphere", typeof(RectTransform), typeof(Canvas), typeof(AetherMenuAtmosphere));
        go.transform.SetParent(canvas.transform, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        var atmosphereCanvas = go.GetComponent<Canvas>();
        atmosphereCanvas.overrideSorting = false;
        go.GetComponent<AetherMenuAtmosphere>().SetAllDirty();
        // Draw above the opaque backdrop but behind the safe-area content.
        if (safeAreaRoot != null)
        {
            go.transform.SetSiblingIndex(Mathf.Max(0, canvas.transform.childCount - 2));
            safeAreaRoot.SetAsLastSibling();
        }
    }

    private void ShowSettings()
    {
        animateMenuUi = false;
        ClearUI();
        Panel("SettingsBackdrop", Ink, Vector2.zero, Vector2.one);
        BuildMenuAtmosphere();
        animateMenuUi = true;
        nextMenuRevealDelay = 0.02f;
        Label("تنظیمات", 54, Color.white, new Vector2(0.23f, 0.80f),
            new Vector2(0.42f, 0.12f), TextAnchor.MiddleCenter);
        Label("این گزینه‌ها روی همین دستگاه ذخیره می‌شوند.", 22,
            new Color(1f, 1f, 1f, 0.72f), new Vector2(0.25f, 0.69f),
            new Vector2(0.48f, 0.06f), TextAnchor.MiddleCenter);
        string motionText = menuMotionEnabled ? "حرکت پس‌زمینه: روشن" : "حرکت پس‌زمینه: خاموش";
        Button motionButton = MakeButton(motionText, new Vector2(0.23f, 0.53f),
            new Vector2(0.34f, 0.078f), ToggleMenuMotion, false);
        string textText = uiTextScale > 1.05f ? "اندازهٔ نوشته: درشت" : "اندازهٔ نوشته: عادی";
        MakeButton(textText, new Vector2(0.23f, 0.41f),
            new Vector2(0.34f, 0.078f), ToggleTextSize, false);
        MakeButton("بازگشت به منو", new Vector2(0.23f, 0.25f),
            new Vector2(0.30f, 0.078f), ShowMenu, true);
        animateMenuUi = false;
        StartCoroutine(FocusButtonWhenVisible(motionButton));
    }

    private void ToggleMenuMotion()
    {
        menuMotionEnabled = !menuMotionEnabled;
        PlayerPrefs.SetInt("Aether.MenuMotionEnabled", menuMotionEnabled ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMenuPreferences();
        ShowSettings();
    }

    private void ToggleTextSize()
    {
        uiTextScale = uiTextScale > 1.05f ? 1f : 1.12f;
        PlayerPrefs.SetFloat("Aether.UiTextScale", uiTextScale);
        PlayerPrefs.Save();
        ShowSettings();
    }

    private static void ApplyMenuPreferences()
    {
        AetherMenuAtmosphere.MotionEnabled = menuMotionEnabled;
        AetherUIReveal.MotionEnabled = menuMotionEnabled;
        AetherUIButtonFeedback.MotionEnabled = menuMotionEnabled;
    }

    private void ShowControls()
    {
        ShowMenuInfoScreen("کنترل‌ها",
            "جوی‌استیک ثابت سمت چپ برای حرکت است؛ مرکز آن با لمس جابه‌جا نمی‌شود.\nبرای پریدن، نشان اختصاصی پرش را لمس کن.\nدر بازی می‌توانی از منوی مکث، چیدمان و اندازهٔ کنترل‌ها را تغییر بدهی.\nحرکت و پرش هم‌زمان پشتیبانی می‌شوند.");
    }

    private void ShowAbout()
    {
        ShowMenuInfoScreen("دربارهٔ بازی",
            "این بازی به‌وسیلهٔ استودیوی وارلون ساخته می‌شود.\nنسخهٔ کنونی نمونهٔ اولیه است؛ طراحی، جهان و سازوکارهای آن همچنان در حال توسعه‌اند.\nبازخورد و آزمایش دقیق به بهترشدن تجربهٔ بازی کمک می‌کند.");
    }

    private void ShowMenuInfoScreen(string heading, string body)
    {
        animateMenuUi = false;
        ClearUI();
        Panel("MenuInfoBackdrop", Ink, Vector2.zero, Vector2.one);
        BuildMenuAtmosphere();
        animateMenuUi = true;
        nextMenuRevealDelay = 0.02f;
        Label(heading, 52, Color.white, new Vector2(0.23f, 0.76f),
            new Vector2(0.42f, 0.12f), TextAnchor.MiddleCenter);
        Label(body, 24, new Color(1f, 1f, 1f, 0.82f), new Vector2(0.25f, 0.51f),
            new Vector2(0.48f, 0.34f), TextAnchor.MiddleCenter);
        Button backButton = MakeButton("بازگشت به منو", new Vector2(0.23f, 0.25f),
            new Vector2(0.29f, 0.078f), ShowMenu, false);
        animateMenuUi = false;
        StartCoroutine(FocusButtonWhenVisible(backButton));
    }

    private void RevealMenuElement(GameObject target)
    {
        if (!animateMenuUi || target == null) return;
        var reveal = target.AddComponent<AetherUIReveal>();
        reveal.Begin(nextMenuRevealDelay, new Vector2(0f, -14f), 0.38f);
        nextMenuRevealDelay += 0.065f;
    }

    private void StartGame()
    {
        Time.timeScale = 1f;
        ClearUI();
        if (worldRoot != null) Destroy(worldRoot);
        worldRoot = new GameObject("Aether Prototype World");
        CreateBlock("Ground", new Vector2(0f, -3.1f), new Vector2(32f, 1f), new Color(0.10f, 0.105f, 0.12f), true);
        CreateBlock("First Step", new Vector2(4.5f, -1.6f), new Vector2(3.5f, 0.35f), Gold, true);
        CreateBlock("Second Step", new Vector2(9f, 0.0f), new Vector2(3f, 0.35f), new Color(0.34f, 0.28f, 0.18f), true);
        CreateBlock("High Ledge", new Vector2(13f, 1.3f), new Vector2(3f, 0.35f), Gold, true);
        var player = CreateBlock("Player", new Vector2(-5f, -1.8f), new Vector2(0.72f, 1.25f), new Color(0.88f, 0.86f, 0.80f), false);
        player.AddComponent<BoxCollider2D>();
        player.AddComponent<Rigidbody2D>().gravityScale = 3.2f;
        player.AddComponent<PrototypePlayerMotor>();
        Camera cam = Camera.main;
        var follow = cam.gameObject.GetComponent<PrototypeCameraFollow>();
        if (follow == null) follow = cam.gameObject.AddComponent<PrototypeCameraFollow>();
        follow.Target = player.transform;
        BuildGameplayUI();
    }

    private void BuildGameplayUI(bool editingLayout = false)
    {
        Panel("GameplayHUD", editingLayout ? new Color(0.01f, 0.012f, 0.018f, 0.76f) : new Color(0f, 0f, 0f, 0.15f), Vector2.zero, Vector2.one);
        if (!editingLayout)
        {
            Label("AETHER  /  آزمایش منطقهٔ سبز", 19, Gold, new Vector2(0.5f, 0.94f), new Vector2(0.7f, 0.06f), TextAnchor.MiddleCenter);
            Label("حرکت  •  پرش  •  کاوش", 18, Muted, new Vector2(0.5f, 0.88f), new Vector2(0.7f, 0.05f), TextAnchor.MiddleCenter);
        }
        else
        {
            Label("چیدمان کنترل‌ها", 34, Color.white, new Vector2(0.5f, 0.83f), new Vector2(0.42f, 0.09f), TextAnchor.MiddleCenter);
            Label("کنترل را بکش تا جابه‌جا شود؛ با انتخاب کنترل و دکمه‌های اندازه، آن را تنظیم کن.", 19, Muted, new Vector2(0.5f, 0.76f), new Vector2(0.72f, 0.06f), TextAnchor.MiddleCenter);
        }

        var joystickObject = new GameObject("Gameplay Joystick Base", typeof(RectTransform), typeof(Image), typeof(FixedVirtualJoystick), typeof(GameplayControlHandle));
        joystickObject.transform.SetParent(safeAreaRoot != null ? safeAreaRoot : canvas.transform, false);
        var joystickRect = joystickObject.GetComponent<RectTransform>();
        GameplayControlHandle.ApplyLayout(joystickRect, "joystick", new Vector2(0.145f, 0.205f), new Vector2(218f, 218f));
        var joystickImage = joystickObject.GetComponent<Image>();
        joystickImage.sprite = GameplayControlSprite("JoystickBase");
        joystickImage.preserveAspect = true;
        joystickImage.color = Color.white;
        joystickImage.raycastTarget = true;

        var knobObject = new GameObject("Gameplay Joystick Knob", typeof(RectTransform), typeof(Image));
        knobObject.transform.SetParent(joystickObject.transform, false);
        var knobRect = knobObject.GetComponent<RectTransform>();
        knobRect.anchorMin = new Vector2(0.5f, 0.5f);
        knobRect.anchorMax = new Vector2(0.5f, 0.5f);
        knobRect.pivot = new Vector2(0.5f, 0.5f);
        knobRect.sizeDelta = new Vector2(92f, 92f);
        knobRect.anchoredPosition = Vector2.zero;
        var knobImage = knobObject.GetComponent<Image>();
        knobImage.sprite = GameplayControlSprite("JoystickKnob");
        knobImage.preserveAspect = true;
        knobImage.raycastTarget = false;

        var joystick = joystickObject.GetComponent<FixedVirtualJoystick>();
        joystick.Configure(knobRect);
        var joystickHandle = joystickObject.GetComponent<GameplayControlHandle>();
        joystickHandle.Configure("joystick", new Vector2(0.145f, 0.205f), new Vector2(218f, 218f), editingLayout);
        joystick.enabled = !editingLayout;
        joystickHandle.enabled = editingLayout;

        var jumpObject = new GameObject("Gameplay Jump Button", typeof(RectTransform), typeof(Image), typeof(TouchHoldButton), typeof(GameplayControlHandle));
        jumpObject.transform.SetParent(safeAreaRoot != null ? safeAreaRoot : canvas.transform, false);
        var jumpRect = jumpObject.GetComponent<RectTransform>();
        GameplayControlHandle.ApplyLayout(jumpRect, "jump", new Vector2(0.875f, 0.19f), new Vector2(154f, 154f));
        var jumpImage = jumpObject.GetComponent<Image>();
        jumpImage.sprite = GameplayControlSprite("Jump");
        jumpImage.preserveAspect = true;
        jumpImage.color = Color.white;
        jumpImage.raycastTarget = true;
        var jump = jumpObject.GetComponent<TouchHoldButton>();
        jump.Configure(TouchHoldButton.Action.Jump);
        jump.enabled = !editingLayout;
        var jumpHandle = jumpObject.GetComponent<GameplayControlHandle>();
        jumpHandle.Configure("jump", new Vector2(0.875f, 0.19f), new Vector2(154f, 154f), editingLayout);
        jumpHandle.enabled = editingLayout;

        if (!editingLayout)
        {
            var pauseObject = new GameObject("Gameplay Pause Button", typeof(RectTransform), typeof(Image), typeof(Button));
            pauseObject.transform.SetParent(safeAreaRoot != null ? safeAreaRoot : canvas.transform, false);
            var pauseRect = pauseObject.GetComponent<RectTransform>();
            pauseRect.anchorMin = new Vector2(0.955f, 0.92f);
            pauseRect.anchorMax = new Vector2(0.955f, 0.92f);
            pauseRect.sizeDelta = new Vector2(76f, 76f);
            var pauseImage = pauseObject.GetComponent<Image>();
            pauseImage.sprite = GameplayControlSprite("Pause");
            pauseImage.preserveAspect = true;
            pauseImage.color = Color.white;
            var pauseButton = pauseObject.GetComponent<Button>();
            pauseButton.targetGraphic = pauseImage;
            pauseButton.onClick.AddListener(ShowGameplayPause);
            var pauseColors = pauseButton.colors;
            pauseColors.normalColor = Color.white;
            pauseColors.highlightedColor = new Color(1f, 1f, 1f, 0.9f);
            pauseColors.pressedColor = new Color(0.86f, 0.67f, 0.29f, 0.72f);
            pauseButton.colors = pauseColors;
        }
        else
        {
            Button choose = MakeButton("انتخاب: جوی‌استیک", new Vector2(0.17f, 0.94f), new Vector2(0.20f, 0.075f), CycleLayoutSelection, false);
            MakeButton("− اندازه", new Vector2(0.38f, 0.94f), new Vector2(0.15f, 0.075f), () => ChangeSelectedControlScale(-0.1f), false);
            MakeButton("+ اندازه", new Vector2(0.54f, 0.94f), new Vector2(0.15f, 0.075f), () => ChangeSelectedControlScale(0.1f), false);
            MakeButton("ذخیره", new Vector2(0.69f, 0.94f), new Vector2(0.12f, 0.075f), SaveControlLayout, true);
            MakeButton("بازنشانی", new Vector2(0.82f, 0.94f), new Vector2(0.13f, 0.075f), ResetControlLayout, false);
            MakeButton("لغو", new Vector2(0.94f, 0.94f), new Vector2(0.09f, 0.075f), ContinueGameplay, false);
            GameplayControlHandle.SetSelectedControl(selectedLayoutControl);
            if (choose != null) StartCoroutine(UpdateLayoutSelectionLabel(choose));
        }
    }

    private IEnumerator UpdateLayoutSelectionLabel(Button button)
    {
        while (button != null && button.isActiveAndEnabled)
        {
            Text label = button.GetComponentInChildren<Text>();
            if (label != null)
                label.text = PrepareText(selectedLayoutControl == "joystick" ? "انتخاب: جوی‌استیک" : "انتخاب: پرش");
            yield return new WaitForSecondsRealtime(0.2f);
        }
    }

    private void CycleLayoutSelection()
    {
        selectedLayoutControl = selectedLayoutControl == "joystick" ? "jump" : "joystick";
        GameplayControlHandle.SetSelectedControl(selectedLayoutControl);
    }

    private void ChangeSelectedControlScale(float delta)
    {
        GameplayControlHandle handle = GameplayControlHandle.Find(selectedLayoutControl);
        if (handle != null) handle.ChangeScale(delta);
    }

    private void SaveControlLayout()
    {
        GameplayControlHandle.SaveAll();
        ContinueGameplay();
    }

    private void ResetControlLayout()
    {
        GameplayControlHandle.ResetAll();
        ShowControlLayout();
    }

    private void ShowControlLayout()
    {
        selectedLayoutControl = "joystick";
        ClearUI();
        BuildGameplayUI(true);
    }

    private void ShowGameplayPause()
    {
        Time.timeScale = 0f;
        TouchHoldButton.ResetAll();
        FixedVirtualJoystick.ResetAll();
        ClearUI();
        Panel("GameplayPauseBackdrop", new Color(0.01f, 0.012f, 0.018f, 0.86f), Vector2.zero, Vector2.one);
        Label("مکث", 52, Color.white, new Vector2(0.5f, 0.72f), new Vector2(0.4f, 0.12f), TextAnchor.MiddleCenter);
        MakeButton("ادامهٔ بازی", new Vector2(0.5f, 0.55f), new Vector2(0.28f, 0.09f), ContinueGameplay, true);
        MakeButton("تنظیم چیدمان کنترل‌ها", new Vector2(0.5f, 0.40f), new Vector2(0.38f, 0.09f), ShowControlLayout, false);
        MakeButton("بازگشت به منو", new Vector2(0.5f, 0.25f), new Vector2(0.28f, 0.09f), ReturnToMenu, false);
    }

    private void ContinueGameplay()
    {
        Time.timeScale = 1f;
        ClearUI();
        BuildGameplayUI(false);
    }

    private void ReturnToMenu()
    {
        Time.timeScale = 1f;
        TouchHoldButton.ResetAll();
        FixedVirtualJoystick.ResetAll();
        if (worldRoot != null) Destroy(worldRoot);
        if (Camera.main != null)
        {
            var follow = Camera.main.GetComponent<PrototypeCameraFollow>();
            if (follow != null) Destroy(follow);
            Camera.main.transform.position = new Vector3(0f, 1.5f, -10f);
        }
        ShowMenu();
    }

    private GameObject CreateBlock(string objectName, Vector2 position, Vector2 size, Color color, bool solid)
    {
        var go = new GameObject(objectName);
        go.transform.SetParent(worldRoot.transform);
        go.transform.position = position;
        go.transform.localScale = size;
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = WhiteSprite();
        renderer.color = color;
        if (solid) go.AddComponent<BoxCollider2D>();
        return go;
    }

    private static Sprite GameplayControlSprite(string id)
    {
        // Load a separately authored PNG texture and wrap it as a Sprite. Pixel art is never drawn in code.
        Texture2D texture = Resources.Load<Texture2D>("GameplayControls/" + id);
        if (texture == null) return null;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        switch (id)
        {
            case "JoystickBase":
                if (joystickBaseSprite == null) joystickBaseSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
                return joystickBaseSprite;
            case "JoystickKnob":
                if (joystickKnobSprite == null) joystickKnobSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
                return joystickKnobSprite;
            case "Jump":
                if (jumpControlSprite == null) jumpControlSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
                return jumpControlSprite;
            case "Pause":
                if (pauseControlSprite == null) pauseControlSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
                return pauseControlSprite;
            default:
                return null;
        }
    }

    private static Sprite WhiteSprite()
    {
        if (cachedWhiteSprite != null) return cachedWhiteSprite;
        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        texture.name = "Aether Runtime Pixel";
        cachedWhiteSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return cachedWhiteSprite;
    }

    private void Panel(string name, Color color, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(canvas.transform, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin; rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        var image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        if (safeAreaRoot != null) safeAreaRoot.SetAsLastSibling();
    }

    private Text Label(string value, int size, Color color, Vector2 center, Vector2 dimensions, TextAnchor alignment)
    {
        var go = new GameObject("Label: " + value, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(safeAreaRoot != null ? safeAreaRoot : canvas.transform, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = center - dimensions * 0.5f;
        rect.anchorMax = center + dimensions * 0.5f;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        var text = go.GetComponent<Text>();
        bool isRtl = ContainsRtl(value);
        text.text = PrepareText(value);
        text.font = isRtl && rtlFont != null ? rtlFont : font;
        text.fontSize = Mathf.Max(12, Mathf.RoundToInt(size * uiTextScale));
        text.color = new Color(1f, 1f, 1f, Mathf.Clamp01(color.a));
        text.fontStyle = FontStyle.Bold;
        text.raycastTarget = false;
        text.supportRichText = false;
        text.lineSpacing = 1.08f;
        text.alignment = alignment; text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        if (size >= 23)
        {
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.62f);
            shadow.effectDistance = new Vector2(0f, -1.5f);
            shadow.useGraphicAlpha = true;
        }
        RevealMenuElement(go);
        return text;
    }

    private Button MakeButton(string title, Vector2 center, Vector2 dimensions, UnityEngine.Events.UnityAction action, bool primary)
    {
        var go = new GameObject("Button: " + title, typeof(RectTransform), typeof(AetherParallelogramGraphic), typeof(Button));
        go.transform.SetParent(safeAreaRoot != null ? safeAreaRoot : canvas.transform, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = center - dimensions * 0.5f; rect.anchorMax = center + dimensions * 0.5f;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        var plate = go.GetComponent<AetherParallelogramGraphic>();
        Color fill = primary
            ? new Color(Gold.r, Gold.g, Gold.b, 0.13f)
            : new Color(Muted.r, Muted.g, Muted.b, 0.035f);
        Color outline = primary
            ? new Color(Gold.r, Gold.g, Gold.b, 0.62f)
            : new Color(Muted.r, Muted.g, Muted.b, 0.22f);
        plate.Configure(fill, outline, 0.12f, 1.6f);
        var button = go.GetComponent<Button>();
        button.targetGraphic = plate;
        var colors = button.colors;
        colors.normalColor = new Color(1f, 1f, 1f, 0.88f);
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.86f, 0.79f, 0.60f, 0.76f);
        colors.selectedColor = colors.normalColor;
        button.colors = colors;
        button.onClick.AddListener(action);
        go.AddComponent<AetherUIButtonFeedback>();
        RevealMenuElement(go);
        var label = Label(title, primary ? 27 : 22, primary ? Color.white : Muted,
            center, dimensions * 0.92f, TextAnchor.MiddleCenter);
        label.transform.SetAsLastSibling();
        return button;
    }

    private void MakeTouchButton(string title, Vector2 center, Vector2 dimensions, TouchHoldButton.Action action)
    {
        var go = new GameObject("Touch: " + title, typeof(RectTransform), typeof(Image), typeof(TouchHoldButton));
        go.transform.SetParent(safeAreaRoot != null ? safeAreaRoot : canvas.transform, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = center - dimensions * 0.5f; rect.anchorMax = center + dimensions * 0.5f;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = new Color(Gold.r, Gold.g, Gold.b, 0.16f);
        var touch = go.GetComponent<TouchHoldButton>();
        touch.Configure(action);
        var label = Label(title, (title == "JUMP" || title == "پرش") ? 24 : 42, Gold, center, dimensions * 0.9f, TextAnchor.MiddleCenter);
        label.transform.SetAsLastSibling();
    }

    private static string PrepareText(string value)
    {
        if (string.IsNullOrEmpty(value) || !ContainsRtl(value)) return value;
        string[] lines = value.Split('\n');
        for (int i = 0; i < lines.Length; i++) lines[i] = RtlText.Visualize(lines[i]);
        return string.Join("\n", lines);
    }

    private static bool ContainsRtl(string value)
    {
        for (int i = 0; i < value.Length; i++)
            if ((value[i] >= '\u0590' && value[i] <= '\u08FF') || (value[i] >= '\uFB1D' && value[i] <= '\uFEFC')) return true;
        return false;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) TouchHoldButton.ResetAll();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused) TouchHoldButton.ResetAll();
    }

    private void ClearUI()
    {
        if (canvas == null) return;
        for (int i = canvas.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = canvas.transform.GetChild(i);
            if (safeAreaRoot != null && child == safeAreaRoot) continue;
            Destroy(child.gameObject);
        }
        if (safeAreaRoot != null)
        {
            for (int i = safeAreaRoot.childCount - 1; i >= 0; i--)
                Destroy(safeAreaRoot.GetChild(i).gameObject);
            safeAreaRoot.SetAsLastSibling();
        }
    }
}
